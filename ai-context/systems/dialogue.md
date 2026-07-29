# Dialogue

Dialogue appears to be an externally connected area around dialogue, beat, memories, line, settings. It contains 4 types, including 2 Unity-facing types.

## Stats

- Types: 4
- Internal relationships: 3
- External relationships: 11
- Entry candidates: 1
- Keywords: `dialogue`, `beat`, `memories`, `line`, `settings`, `so`, `speaker`, `typing`

## Start Here

- `DialogueTypingSettings.Load()` - flow_candidate / H:\Unity\Project-Memories\Assets\Scripts\Dialogue\DialogueTypingSettings.cs:27

## Core Types

- `DialogueSO` - class / Unity / 2 out / 7 in / H:\Unity\Project-Memories\Assets\Scripts\Dialogue\DialogueSO.cs:11
- `DialogueLine` - class / 1 out / 3 in / H:\Unity\Project-Memories\Assets\Scripts\Dialogue\DialogueLine.cs:13
- `DialogueTypingSettings` - class / Unity / 1 out / 2 in / H:\Unity\Project-Memories\Assets\Scripts\Dialogue\DialogueTypingSettings.cs:7
- `DialogueSpeaker` - enum / 0 out / 1 in / H:\Unity\Project-Memories\Assets\Scripts\Dialogue\DialogueLine.cs:5

## Likely Method Flows

- `DialogueTypingSettings.Load()`
  - `DialogueTypingSettings.Load() / terminal`

## Internal Type Relationships

- `DialogueSO` -> `DialogueLine` - internal / creates / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Dialogue\DialogueSO.cs:13 / List<DialogueLine>`
- `DialogueLine` -> `DialogueSpeaker` - internal / has_field_type / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Dialogue\DialogueLine.cs:15 / DialogueSpeaker`
- `DialogueSO` -> `DialogueLine` - internal / has_field_type / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Dialogue\DialogueSO.cs:13 / List<DialogueLine>`

## External Touchpoints

- `StageSO` -> `DialogueSO` - incoming / has_field_type / 3 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Managers\StageSO.cs:64 / DialogueSO`
- `DialogueViewer` -> `DialogueLine` - incoming / accepts_parameter / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\View\DialogueViewer.cs:122 / DialogueLine`
- `DialogueViewer` -> `DialogueSO` - incoming / accepts_parameter / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\View\DialogueViewer.cs:96 / DialogueSO`
- `StageManager` -> `DialogueSO` - incoming / accepts_parameter / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Managers\StageManager.cs:122 / DialogueSO`
- `DialogueTypingSettings` -> `GameSettings` - outgoing / calls_member / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Dialogue\DialogueTypingSettings.cs:32 / GameSettings.ApplySfxVolume(settings.output.audioMixer)`
- `DialogueViewer` -> `DialogueTypingSettings` - incoming / calls_member / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\View\DialogueViewer.cs:72 / DialogueTypingSettings.Load()`
- `StageSO` -> `DialogueSO` - incoming / creates / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Managers\StageSO.cs:70 / List<DialogueSO>`
- `DialogueViewer` -> `DialogueTypingSettings` - incoming / has_field_type / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\View\DialogueViewer.cs:31 / DialogueTypingSettings`
- `RoundManager` -> `DialogueSO` - incoming / returns / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Managers\RoundManager.cs:867 / DialogueSO`

## Internal Method Calls

- None detected.

## Evidence

- Likely flow - DialogueTypingSettings.Load() -> 
- incoming has_field_type - StageSO -> DialogueSO / 3 refs
  - `H:\Unity\Project-Memories\Assets\Scripts\Managers\StageSO.cs:64 / DialogueSO`
- incoming accepts_parameter - DialogueViewer -> DialogueLine / 1 refs
  - `H:\Unity\Project-Memories\Assets\Scripts\View\DialogueViewer.cs:122 / DialogueLine`
- incoming accepts_parameter - DialogueViewer -> DialogueSO / 1 refs
  - `H:\Unity\Project-Memories\Assets\Scripts\View\DialogueViewer.cs:96 / DialogueSO`
- Internal creates - DialogueSO -> DialogueLine / 1 refs
  - `H:\Unity\Project-Memories\Assets\Scripts\Dialogue\DialogueSO.cs:13 / List<DialogueLine>`
- Internal has_field_type - DialogueLine -> DialogueSpeaker / 1 refs
  - `H:\Unity\Project-Memories\Assets\Scripts\Dialogue\DialogueLine.cs:15 / DialogueSpeaker`
- Internal has_field_type - DialogueSO -> DialogueLine / 1 refs
  - `H:\Unity\Project-Memories\Assets\Scripts\Dialogue\DialogueSO.cs:13 / List<DialogueLine>`

## Suggested AI Task

Use the Dialogue context to explain the reading order, likely runtime flow, and risky assumptions. Cite method names, relationship edges, and file references when possible.

