# Character

Character appears to be an externally connected area around character, beat, memories, anchor, binding. It contains 4 types, including 2 Unity-facing types.

## Stats

- Types: 4
- Internal relationships: 8
- External relationships: 20
- Entry candidates: 0
- Keywords: `character`, `beat`, `memories`, `anchor`, `binding`

## Start Here

- None detected.

## Core Types

- `CharacterView` - class / Unity / 7 out / 18 in / H:\Unity\Project-Memories\Assets\Scripts\Character\CharacterView.cs:28
- `CharacterAnchorBinding` - struct / 1 out / 3 in / H:\Unity\Project-Memories\Assets\Scripts\Character\CharacterView.cs:17
- `CharacterData` - class / Unity / 0 out / 4 in / H:\Unity\Project-Memories\Assets\Scripts\Character\CharacterData.cs:9
- `CharacterAnchorType` - enum / 0 out / 3 in / H:\Unity\Project-Memories\Assets\Scripts\Character\CharacterView.cs:7

## Likely Method Flows

- No internal method flow detected.

## Internal Type Relationships

- `CharacterView` -> `CharacterAnchorType` - internal / accepts_parameter / 2 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Character\CharacterView.cs:52 / CharacterAnchorType`
- `CharacterView` -> `CharacterAnchorBinding` - internal / creates / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Character\CharacterView.cs:41 / List<CharacterAnchorBinding>`
- `CharacterAnchorBinding` -> `CharacterAnchorType` - internal / has_field_type / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Character\CharacterView.cs:19 / CharacterAnchorType`
- `CharacterView` -> `CharacterAnchorBinding` - internal / has_field_type / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Character\CharacterView.cs:40 / List<CharacterAnchorBinding>`
- `CharacterView` -> `CharacterData` - internal / has_field_type / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Character\CharacterView.cs:30 / CharacterData`
- `CharacterView` -> `CharacterData` - internal / has_property_type / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Character\CharacterView.cs:43 / CharacterData`
- `CharacterView` -> `CharacterAnchorBinding` - internal / uses_local_type / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Character\CharacterView.cs:71 / CharacterAnchorBinding`

## External Touchpoints

- `HudView` -> `CharacterView` - incoming / calls_member / 5 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\View\HudView.cs:1210 / view.GetComponent<KeyframeAnimator>()`
- `HudView` -> `CharacterView` - incoming / uses_local_type / 4 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\View\HudView.cs:1208 / CharacterView`
- `StageManager` -> `CharacterView` - incoming / accepts_parameter / 2 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Managers\StageManager.cs:414 / CharacterView`
- `StageManager` -> `CharacterView` - incoming / has_field_type / 2 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Managers\StageManager.cs:39 / CharacterView`
- `StageSO` -> `CharacterView` - incoming / has_field_type / 2 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Managers\StageSO.cs:26 / CharacterView`
- `StageManager` -> `CharacterView` - incoming / has_property_type / 2 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Managers\StageManager.cs:42 / CharacterView`
- `Enemy` -> `CharacterData` - incoming / inherits / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Enemy\Enemy.cs:11 / CharacterData`
- `PlayerCharacterData` -> `CharacterData` - incoming / inherits / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Character\PlayerCharacterData.cs:9 / CharacterData`
- `HudView` -> `CharacterView` - incoming / returns / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\View\HudView.cs:1809 / CharacterView`

## Internal Method Calls

- `CharacterView.GetAnchorPosition(CharacterAnchorType)` -> `CharacterView.GetAnchor(CharacterAnchorType)` / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Character\CharacterView.cs:82 / GetAnchor(type)`

## Evidence

- Internal call - CharacterView.GetAnchorPosition(CharacterAnchorType) -> CharacterView.GetAnchor(CharacterAnchorType)
  - `H:\Unity\Project-Memories\Assets\Scripts\Character\CharacterView.cs:82 / GetAnchor(type)`
- incoming calls_member - HudView -> CharacterView / 5 refs
  - `H:\Unity\Project-Memories\Assets\Scripts\View\HudView.cs:1210 / view.GetComponent<KeyframeAnimator>()`
- incoming uses_local_type - HudView -> CharacterView / 4 refs
  - `H:\Unity\Project-Memories\Assets\Scripts\View\HudView.cs:1208 / CharacterView`
- incoming accepts_parameter - StageManager -> CharacterView / 2 refs
  - `H:\Unity\Project-Memories\Assets\Scripts\Managers\StageManager.cs:414 / CharacterView`
- Internal accepts_parameter - CharacterView -> CharacterAnchorType / 2 refs
  - `H:\Unity\Project-Memories\Assets\Scripts\Character\CharacterView.cs:52 / CharacterAnchorType`
- Internal creates - CharacterView -> CharacterAnchorBinding / 1 refs
  - `H:\Unity\Project-Memories\Assets\Scripts\Character\CharacterView.cs:41 / List<CharacterAnchorBinding>`
- Internal has_field_type - CharacterAnchorBinding -> CharacterAnchorType / 1 refs
  - `H:\Unity\Project-Memories\Assets\Scripts\Character\CharacterView.cs:19 / CharacterAnchorType`
- Internal has_field_type - CharacterView -> CharacterAnchorBinding / 1 refs
  - `H:\Unity\Project-Memories\Assets\Scripts\Character\CharacterView.cs:40 / List<CharacterAnchorBinding>`
- Internal has_field_type - CharacterView -> CharacterData / 1 refs
  - `H:\Unity\Project-Memories\Assets\Scripts\Character\CharacterView.cs:30 / CharacterData`
- Internal has_property_type - CharacterView -> CharacterData / 1 refs
  - `H:\Unity\Project-Memories\Assets\Scripts\Character\CharacterView.cs:43 / CharacterData`
- Internal uses_local_type - CharacterView -> CharacterAnchorBinding / 1 refs
  - `H:\Unity\Project-Memories\Assets\Scripts\Character\CharacterView.cs:71 / CharacterAnchorBinding`

## Suggested AI Task

Use the Character context to explain the reading order, likely runtime flow, and risky assumptions. Cite method names, relationship edges, and file references when possible.

