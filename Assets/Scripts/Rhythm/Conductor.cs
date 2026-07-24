using System;
using UnityEngine;

namespace BeatMemories
{
    /// <summary>
    /// 비트 클록. BPM에 맞춰 박을 세고 8박 사이클(제시 4박 + 응답 4박)을 관리한다.
    /// P0는 오디오 없이 <see cref="Time.timeAsDouble"/> 기반으로 돌린다.
    /// </summary>
    public class Conductor : MonoBehaviour
    {
        public const int BeatsPerMeasure = 4;
        public const int MeasuresPerCycle = 2;
        public const int BeatsPerCycle = BeatsPerMeasure * MeasuresPerCycle; // 8

        [Header("템포 (인스펙터 조정)")]
        [SerializeField, Min(1f)] private float bpm = 90f;
        [SerializeField] private bool playOnStart = true;

        [Tooltip("시작 전 카운트인(준비) 시간(초). 이 시간 동안은 박이 진행되지 않는다.")]
        [SerializeField, Min(0f)] private float startDelay = 3f;

        public float Bpm { get => bpm; set => bpm = Mathf.Max(1f, value); }
        public float SecondsPerBeat => 60f / bpm;
        public bool IsRunning { get; private set; }

        /// <summary>시작(첫 박)까지 남은 시간(초). 카운트인 표시용. 시작 후 0.</summary>
        public double TimeUntilStart => IsRunning ? System.Math.Max(0.0, startTime - Time.timeAsDouble) : startDelay;

        /// <summary>카운트인 중인가(박이 아직 시작 안 함).</summary>
        public bool IsCountingDown => IsRunning && Time.timeAsDouble < startTime;

        /// <summary>첫 박(beat 0) 기준 경과 시간(초). 카운트인 중엔 음수.</summary>
        public double SongPosition => Time.timeAsDouble - startTime;

        /// <summary>전역 박 인덱스의 이상적 발생 시각(SongPosition 기준, 초).</summary>
        public double BeatToTime(int globalBeat) => globalBeat * (double)SecondsPerBeat;

        /// <summary>시작 후 누적 박 수(첫 박 = 0). 시작 전 -1.</summary>
        public int TotalBeats { get; private set; } = -1;
        public int CycleIndex { get; private set; }
        public int BeatInCycle { get; private set; }
        public int BeatInMeasure => BeatInCycle % BeatsPerMeasure;
        public bool IsResponseMeasure => BeatInCycle >= BeatsPerMeasure;

        private double startTime;

        /// <summary>매 박 정각. 인자: 사이클 내 박(0..7).</summary>
        public event Action<int> OnBeat;
        /// <summary>제시 마디 시작(BeatInCycle==0). 인자: cycleIndex.</summary>
        public event Action<int> OnPresentMeasureStart;
        /// <summary>응답 마디 시작(BeatInCycle==4). 인자: cycleIndex.</summary>
        public event Action<int> OnResponseMeasureStart;

        private void Start()
        {
            if (playOnStart) StartClock();
        }

        public void StartClock()
        {
            IsRunning = true;
            TotalBeats = -1;
            startTime = Time.timeAsDouble + startDelay; // 카운트인만큼 미룬다
        }

        public void StopClock() => IsRunning = false;

        private void Update()
        {
            if (!IsRunning) return;
            double elapsed = Time.timeAsDouble - startTime;
            if (elapsed < 0.0) return; // 카운트인 중 — 아직 박 시작 전
            int beatsNow = (int)(elapsed / SecondsPerBeat);
            while (beatsNow > TotalBeats)
            {
                TotalBeats++;
                AdvanceBeat();
            }
        }

        private void AdvanceBeat()
        {
            CycleIndex = TotalBeats / BeatsPerCycle;
            BeatInCycle = TotalBeats % BeatsPerCycle;

            if (BeatInCycle == 0) OnPresentMeasureStart?.Invoke(CycleIndex);
            else if (BeatInCycle == BeatsPerMeasure) OnResponseMeasureStart?.Invoke(CycleIndex);

            OnBeat?.Invoke(BeatInCycle);
        }
    }
}
