# Map Generation

Map Generation appears to be an externally connected area around rhythm, tempo, beat, map, memories. It contains 2 types, including 0 Unity-facing types.

## Stats

- Types: 2
- Internal relationships: 12
- External relationships: 21
- Entry candidates: 1
- Keywords: `rhythm`, `tempo`, `beat`, `map`, `memories`, `segment`

## Start Here

- `RhythmTempoMap.Reset(double, double)` - unity_lifecycle / H:\Unity\Project-Memories\Assets\Scripts\Rhythm\RhythmTempoMap.cs:31

## Core Types

- `RhythmTempoMap` - class / 12 out / 21 in / H:\Unity\Project-Memories\Assets\Scripts\Rhythm\RhythmTempoMap.cs:10
- `RhythmTempoMap+TempoSegment` - struct / 0 out / 12 in / H:\Unity\Project-Memories\Assets\Scripts\Rhythm\RhythmTempoMap.cs:12

## Likely Method Flows

- `RhythmTempoMap.Reset(double, double)`
  - `RhythmTempoMap.Reset(double, double)`
  - `RhythmTempoMap.ValidateBpm(double, string) / terminal`

## Internal Type Relationships

- `RhythmTempoMap` -> `RhythmTempoMap+TempoSegment` - internal / calls_member / 3 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Rhythm\RhythmTempoMap.cs:35 / segments.Clear()`
- `RhythmTempoMap` -> `RhythmTempoMap+TempoSegment` - internal / creates / 3 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Rhythm\RhythmTempoMap.cs:27 / List<TempoSegment>`
- `RhythmTempoMap` -> `RhythmTempoMap+TempoSegment` - internal / uses_local_type / 3 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Rhythm\RhythmTempoMap.cs:45 / TempoSegment`
- `RhythmTempoMap` -> `RhythmTempoMap+TempoSegment` - internal / returns / 2 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Rhythm\RhythmTempoMap.cs:88 / TempoSegment`
- `RhythmTempoMap` -> `RhythmTempoMap+TempoSegment` - internal / has_field_type / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Rhythm\RhythmTempoMap.cs:27 / List<TempoSegment>`

## External Touchpoints

- `Conductor` -> `RhythmTempoMap` - incoming / calls_member / 17 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Rhythm\Conductor.cs:44 / clockTempoMap.TempoAt(AudioSettings.dspTime)`
- `Conductor` -> `RhythmTempoMap` - incoming / creates / 2 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Rhythm\Conductor.cs:35 / RhythmTempoMap`
- `Conductor` -> `RhythmTempoMap` - incoming / has_field_type / 2 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Rhythm\Conductor.cs:35 / RhythmTempoMap`

## Internal Method Calls

- `RhythmTempoMap.Reset(double, double)` -> `RhythmTempoMap.ValidateBpm(double, string)` / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Rhythm\RhythmTempoMap.cs:34 / ValidateBpm(bpm, nameof(bpm))`
- `RhythmTempoMap.Reset(double, double)` -> `RhythmTempoMap.ValidateTime(double, string)` / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Rhythm\RhythmTempoMap.cs:33 / ValidateTime(startTime, nameof(startTime))`
- `RhythmTempoMap.ScheduleTempoChange(double, double)` -> `RhythmTempoMap.BeatPositionAt(double)` / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Rhythm\RhythmTempoMap.cs:58 / BeatPositionAt(startTime)`
- `RhythmTempoMap.ScheduleTempoChange(double, double)` -> `RhythmTempoMap.EnsureInitialized()` / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Rhythm\RhythmTempoMap.cs:41 / EnsureInitialized()`
- `RhythmTempoMap.ScheduleTempoChange(double, double)` -> `RhythmTempoMap.ValidateBpm(double, string)` / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Rhythm\RhythmTempoMap.cs:43 / ValidateBpm(bpm, nameof(bpm))`
- `RhythmTempoMap.ScheduleTempoChange(double, double)` -> `RhythmTempoMap.ValidateTime(double, string)` / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Rhythm\RhythmTempoMap.cs:42 / ValidateTime(startTime, nameof(startTime))`
- `RhythmTempoMap.BeatPositionAt(double)` -> `RhythmTempoMap.EnsureInitialized()` / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Rhythm\RhythmTempoMap.cs:65 / EnsureInitialized()`
- `RhythmTempoMap.BeatPositionAt(double)` -> `RhythmTempoMap.SegmentAtTime(double)` / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Rhythm\RhythmTempoMap.cs:67 / SegmentAtTime(time)`
- `RhythmTempoMap.BeatPositionAt(double)` -> `RhythmTempoMap.ValidateTime(double, string)` / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Rhythm\RhythmTempoMap.cs:66 / ValidateTime(time, nameof(time))`
- `RhythmTempoMap.TimeAtBeat(double)` -> `RhythmTempoMap.EnsureInitialized()` / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Rhythm\RhythmTempoMap.cs:74 / EnsureInitialized()`
- `RhythmTempoMap.TimeAtBeat(double)` -> `RhythmTempoMap.SegmentAtBeat(double)` / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Rhythm\RhythmTempoMap.cs:76 / SegmentAtBeat(beat)`
- `RhythmTempoMap.TimeAtBeat(double)` -> `RhythmTempoMap.ValidateTime(double, string)` / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Rhythm\RhythmTempoMap.cs:75 / ValidateTime(beat, nameof(beat))`
- `RhythmTempoMap.TempoAt(double)` -> `RhythmTempoMap.EnsureInitialized()` / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Rhythm\RhythmTempoMap.cs:83 / EnsureInitialized()`
- `RhythmTempoMap.TempoAt(double)` -> `RhythmTempoMap.SegmentAtTime(double)` / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Rhythm\RhythmTempoMap.cs:85 / SegmentAtTime(time)`
- `RhythmTempoMap.TempoAt(double)` -> `RhythmTempoMap.ValidateTime(double, string)` / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Rhythm\RhythmTempoMap.cs:84 / ValidateTime(time, nameof(time))`

## Evidence

- Likely flow - RhythmTempoMap.Reset(double, double) -> RhythmTempoMap.ValidateBpm(double, string) / terminal
- Internal call - RhythmTempoMap.Reset(double, double) -> RhythmTempoMap.ValidateBpm(double, string)
  - `H:\Unity\Project-Memories\Assets\Scripts\Rhythm\RhythmTempoMap.cs:34 / ValidateBpm(bpm, nameof(bpm))`
- Internal call - RhythmTempoMap.Reset(double, double) -> RhythmTempoMap.ValidateTime(double, string)
  - `H:\Unity\Project-Memories\Assets\Scripts\Rhythm\RhythmTempoMap.cs:33 / ValidateTime(startTime, nameof(startTime))`
- Internal call - RhythmTempoMap.ScheduleTempoChange(double, double) -> RhythmTempoMap.BeatPositionAt(double)
  - `H:\Unity\Project-Memories\Assets\Scripts\Rhythm\RhythmTempoMap.cs:58 / BeatPositionAt(startTime)`
- incoming calls_member - Conductor -> RhythmTempoMap / 17 refs
  - `H:\Unity\Project-Memories\Assets\Scripts\Rhythm\Conductor.cs:44 / clockTempoMap.TempoAt(AudioSettings.dspTime)`
- incoming creates - Conductor -> RhythmTempoMap / 2 refs
  - `H:\Unity\Project-Memories\Assets\Scripts\Rhythm\Conductor.cs:35 / RhythmTempoMap`
- incoming has_field_type - Conductor -> RhythmTempoMap / 2 refs
  - `H:\Unity\Project-Memories\Assets\Scripts\Rhythm\Conductor.cs:35 / RhythmTempoMap`
- Internal calls_member - RhythmTempoMap -> RhythmTempoMap+TempoSegment / 3 refs
  - `H:\Unity\Project-Memories\Assets\Scripts\Rhythm\RhythmTempoMap.cs:35 / segments.Clear()`
- Internal creates - RhythmTempoMap -> RhythmTempoMap+TempoSegment / 3 refs
  - `H:\Unity\Project-Memories\Assets\Scripts\Rhythm\RhythmTempoMap.cs:27 / List<TempoSegment>`
- Internal uses_local_type - RhythmTempoMap -> RhythmTempoMap+TempoSegment / 3 refs
  - `H:\Unity\Project-Memories\Assets\Scripts\Rhythm\RhythmTempoMap.cs:45 / TempoSegment`
- Internal returns - RhythmTempoMap -> RhythmTempoMap+TempoSegment / 2 refs
  - `H:\Unity\Project-Memories\Assets\Scripts\Rhythm\RhythmTempoMap.cs:88 / TempoSegment`
- Internal has_field_type - RhythmTempoMap -> RhythmTempoMap+TempoSegment / 1 refs
  - `H:\Unity\Project-Memories\Assets\Scripts\Rhythm\RhythmTempoMap.cs:27 / List<TempoSegment>`

## Suggested AI Task

Use the Map Generation context to explain the reading order, likely runtime flow, and risky assumptions. Cite method names, relationship edges, and file references when possible.

