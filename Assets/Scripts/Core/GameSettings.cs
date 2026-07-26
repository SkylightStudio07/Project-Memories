using UnityEngine;
using UnityEngine.Audio;

namespace BeatMemories
{
    public static class GameSettings
    {
        public const string InputOffsetKey = "settings.inputOffsetMs";
        public const string ResolutionWidthKey = "settings.resolutionWidth";
        public const string ResolutionHeightKey = "settings.resolutionHeight";
        public const string WindowModeKey = "settings.windowMode";
        public const string VSyncKey = "settings.vSync";
        public const string BgmVolumeKey = "settings.bgmVolume";
        public const string SfxVolumeKey = "settings.sfxVolume";
        public const string SfxVolumeParameter = "SfxVolumeDb";

        public const int DefaultInputOffsetMilliseconds = 0;
        public const float DefaultBgmVolume = 1f;
        public const float DefaultSfxVolume = 1f;
        public const float MinimumBgmVolumeDecibels = -80f;

        public static int InputOffsetMilliseconds
            => PlayerPrefs.GetInt(InputOffsetKey, DefaultInputOffsetMilliseconds);

        public static double InputOffsetSeconds => InputOffsetMilliseconds / 1000.0;

        public static float BgmVolume
            => Mathf.Clamp01(PlayerPrefs.GetFloat(BgmVolumeKey, DefaultBgmVolume));

        public static float SfxVolume
            => Mathf.Clamp01(PlayerPrefs.GetFloat(SfxVolumeKey, DefaultSfxVolume));

        public static float BgmVolumeToDecibels(float linearVolume)
        {
            float clampedVolume = Mathf.Clamp01(linearVolume);
            return clampedVolume <= 0f
                ? MinimumBgmVolumeDecibels
                : Mathf.Max(
                    MinimumBgmVolumeDecibels,
                    20f * Mathf.Log10(clampedVolume));
        }

        public static void ApplySfxVolume(AudioMixer mixer)
        {
            if (mixer == null) return;
            mixer.SetFloat(
                SfxVolumeParameter,
                BgmVolumeToDecibels(SfxVolume));
        }
    }
}
