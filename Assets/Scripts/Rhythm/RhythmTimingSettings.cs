using UnityEngine;

namespace BeatMemories
{
    /// <summary>모든 스테이지가 공유하는 BPM 및 정박 판정 허용 범위.</summary>
    [CreateAssetMenu(
        fileName = ResourceName,
        menuName = "Beat Memories/Rhythm Timing Settings",
        order = 2)]
    public sealed class RhythmTimingSettings : ScriptableObject
    {
        public const string ResourceName = "RhythmTimingSettings";

        [Header("전역 박자")]
        [SerializeField, Min(1f)] private float bpm = 90f;

        [Header("정박 판정 Offset (초)")]
        [Tooltip("정박보다 이 시간 이내로 빠른 입력까지 성공으로 인정한다.")]
        [SerializeField, Min(0f)] private float earlyOffset = 0.12f;
        [Tooltip("정박보다 이 시간 이내로 느린 입력까지 성공으로 인정한다.")]
        [SerializeField, Min(0f)] private float lateOffset = 0.12f;

        [Header("메트로놈")]
        [SerializeField] private AudioClip tick;
        [SerializeField] private AudioClip tack;
        [SerializeField, Range(0f, 1f)] private float metronomeVolume = 0.7f;

        public float Bpm => Mathf.Max(1f, bpm);
        public float EarlyOffset => Mathf.Max(0f, earlyOffset);
        public float LateOffset => Mathf.Max(0f, lateOffset);
        public AudioClip Tick => tick;
        public AudioClip Tack => tack;
        public float MetronomeVolume => Mathf.Clamp01(metronomeVolume);
    }
}
