using UnityEngine;

namespace BeatMemories
{
    public static class GameSettings
    {
        public const string InputOffsetKey = "settings.inputOffsetMs";
        public const string ResolutionWidthKey = "settings.resolutionWidth";
        public const string ResolutionHeightKey = "settings.resolutionHeight";
        public const string WindowModeKey = "settings.windowMode";
        public const string VSyncKey = "settings.vSync";

        public const int DefaultInputOffsetMilliseconds = 0;

        public static int InputOffsetMilliseconds
            => PlayerPrefs.GetInt(InputOffsetKey, DefaultInputOffsetMilliseconds);

        public static double InputOffsetSeconds => InputOffsetMilliseconds / 1000.0;
    }
}
