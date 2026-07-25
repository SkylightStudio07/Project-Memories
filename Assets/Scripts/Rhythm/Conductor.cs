using System;
using UnityEngine;

namespace BeatMemories
{
    /// <summary>
    /// 비트 클록. 준비 4박 뒤 제시 4박→응답 4박 교환 두 번이 이어지는
    /// 페이즈 타임라인을 관리한다.
    /// P0는 오디오 없이 <see cref="Time.realtimeSinceStartupAsDouble"/> 기반으로 돌린다.
    /// </summary>
    [DefaultExecutionOrder(-50)]
    public class Conductor : MonoBehaviour
    {
        public const int BeatsPerMeasure = CombatTimeline.BeatsPerMeasure;
        public const int ResponseStartBeat = BeatsPerMeasure; // 4
        public const int BeatsPerCycle = CombatTimeline.BeatsPerExchange; // 8
        public const int PreparationBeats = CombatTimeline.DefaultPreparationBeats; // 4

        [Header("템포 (인스펙터 조정)")]
        [SerializeField, Min(1f)] private float bpm = 90f;
        [SerializeField] private bool playOnStart = true;

        [Tooltip("시작 전 카운트인(준비) 시간(초). 이 시간 동안은 박이 진행되지 않는다.")]
        [SerializeField, Min(0f)] private float startDelay = 3f;

        public float Bpm { get => bpm; set => bpm = Mathf.Max(1f, value); }
        public float StartDelay { get => startDelay; set => startDelay = Mathf.Max(0f, value); }
        public float SecondsPerBeat => 60f / bpm;
        public bool IsRunning { get; private set; }
        public int ExchangesPerPhase { get; private set; } = CombatTimeline.DefaultExchangesPerPhase;
        public int BeatsPerPhase => CombatTimeline.BeatsPerPhase(ExchangesPerPhase, PreparationBeats);

        /// <summary>시작(첫 박)까지 남은 시간(초). 카운트인 표시용. 시작 후 0.</summary>
        public double TimeUntilStart => IsRunning ? System.Math.Max(0.0, startTime - Time.realtimeSinceStartupAsDouble) : startDelay;

        /// <summary>카운트인 중인가(박이 아직 시작 안 함).</summary>
        public bool IsCountingDown => IsRunning && Time.realtimeSinceStartupAsDouble < startTime;

        /// <summary>첫 박(beat 0) 기준 경과 시간(초). 카운트인 중엔 음수.</summary>
        public double SongPosition => Time.realtimeSinceStartupAsDouble - startTime;

        /// <summary>Input System의 실시간 타임스탬프를 곡 시작 기준 시간으로 변환한다.</summary>
        public double RealtimeToSongPosition(double realtimeTime) => realtimeTime - startTime;

        /// <summary>전역 박 인덱스의 이상적 발생 시각(SongPosition 기준, 초).</summary>
        public double BeatToTime(int globalBeat) => globalBeat * (double)SecondsPerBeat;

        /// <summary>시작 후 누적 박 수(첫 박 = 0). 시작 전 -1.</summary>
        public int TotalBeats { get; private set; } = -1;
        public int PhaseIndex { get; private set; } = -1;
        public int CycleIndex { get; private set; } = -1;
        public int ExchangeInPhase { get; private set; } = -1;
        public int BeatInCycle { get; private set; } = -1;
        public int BeatInMeasure { get; private set; } = -1;
        public CombatSection Section { get; private set; } = CombatSection.Preparation;
        public bool IsPreparing => Section == CombatSection.Preparation;
        public bool IsResponseMeasure => Section == CombatSection.Response;

        private double startTime;
        private bool pendingBeatDispatch;

        /// <summary>준비를 포함한 모든 박 정각. 인자: 절대 박 인덱스.</summary>
        public event Action<int> OnClockBeat;
        /// <summary>준비 마디 시작. 인자: phaseIndex.</summary>
        public event Action<int> OnPreparationMeasureStart;
        /// <summary>준비 마디의 매 박. 인자: 준비 내 박(0..3).</summary>
        public event Action<int> OnPreparationBeat;
        /// <summary>활성 교환의 매 박 정각. 인자: 사이클 내 박(0..7).</summary>
        public event Action<int> OnBeat;
        /// <summary>제시 마디 시작(BeatInCycle==0). 인자: cycleIndex.</summary>
        public event Action<int> OnPresentMeasureStart;
        /// <summary>응답 시작(BeatInCycle==4, 네 번의 제시 직후). 인자: cycleIndex.</summary>
        public event Action<int> OnResponseMeasureStart;
        /// <summary>응답 종료. 다음 사이클의 첫 제시보다 먼저 발생한다. 인자: 종료하는 cycleIndex.</summary>
        internal event Action<int> OnResponseMeasureEnd;

        private void Start()
        {
            if (playOnStart) StartClock();
        }

        public void StartClock()
        {
            IsRunning = true;
            TotalBeats = -1;
            PhaseIndex = -1;
            CycleIndex = -1;
            ExchangeInPhase = -1;
            BeatInCycle = -1;
            BeatInMeasure = -1;
            pendingBeatDispatch = false;
            startTime = Time.realtimeSinceStartupAsDouble + startDelay; // 카운트인만큼 미룬다
        }

        public void StopClock() => IsRunning = false;

        public void ConfigureTimeline(int exchangesPerPhase)
            => ExchangesPerPhase = Mathf.Max(1, exchangesPerPhase);

        private void Update()
        {
            if (!IsRunning) return;

            // 응답 종료 프레임의 입력·미입력 판정이 LateUpdate에서 끝난 다음 프레임에
            // 새 사이클 첫 제시를 보낸다. 비트 수를 늘리지 않고 화면상 순서만 보장한다.
            if (pendingBeatDispatch)
            {
                pendingBeatDispatch = false;
                DispatchCurrentBeat();
                if (!IsRunning) return;
            }

            double elapsed = Time.realtimeSinceStartupAsDouble - startTime;
            if (elapsed < 0.0) return; // 카운트인 중 — 아직 박 시작 전
            int beatsNow = (int)(elapsed / SecondsPerBeat);
            while (beatsNow > TotalBeats)
            {
                TotalBeats++;
                if (!AdvanceBeat()) break;
            }
        }

        private bool AdvanceBeat()
        {
            if (CombatTimeline.StartsAfterResponse(TotalBeats, ExchangesPerPhase, PreparationBeats))
            {
                CombatTimelinePosition previous =
                    CombatTimeline.Resolve(TotalBeats - 1, ExchangesPerPhase, PreparationBeats);
                OnResponseMeasureEnd?.Invoke(previous.ExchangeIndex);
                if (!IsRunning) return false;
                pendingBeatDispatch = true;
                return false;
            }

            DispatchCurrentBeat();
            return IsRunning;
        }

        private void DispatchCurrentBeat()
        {
            CombatTimelinePosition position =
                CombatTimeline.Resolve(TotalBeats, ExchangesPerPhase, PreparationBeats);
            PhaseIndex = position.PhaseIndex;
            CycleIndex = position.ExchangeIndex;
            ExchangeInPhase = position.ExchangeInPhase;
            BeatInCycle = position.BeatInCycle;
            BeatInMeasure = position.BeatInMeasure;
            Section = position.Section;

            if (position.IsPreparation)
            {
                if (BeatInMeasure == 0)
                {
                    OnPreparationMeasureStart?.Invoke(PhaseIndex);
                    if (!IsRunning) return;
                }
                OnClockBeat?.Invoke(TotalBeats);
                OnPreparationBeat?.Invoke(BeatInMeasure);
                return;
            }

            if (position.IsPreview && BeatInMeasure == 0)
            {
                OnPresentMeasureStart?.Invoke(CycleIndex);
                if (!IsRunning) return;
            }

            OnClockBeat?.Invoke(TotalBeats);
            OnBeat?.Invoke(BeatInCycle);
            if (position.IsResponse && BeatInMeasure == 0)
                OnResponseMeasureStart?.Invoke(CycleIndex);
        }
    }
}
