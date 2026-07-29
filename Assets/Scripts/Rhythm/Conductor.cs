using System;
using UnityEngine;

namespace BeatMemories
{
    /// <summary>
    /// DSP-backed rhythm clock. Gameplay, metronome audio, and presentation all
    /// derive their phase from the same scheduled DSP timeline.
    /// </summary>
    [DefaultExecutionOrder(-50)]
    public class Conductor : MonoBehaviour
    {
        public const int BeatsPerMeasure = 4;
        public const int ResponseStartBeat = BeatsPerMeasure;
        public const int BeatsPerCycle = ResponseStartBeat + BeatsPerMeasure;

        [Header("Tempo")]
        [Tooltip("Shared judgment offsets and metronome clips.")]
        [SerializeField] private RhythmTimingSettings timingSettings;
        [Tooltip("Fallback BPM used until the BGM catalog supplies its runtime tempo.")]
        [SerializeField, Min(1f)] private float bpm = 90f;
        [SerializeField] private bool playOnStart = true;

        [Tooltip("Seconds between scheduling the clock and its first beat.")]
        [SerializeField, Min(0f)] private float startDelay = 3f;

        private bool runtimeTempoEnabled;
        private float runtimeBpm;
        private float runtimeStartDelay;
        private float pendingTempoAfterPreparation;
        private bool hasPendingTempoAfterPreparation;
        private float scheduledTempoAfterPreparation;
        private bool hasScheduledTempoAfterPreparation;

        private readonly RhythmTempoMap clockTempoMap = new RhythmTempoMap();
        private readonly RhythmTempoMap gameplayTempoMap = new RhythmTempoMap();
        private bool tempoMapsInitialized;

        public float Bpm
        {
            get
            {
                if (IsRunning && tempoMapsInitialized)
                    return (float)clockTempoMap.TempoAt(AudioSettings.dspTime);
                return ConfiguredBpm;
            }
            set => bpm = Mathf.Max(1f, value);
        }

        public float SecondsPerBeat => 60f / Bpm;
        public float EarlyOffset => timingSettings != null
            ? timingSettings.EarlyOffset
            : 0.12f;
        public float LateOffset => timingSettings != null
            ? timingSettings.LateOffset
            : 0.12f;
        public float InputBufferSeconds => timingSettings != null
            ? timingSettings.InputBufferSeconds
            : 0.15f;
        public RhythmTimingSettings TimingSettings => timingSettings;
        public bool IsRunning { get; private set; }

        /// <summary>The immutable DSP anchor for clock beat zero.</summary>
        public double ScheduledStartDspTime { get; private set; }

        /// <summary>
        /// Continuous metronome beat position derived from DSP time. Beat zero
        /// is the first scheduled click; preparation beats remain on this axis.
        /// </summary>
        public double ClockBeatPosition => IsRunning && tempoMapsInitialized
            ? clockTempoMap.BeatPositionAt(AudioSettings.dspTime)
            : -1.0;

        public double TimeUntilStart => IsRunning && TotalBeats < 0
            ? Math.Max(0.0, gameplayStartDspTime - AudioSettings.dspTime)
            : 0.0;

        public bool IsCountingDown
            => IsRunning
               && TotalBeats < 0
               && AudioSettings.dspTime < gameplayStartDspTime;

        /// <summary>
        /// Gameplay song time excludes preparation sections. It remains
        /// compatible with the existing input and judgment time domain.
        /// </summary>
        public double SongPosition => AudioSettings.dspTime - gameplayStartDspTime;

        public double RealtimeToSongPosition(double realtimeTime)
            => realtimeTime - startTime;

        /// <summary>
        /// Ideal gameplay time for a global gameplay beat. Preparation beats
        /// are deliberately absent from this axis.
        /// </summary>
        public double BeatToTime(int globalBeat)
        {
            if (tempoMapsInitialized)
                return gameplayTempoMap.TimeAtBeat(globalBeat);
            return globalBeat * (60.0 / ConfiguredBpm);
        }

        public int TotalBeats { get; private set; } = -1;
        public int ClockBeats { get; private set; } = -1;
        public int CycleIndex { get; private set; }
        public int BeatInCycle { get; private set; }
        public int BeatInMeasure
            => IsResponseMeasure ? BeatInCycle - ResponseStartBeat : BeatInCycle;
        public bool IsResponseMeasure => BeatInCycle >= ResponseStartBeat;

        private float ConfiguredBpm => runtimeTempoEnabled
            ? Mathf.Max(1f, runtimeBpm)
            : Mathf.Max(1f, bpm);

        private float ConfiguredStartDelay => runtimeTempoEnabled
            ? Mathf.Max(0f, runtimeStartDelay)
            : Mathf.Max(0f, startDelay);

        private double startTime;
        private double gameplayStartDspTime;
        private const double MinimumDspScheduleLeadSeconds = 0.2;
        private const int MetronomeSourceCount = 8;
        private const int MetronomeLookaheadBeats = 4;
        private readonly AudioSource[] metronomeSources =
            new AudioSource[MetronomeSourceCount];
        private readonly int[] metronomeScheduledBeats =
            new int[MetronomeSourceCount];
        private readonly double[] metronomeScheduledDspTimes =
            new double[MetronomeSourceCount];
        private int nextMetronomeBeatToSchedule;
        private bool pendingBeatDispatch;
        private int queuedPreparationBeats;
        private int preparationBeatCount;
        private int preparationBeatIndex = -1;
        private int preparationStartClockBeat;
        private double preparationStartDspTime;
        private double preparationEndDspTime;

        public bool IsPreparing => preparationBeatCount > 0;

        public event Action<double> OnClockScheduled;
        public event Action OnClockStopped;
        public event Action<double, double, int> OnPreparationScheduled;
        public event Action<int> OnClockBeat;
        public event Action<int> OnPreparationMeasureStart;
        public event Action<int> OnPreparationBeat;
        public event Action<int> OnBeat;
        public event Action<int> OnPresentMeasureStart;
        public event Action<int> OnResponseMeasureStart;
        internal event Action<int> OnResponseMeasureEnd;

        private void Awake()
        {
            if (timingSettings == null)
            {
                timingSettings =
                    Resources.Load<RhythmTimingSettings>(
                        RhythmTimingSettings.ResourceName);
            }

            if (timingSettings == null
                || (timingSettings.Tick == null && timingSettings.Tack == null))
            {
                return;
            }

            for (int i = 0; i < metronomeSources.Length; i++)
            {
                AudioSource source = gameObject.AddComponent<AudioSource>();
                source.playOnAwake = false;
                source.loop = false;
                source.spatialBlend = 0f;
                source.outputAudioMixerGroup = timingSettings.MetronomeOutput;
                metronomeSources[i] = source;
                metronomeScheduledBeats[i] = -1;
            }
        }

        private void OnDisable()
        {
            StopClock();
        }

        private void Start()
        {
            if (playOnStart) StartClock();
        }

        /// <summary>
        /// Enables scene-specific tempo configuration. Call before StartClock;
        /// a running clock keeps its already-scheduled tempo map unchanged.
        /// </summary>
        public void SetRuntimeTempo(float newBpm, float newStartDelay)
        {
            runtimeBpm = Mathf.Max(1f, newBpm);
            runtimeStartDelay = Mathf.Max(0f, newStartDelay);
            runtimeTempoEnabled = true;
        }

        /// <summary>
        /// Requests a tempo change at the exact end of the next/current
        /// preparation section. Preparation itself keeps the old tempo.
        /// </summary>
        public void SetTempoAfterPreparation(float newBpm)
        {
            if (!runtimeTempoEnabled)
            {
                runtimeBpm = ConfiguredBpm;
                runtimeStartDelay = ConfiguredStartDelay;
                runtimeTempoEnabled = true;
            }

            pendingTempoAfterPreparation = Mathf.Max(1f, newBpm);
            hasPendingTempoAfterPreparation = true;

            if (IsPreparing)
                SchedulePendingPreparationTempo();
        }

        public void StartClock()
        {
            if (IsRunning) StopClock();

            StopMetronome();
            IsRunning = true;
            TotalBeats = -1;
            ClockBeats = -1;
            CycleIndex = 0;
            BeatInCycle = 0;
            pendingBeatDispatch = false;
            queuedPreparationBeats = 0;
            preparationBeatCount = 0;
            preparationBeatIndex = -1;
            // 정지 상태에서 페이지 전환 이벤트가 예약한 템포는 보존한다.
            // 예약 시점(DSP)은 새 준비 구간이 실제로 시작될 때 계산한다.
            hasScheduledTempoAfterPreparation = false;

            double dspNow = AudioSettings.dspTime;
            double realtimeNow = Time.realtimeSinceStartupAsDouble;
            double delay = ConfiguredStartDelay;
            double initialBpm = ConfiguredBpm;

            ScheduledStartDspTime =
                CalculateScheduledStartDspTime(dspNow, delay);
            gameplayStartDspTime = ScheduledStartDspTime;
            startTime = realtimeNow + (ScheduledStartDspTime - dspNow);
            clockTempoMap.Reset(ScheduledStartDspTime, initialBpm);
            gameplayTempoMap.Reset(0.0, initialBpm);
            tempoMapsInitialized = true;
            nextMetronomeBeatToSchedule = 0;

            OnClockScheduled?.Invoke(ScheduledStartDspTime);
            ScheduleMetronomeLookahead();
        }

        internal static double CalculateScheduledStartDspTime(
            double dspNow,
            double configuredDelay)
            => dspNow
               + Math.Max(
                   Math.Max(0.0, configuredDelay),
                   MinimumDspScheduleLeadSeconds);

        public void StopClock()
        {
            bool wasRunning = IsRunning;
            IsRunning = false;
            queuedPreparationBeats = 0;
            preparationBeatCount = 0;
            preparationBeatIndex = -1;
            hasPendingTempoAfterPreparation = false;
            hasScheduledTempoAfterPreparation = false;
            StopMetronome();
            if (wasRunning) OnClockStopped?.Invoke();
        }

        public void QueuePreparationBeats(int beats)
        {
            if (!IsRunning || beats <= 0) return;
            queuedPreparationBeats = Math.Max(queuedPreparationBeats, beats);
        }

        public bool AdvanceResponseBeatNow(int resolvedSlot)
        {
            return false;
        }

        public void DelayClock(double seconds)
        {
        }

        private void Update()
        {
            if (!IsRunning) return;
            ScheduleMetronomeLookahead();

            if (pendingBeatDispatch)
            {
                if (queuedPreparationBeats > 0)
                {
                    pendingBeatDispatch = false;
                    BeginPreparation();
                    return;
                }

                pendingBeatDispatch = false;
                DispatchCurrentBeat();
                if (!IsRunning) return;
            }

            if (IsPreparing)
            {
                UpdatePreparation();
                return;
            }

            double songPosition = SongPosition;
            if (songPosition < 0.0) return;

            int beatsNow =
                (int)Math.Floor(gameplayTempoMap.BeatPositionAt(songPosition));
            while (beatsNow > TotalBeats)
            {
                TotalBeats++;
                if (!AdvanceBeat()) break;
            }
        }

        private bool AdvanceBeat()
        {
            if (TotalBeats > 0 && TotalBeats % BeatsPerCycle == 0)
            {
                OnResponseMeasureEnd?.Invoke((TotalBeats / BeatsPerCycle) - 1);
                if (!IsRunning) return false;
                if (queuedPreparationBeats > 0)
                {
                    BeginPreparation();
                    return false;
                }

                DispatchCurrentBeat();
                return IsRunning;
            }

            DispatchCurrentBeat();
            return IsRunning;
        }

        private void DispatchCurrentBeat()
        {
            CycleIndex = TotalBeats / BeatsPerCycle;
            BeatInCycle = TotalBeats % BeatsPerCycle;

            if (BeatInCycle == 0)
                OnPresentMeasureStart?.Invoke(CycleIndex);

            DispatchClockBeat();
            OnBeat?.Invoke(BeatInCycle);
            if (BeatInCycle == ResponseStartBeat)
                OnResponseMeasureStart?.Invoke(CycleIndex);
        }

        private void DispatchClockBeat()
        {
            ClockBeats++;
            OnClockBeat?.Invoke(ClockBeats);
        }

        private void ScheduleMetronomeLookahead()
        {
            if (!tempoMapsInitialized
                || timingSettings == null
                || metronomeSources[0] == null)
            {
                return;
            }

            double scheduleThroughBeat =
                clockTempoMap.BeatPositionAt(AudioSettings.dspTime)
                + MetronomeLookaheadBeats;
            while (nextMetronomeBeatToSchedule <= scheduleThroughBeat)
            {
                ScheduleMetronomeBeat(nextMetronomeBeatToSchedule);
                nextMetronomeBeatToSchedule++;
            }
        }

        private void ScheduleMetronomeBeat(int clockBeat)
        {
            int beatInMeasure = clockBeat % BeatsPerMeasure;
            AudioClip clip = beatInMeasure == 0
                ? timingSettings.Tick
                : timingSettings.Tack;
            if (clip == null) return;

            int sourceIndex = clockBeat % metronomeSources.Length;
            AudioSource source = metronomeSources[sourceIndex];
            source.clip = clip;
            source.volume = timingSettings.MetronomeVolume;

            double beatDspTime = clockTempoMap.TimeAtBeat(clockBeat);
            double secondsPerBeat =
                60.0 / clockTempoMap.TempoAt(beatDspTime);
            source.PlayScheduled(beatDspTime);
            source.SetScheduledEndTime(
                beatDspTime + Math.Min(clip.length, secondsPerBeat));
            metronomeScheduledBeats[sourceIndex] = clockBeat;
            metronomeScheduledDspTimes[sourceIndex] = beatDspTime;
        }

        private void StopMetronome()
        {
            for (int i = 0; i < metronomeSources.Length; i++)
            {
                metronomeSources[i]?.Stop();
                metronomeScheduledBeats[i] = -1;
                metronomeScheduledDspTimes[i] = 0.0;
            }

            nextMetronomeBeatToSchedule = 0;
        }

        private void RescheduleMetronomeFromBeat(int firstChangedBeat)
        {
            double now = AudioSettings.dspTime;
            int firstReschedulableBeat = firstChangedBeat;
            for (int i = 0; i < metronomeSources.Length; i++)
            {
                if (metronomeScheduledBeats[i] < firstChangedBeat
                    || metronomeScheduledDspTimes[i] > now)
                {
                    continue;
                }

                // Never interrupt or duplicate a click whose attack has
                // already reached the audio device.
                firstReschedulableBeat = Math.Max(
                    firstReschedulableBeat,
                    metronomeScheduledBeats[i] + 1);
            }

            for (int i = 0; i < metronomeSources.Length; i++)
            {
                if (metronomeScheduledBeats[i] < firstReschedulableBeat
                    || metronomeScheduledDspTimes[i] <= now)
                {
                    continue;
                }

                metronomeSources[i]?.Stop();
                metronomeScheduledBeats[i] = -1;
                metronomeScheduledDspTimes[i] = 0.0;
            }

            nextMetronomeBeatToSchedule =
                Math.Min(
                    nextMetronomeBeatToSchedule,
                    firstReschedulableBeat);
            ScheduleMetronomeLookahead();
        }

        private void BeginPreparation()
        {
            EnsureTempoMapsInitialized();
            preparationBeatCount = queuedPreparationBeats;
            queuedPreparationBeats = 0;
            preparationBeatIndex = -1;
            preparationStartClockBeat = ClockBeats + 1;
            preparationStartDspTime =
                clockTempoMap.TimeAtBeat(preparationStartClockBeat);
            preparationEndDspTime =
                clockTempoMap.TimeAtBeat(
                    preparationStartClockBeat + preparationBeatCount);

            double preparationDuration =
                preparationEndDspTime - preparationStartDspTime;
            startTime += preparationDuration;
            gameplayStartDspTime += preparationDuration;

            SchedulePendingPreparationTempo();
            OnPreparationScheduled?.Invoke(
                preparationStartDspTime,
                preparationEndDspTime,
                preparationBeatCount);
            OnPreparationMeasureStart?.Invoke(TotalBeats / BeatsPerCycle);
        }

        private void EnsureTempoMapsInitialized()
        {
            if (tempoMapsInitialized) return;

            double initialBpm = ConfiguredBpm;
            int nextClockBeat = Math.Max(0, ClockBeats + 1);
            double anchor =
                AudioSettings.dspTime - nextClockBeat * (60.0 / initialBpm);
            ScheduledStartDspTime = anchor;
            gameplayStartDspTime = anchor;
            startTime = Time.realtimeSinceStartupAsDouble
                        + (anchor - AudioSettings.dspTime);
            clockTempoMap.Reset(anchor, initialBpm);
            gameplayTempoMap.Reset(0.0, initialBpm);
            tempoMapsInitialized = true;
        }

        private void SchedulePendingPreparationTempo()
        {
            if (!IsPreparing
                || !hasPendingTempoAfterPreparation
                || hasScheduledTempoAfterPreparation)
            {
                return;
            }

            scheduledTempoAfterPreparation = pendingTempoAfterPreparation;
            hasScheduledTempoAfterPreparation = true;

            clockTempoMap.ScheduleTempoChange(
                preparationEndDspTime,
                scheduledTempoAfterPreparation);
            gameplayTempoMap.ScheduleTempoChange(
                gameplayTempoMap.TimeAtBeat(TotalBeats),
                scheduledTempoAfterPreparation);

            int firstBeatWithChangedInterval =
                preparationStartClockBeat + preparationBeatCount + 1;
            RescheduleMetronomeFromBeat(firstBeatWithChangedInterval);
        }

        private void UpdatePreparation()
        {
            double now = AudioSettings.dspTime;
            while (preparationBeatIndex + 1 < preparationBeatCount)
            {
                int nextBeat = preparationBeatIndex + 1;
                double beatTime = clockTempoMap.TimeAtBeat(
                    preparationStartClockBeat + nextBeat);
                if (now < beatTime) break;

                preparationBeatIndex = nextBeat;
                DispatchClockBeat();
                OnPreparationBeat?.Invoke(preparationBeatIndex);
            }

            if (now < preparationEndDspTime) return;

            if (hasScheduledTempoAfterPreparation)
            {
                runtimeBpm = scheduledTempoAfterPreparation;
                runtimeTempoEnabled = true;
            }

            hasPendingTempoAfterPreparation = false;
            hasScheduledTempoAfterPreparation = false;
            preparationBeatCount = 0;
            preparationBeatIndex = -1;
            DispatchCurrentBeat();
        }
    }
}
