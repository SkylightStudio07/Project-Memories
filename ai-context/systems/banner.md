# Banner

Banner appears to be an externally connected area around banner, beat, clear, memories, stage. It contains 1 types, including 1 Unity-facing types.

## Stats

- Types: 1
- Internal relationships: 0
- External relationships: 2
- Entry candidates: 1
- Keywords: `banner`, `beat`, `clear`, `memories`, `stage`

## Start Here

- `StageClearBanner.Awake()` - unity_lifecycle / H:\Unity\Project-Memories\Assets\Scripts\View\StageClearBanner.cs:44

## Core Types

- `StageClearBanner` - class / Unity / 0 out / 2 in / H:\Unity\Project-Memories\Assets\Scripts\View\StageClearBanner.cs:14

## Likely Method Flows

- `StageClearBanner.Awake()`
  - `StageClearBanner.Awake()`
  - `StageClearBanner.EnsureDimBackground() / terminal`

## Internal Type Relationships

- None detected.

## External Touchpoints

- `StageManager` -> `StageClearBanner` - incoming / calls_member / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Managers\StageManager.cs:207 / stageClearBanner.PlayActClear(CurrentStage.stageNumber)`
- `StageManager` -> `StageClearBanner` - incoming / has_field_type / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Managers\StageManager.cs:63 / StageClearBanner`

## Internal Method Calls

- `StageClearBanner.Awake()` -> `StageClearBanner.EnsureDimBackground()` / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\View\StageClearBanner.cs:50 / EnsureDimBackground()`
- `StageClearBanner.PlayActClear(int)` -> `StageClearBanner.EnsureDimBackground()` / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\View\StageClearBanner.cs:63 / EnsureDimBackground()`
- `StageClearBanner.PlayActClear(int)` -> `StageClearBanner.Hide()` / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\View\StageClearBanner.cs:116 / Hide()`
- `StageClearBanner.PlayActClear(int)` -> `StageClearBanner.SetAlpha(float)` / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\View\StageClearBanner.cs:79 / SetAlpha(0f)`
- `StageClearBanner.PlayActClear(int)` -> `StageClearBanner.SetDimAlpha(float)` / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\View\StageClearBanner.cs:68 / SetDimAlpha(0f)`
- `StageClearBanner.Hide()` -> `StageClearBanner.SetAlpha(float)` / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\View\StageClearBanner.cs:124 / SetAlpha(1f)`
- `StageClearBanner.Hide()` -> `StageClearBanner.SetDimAlpha(float)` / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\View\StageClearBanner.cs:128 / SetDimAlpha(0f)`

## Evidence

- Likely flow - StageClearBanner.Awake() -> StageClearBanner.EnsureDimBackground() / terminal
- Internal call - StageClearBanner.Awake() -> StageClearBanner.EnsureDimBackground()
  - `H:\Unity\Project-Memories\Assets\Scripts\View\StageClearBanner.cs:50 / EnsureDimBackground()`
- Internal call - StageClearBanner.PlayActClear(int) -> StageClearBanner.EnsureDimBackground()
  - `H:\Unity\Project-Memories\Assets\Scripts\View\StageClearBanner.cs:63 / EnsureDimBackground()`
- Internal call - StageClearBanner.PlayActClear(int) -> StageClearBanner.Hide()
  - `H:\Unity\Project-Memories\Assets\Scripts\View\StageClearBanner.cs:116 / Hide()`
- incoming calls_member - StageManager -> StageClearBanner / 1 refs
  - `H:\Unity\Project-Memories\Assets\Scripts\Managers\StageManager.cs:207 / stageClearBanner.PlayActClear(CurrentStage.stageNumber)`
- incoming has_field_type - StageManager -> StageClearBanner / 1 refs
  - `H:\Unity\Project-Memories\Assets\Scripts\Managers\StageManager.cs:63 / StageClearBanner`

## Suggested AI Task

Use the Banner context to explain the reading order, likely runtime flow, and risky assumptions. Cite method names, relationship edges, and file references when possible.

