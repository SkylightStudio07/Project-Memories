# Rhythm

Rhythm appears to be an externally connected area around rhythm, beat, memories, pattern, settings. It contains 2 types, including 2 Unity-facing types.

## Stats

- Types: 2
- Internal relationships: 0
- External relationships: 6
- Entry candidates: 1
- Keywords: `rhythm`, `beat`, `memories`, `pattern`, `settings`, `so`, `timing`

## Start Here

- `RhythmPatternSO.OnValidate()` - unity_lifecycle / H:\Unity\Project-Memories\Assets\Scripts\Rhythm\RhythmPatternSO.cs:54

## Core Types

- `RhythmPatternSO` - class / Unity / 0 out / 4 in / H:\Unity\Project-Memories\Assets\Scripts\Rhythm\RhythmPatternSO.cs:14
- `RhythmTimingSettings` - class / Unity / 0 out / 2 in / H:\Unity\Project-Memories\Assets\Scripts\Rhythm\RhythmTimingSettings.cs:11

## Likely Method Flows

- `RhythmPatternSO.OnValidate()`
  - `RhythmPatternSO.OnValidate() / terminal`

## Internal Type Relationships

- None detected.

## External Touchpoints

- `RoundManager` -> `RhythmPatternSO` - incoming / calls_member / 2 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Managers\RoundManager.cs:198 / pattern.SpotlightBeatIndices()`
- `Conductor` -> `RhythmTimingSettings` - incoming / has_field_type / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Rhythm\Conductor.cs:19 / RhythmTimingSettings`
- `RoundManager` -> `RhythmPatternSO` - incoming / has_field_type / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Managers\RoundManager.cs:34 / RhythmPatternSO`
- `StageSO` -> `RhythmPatternSO` - incoming / has_field_type / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Managers\StageSO.cs:36 / RhythmPatternSO`
- `Conductor` -> `RhythmTimingSettings` - incoming / has_property_type / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Rhythm\Conductor.cs:57 / RhythmTimingSettings`

## Internal Method Calls

- None detected.

## Evidence

- Likely flow - RhythmPatternSO.OnValidate() -> 
- incoming calls_member - RoundManager -> RhythmPatternSO / 2 refs
  - `H:\Unity\Project-Memories\Assets\Scripts\Managers\RoundManager.cs:198 / pattern.SpotlightBeatIndices()`
- incoming has_field_type - Conductor -> RhythmTimingSettings / 1 refs
  - `H:\Unity\Project-Memories\Assets\Scripts\Rhythm\Conductor.cs:19 / RhythmTimingSettings`
- incoming has_field_type - RoundManager -> RhythmPatternSO / 1 refs
  - `H:\Unity\Project-Memories\Assets\Scripts\Managers\RoundManager.cs:34 / RhythmPatternSO`

## Suggested AI Task

Use the Rhythm context to explain the reading order, likely runtime flow, and risky assumptions. Cite method names, relationship edges, and file references when possible.

