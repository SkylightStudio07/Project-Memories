using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace BeatMemories.Tests
{
    public class RhythmAudioControllerTests
    {
        [Test]
        public void LoopScheduleUsesAbsoluteDspAnchorWithoutAccumulation()
        {
            Type controllerType = RuntimeType("BeatMemories.RhythmAudioController");
            MethodInfo calculate = controllerType.GetMethod(
                "CalculateLoopDspTime",
                BindingFlags.Static | BindingFlags.NonPublic);

            Assert.That(calculate, Is.Not.Null);
            const double anchor = 1234.5;
            const long iteration = 10000;
            const int beats = 32;
            const float bpm = 94f;
            double actual = (double)calculate.Invoke(
                null,
                new object[] { anchor, iteration, beats, bpm });
            double expected = anchor + iteration * beats * 60.0 / bpm;

            Assert.That(actual, Is.EqualTo(expected).Within(0.000000001));
        }

        [Test]
        public void ZeroCountdownStillSchedulesAtLeastTwoHundredMillisecondsAhead()
        {
            Type conductorType = RuntimeType("BeatMemories.Conductor");
            MethodInfo calculate = conductorType.GetMethod(
                "CalculateScheduledStartDspTime",
                BindingFlags.Static | BindingFlags.NonPublic);

            Assert.That(calculate, Is.Not.Null);
            double zeroDelay = (double)calculate.Invoke(
                null,
                new object[] { 100.0, 0.0 });
            double authoredDelay = (double)calculate.Invoke(
                null,
                new object[] { 100.0, 3.0 });

            Assert.That(zeroDelay, Is.EqualTo(100.2).Within(0.000000001));
            Assert.That(authoredDelay, Is.EqualTo(103.0).Within(0.000000001));
        }

        [Test]
        public void StageFiveGateCollectsBothBossPagesForPreloading()
        {
            const string catalogPath =
                "Assets/Data/Stages/StageSoundtrackCatalog_Dayeon_BGM.asset";
            Type controllerType =
                RuntimeType("BeatMemories.RhythmAudioController");
            Type catalogType =
                RuntimeType("BeatMemories.StageSoundtrackCatalogSO");
            UnityEngine.Object catalog =
                AssetDatabase.LoadAssetAtPath(catalogPath, catalogType);
            Assert.That(catalog, Is.Not.Null);

            object entries = catalogType.GetProperty("Entries").GetValue(catalog);
            PropertyInfo item = entries.GetType().GetProperty("Item");
            object bossPageOne = item.GetValue(entries, new object[] { 4 });

            var gameObject = new GameObject("Boss preload test");
            try
            {
                Component controller = gameObject.AddComponent(controllerType);
                controllerType.GetField(
                        "catalog",
                        BindingFlags.Instance | BindingFlags.NonPublic)
                    .SetValue(controller, catalog);
                controllerType.GetField(
                        "selectedCue",
                        BindingFlags.Instance | BindingFlags.NonPublic)
                    .SetValue(controller, bossPageOne);

                MethodInfo collect = controllerType.GetMethod(
                    "CollectCurrentStageClips",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                object clips = collect.Invoke(controller, null);
                int count = (int)clips.GetType().GetProperty("Count")
                    .GetValue(clips);

                Assert.That(count, Is.EqualTo(2));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(gameObject);
            }
        }

        [Test]
        public void CatalogLookupRequiresExactStageAndEnemyPage()
        {
            Type catalogType = RuntimeType("BeatMemories.StageSoundtrackCatalogSO");
            Type stageType = RuntimeType("BeatMemories.StageSO");
            ScriptableObject catalog =
                ScriptableObject.CreateInstance(catalogType);
            ScriptableObject stage =
                ScriptableObject.CreateInstance(stageType);
            AudioClip clip = AudioClip.Create(
                "Catalog Test Clip",
                48000,
                2,
                48000,
                false);

            try
            {
                var serializedCatalog = new SerializedObject(catalog);
                SerializedProperty entries =
                    serializedCatalog.FindProperty("entries");
                entries.arraySize = 1;
                SerializedProperty entry =
                    entries.GetArrayElementAtIndex(0);
                entry.FindPropertyRelative("stage").objectReferenceValue = stage;
                entry.FindPropertyRelative("enemyPage").intValue = 2;
                entry.FindPropertyRelative("clip").objectReferenceValue = clip;
                entry.FindPropertyRelative("bpm").floatValue = 92f;
                entry.FindPropertyRelative("loopBeats").intValue = 228;
                entry.FindPropertyRelative("volume").floatValue = 0.35f;
                serializedCatalog.ApplyModifiedPropertiesWithoutUndo();

                MethodInfo lookup = catalogType.GetMethod("TryGetCue");
                object[] exactArguments = { stage, 2, null };
                object[] wrongPageArguments = { stage, 1, null };

                Assert.That(
                    lookup.Invoke(catalog, exactArguments),
                    Is.EqualTo(true));
                Assert.That(exactArguments[2], Is.Not.Null);
                Assert.That(
                    lookup.Invoke(catalog, wrongPageArguments),
                    Is.EqualTo(false));
                Assert.That(wrongPageArguments[2], Is.Null);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(clip);
                UnityEngine.Object.DestroyImmediate(stage);
                UnityEngine.Object.DestroyImmediate(catalog);
            }
        }

        [Test]
        public void RuntimeSourcesAreTwoDimensionalAndNeverUseUnityLooping()
        {
            Type controllerType = RuntimeType("BeatMemories.RhythmAudioController");
            var gameObject = new GameObject("Rhythm Audio Controller Test");
            try
            {
                gameObject.AddComponent(controllerType);
                MethodInfo ensureSources = controllerType.GetMethod(
                    "EnsureAudioSources",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                Component controller = gameObject.GetComponent(controllerType);
                ensureSources.Invoke(controller, null);

                AudioSource[] sources = gameObject.GetComponents<AudioSource>();
                Assert.That(sources.Length, Is.EqualTo(10));
                foreach (AudioSource source in sources)
                {
                    Assert.That(source.playOnAwake, Is.False);
                    Assert.That(source.loop, Is.False);
                    Assert.That(source.spatialBlend, Is.Zero);
                }
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(gameObject);
            }
        }

        private static Type RuntimeType(string fullName)
        {
            Assembly runtimeAssembly = AppDomain.CurrentDomain.GetAssemblies()
                .First(assembly => assembly.GetType(fullName) != null);
            return runtimeAssembly.GetType(fullName, true);
        }
    }
}
