using UnityEngine;

namespace BeatMemories
{
    [CreateAssetMenu(menuName = "Beat Memories/Dialogue Typing Settings")]
    public sealed class DialogueTypingSettings : ScriptableObject
    {
        public const string ResourceName = "DialogueTypingSettings";

        [SerializeField, Min(0f)] private float charactersPerSecond = 40f;
        [SerializeField, Range(0f, 1f)] private float bleepVolume = 0.35f;
        [SerializeField, Range(0.5f, 2f)] private float minPitch = 0.94f;
        [SerializeField, Range(0.5f, 2f)] private float maxPitch = 1.06f;
        [SerializeField, Min(1)] private int bleepEveryCharacters = 1;
        [SerializeField] private AudioClip[] bleepClips;

        public float CharactersPerSecond => Mathf.Max(0f, charactersPerSecond);
        public float BleepVolume => Mathf.Clamp01(bleepVolume);
        public float MinPitch => Mathf.Min(minPitch, maxPitch);
        public float MaxPitch => Mathf.Max(minPitch, maxPitch);
        public int BleepEveryCharacters => Mathf.Max(1, bleepEveryCharacters);
        public AudioClip[] BleepClips => bleepClips;

        public static DialogueTypingSettings Load() =>
            Resources.Load<DialogueTypingSettings>(ResourceName);
    }
}
