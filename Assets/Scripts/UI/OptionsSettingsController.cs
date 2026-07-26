using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace BeatMemories
{
    [DefaultExecutionOrder(-100)]
    public sealed class OptionsSettingsController : MonoBehaviour
    {
        private const int InputOffsetStep = 10;
        private const int MinimumInputOffset = -200;
        private const int MaximumInputOffset = 200;
        private const string MusicVolumeParameter = "MusicVolumeDb";

        private static readonly FullScreenMode[] WindowModes =
        {
            FullScreenMode.ExclusiveFullScreen,
            FullScreenMode.FullScreenWindow,
            FullScreenMode.Windowed,
        };

        private static readonly string[] WindowModeLabels =
        {
            "Fullscreen",
            "Borderless",
            "Windowed",
        };

        [Header("Input Offset")]
        [SerializeField] private Button inputOffsetLeftButton;
        [SerializeField] private Button inputOffsetRightButton;
        [SerializeField] private TMP_Text inputOffsetValue;

        [Header("Resolution")]
        [SerializeField] private Button resolutionLeftButton;
        [SerializeField] private Button resolutionRightButton;
        [SerializeField] private TMP_Text resolutionValue;

        [Header("Window Mode")]
        [SerializeField] private Button windowModeLeftButton;
        [SerializeField] private Button windowModeRightButton;
        [SerializeField] private TMP_Text windowModeValue;

        [Header("VSync")]
        [SerializeField] private Button vSyncLeftButton;
        [SerializeField] private Button vSyncRightButton;
        [SerializeField] private TMP_Text vSyncValue;

        [Header("BGM Volume")]
        [SerializeField] private Slider bgmVolumeSlider;
        [SerializeField] private AudioMixer bgmMixer;

        [Header("SFX + Metronome Volume")]
        [SerializeField] private Slider sfxVolumeSlider;

        private readonly List<Vector2Int> resolutions = new List<Vector2Int>();
        private int inputOffsetMilliseconds;
        private int resolutionIndex;
        private int windowModeIndex;
        private int vSyncCount;
        private float bgmVolume;
        private float sfxVolume;

        private void Awake()
        {
            if (!HasRequiredReferences())
            {
                enabled = false;
                return;
            }

            BuildResolutionList();
            LoadSettings();
            RegisterListeners();
            ApplyAllSettings();
            RefreshLabels();
        }

        private void Start()
        {
            ApplyBgmVolume();
        }

        private void OnDestroy()
        {
            UnregisterListeners();
        }

        private bool HasRequiredReferences()
        {
            bool valid = inputOffsetLeftButton != null
                && inputOffsetRightButton != null
                && inputOffsetValue != null
                && resolutionLeftButton != null
                && resolutionRightButton != null
                && resolutionValue != null
                && windowModeLeftButton != null
                && windowModeRightButton != null
                && windowModeValue != null
                && vSyncLeftButton != null
                && vSyncRightButton != null
                && vSyncValue != null
                && bgmVolumeSlider != null
                && sfxVolumeSlider != null
                && bgmMixer != null;

            if (!valid)
            {
                Debug.LogError($"{nameof(OptionsSettingsController)} has missing settings references.", this);
            }

            return valid;
        }

        private void BuildResolutionList()
        {
            resolutions.Clear();

            Resolution[] available = Screen.resolutions;
            for (int i = 0; i < available.Length; i++)
            {
                Vector2Int size = new Vector2Int(available[i].width, available[i].height);
                if (size.x < 800 || size.y < 600 || resolutions.Contains(size))
                {
                    continue;
                }

                resolutions.Add(size);
            }

            if (resolutions.Count == 0)
            {
                resolutions.Add(new Vector2Int(1280, 720));
                resolutions.Add(new Vector2Int(1600, 900));
                resolutions.Add(new Vector2Int(1920, 1080));
            }

            Vector2Int current = new Vector2Int(Screen.width, Screen.height);
            if (!resolutions.Contains(current))
            {
                resolutions.Add(current);
            }

            resolutions.Sort((left, right) =>
            {
                int widthComparison = left.x.CompareTo(right.x);
                return widthComparison != 0 ? widthComparison : left.y.CompareTo(right.y);
            });
        }

        private void LoadSettings()
        {
            inputOffsetMilliseconds = Mathf.Clamp(
                PlayerPrefs.GetInt(
                    GameSettings.InputOffsetKey,
                    GameSettings.DefaultInputOffsetMilliseconds),
                MinimumInputOffset,
                MaximumInputOffset);

            int savedWidth = PlayerPrefs.GetInt(GameSettings.ResolutionWidthKey, Screen.width);
            int savedHeight = PlayerPrefs.GetInt(GameSettings.ResolutionHeightKey, Screen.height);
            resolutionIndex = FindClosestResolution(savedWidth, savedHeight);

            int defaultWindowMode = GetWindowModeIndex(Screen.fullScreenMode);
            windowModeIndex = Mathf.Clamp(
                PlayerPrefs.GetInt(GameSettings.WindowModeKey, defaultWindowMode),
                0,
                WindowModes.Length - 1);
            vSyncCount = Mathf.Clamp(
                PlayerPrefs.GetInt(GameSettings.VSyncKey, QualitySettings.vSyncCount > 0 ? 1 : 0),
                0,
                1);

            bgmVolume = GameSettings.BgmVolume;
            bgmVolumeSlider.SetValueWithoutNotify(bgmVolume);
            sfxVolume = GameSettings.SfxVolume;
            sfxVolumeSlider.SetValueWithoutNotify(sfxVolume);
        }

        private int FindClosestResolution(int width, int height)
        {
            int closestIndex = 0;
            long closestDistance = long.MaxValue;

            for (int i = 0; i < resolutions.Count; i++)
            {
                long widthDifference = resolutions[i].x - width;
                long heightDifference = resolutions[i].y - height;
                long distance = widthDifference * widthDifference + heightDifference * heightDifference;
                if (distance >= closestDistance)
                {
                    continue;
                }

                closestDistance = distance;
                closestIndex = i;
            }

            return closestIndex;
        }

        private static int GetWindowModeIndex(FullScreenMode mode)
        {
            for (int i = 0; i < WindowModes.Length; i++)
            {
                if (WindowModes[i] == mode)
                {
                    return i;
                }
            }

            return 1;
        }

        private void RegisterListeners()
        {
            inputOffsetLeftButton.onClick.AddListener(DecreaseInputOffset);
            inputOffsetRightButton.onClick.AddListener(IncreaseInputOffset);
            resolutionLeftButton.onClick.AddListener(PreviousResolution);
            resolutionRightButton.onClick.AddListener(NextResolution);
            windowModeLeftButton.onClick.AddListener(PreviousWindowMode);
            windowModeRightButton.onClick.AddListener(NextWindowMode);
            vSyncLeftButton.onClick.AddListener(ToggleVSync);
            vSyncRightButton.onClick.AddListener(ToggleVSync);
            bgmVolumeSlider.onValueChanged.AddListener(SetBgmVolume);
            sfxVolumeSlider.onValueChanged.AddListener(SetSfxVolume);
        }

        private void UnregisterListeners()
        {
            inputOffsetLeftButton?.onClick.RemoveListener(DecreaseInputOffset);
            inputOffsetRightButton?.onClick.RemoveListener(IncreaseInputOffset);
            resolutionLeftButton?.onClick.RemoveListener(PreviousResolution);
            resolutionRightButton?.onClick.RemoveListener(NextResolution);
            windowModeLeftButton?.onClick.RemoveListener(PreviousWindowMode);
            windowModeRightButton?.onClick.RemoveListener(NextWindowMode);
            vSyncLeftButton?.onClick.RemoveListener(ToggleVSync);
            vSyncRightButton?.onClick.RemoveListener(ToggleVSync);
            bgmVolumeSlider?.onValueChanged.RemoveListener(SetBgmVolume);
            sfxVolumeSlider?.onValueChanged.RemoveListener(SetSfxVolume);
        }

        private void DecreaseInputOffset()
        {
            inputOffsetMilliseconds = Mathf.Max(
                MinimumInputOffset,
                inputOffsetMilliseconds - InputOffsetStep);
            SaveInputOffset();
        }

        private void IncreaseInputOffset()
        {
            inputOffsetMilliseconds = Mathf.Min(
                MaximumInputOffset,
                inputOffsetMilliseconds + InputOffsetStep);
            SaveInputOffset();
        }

        private void SaveInputOffset()
        {
            PlayerPrefs.SetInt(GameSettings.InputOffsetKey, inputOffsetMilliseconds);
            PlayerPrefs.Save();
            RefreshLabels();
        }

        private void PreviousResolution()
        {
            resolutionIndex = WrapIndex(resolutionIndex - 1, resolutions.Count);
            ApplyDisplaySettings();
        }

        private void NextResolution()
        {
            resolutionIndex = WrapIndex(resolutionIndex + 1, resolutions.Count);
            ApplyDisplaySettings();
        }

        private void PreviousWindowMode()
        {
            windowModeIndex = WrapIndex(windowModeIndex - 1, WindowModes.Length);
            ApplyDisplaySettings();
        }

        private void NextWindowMode()
        {
            windowModeIndex = WrapIndex(windowModeIndex + 1, WindowModes.Length);
            ApplyDisplaySettings();
        }

        private void ToggleVSync()
        {
            vSyncCount = vSyncCount == 0 ? 1 : 0;
            QualitySettings.vSyncCount = vSyncCount;
            PlayerPrefs.SetInt(GameSettings.VSyncKey, vSyncCount);
            PlayerPrefs.Save();
            RefreshLabels();
        }

        private void SetBgmVolume(float value)
        {
            bgmVolume = Mathf.Clamp01(value);
            bgmVolumeSlider.SetValueWithoutNotify(bgmVolume);
            ApplyBgmVolume();

            PlayerPrefs.SetFloat(GameSettings.BgmVolumeKey, bgmVolume);
            PlayerPrefs.Save();
        }

        private void ApplyBgmVolume()
        {
            bgmMixer.SetFloat(
                MusicVolumeParameter,
                GameSettings.BgmVolumeToDecibels(bgmVolume));
        }

        private void SetSfxVolume(float value)
        {
            sfxVolume = Mathf.Clamp01(value);
            sfxVolumeSlider.SetValueWithoutNotify(sfxVolume);
            PlayerPrefs.SetFloat(GameSettings.SfxVolumeKey, sfxVolume);
            PlayerPrefs.Save();
            ApplySfxVolume();
        }

        private void ApplySfxVolume()
        {
            GameSettings.ApplySfxVolume(bgmMixer);
        }

        private void ApplyAllSettings()
        {
            QualitySettings.vSyncCount = vSyncCount;
            ApplyBgmVolume();
            ApplySfxVolume();
            ApplyResolutionAndWindowMode();
        }

        private void ApplyDisplaySettings()
        {
            ApplyResolutionAndWindowMode();
            SaveDisplaySettings();
            RefreshLabels();
        }

        private void ApplyResolutionAndWindowMode()
        {
            Vector2Int resolution = resolutions[resolutionIndex];
            Screen.SetResolution(resolution.x, resolution.y, WindowModes[windowModeIndex]);
        }

        private void SaveDisplaySettings()
        {
            Vector2Int resolution = resolutions[resolutionIndex];
            PlayerPrefs.SetInt(GameSettings.ResolutionWidthKey, resolution.x);
            PlayerPrefs.SetInt(GameSettings.ResolutionHeightKey, resolution.y);
            PlayerPrefs.SetInt(GameSettings.WindowModeKey, windowModeIndex);
            PlayerPrefs.Save();
        }

        private void RefreshLabels()
        {
            inputOffsetValue.text = inputOffsetMilliseconds >= 0
                ? $"+{inputOffsetMilliseconds}"
                : inputOffsetMilliseconds.ToString();

            Vector2Int resolution = resolutions[resolutionIndex];
            resolutionValue.text = $"{resolution.x} x {resolution.y}";
            windowModeValue.text = WindowModeLabels[windowModeIndex];
            vSyncValue.text = vSyncCount == 0 ? "Off" : "On";
        }

        private static int WrapIndex(int index, int count)
        {
            return (index % count + count) % count;
        }
    }
}
