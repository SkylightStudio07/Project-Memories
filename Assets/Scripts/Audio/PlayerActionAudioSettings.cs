using UnityEngine;
using UnityEngine.Audio;

namespace BeatMemories
{
    [CreateAssetMenu(menuName = "Beat Memories/Player Action Audio Settings")]
    public sealed class PlayerActionAudioSettings : ScriptableObject
    {
        public const string ResourceName = "PlayerActionAudioSettings";

        [SerializeField, Range(0f, 1f)] private float volume = 1f;
        [SerializeField] private AudioMixerGroup output;
        [SerializeField] private AudioClip[] attackVoices;
        [SerializeField] private AudioClip[] chargedAttackVoices;
        [SerializeField] private AudioClip[] damageVoices;
        [SerializeField] private AudioClip[] mistakeVoices;
        [Header("Shared Character Action Effects")]
        [SerializeField] private AudioClip beamEffect;
        [SerializeField, Range(0f, 1f)] private float beamVolume = 1f;
        [SerializeField] private AudioClip guardEffect;
        [SerializeField] private AudioClip parryEffect;
        [SerializeField] private AudioClip chargeEffect;

        public float Volume => Mathf.Clamp01(volume);
        public AudioMixerGroup Output => output;
        public AudioClip[] AttackVoices => attackVoices;
        public AudioClip[] ChargedAttackVoices => chargedAttackVoices;
        public AudioClip[] DamageVoices => damageVoices;
        public AudioClip[] MistakeVoices => mistakeVoices;
        public AudioClip BeamEffect => beamEffect;
        public float BeamVolume => Mathf.Clamp01(beamVolume);
        public AudioClip GuardEffect => guardEffect;
        public AudioClip ParryEffect => parryEffect;
        public AudioClip ChargeEffect => chargeEffect;

        public void ApplySavedVolume()
        {
            if (output != null)
                GameSettings.ApplySfxVolume(output.audioMixer);
        }

        public static PlayerActionAudioSettings Load()
        {
            PlayerActionAudioSettings settings =
                Resources.Load<PlayerActionAudioSettings>(ResourceName);
            settings?.ApplySavedVolume();
            return settings;
        }
    }
}
