using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace BeatMemories.Tests
{
    public class TitleAudioSettingsTests
    {
        private const string TitleScenePath = "Assets/Scenes/Title.unity";
        private const string DayeonScenePath =
            "Assets/Scenes/BeatMemories_Dayeon.unity";
        private const string MusicPath =
            "Assets/Audio/Soul Funk Blues by Audio Library Beats (No Copyright Background Music) Dreamscape.mp3";
        private const string MixerPath =
            "Assets/Audio/Dayeon_BGM/Dayeon_BGM_Mixer.mixer";
        private const string BgmVolumeKey = "settings.bgmVolume";

        [Test]
        public void GameSettingsMapsAndRestoresLinearBgmVolume()
        {
            Type settingsType = RuntimeType("BeatMemories.GameSettings");
            PropertyInfo volumeProperty =
                settingsType.GetProperty("BgmVolume");
            MethodInfo toDecibels =
                settingsType.GetMethod("BgmVolumeToDecibels");
            FieldInfo defaultVolume =
                settingsType.GetField("DefaultBgmVolume");
            FieldInfo minimumDecibels =
                settingsType.GetField("MinimumBgmVolumeDecibels");

            Assert.That(volumeProperty, Is.Not.Null);
            Assert.That(toDecibels, Is.Not.Null);
            Assert.That(defaultVolume, Is.Not.Null);
            Assert.That(minimumDecibels, Is.Not.Null);

            bool hadSavedValue = PlayerPrefs.HasKey(BgmVolumeKey);
            float savedValue = PlayerPrefs.GetFloat(BgmVolumeKey);
            try
            {
                PlayerPrefs.DeleteKey(BgmVolumeKey);
                Assert.That(
                    (float)defaultVolume.GetRawConstantValue(),
                    Is.EqualTo(1f));
                Assert.That(
                    (float)volumeProperty.GetValue(null),
                    Is.EqualTo(1f));

                PlayerPrefs.SetFloat(BgmVolumeKey, 0.5f);
                PlayerPrefs.Save();
                Assert.That(
                    (float)volumeProperty.GetValue(null),
                    Is.EqualTo(0.5f).Within(0.0001f));

                Assert.That(
                    Convert.ToSingle(toDecibels.Invoke(null, new object[] { 1f })),
                    Is.EqualTo(0f).Within(0.0001f));
                Assert.That(
                    Convert.ToSingle(toDecibels.Invoke(null, new object[] { 0.5f })),
                    Is.EqualTo(-6.0206f).Within(0.001f));
                Assert.That(
                    Convert.ToSingle(toDecibels.Invoke(null, new object[] { 0f })),
                    Is.EqualTo(-80f).Within(0.0001f));
                Assert.That(
                    (float)minimumDecibels.GetRawConstantValue(),
                    Is.EqualTo(-80f));
            }
            finally
            {
                if (hadSavedValue)
                    PlayerPrefs.SetFloat(BgmVolumeKey, savedValue);
                else
                    PlayerPrefs.DeleteKey(BgmVolumeKey);

                PlayerPrefs.Save();
            }
        }

        [Test]
        public void TitleSceneUsesStreamingLoopOnGlobalMusicBus()
        {
            Type titleType = RuntimeType("Title");
            Type optionsType =
                RuntimeType("BeatMemories.OptionsSettingsController");
            AudioClip expectedClip =
                AssetDatabase.LoadAssetAtPath<AudioClip>(MusicPath);
            AudioMixer expectedMixer =
                AssetDatabase.LoadAssetAtPath<AudioMixer>(MixerPath);
            Assert.That(expectedClip, Is.Not.Null);
            Assert.That(expectedMixer, Is.Not.Null);

            AudioMixerGroup expectedMusicGroup =
                expectedMixer.FindMatchingGroups("Music").Single(
                    group => group.name == "Music");

            Scene scene = SceneManager.GetSceneByPath(TitleScenePath);
            bool closeSceneAfterTest = !scene.IsValid() || !scene.isLoaded;
            if (closeSceneAfterTest)
            {
                scene = EditorSceneManager.OpenScene(
                    TitleScenePath,
                    OpenSceneMode.Additive);
            }
            try
            {
                Component title = FindInScene(scene, titleType);
                Component options = FindInScene(scene, optionsType);
                Assert.That(title, Is.Not.Null);
                Assert.That(options, Is.Not.Null);

                var titleObject = new SerializedObject(title);
                AudioSource source = titleObject
                    .FindProperty("titleMusicSource")
                    .objectReferenceValue as AudioSource;
                Assert.That(source, Is.Not.Null);
                Assert.That(source.gameObject, Is.SameAs(title.gameObject));
                Assert.That(source.clip, Is.SameAs(expectedClip));
                Assert.That(source.playOnAwake, Is.False);
                Assert.That(source.loop, Is.True);
                Assert.That(source.spatialBlend, Is.Zero);
                Assert.That(source.volume, Is.EqualTo(1f));
                Assert.That(
                    source.outputAudioMixerGroup,
                    Is.SameAs(expectedMusicGroup));
                Assert.That(
                    titleObject.FindProperty("gameStartMusicFadeDuration")
                        .floatValue,
                    Is.EqualTo(0.3f).Within(0.0001f));

                var optionsObject = new SerializedObject(options);
                Slider slider = optionsObject.FindProperty("bgmVolumeSlider")
                    .objectReferenceValue as Slider;
                Assert.That(slider, Is.Not.Null);
                Assert.That(slider.value, Is.EqualTo(1f));
                Assert.That(
                    optionsObject.FindProperty("bgmMixer")
                        .objectReferenceValue,
                    Is.SameAs(expectedMixer));

                Assert.That(
                    expectedMixer.GetFloat("MusicVolumeDb", out float volume),
                    Is.True);
                Assert.That(volume, Is.EqualTo(0f).Within(0.0001f));

                AudioImporter importer =
                    AssetImporter.GetAtPath(MusicPath) as AudioImporter;
                Assert.That(importer, Is.Not.Null);
                Assert.That(
                    importer.defaultSampleSettings.loadType,
                    Is.EqualTo(AudioClipLoadType.Streaming));

                Assert.That(
                    scene.GetRootGameObjects()
                        .SelectMany(root =>
                            root.GetComponentsInChildren<Component>(true)),
                    Has.None.Null);
            }
            finally
            {
                if (closeSceneAfterTest)
                    EditorSceneManager.CloseScene(scene, true);
            }
        }

        [Test]
        public void DayeonDspBgmSharesMusicBusWithoutChangingOtherGroups()
        {
            Type audioType =
                RuntimeType("BeatMemories.RhythmAudioController");
            AudioMixer mixer =
                AssetDatabase.LoadAssetAtPath<AudioMixer>(MixerPath);
            AudioMixerGroup musicGroup = mixer.FindMatchingGroups("Music")
                .Single(group => group.name == "Music");

            Scene scene = SceneManager.GetSceneByPath(DayeonScenePath);
            bool closeSceneAfterTest = !scene.IsValid() || !scene.isLoaded;
            if (closeSceneAfterTest)
            {
                scene = EditorSceneManager.OpenScene(
                    DayeonScenePath,
                    OpenSceneMode.Additive);
            }
            try
            {
                Component audio = FindInScene(scene, audioType);
                Assert.That(audio, Is.Not.Null);
                var serializedAudio = new SerializedObject(audio);
                Assert.That(
                    serializedAudio.FindProperty("musicOutput")
                        .objectReferenceValue,
                    Is.SameAs(musicGroup));
                Assert.That(
                    mixer.FindMatchingGroups("Metronome")
                        .Count(group => group.name == "Metronome"),
                    Is.EqualTo(1));
                Assert.That(
                    mixer.FindMatchingGroups("SFX")
                        .Count(group => group.name == "SFX"),
                    Is.EqualTo(1));
            }
            finally
            {
                if (closeSceneAfterTest)
                    EditorSceneManager.CloseScene(scene, true);
            }
        }

        private static Component FindInScene(Scene scene, Type type)
        {
            return scene.GetRootGameObjects()
                .SelectMany(root =>
                    root.GetComponentsInChildren(type, true))
                .Cast<Component>()
                .FirstOrDefault();
        }

        private static Type RuntimeType(string fullName)
        {
            Assembly runtimeAssembly = Array.Find(
                AppDomain.CurrentDomain.GetAssemblies(),
                assembly => assembly.GetType(fullName) != null);
            Assert.That(runtimeAssembly, Is.Not.Null, fullName);
            return runtimeAssembly.GetType(fullName, true);
        }
    }
}
