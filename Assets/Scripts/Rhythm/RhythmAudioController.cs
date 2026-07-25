using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace BeatMemories
{
    /// <summary>
    /// Schedules music loops and preparation snares on the Conductor's DSP
    /// timeline. This component is opt-in; scenes without it keep the legacy
    /// audio behavior.
    /// </summary>
    [DefaultExecutionOrder(-40)]
    [DisallowMultipleComponent]
    public sealed class RhythmAudioController : MonoBehaviour
    {
        private const int MusicSourceCount = 2;
        private const int SnareSourceCount = 8;
        private const double ScheduleAheadSeconds = 1.0;
        private const double MinimumSwitchLeadSeconds = 0.05;
        private const double DspEpsilon = 0.001;

        [Header("References")]
        [SerializeField] private RoundManager round;
        [SerializeField] private Conductor conductor;
        [SerializeField] private StageSoundtrackCatalogSO catalog;

        [Header("Mixer routing")]
        [SerializeField] private AudioMixerGroup musicOutput;
        [SerializeField] private AudioMixerGroup sfxOutput;

        [Header("Preparation snare")]
        [SerializeField] private AudioClip preparationSnare;
        [SerializeField, Range(0f, 1f)] private float preparationSnareScale = 1f;
        [Tooltip("0이면 PhaseSO 볼륨을 사용한다. 복제 씬에서는 모든 준비 구간과 보스 페이지 전환에 동일한 값을 적용한다.")]
        [SerializeField, Range(0f, 1f)]
        private float preparationSnareVolumeOverride;

        private readonly AudioSource[] musicSources =
            new AudioSource[MusicSourceCount];
        private readonly AudioSource[] snareSources =
            new AudioSource[SnareSourceCount];
        private readonly double[] musicSourceStartDsp =
        {
            double.NegativeInfinity,
            double.NegativeInfinity,
        };
        private readonly double[] musicSourceEndDsp =
        {
            double.NegativeInfinity,
            double.NegativeInfinity,
        };

        private StageSoundtrackCatalogSO.Entry selectedCue;
        private StageSoundtrackCatalogSO.Entry loopCue;
        private StageSoundtrackCatalogSO.Entry pendingPageCue;
        private bool pendingPageCueRequested;
        private bool loopSchedulingEnabled;
        private bool switchPending;
        private double loopStartDspTime;
        private double pendingSwitchDspTime;
        private int firstLoopSourceIndex;
        private long nextLoopIteration;
        private int nextSnareSourceIndex;
        private float nextPreparationSnareVolume;
        private bool currentStageClipsReady;

        public bool IsCurrentClipReady => currentStageClipsReady;

        private void Awake()
        {
            if (round == null) round = FindFirstObjectByType<RoundManager>();
            if (conductor == null) conductor = FindFirstObjectByType<Conductor>();
            EnsureAudioSources();
        }

        private void OnEnable()
        {
            if (round != null)
            {
                round.OnStageApplied += HandleStageApplied;
                round.OnEnemyPageTransitionStarted +=
                    HandleEnemyPageTransitionStarted;
            }

            if (conductor != null)
            {
                conductor.OnClockScheduled += HandleClockScheduled;
                conductor.OnClockStopped += HandleClockStopped;
                conductor.OnPreparationScheduled += HandlePreparationScheduled;
            }

            if (round != null && round.CurrentStage != null)
                HandleStageApplied(round.CurrentStage);
        }

        private void OnDisable()
        {
            if (round != null)
            {
                round.OnStageApplied -= HandleStageApplied;
                round.OnEnemyPageTransitionStarted -=
                    HandleEnemyPageTransitionStarted;
            }

            if (conductor != null)
            {
                conductor.OnClockScheduled -= HandleClockScheduled;
                conductor.OnClockStopped -= HandleClockStopped;
                conductor.OnPreparationScheduled -= HandlePreparationScheduled;
            }

            StopAllAudio();
        }

        private void Update()
        {
            TrySchedulePendingSwitch();
            ScheduleMusicLookahead();
        }

        /// <summary>
        /// Supplies the authored volume for the next preparation interval.
        /// A null phase intentionally makes that interval silent.
        /// </summary>
        public void SetPreparationPhase(PhaseSO phase)
        {
            nextPreparationSnareVolume =
                preparationSnareVolumeOverride > 0f
                    ? preparationSnareVolumeOverride
                    : phase != null
                        ? phase.PreparationSnareVolume
                        : 0f;
        }

        /// <summary>
        /// Requests every cue used by the selected stage and yields until
        /// Unity reports either Loaded or Failed. This also warms a later boss
        /// page before its fixed DSP transition deadline.
        /// </summary>
        public IEnumerator PrepareCurrentClip()
        {
            currentStageClipsReady = false;
            List<AudioClip> clips = CollectCurrentStageClips();
            if (clips.Count == 0)
            {
                Debug.LogError(
                    "[RhythmAudio] The current stage has no clips to load.",
                    this);
                yield break;
            }

            for (int i = 0; i < clips.Count; i++)
                RequestClipLoad(clips[i]);

            bool loading;
            do
            {
                loading = false;
                for (int i = 0; i < clips.Count; i++)
                {
                    if (clips[i].loadState == AudioDataLoadState.Loading)
                    {
                        loading = true;
                        break;
                    }
                }

                if (loading) yield return null;
            }
            while (loading);

            bool allLoaded = true;
            for (int i = 0; i < clips.Count; i++)
            {
                if (clips[i].loadState == AudioDataLoadState.Loaded)
                    continue;

                allLoaded = false;
                Debug.LogError(
                    $"[RhythmAudio] '{clips[i].name}' did not reach the " +
                    $"Loaded state ({clips[i].loadState}).",
                    this);
            }

            currentStageClipsReady = allLoaded;
        }

        private List<AudioClip> CollectCurrentStageClips()
        {
            var clips = new List<AudioClip>();
            StageSO currentStage = selectedCue != null
                ? selectedCue.Stage
                : round != null
                    ? round.CurrentStage
                    : null;

            if (catalog != null && currentStage != null)
            {
                IReadOnlyList<StageSoundtrackCatalogSO.Entry> entries =
                    catalog.Entries;
                for (int i = 0; i < entries.Count; i++)
                {
                    StageSoundtrackCatalogSO.Entry entry = entries[i];
                    if (entry == null
                        || entry.Stage != currentStage
                        || entry.Clip == null
                        || clips.Contains(entry.Clip))
                    {
                        continue;
                    }

                    clips.Add(entry.Clip);
                }
            }

            if (clips.Count == 0
                && selectedCue != null
                && selectedCue.Clip != null)
            {
                clips.Add(selectedCue.Clip);
            }

            return clips;
        }

        private void HandleStageApplied(StageSO stage)
        {
            StopMusicImmediately();
            selectedCue = null;
            loopCue = null;
            pendingPageCue = null;
            pendingPageCueRequested = false;
            switchPending = false;
            nextPreparationSnareVolume = 0f;
            currentStageClipsReady = false;

            bool hasCue = TrySelectCue(stage, 1, out selectedCue);
            if (hasCue) RequestClipLoad(selectedCue.Clip);
            if (conductor == null || stage == null) return;

            if (conductor.IsRunning)
            {
                Debug.LogError(
                    "[RhythmAudio] A stage soundtrack must be configured " +
                    "while the Conductor is stopped.",
                    this);
                return;
            }

            conductor.SetRuntimeTempo(
                hasCue ? selectedCue.Bpm : stage.bpm,
                stage.startDelay);
        }

        private void HandleClockScheduled(double scheduledStartDspTime)
        {
            StopMusicImmediately();
            if (!IsPlayable(selectedCue)) return;

            if (selectedCue.Clip.loadState != AudioDataLoadState.Loaded)
            {
                Debug.LogError(
                    $"[RhythmAudio] '{selectedCue.Clip.name}' was not loaded " +
                    "before the DSP clock started. Music remains silent.",
                    this);
                return;
            }

            if (!ValidateLoopLength(selectedCue)) return;
            BeginLoopTimeline(selectedCue, scheduledStartDspTime);
        }

        private void HandleClockStopped()
        {
            StopAllAudio();
            pendingPageCue = null;
            pendingPageCueRequested = false;
            switchPending = false;
            nextPreparationSnareVolume = 0f;
        }

        private void HandleEnemyPageTransitionStarted(
            int page,
            int pageCount,
            int preparationBeats)
        {
            if (preparationBeats > 0
                && preparationSnareVolumeOverride > 0f)
            {
                nextPreparationSnareVolume =
                    preparationSnareVolumeOverride;
            }

            pendingPageCueRequested = true;
            pendingPageCue = null;

            StageSO stage = round != null ? round.CurrentStage : null;
            if (!TrySelectCue(stage, page, out pendingPageCue))
                return;

            RequestClipLoad(pendingPageCue.Clip);
            conductor?.SetTempoAfterPreparation(pendingPageCue.Bpm);

            if (preparationBeats <= 0)
            {
                Debug.LogError(
                    "[RhythmAudio] Page soundtrack changes require a " +
                    "scheduled preparation boundary.",
                    this);
            }
        }

        private void HandlePreparationScheduled(
            double startDspTime,
            double endDspTime,
            int beats)
        {
            SchedulePreparationSnares(startDspTime, endDspTime, beats);
            nextPreparationSnareVolume = 0f;

            if (!pendingPageCueRequested) return;

            pendingPageCueRequested = false;
            StopLoopTimelineAt(endDspTime);

            if (!IsPlayable(pendingPageCue))
            {
                pendingPageCue = null;
                return;
            }

            pendingSwitchDspTime = endDspTime;
            switchPending = true;
            TrySchedulePendingSwitch();
        }

        private bool TrySelectCue(
            StageSO stage,
            int enemyPage,
            out StageSoundtrackCatalogSO.Entry cue)
        {
            if (catalog != null
                && catalog.TryGetCue(stage, enemyPage, out cue)
                && IsPlayable(cue))
                return true;

            string stageName = stage != null ? stage.name : "(null)";
            Debug.LogError(
                $"[RhythmAudio] Missing or invalid soundtrack cue for " +
                $"{stageName}, enemy page {Mathf.Max(1, enemyPage)}. " +
                "Music remains silent.",
                this);
            cue = null;
            return false;
        }

        private static bool IsPlayable(StageSoundtrackCatalogSO.Entry cue)
            => cue != null
               && cue.Stage != null
               && cue.Clip != null
               && cue.Bpm > 0f
               && cue.LoopBeats > 0;

        private static void RequestClipLoad(AudioClip clip)
        {
            if (clip != null && clip.loadState == AudioDataLoadState.Unloaded)
                clip.LoadAudioData();
        }

        private bool ValidateLoopLength(StageSoundtrackCatalogSO.Entry cue)
        {
            double expectedSeconds = LoopDurationSeconds(
                cue.LoopBeats,
                cue.Bpm);
            double toleranceSeconds = 1.5 / cue.Clip.frequency;
            double errorSeconds = Math.Abs(cue.Clip.length - expectedSeconds);
            if (errorSeconds <= toleranceSeconds) return true;

            Debug.LogError(
                $"[RhythmAudio] '{cue.Clip.name}' is {cue.Clip.length:F6}s, " +
                $"but {cue.LoopBeats} beats at {cue.Bpm:F5} BPM require " +
                $"{expectedSeconds:F6}s. Music remains silent.",
                this);
            return false;
        }

        private void BeginLoopTimeline(
            StageSoundtrackCatalogSO.Entry cue,
            double cueStartDspTime)
        {
            int sourceIndex = FindAssignableMusicSource();
            if (sourceIndex < 0)
            {
                Debug.LogError(
                    "[RhythmAudio] No free music source was available for " +
                    "the scheduled cue.",
                    this);
                return;
            }

            loopCue = cue;
            loopStartDspTime = cueStartDspTime;
            firstLoopSourceIndex = sourceIndex;
            nextLoopIteration = 0;
            loopSchedulingEnabled = true;
            if (ScheduleLoop(nextLoopIteration))
                nextLoopIteration++;
        }

        private void ScheduleMusicLookahead()
        {
            if (!loopSchedulingEnabled || !IsPlayable(loopCue)) return;

            double horizon = AudioSettings.dspTime + ScheduleAheadSeconds;
            while (LoopStartDspTime(nextLoopIteration) <= horizon)
            {
                if (!ScheduleLoop(nextLoopIteration)) return;
                nextLoopIteration++;
            }
        }

        private bool ScheduleLoop(long iteration)
        {
            int sourceIndex =
                (firstLoopSourceIndex + (int)(iteration % MusicSourceCount))
                % MusicSourceCount;
            double startDspTime = LoopStartDspTime(iteration);
            double endDspTime = LoopStartDspTime(iteration + 1);
            double now = AudioSettings.dspTime;

            if (musicSourceEndDsp[sourceIndex] > now + DspEpsilon)
            {
                Debug.LogError(
                    $"[RhythmAudio] Music source {sourceIndex} was still " +
                    $"reserved when loop {iteration} needed scheduling.",
                    this);
                return false;
            }

            if (startDspTime < now - DspEpsilon)
            {
                Debug.LogError(
                    $"[RhythmAudio] Missed the DSP scheduling deadline for " +
                    $"loop {iteration}. Music stops instead of drifting.",
                    this);
                loopSchedulingEnabled = false;
                return false;
            }

            AudioSource source = musicSources[sourceIndex];
            source.clip = loopCue.Clip;
            source.outputAudioMixerGroup = musicOutput;
            source.volume = loopCue.Volume;
            source.timeSamples = 0;
            source.PlayScheduled(startDspTime);
            source.SetScheduledEndTime(endDspTime);
            musicSourceStartDsp[sourceIndex] = startDspTime;
            musicSourceEndDsp[sourceIndex] = endDspTime;
            return true;
        }

        private double LoopStartDspTime(long iteration)
            => CalculateLoopDspTime(
                loopStartDspTime,
                iteration,
                loopCue.LoopBeats,
                loopCue.Bpm);

        internal static double CalculateLoopDspTime(
            double cueStartDspTime,
            long iteration,
            int loopBeats,
            float bpm)
            => cueStartDspTime
               + iteration * LoopDurationSeconds(loopBeats, bpm);

        internal static double LoopDurationSeconds(int loopBeats, float bpm)
            => Math.Max(1, loopBeats) * 60.0 / Math.Max(1f, bpm);

        private void StopLoopTimelineAt(double endDspTime)
        {
            loopSchedulingEnabled = false;
            for (int i = 0; i < musicSources.Length; i++)
            {
                if (musicSourceStartDsp[i] >= endDspTime - DspEpsilon)
                {
                    StopMusicSource(i);
                    continue;
                }

                if (musicSourceEndDsp[i] <= endDspTime + DspEpsilon)
                    continue;

                musicSources[i].SetScheduledEndTime(endDspTime);
                musicSourceEndDsp[i] = endDspTime;
            }
        }

        private void TrySchedulePendingSwitch()
        {
            if (!switchPending) return;

            if (pendingPageCue == null
                || pendingPageCue.Clip == null)
            {
                switchPending = false;
                return;
            }

            AudioDataLoadState loadState =
                pendingPageCue.Clip.loadState;
            if (loadState == AudioDataLoadState.Unloaded)
            {
                RequestClipLoad(pendingPageCue.Clip);
                return;
            }

            if (loadState == AudioDataLoadState.Loading)
            {
                if (AudioSettings.dspTime
                    >= pendingSwitchDspTime - DspEpsilon)
                {
                    Debug.LogError(
                        $"[RhythmAudio] '{pendingPageCue.Clip.name}' missed " +
                        "the page-transition DSP deadline while loading. " +
                        "Music becomes silent.",
                        this);
                    switchPending = false;
                    pendingPageCue = null;
                }
                return;
            }

            if (loadState == AudioDataLoadState.Failed)
            {
                Debug.LogError(
                    $"[RhythmAudio] Failed to load " +
                    $"'{pendingPageCue.Clip.name}' for the page transition.",
                    this);
                switchPending = false;
                pendingPageCue = null;
                return;
            }

            int sourceIndex = FindAssignableMusicSource();
            if (sourceIndex < 0)
            {
                if (AudioSettings.dspTime
                    >= pendingSwitchDspTime - MinimumSwitchLeadSeconds)
                {
                    Debug.LogError(
                        "[RhythmAudio] No music source became free in time " +
                        "for the page soundtrack transition.",
                        this);
                    switchPending = false;
                    pendingPageCue = null;
                }
                return;
            }

            StageSoundtrackCatalogSO.Entry cue = pendingPageCue;
            if (!ValidateLoopLength(cue))
            {
                switchPending = false;
                pendingPageCue = null;
                return;
            }

            pendingPageCue = null;
            switchPending = false;
            selectedCue = cue;
            firstLoopSourceIndex = sourceIndex;
            loopCue = cue;
            loopStartDspTime = pendingSwitchDspTime;
            nextLoopIteration = 0;
            loopSchedulingEnabled = true;
            if (ScheduleLoop(nextLoopIteration))
                nextLoopIteration++;
        }

        private int FindAssignableMusicSource()
        {
            double now = AudioSettings.dspTime;
            for (int i = 0; i < musicSources.Length; i++)
            {
                if (musicSources[i] == null) continue;
                if (musicSourceEndDsp[i] > now + DspEpsilon) continue;
                return i;
            }
            return -1;
        }

        private void SchedulePreparationSnares(
            double startDspTime,
            double endDspTime,
            int beats)
        {
            float volume = Mathf.Clamp01(
                nextPreparationSnareVolume * preparationSnareScale);
            if (preparationSnare == null || volume <= 0f || beats <= 1)
                return;

            double secondsPerBeat = (endDspTime - startDspTime) / beats;
            if (secondsPerBeat <= 0.0) return;

            for (int beat = 1; beat < beats; beat++)
            {
                double beatDspTime = startDspTime + beat * secondsPerBeat;
                if (beatDspTime <= AudioSettings.dspTime + DspEpsilon)
                    continue;

                AudioSource source =
                    snareSources[nextSnareSourceIndex % snareSources.Length];
                nextSnareSourceIndex++;
                source.Stop();
                source.clip = preparationSnare;
                source.outputAudioMixerGroup = sfxOutput;
                source.volume = volume;
                source.timeSamples = 0;
                source.PlayScheduled(beatDspTime);
                source.SetScheduledEndTime(
                    beatDspTime
                    + Math.Min(preparationSnare.length, secondsPerBeat));
            }
        }

        private void EnsureAudioSources()
        {
            for (int i = 0; i < musicSources.Length; i++)
            {
                if (musicSources[i] != null) continue;
                AudioSource source = gameObject.AddComponent<AudioSource>();
                ConfigureSource(source, musicOutput);
                musicSources[i] = source;
            }

            for (int i = 0; i < snareSources.Length; i++)
            {
                if (snareSources[i] != null) continue;
                AudioSource source = gameObject.AddComponent<AudioSource>();
                ConfigureSource(source, sfxOutput);
                snareSources[i] = source;
            }
        }

        private static void ConfigureSource(
            AudioSource source,
            AudioMixerGroup output)
        {
            source.playOnAwake = false;
            source.loop = false;
            source.spatialBlend = 0f;
            source.outputAudioMixerGroup = output;
        }

        private void StopAllAudio()
        {
            StopMusicImmediately();
            for (int i = 0; i < snareSources.Length; i++)
                snareSources[i]?.Stop();
            nextSnareSourceIndex = 0;
        }

        private void StopMusicImmediately()
        {
            loopSchedulingEnabled = false;
            switchPending = false;
            loopCue = null;
            nextLoopIteration = 0;
            for (int i = 0; i < musicSources.Length; i++)
                StopMusicSource(i);
        }

        private void StopMusicSource(int index)
        {
            musicSources[index]?.Stop();
            musicSourceStartDsp[index] = double.NegativeInfinity;
            musicSourceEndDsp[index] = double.NegativeInfinity;
        }
    }
}
