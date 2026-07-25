using System;
using UnityEngine;

namespace BeatMemories
{
    /// <summary>
    /// 비트 클록. BPM에 맞춰 박을 세고 제시 4박 뒤 응답 4박이 이어지는 8박 사이클을 관리한다.
    /// P0는 오디오 없이 <see cref="Time.realtimeSinceStartupAsDouble"/> 기반으로 돌린다.
    /// </summary>
    [DefaultExecutionOrder(-50)]
    public class Conductor : MonoBehaviour
    {
        public const int BeatsPerMeasure = 4;
        public const int ResponseStartBeat = BeatsPerMeasure; // 4
        public const int BeatsPerCycle = ResponseStartBeat + BeatsPerMeasure; // 8

        [Header("템포 (인스펙터 조정)")]
        [SerializeField, Min(1f)] private float bpm = 90f;
        [SerializeField] private bool playOnStart = true;

        [Tooltip("시작 전 카운트인(준비) 시간(초). 이 시간 동안은 박이 진행되지 않는다.")]
        [SerializeField, Min(0f)] private float startDelay = 3f;

        public float Bpm { get => bpm; set => bpm = Mathf.Max(1f, value); }
        public float SecondsPerBeat => 60f / bpm;
        public bool IsRunning { get; private set; }

        /// <summary>오디오 DSP 기준 첫 박(카운트인 종료) 예약 시각. PlayScheduled 동기용.</summary>
        public double ScheduledStartDspTime { get; private set; }

        /// <summary>시작(첫 박)까지 남은 시간(초). 카운트인 표시용. 시작 후 0.</summary>
        public double TimeUntilStart => IsRunning && TotalBeats < 0
            ? System.Math.Max(0.0, startTime - Time.realtimeSinceStartupAsDouble)
            : 0.0;

        /// <summary>카운트인 중인가(박이 아직 시작 안 함).</summary>
        public bool IsCountingDown
            => IsRunning && TotalBeats < 0 && Time.realtimeSinceStartupAsDouble < startTime;

        /// <summary>첫 박(beat 0) 기준 경과 시간(초). 카운트인 중엔 음수.</summary>
        public double SongPosition => Time.realtimeSinceStartupAsDouble - startTime;

        /// <summary>Input System의 실시간 타임스탬프를 곡 시작 기준 시간으로 변환한다.</summary>
        public double RealtimeToSongPosition(double realtimeTime) => realtimeTime - startTime;

        /// <summary>전역 박 인덱스의 이상적 발생 시각(SongPosition 기준, 초).</summary>
        public double BeatToTime(int globalBeat) => globalBeat * (double)SecondsPerBeat;

        /// <summary>시작 후 누적 박 수(첫 박 = 0). 시작 전 -1.</summary>
        public int TotalBeats { get; private set; } = -1;
        public int CycleIndex { get; private set; }
        public int BeatInCycle { get; private set; }
        public int BeatInMeasure => IsResponseMeasure ? BeatInCycle - ResponseStartBeat : BeatInCycle;
        public bool IsResponseMeasure => BeatInCycle >= ResponseStartBeat;

        private double startTime;
        private bool pendingBeatDispatch;
        private int queuedPreparationBeats;
        private int preparationBeatCount;
        private int preparationBeatIndex = -1;
        private double preparationStartRealtime;

        public bool IsPreparing => preparationBeatCount > 0;

        /// <summary>매 박 정각(전역 박 인덱스). 카운트인 이후 모든 박에서 발생. 애니·연출 클록용.</summary>
        public event Action<int> OnClockBeat;
        /// <summary>요청된 페이즈 준비 구간 시작. 인자: 다음 사이클 인덱스.</summary>
        public event Action<int> OnPreparationMeasureStart;
        public event Action<int> OnPreparationBeat;
        /// <summary>매 박 정각. 인자: 사이클 내 박(0..7).</summary>
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
            pendingBeatDispatch = false;
            queuedPreparationBeats = 0;
            preparationBeatCount = 0;
            preparationBeatIndex = -1;
            startTime = Time.realtimeSinceStartupAsDouble + startDelay; // 카운트인만큼 미룬다
            ScheduledStartDspTime = AudioSettings.dspTime + startDelay;  // 오디오 예약 재생 동기용
        }

        public void StopClock() => IsRunning = false;

        public void QueuePreparationBeats(int beats)
        {
            if (!IsRunning || beats <= 0) return;
            queuedPreparationBeats = System.Math.Max(queuedPreparationBeats, beats);
        }

        /// <summary>
        /// 현재 응답 슬롯이 입력/미스로 확정됐을 때 남은 박 시간을 생략하고 다음 박을 시작한다.
        /// 기존 Beat 이벤트 순서와 사이클 경계 처리는 <see cref="AdvanceBeat"/>를 그대로 사용한다.
        /// </summary>
        public bool AdvanceResponseBeatNow(int resolvedSlot)
        {
            if (!IsRunning || pendingBeatDispatch) return false;
            if (BeatInCycle != ResponseStartBeat + resolvedSlot) return false;

            TotalBeats++;
            startTime = Time.realtimeSinceStartupAsDouble - BeatToTime(TotalBeats);
            AdvanceBeat();
            return true;
        }

        /// <summary>Hit Stop처럼 실시간 정지가 발생한 만큼 비트 기준시각도 함께 뒤로 민다.</summary>
        public void DelayClock(double seconds)
        {
            if (IsRunning && seconds > 0.0) startTime += seconds;
        }

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

            if (IsPreparing)
            {
                UpdatePreparation();
                return;
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
            if (TotalBeats > 0 && TotalBeats % BeatsPerCycle == 0)
            {
                OnResponseMeasureEnd?.Invoke((TotalBeats / BeatsPerCycle) - 1);
                if (!IsRunning) return false;
                if (queuedPreparationBeats > 0)
                {
                    BeginPreparation();
                    return false;
                }
                pendingBeatDispatch = true;
                return false;
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

            OnClockBeat?.Invoke(TotalBeats);
            OnBeat?.Invoke(BeatInCycle);
            if (BeatInCycle == ResponseStartBeat)
                OnResponseMeasureStart?.Invoke(CycleIndex);
        }

        private void BeginPreparation()
        {
            preparationBeatCount = queuedPreparationBeats;
            queuedPreparationBeats = 0;
            preparationBeatIndex = -1;
            preparationStartRealtime = startTime + BeatToTime(TotalBeats);
            startTime += preparationBeatCount * (double)SecondsPerBeat;
            OnPreparationMeasureStart?.Invoke(TotalBeats / BeatsPerCycle);
        }

        private void UpdatePreparation()
        {
            double now = Time.realtimeSinceStartupAsDouble;
            while (preparationBeatIndex + 1 < preparationBeatCount)
            {
                int nextBeat = preparationBeatIndex + 1;
                double beatTime =
                    preparationStartRealtime + nextBeat * (double)SecondsPerBeat;
                if (now < beatTime) break;

                preparationBeatIndex = nextBeat;
                OnPreparationBeat?.Invoke(preparationBeatIndex);
            }

            double endTime =
                preparationStartRealtime + preparationBeatCount * (double)SecondsPerBeat;
            if (now < endTime) return;

            preparationBeatCount = 0;
            preparationBeatIndex = -1;
            DispatchCurrentBeat();
        }
    }
}
