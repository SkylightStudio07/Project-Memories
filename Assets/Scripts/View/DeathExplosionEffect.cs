using UnityEngine;
using UnityEngine.Audio;

namespace BeatMemories
{
    /// <summary>
    /// Presentation settings owned by the reusable death explosion prefab.
    /// HudView reads the repeat interval while each spawned instance plays
    /// one pitch-varied explosion sound.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class DeathExplosionEffect : MonoBehaviour
    {
        [Header("Explosion Audio")]
        [SerializeField] private AudioClip explosionClip;
        [SerializeField] private AudioMixerGroup output;
        [SerializeField, Range(0f, 1f)] private float volume = 0.75f;
        [SerializeField, Range(0.5f, 1.5f)] private float minPitch = 0.88f;
        [SerializeField, Range(0.5f, 1.5f)] private float maxPitch = 1.12f;

        [Header("Death Flight Repeat")]
        [Tooltip("Time between explosion instances while the actor is flying.")]
        [SerializeField, Min(0.02f)] private float repeatInterval = 0.11f;

        public float RepeatInterval => Mathf.Max(0.02f, repeatInterval);

        private void Awake()
        {
            if (explosionClip == null) return;

            AudioSource source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = false;
            source.spatialBlend = 0f;
            source.outputAudioMixerGroup = output;
            source.volume = Mathf.Clamp01(volume);
            source.pitch = Random.Range(
                Mathf.Min(minPitch, maxPitch),
                Mathf.Max(minPitch, maxPitch));
            source.PlayOneShot(explosionClip);
        }
    }
}
