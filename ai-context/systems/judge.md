# Judge

Judge appears to be an externally connected area around judge, beat, memories, result. It contains 2 types, including 0 Unity-facing types.

## Stats

- Types: 2
- Internal relationships: 3
- External relationships: 20
- Entry candidates: 0
- Keywords: `judge`, `beat`, `memories`, `result`

## Start Here

- None detected.

## Core Types

- `JudgeResult` - struct / 4 out / 14 in / H:\Unity\Project-Memories\Assets\Scripts\Judge\JudgeResult.cs:7
- `JudgeSystem` - class / 7 out / 1 in / H:\Unity\Project-Memories\Assets\Scripts\Judge\JudgeSystem.cs:8

## Likely Method Flows

- No internal method flow detected.

## Internal Type Relationships

- `JudgeSystem` -> `JudgeResult` - internal / creates / 2 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Judge\JudgeSystem.cs:13 / JudgeResult`
- `JudgeSystem` -> `JudgeResult` - internal / returns / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Judge\JudgeSystem.cs:10 / JudgeResult`

## External Touchpoints

- `HudView` -> `JudgeResult` - incoming / accepts_parameter / 4 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\View\HudView.cs:455 / JudgeResult`
- `RoundManager` -> `JudgeResult` - incoming / creates / 4 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Managers\RoundManager.cs:530 / JudgeResult`
- `JudgeResult` -> `OutcomeType` - outgoing / accepts_parameter / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Judge\JudgeResult.cs:24 / OutcomeType`
- `JudgeResult` -> `PlayerAction` - outgoing / accepts_parameter / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Judge\JudgeResult.cs:24 / PlayerAction`
- `JudgeSystem` -> `Enemy` - outgoing / accepts_parameter / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Judge\JudgeSystem.cs:10 / Enemy`
- `JudgeSystem` -> `PlayerAction` - outgoing / accepts_parameter / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Judge\JudgeSystem.cs:10 / PlayerAction`
- `RhythmTimingDisplay` -> `JudgeResult` - incoming / accepts_parameter / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\View\RhythmTimingDisplay.cs:248 / JudgeResult`
- `RoundManager` -> `JudgeSystem` - incoming / calls_member / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Managers\RoundManager.cs:525 / JudgeSystem.Judge(enemy, action, charged)`
- `RoundManager` -> `JudgeResult` - incoming / has_event_type / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Managers\RoundManager.cs:66 / Action<int, Enemy, JudgeResult>`
- `JudgeResult` -> `OutcomeType` - outgoing / has_field_type / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Judge\JudgeResult.cs:13 / OutcomeType`
- `JudgeResult` -> `PlayerAction` - outgoing / has_field_type / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Judge\JudgeResult.cs:10 / PlayerAction`
- `JudgeSystem` -> `OutcomeType` - outgoing / uses_local_type / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Judge\JudgeSystem.cs:32 / OutcomeType`
- `JudgeSystem` -> `PlayerAction` - outgoing / uses_local_type / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Judge\JudgeSystem.cs:15 / PlayerAction`
- `RoundManager` -> `JudgeResult` - incoming / uses_local_type / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Managers\RoundManager.cs:525 / JudgeResult`

## Internal Method Calls

- None detected.

## Evidence

- incoming accepts_parameter - HudView -> JudgeResult / 4 refs
  - `H:\Unity\Project-Memories\Assets\Scripts\View\HudView.cs:455 / JudgeResult`
- incoming creates - RoundManager -> JudgeResult / 4 refs
  - `H:\Unity\Project-Memories\Assets\Scripts\Managers\RoundManager.cs:530 / JudgeResult`
- outgoing accepts_parameter - JudgeResult -> OutcomeType / 1 refs
  - `H:\Unity\Project-Memories\Assets\Scripts\Judge\JudgeResult.cs:24 / OutcomeType`
- Internal creates - JudgeSystem -> JudgeResult / 2 refs
  - `H:\Unity\Project-Memories\Assets\Scripts\Judge\JudgeSystem.cs:13 / JudgeResult`
- Internal returns - JudgeSystem -> JudgeResult / 1 refs
  - `H:\Unity\Project-Memories\Assets\Scripts\Judge\JudgeSystem.cs:10 / JudgeResult`

## Suggested AI Task

Use the Judge context to explain the reading order, likely runtime flow, and risky assumptions. Cite method names, relationship edges, and file references when possible.

