using System;
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace BeatMemories.Tests
{
    public class StageManagerAudioStartupTests
    {
        [UnityTest]
        public IEnumerator AudioReadyStartupSchedulesTheClockExactlyOnce()
        {
            Type conductorType = RuntimeType("BeatMemories.Conductor");
            Type roundType = RuntimeType("BeatMemories.RoundManager");
            Type audioType = RuntimeType(
                "BeatMemories.RhythmAudioController");
            Type stageManagerType = RuntimeType(
                "BeatMemories.StageManager");
            Type stageType = RuntimeType("BeatMemories.StageSO");
            Type rosterType = RuntimeType("BeatMemories.StageRosterSO");
            Type catalogType = RuntimeType(
                "BeatMemories.StageSoundtrackCatalogSO");
            Type entryType = catalogType.GetNestedType("Entry");

            var root = new GameObject("StageManagerAudioStartupTest");
            root.SetActive(false);
            root.AddComponent<AudioListener>();
            Component conductor = root.AddComponent(conductorType);
            Component round = root.AddComponent(roundType);
            Component rhythmAudio = root.AddComponent(audioType);
            Component stageManager = root.AddComponent(stageManagerType);

            ScriptableObject stage =
                ScriptableObject.CreateInstance(stageType);
            ScriptableObject roster =
                ScriptableObject.CreateInstance(rosterType);
            ScriptableObject catalog =
                ScriptableObject.CreateInstance(catalogType);
            AudioClip clip = AudioClip.Create(
                "StageManagerAudioStartupTestClip",
                1440000,
                2,
                48000,
                false);

            try
            {
                SetField(stage, "stageNumber", 1);
                SetField(stage, "displayName", "Audio Startup Test");
                SetField(stage, "bpm", 90f);
                SetField(stage, "startDelay", 0f);
                ((IList)GetField(roster, "stages")).Add(stage);

                object entry = Activator.CreateInstance(entryType);
                SetField(entry, "stage", stage);
                SetField(entry, "enemyPage", 1);
                SetField(entry, "clip", clip);
                SetField(entry, "bpm", 94f);
                SetField(entry, "loopBeats", 47);
                SetField(entry, "volume", 0.35f);
                ((IList)GetField(catalog, "entries")).Add(entry);

                SetField(conductor, "playOnStart", false);
                SetField(round, "conductor", conductor);
                SetField(rhythmAudio, "round", round);
                SetField(rhythmAudio, "conductor", conductor);
                SetField(rhythmAudio, "catalog", catalog);
                SetField(stageManager, "roster", roster);
                SetField(stageManager, "round", round);
                SetField(stageManager, "rhythmAudio", rhythmAudio);
                SetField(stageManager, "gateClockUntilAudioReady", true);

                int scheduleCount = 0;
                Action<double> onScheduled = _ => scheduleCount++;
                conductorType.GetEvent("OnClockScheduled")
                    .AddEventHandler(conductor, onScheduled);

                root.SetActive(true);
                for (int frame = 0;
                     frame < 10 && scheduleCount == 0;
                     frame++)
                {
                    yield return null;
                }
                yield return null;

                Assert.That(scheduleCount, Is.EqualTo(1));
                Assert.That(
                    (bool)conductorType.GetProperty("IsRunning")
                        .GetValue(conductor),
                    Is.True);
            }
            finally
            {
                UnityEngine.Object.Destroy(root);
                UnityEngine.Object.Destroy(stage);
                UnityEngine.Object.Destroy(roster);
                UnityEngine.Object.Destroy(catalog);
                UnityEngine.Object.Destroy(clip);
            }
        }

        private static Type RuntimeType(string fullName)
        {
            Assembly runtimeAssembly = Array.Find(
                AppDomain.CurrentDomain.GetAssemblies(),
                assembly => assembly.GetType(fullName) != null);
            Assert.That(runtimeAssembly, Is.Not.Null, fullName);
            return runtimeAssembly.GetType(fullName, true);
        }

        private static object GetField(object target, string fieldName)
        {
            FieldInfo field = FindField(target.GetType(), fieldName);
            Assert.That(field, Is.Not.Null, fieldName);
            return field.GetValue(target);
        }

        private static void SetField(
            object target,
            string fieldName,
            object value)
        {
            FieldInfo field = FindField(target.GetType(), fieldName);
            Assert.That(field, Is.Not.Null, fieldName);
            field.SetValue(target, value);
        }

        private static FieldInfo FindField(Type type, string fieldName)
        {
            while (type != null)
            {
                FieldInfo field = type.GetField(
                    fieldName,
                    BindingFlags.Instance
                    | BindingFlags.Public
                    | BindingFlags.NonPublic);
                if (field != null) return field;
                type = type.BaseType;
            }

            return null;
        }
    }
}
