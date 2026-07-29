# Animator

Animator appears to be an externally connected area around animator, beat, keyframe, memories. It contains 1 types, including 1 Unity-facing types.

## Stats

- Types: 1
- Internal relationships: 0
- External relationships: 8
- Entry candidates: 4
- Keywords: `animator`, `beat`, `keyframe`, `memories`

## Start Here

- `KeyframeAnimator.Awake()` - unity_lifecycle / H:\Unity\Project-Memories\Assets\Scripts\View\KeyframeAnimator.cs:33
- `KeyframeAnimator.OnEnable()` - unity_lifecycle / H:\Unity\Project-Memories\Assets\Scripts\View\KeyframeAnimator.cs:35
- `KeyframeAnimator.OnDisable()` - unity_lifecycle / H:\Unity\Project-Memories\Assets\Scripts\View\KeyframeAnimator.cs:41
- `KeyframeAnimator.Update()` - unity_lifecycle / H:\Unity\Project-Memories\Assets\Scripts\View\KeyframeAnimator.cs:46

## Core Types

- `KeyframeAnimator` - class / Unity / 1 out / 7 in / H:\Unity\Project-Memories\Assets\Scripts\View\KeyframeAnimator.cs:11

## Likely Method Flows

- `KeyframeAnimator.OnEnable()`
  - `KeyframeAnimator.OnEnable()`
  - `KeyframeAnimator.Show() / terminal`
- `KeyframeAnimator.Update()`
  - `KeyframeAnimator.Update()`
  - `KeyframeAnimator.Advance()`
  - `KeyframeAnimator.Show() / terminal`
- `KeyframeAnimator.Awake()`
  - `KeyframeAnimator.Awake() / terminal`
- `KeyframeAnimator.OnDisable()`
  - `KeyframeAnimator.OnDisable() / terminal`

## Internal Type Relationships

- None detected.

## External Touchpoints

- `HudView` -> `KeyframeAnimator` - incoming / calls_member / 2 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\View\HudView.cs:852 / playerIdleAnim.Pause()`
- `HudView` -> `KeyframeAnimator` - incoming / has_field_type / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\View\HudView.cs:34 / KeyframeAnimator`
- `KeyframeAnimator` -> `Conductor` - outgoing / has_field_type / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\View\KeyframeAnimator.cs:19 / Conductor`
- `HudView` -> `KeyframeAnimator` - incoming / unity_get_component / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\View\HudView.cs:1210 / KeyframeAnimator`
- `StageManager` -> `KeyframeAnimator` - incoming / unity_get_component / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Managers\StageManager.cs:447 / KeyframeAnimator`
- `HudView` -> `KeyframeAnimator` - incoming / uses_local_type / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\View\HudView.cs:1209 / KeyframeAnimator`
- `StageManager` -> `KeyframeAnimator` - incoming / uses_local_type / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Managers\StageManager.cs:446 / KeyframeAnimator`

## Internal Method Calls

- `KeyframeAnimator.OnEnable()` -> `KeyframeAnimator.Show()` / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\View\KeyframeAnimator.cs:38 / Show()`
- `KeyframeAnimator.Update()` -> `KeyframeAnimator.Advance()` / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\View\KeyframeAnimator.cs:54 / Advance()`
- `KeyframeAnimator.OnBeat(int)` -> `KeyframeAnimator.Advance()` / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\View\KeyframeAnimator.cs:62 / Advance()`
- `KeyframeAnimator.Resume()` -> `KeyframeAnimator.Show()` / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\View\KeyframeAnimator.cs:72 / Show()`
- `KeyframeAnimator.Advance()` -> `KeyframeAnimator.Show()` / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\View\KeyframeAnimator.cs:78 / Show()`

## Evidence

- Likely flow - KeyframeAnimator.OnEnable() -> KeyframeAnimator.Show() / terminal
- Likely flow - KeyframeAnimator.Update() -> KeyframeAnimator.Advance() -> KeyframeAnimator.Show() / terminal
- Internal call - KeyframeAnimator.OnEnable() -> KeyframeAnimator.Show()
  - `H:\Unity\Project-Memories\Assets\Scripts\View\KeyframeAnimator.cs:38 / Show()`
- Internal call - KeyframeAnimator.Update() -> KeyframeAnimator.Advance()
  - `H:\Unity\Project-Memories\Assets\Scripts\View\KeyframeAnimator.cs:54 / Advance()`
- Internal call - KeyframeAnimator.OnBeat(int) -> KeyframeAnimator.Advance()
  - `H:\Unity\Project-Memories\Assets\Scripts\View\KeyframeAnimator.cs:62 / Advance()`
- incoming calls_member - HudView -> KeyframeAnimator / 2 refs
  - `H:\Unity\Project-Memories\Assets\Scripts\View\HudView.cs:852 / playerIdleAnim.Pause()`
- incoming has_field_type - HudView -> KeyframeAnimator / 1 refs
  - `H:\Unity\Project-Memories\Assets\Scripts\View\HudView.cs:34 / KeyframeAnimator`
- outgoing has_field_type - KeyframeAnimator -> Conductor / 1 refs
  - `H:\Unity\Project-Memories\Assets\Scripts\View\KeyframeAnimator.cs:19 / Conductor`

## Suggested AI Task

Use the Animator context to explain the reading order, likely runtime flow, and risky assumptions. Cite method names, relationship edges, and file references when possible.

