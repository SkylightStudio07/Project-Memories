using System;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace BeatMemories.Tests
{
    public class RhythmClockIntegrityTests
    {
        [Test]
        public void InputAndPresentationCannotMoveTheFixedClock()
        {
            Assembly runtimeAssembly = AppDomain.CurrentDomain.GetAssemblies()
                .First(assembly => assembly.GetType("BeatMemories.Conductor") != null);
            Type conductorType = runtimeAssembly.GetType("BeatMemories.Conductor", true);
            var gameObject = new GameObject("Fixed Clock Test");
            try
            {
                Component conductor = gameObject.AddComponent(conductorType);
                conductorType.GetMethod("StartClock").Invoke(conductor, null);

                FieldInfo startTime = conductorType.GetField(
                    "startTime",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                PropertyInfo scheduled = conductorType.GetProperty("ScheduledStartDspTime");
                PropertyInfo totalBeats = conductorType.GetProperty("TotalBeats");
                FieldInfo gameplayOrigin = conductorType.GetField(
                    "gameplayStartDspTime",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                double originalStart = (double)startTime.GetValue(conductor);
                double originalScheduled = (double)scheduled.GetValue(conductor);
                int originalBeat = (int)totalBeats.GetValue(conductor);

                Assert.That(gameplayOrigin.GetValue(conductor), Is.EqualTo(originalScheduled));

                conductorType.GetMethod("DelayClock")
                    .Invoke(conductor, new object[] { 10.0 });
                object advanced = conductorType.GetMethod("AdvanceResponseBeatNow")
                    .Invoke(conductor, new object[] { 0 });

                Assert.That(advanced, Is.False);
                Assert.That(startTime.GetValue(conductor), Is.EqualTo(originalStart));
                Assert.That(scheduled.GetValue(conductor), Is.EqualTo(originalScheduled));
                Assert.That(totalBeats.GetValue(conductor), Is.EqualTo(originalBeat));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(gameObject);
            }
        }

        [Test]
        public void ScoreDigitsUseTheRequestedNumberSpriteSheet()
        {
            const string expectedGuid = "31a6095b542445ab9344ab13512922c3";
            foreach (string scene in new[]
                     {
                         "Assets/Scenes/BeatMemories.unity",
                         "Assets/Scenes/BeatMemories_Dayeon.unity",
                     })
            {
                string yaml = File.ReadAllText(scene);
                int references = yaml.Split(new[] { expectedGuid }, StringSplitOptions.None)
                    .Length - 1;
                Assert.That(references, Is.GreaterThanOrEqualTo(10), scene);
            }
        }
    }
}
