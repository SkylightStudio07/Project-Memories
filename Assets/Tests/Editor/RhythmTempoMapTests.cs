using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace BeatMemories.Tests
{
    public class RhythmTempoMapTests
    {
        private Type tempoMapType;
        private MethodInfo reset;
        private MethodInfo scheduleTempoChange;
        private MethodInfo beatPositionAt;
        private MethodInfo timeAtBeat;
        private MethodInfo tempoAt;

        [SetUp]
        public void SetUp()
        {
            Assembly runtimeAssembly = AppDomain.CurrentDomain.GetAssemblies()
                .First(assembly =>
                    assembly.GetType("BeatMemories.RhythmTempoMap") != null);
            tempoMapType = runtimeAssembly.GetType(
                "BeatMemories.RhythmTempoMap",
                true);
            reset = tempoMapType.GetMethod("Reset");
            scheduleTempoChange =
                tempoMapType.GetMethod("ScheduleTempoChange");
            beatPositionAt = tempoMapType.GetMethod("BeatPositionAt");
            timeAtBeat = tempoMapType.GetMethod("TimeAtBeat");
            tempoAt = tempoMapType.GetMethod("TempoAt");
        }

        [Test]
        public void ConstantTempoHasNoAccumulatedErrorAcrossTenThousandBeats()
        {
            object map = Activator.CreateInstance(tempoMapType);
            const double anchor = 123.456;
            const double bpm = 94.0;
            reset.Invoke(map, new object[] { anchor, bpm });

            double expected = anchor + 10000.0 * 60.0 / bpm;
            double actual =
                (double)timeAtBeat.Invoke(map, new object[] { 10000.0 });
            double roundTrip =
                (double)beatPositionAt.Invoke(map, new object[] { actual });

            Assert.That(actual, Is.EqualTo(expected).Within(1e-9));
            Assert.That(roundTrip, Is.EqualTo(10000.0).Within(1e-9));
        }

        [Test]
        public void ScheduledTempoChangeKeepsBoundaryPhaseContinuous()
        {
            object map = Activator.CreateInstance(tempoMapType);
            const double anchor = 10.0;
            const double oldBpm = 95.0;
            const double newBpm = 92.0;
            reset.Invoke(map, new object[] { anchor, oldBpm });

            double boundary =
                (double)timeAtBeat.Invoke(map, new object[] { 12.0 });
            scheduleTempoChange.Invoke(
                map,
                new object[] { boundary, newBpm });

            double boundaryBeat =
                (double)beatPositionAt.Invoke(map, new object[] { boundary });
            double nextBeat =
                (double)timeAtBeat.Invoke(map, new object[] { 13.0 });
            double oldBeat =
                (double)timeAtBeat.Invoke(map, new object[] { 6.0 });

            Assert.That(boundaryBeat, Is.EqualTo(12.0).Within(1e-10));
            Assert.That(
                nextBeat,
                Is.EqualTo(boundary + 60.0 / newBpm).Within(1e-10));
            Assert.That(
                oldBeat,
                Is.EqualTo(anchor + 6.0 * 60.0 / oldBpm).Within(1e-10));
            Assert.That(
                (double)tempoAt.Invoke(
                    map,
                    new object[] { boundary - 0.001 }),
                Is.EqualTo(oldBpm));
            Assert.That(
                (double)tempoAt.Invoke(map, new object[] { boundary }),
                Is.EqualTo(newBpm));
        }

        [Test]
        public void MultipleSegmentsRoundTripFarFutureBeatExactly()
        {
            object map = Activator.CreateInstance(tempoMapType);
            reset.Invoke(map, new object[] { 50.0, 97.0 });

            double firstBoundary =
                (double)timeAtBeat.Invoke(map, new object[] { 256.0 });
            scheduleTempoChange.Invoke(
                map,
                new object[] { firstBoundary, 137.0 });
            double secondBoundary =
                (double)timeAtBeat.Invoke(map, new object[] { 304.0 });
            scheduleTempoChange.Invoke(
                map,
                new object[] { secondBoundary, 93.0 });

            double futureTime =
                (double)timeAtBeat.Invoke(map, new object[] { 10000.0 });
            double futureBeat =
                (double)beatPositionAt.Invoke(map, new object[] { futureTime });

            Assert.That(futureBeat, Is.EqualTo(10000.0).Within(1e-9));
        }
    }
}
