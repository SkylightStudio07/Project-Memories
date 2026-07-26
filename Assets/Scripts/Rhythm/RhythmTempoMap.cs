using System;
using System.Collections.Generic;

namespace BeatMemories
{
    /// <summary>
    /// Maps an absolute time axis to a continuous beat axis.
    /// Tempo changes are append-only so previously scheduled beats never move.
    /// </summary>
    public sealed class RhythmTempoMap
    {
        public readonly struct TempoSegment
        {
            public TempoSegment(double startTime, double startBeat, double bpm)
            {
                StartTime = startTime;
                StartBeat = startBeat;
                Bpm = bpm;
            }

            public double StartTime { get; }
            public double StartBeat { get; }
            public double Bpm { get; }
            public double SecondsPerBeat => 60.0 / Bpm;
        }

        private readonly List<TempoSegment> segments = new List<TempoSegment>();

        public int SegmentCount => segments.Count;

        public void Reset(double startTime, double bpm)
        {
            ValidateTime(startTime, nameof(startTime));
            ValidateBpm(bpm, nameof(bpm));
            segments.Clear();
            segments.Add(new TempoSegment(startTime, 0.0, bpm));
        }

        public void ScheduleTempoChange(double startTime, double bpm)
        {
            EnsureInitialized();
            ValidateTime(startTime, nameof(startTime));
            ValidateBpm(bpm, nameof(bpm));

            TempoSegment last = segments[segments.Count - 1];
            if (startTime < last.StartTime)
                throw new ArgumentOutOfRangeException(
                    nameof(startTime),
                    "Tempo changes must be appended in chronological order.");

            if (startTime == last.StartTime)
            {
                if (bpm == last.Bpm) return;
                throw new InvalidOperationException(
                    "A tempo segment already starts at this time.");
            }

            double startBeat = BeatPositionAt(startTime);
            if (bpm == last.Bpm) return;
            segments.Add(new TempoSegment(startTime, startBeat, bpm));
        }

        public double BeatPositionAt(double time)
        {
            EnsureInitialized();
            ValidateTime(time, nameof(time));
            TempoSegment segment = SegmentAtTime(time);
            return segment.StartBeat
                   + (time - segment.StartTime) / segment.SecondsPerBeat;
        }

        public double TimeAtBeat(double beat)
        {
            EnsureInitialized();
            ValidateTime(beat, nameof(beat));
            TempoSegment segment = SegmentAtBeat(beat);
            return segment.StartTime
                   + (beat - segment.StartBeat) * segment.SecondsPerBeat;
        }

        public double TempoAt(double time)
        {
            EnsureInitialized();
            ValidateTime(time, nameof(time));
            return SegmentAtTime(time).Bpm;
        }

        private TempoSegment SegmentAtTime(double time)
        {
            for (int i = segments.Count - 1; i >= 0; i--)
            {
                if (time >= segments[i].StartTime)
                    return segments[i];
            }

            return segments[0];
        }

        private TempoSegment SegmentAtBeat(double beat)
        {
            for (int i = segments.Count - 1; i >= 0; i--)
            {
                if (beat >= segments[i].StartBeat)
                    return segments[i];
            }

            return segments[0];
        }

        private void EnsureInitialized()
        {
            if (segments.Count == 0)
                throw new InvalidOperationException(
                    "Reset must be called before querying the tempo map.");
        }

        private static void ValidateBpm(double bpm, string parameterName)
        {
            if (double.IsNaN(bpm) || double.IsInfinity(bpm) || bpm <= 0.0)
                throw new ArgumentOutOfRangeException(parameterName);
        }

        private static void ValidateTime(double value, string parameterName)
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
                throw new ArgumentOutOfRangeException(parameterName);
        }
    }
}
