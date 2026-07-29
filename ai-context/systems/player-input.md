# Player / Input

Player / Input appears to be an externally connected area around player, beat, memories, character, action. It contains 6 types, including 3 Unity-facing types.

## Stats

- Types: 6
- Internal relationships: 9
- External relationships: 43
- Entry candidates: 8
- Keywords: `player`, `beat`, `memories`, `character`, `action`, `camera`, `input`, `key`, `mode`, `reader`, `sway`, `timed`

## Start Here

- `CameraSway.Start()` - unity_lifecycle / H:\Unity\Project-Memories\Assets\Scripts\View\CameraSway.cs:33
- `InputReader.Awake()` - unity_lifecycle / H:\Unity\Project-Memories\Assets\Scripts\Player\InputReader.cs:57
- `InputReader.OnEnable()` - unity_lifecycle / H:\Unity\Project-Memories\Assets\Scripts\Player\InputReader.cs:71
- `InputReader.OnDisable()` - unity_lifecycle / H:\Unity\Project-Memories\Assets\Scripts\Player\InputReader.cs:79
- `PlayerData.Awake()` - unity_lifecycle / H:\Unity\Project-Memories\Assets\Scripts\Player\PlayerData.cs:84
- `InputReader.OnDestroy()` - unity_lifecycle / H:\Unity\Project-Memories\Assets\Scripts\Player\InputReader.cs:86
- `PlayerData.OnEnable()` - unity_lifecycle / H:\Unity\Project-Memories\Assets\Scripts\Player\PlayerData.cs:91
- `PlayerData.OnDisable()` - unity_lifecycle / H:\Unity\Project-Memories\Assets\Scripts\Player\PlayerData.cs:92

## Core Types

- `TimedPlayerAction` - struct / 2 out / 16 in / H:\Unity\Project-Memories\Assets\Scripts\Player\InputReader.cs:18
- `PlayerData` - class / Unity / 8 out / 9 in / H:\Unity\Project-Memories\Assets\Scripts\Player\PlayerData.cs:12
- `InputReader` - class / Unity / 10 out / 5 in / H:\Unity\Project-Memories\Assets\Scripts\Player\InputReader.cs:37
- `PlayerCharacterData` - class / 2 out / 5 in / H:\Unity\Project-Memories\Assets\Scripts\Character\PlayerCharacterData.cs:9
- `KeyMode` - enum / 0 out / 3 in / H:\Unity\Project-Memories\Assets\Scripts\Player\InputReader.cs:9
- `CameraSway` - class / Unity / 0 out / 1 in / H:\Unity\Project-Memories\Assets\Scripts\View\CameraSway.cs:11

## Likely Method Flows

- `PlayerData.Awake()`
  - `PlayerData.Awake()`
  - `PlayerData.ResetState() / terminal`
- `CameraSway.Start()`
  - `CameraSway.Start() / terminal`
- `InputReader.Awake()`
  - `InputReader.Awake() / terminal`
- `InputReader.OnEnable()`
  - `InputReader.OnEnable() / terminal`
- `InputReader.OnDisable()`
  - `InputReader.OnDisable() / terminal`
- `InputReader.OnDestroy()`
  - `InputReader.OnDestroy() / terminal`
- `PlayerData.OnEnable()`
  - `PlayerData.OnEnable() / terminal`
- `PlayerData.OnDisable()`
  - `PlayerData.OnDisable() / terminal`

## Internal Type Relationships

- `PlayerData` -> `PlayerCharacterData` - internal / accepts_parameter / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Player\PlayerData.cs:76 / PlayerCharacterData`
- `PlayerData` -> `PlayerCharacterData` - internal / calls_member / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Player\PlayerData.cs:147 / characterData.SpritesFor(action)`
- `InputReader` -> `TimedPlayerAction` - internal / creates / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Player\InputReader.cs:115 / TimedPlayerAction`
- `InputReader` -> `TimedPlayerAction` - internal / has_event_type / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Player\InputReader.cs:49 / Action<TimedPlayerAction>`
- `InputReader` -> `KeyMode` - internal / has_field_type / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Player\InputReader.cs:41 / KeyMode`
- `PlayerData` -> `InputReader` - internal / has_field_type / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Player\PlayerData.cs:38 / InputReader`
- `PlayerData` -> `PlayerCharacterData` - internal / has_field_type / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Player\PlayerData.cs:16 / PlayerCharacterData`
- `InputReader` -> `KeyMode` - internal / has_property_type / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Player\InputReader.cs:44 / KeyMode`
- `PlayerData` -> `PlayerCharacterData` - internal / has_property_type / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Player\PlayerData.cs:48 / PlayerCharacterData`

## External Touchpoints

- `RoundManager` -> `TimedPlayerAction` - incoming / calls_member / 10 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Managers\RoundManager.cs:179 / pendingInputs.Clear()`
- `InputReader` -> `PlayerAction` - outgoing / accepts_parameter / 4 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Player\InputReader.cs:101 / PlayerAction`
- `RoundManager` -> `PlayerData` - incoming / calls_member / 4 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Managers\RoundManager.cs:162 / player.SetMaxHp(s.playerMaxHp)`
- `PlayerData` -> `PlayerAction` - outgoing / accepts_parameter / 2 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Player\PlayerData.cs:133 / PlayerAction`
- `RoundManager` -> `TimedPlayerAction` - incoming / accepts_parameter / 2 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Managers\RoundManager.cs:368 / TimedPlayerAction`
- `ActionButton` -> `InputReader` - incoming / calls_member / 2 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\View\ActionButton.cs:63 / input.IsActionAvailable(action)`
- `InputReader` -> `PlayerAction` - outgoing / has_event_type / 2 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Player\InputReader.cs:47 / Action<PlayerAction>`
- `PlayerCharacterData` -> `PlayerAction` - outgoing / accepts_parameter / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Character\PlayerCharacterData.cs:38 / PlayerAction`
- `TimedPlayerAction` -> `PlayerAction` - outgoing / accepts_parameter / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Player\InputReader.cs:24 / PlayerAction`
- `StageManager` -> `PlayerData` - incoming / calls_member / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Managers\StageManager.cs:409 / playerData.SetCharacterData(definition)`
- `RoundManager` -> `TimedPlayerAction` - incoming / creates / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Managers\RoundManager.cs:115 / Queue<TimedPlayerAction>`
- `PlayerData` -> `PlayerAction` - outgoing / has_event_type / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Player\PlayerData.cs:71 / Action<PlayerAction, Sprite>`
- `ActionButton` -> `InputReader` - incoming / has_field_type / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\View\ActionButton.cs:15 / InputReader`
- `HudView` -> `CameraSway` - incoming / has_field_type / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\View\HudView.cs:25 / CameraSway`
- `HudView` -> `PlayerData` - incoming / has_field_type / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\View\HudView.cs:23 / PlayerData`
- `RoundManager` -> `InputReader` - incoming / has_field_type / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Managers\RoundManager.cs:33 / InputReader`
- `RoundManager` -> `PlayerData` - incoming / has_field_type / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Managers\RoundManager.cs:32 / PlayerData`
- `RoundManager` -> `TimedPlayerAction` - incoming / has_field_type / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Managers\RoundManager.cs:115 / Queue<TimedPlayerAction>`
- `StageManager` -> `PlayerData` - incoming / has_field_type / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Managers\StageManager.cs:37 / PlayerData`
- `StageSO` -> `KeyMode` - incoming / has_field_type / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Managers\StageSO.cs:19 / KeyMode`
- `TimedPlayerAction` -> `PlayerAction` - outgoing / has_field_type / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Player\InputReader.cs:20 / PlayerAction`
- `PlayerCharacterData` -> `CharacterData` - outgoing / inherits / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Character\PlayerCharacterData.cs:9 / CharacterData`
- `StageManager` -> `PlayerCharacterData` - incoming / type_check / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Managers\StageManager.cs:407 / PlayerCharacterData`
- `StageManager` -> `PlayerData` - incoming / unity_find_object / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Managers\StageManager.cs:382 / PlayerData`

## Internal Method Calls

- `PlayerData.SetCharacterData(PlayerCharacterData)` -> `PlayerData.ResetState()` / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Player\PlayerData.cs:81 / ResetState()`
- `PlayerData.Awake()` -> `PlayerData.ResetState()` / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Player\PlayerData.cs:88 / ResetState()`
- `InputReader.OnGuard(InputAction.CallbackContext)` -> `InputReader.Emit(PlayerAction, double)` / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Player\InputReader.cs:93 / Emit(PlayerAction.Guard, context.time)`
- `InputReader.OnAttack(InputAction.CallbackContext)` -> `InputReader.Emit(PlayerAction, double)` / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Player\InputReader.cs:94 / Emit(PlayerAction.Attack, context.time)`
- `InputReader.OnCharge(InputAction.CallbackContext)` -> `InputReader.Emit(PlayerAction, double)` / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Player\InputReader.cs:97 / Emit(PlayerAction.Charge, context.time)`
- `PlayerData.SetMaxHp(int)` -> `PlayerData.ResetState()` / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Player\PlayerData.cs:106 / ResetState()`
- `InputReader.Press(PlayerAction)` -> `InputReader.Emit(PlayerAction, double)` / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Player\InputReader.cs:109 / Emit(action, InputState.currentTime)`
- `InputReader.Press(PlayerAction)` -> `InputReader.IsActionAvailable(PlayerAction)` / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Player\InputReader.cs:108 / IsActionAvailable(action)`
- `PlayerData.ConsumeCharge()` -> `PlayerData.SetCharged(bool)` / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Player\PlayerData.cs:128 / SetCharged(false)`
- `PlayerData.HandleAction(PlayerAction)` -> `PlayerData.SpritesFor(PlayerAction)` / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Player\PlayerData.cs:135 / SpritesFor(action)`
- `PlayerData.SpritesFor(PlayerAction)` -> `PlayerCharacterData.SpritesFor(PlayerAction)` / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Player\PlayerData.cs:147 / characterData.SpritesFor(action)`

## Evidence

- Likely flow - PlayerData.Awake() -> PlayerData.ResetState() / terminal
- Likely flow - CameraSway.Start() -> 
- Internal call - PlayerData.SetCharacterData(PlayerCharacterData) -> PlayerData.ResetState()
  - `H:\Unity\Project-Memories\Assets\Scripts\Player\PlayerData.cs:81 / ResetState()`
- Internal call - PlayerData.Awake() -> PlayerData.ResetState()
  - `H:\Unity\Project-Memories\Assets\Scripts\Player\PlayerData.cs:88 / ResetState()`
- Internal call - InputReader.OnGuard(InputAction.CallbackContext) -> InputReader.Emit(PlayerAction, double)
  - `H:\Unity\Project-Memories\Assets\Scripts\Player\InputReader.cs:93 / Emit(PlayerAction.Guard, context.time)`
- incoming calls_member - RoundManager -> TimedPlayerAction / 10 refs
  - `H:\Unity\Project-Memories\Assets\Scripts\Managers\RoundManager.cs:179 / pendingInputs.Clear()`
- outgoing accepts_parameter - InputReader -> PlayerAction / 4 refs
  - `H:\Unity\Project-Memories\Assets\Scripts\Player\InputReader.cs:101 / PlayerAction`
- incoming calls_member - RoundManager -> PlayerData / 4 refs
  - `H:\Unity\Project-Memories\Assets\Scripts\Managers\RoundManager.cs:162 / player.SetMaxHp(s.playerMaxHp)`
- Internal accepts_parameter - PlayerData -> PlayerCharacterData / 1 refs
  - `H:\Unity\Project-Memories\Assets\Scripts\Player\PlayerData.cs:76 / PlayerCharacterData`
- Internal calls_member - PlayerData -> PlayerCharacterData / 1 refs
  - `H:\Unity\Project-Memories\Assets\Scripts\Player\PlayerData.cs:147 / characterData.SpritesFor(action)`
- Internal creates - InputReader -> TimedPlayerAction / 1 refs
  - `H:\Unity\Project-Memories\Assets\Scripts\Player\InputReader.cs:115 / TimedPlayerAction`
- Internal has_event_type - InputReader -> TimedPlayerAction / 1 refs
  - `H:\Unity\Project-Memories\Assets\Scripts\Player\InputReader.cs:49 / Action<TimedPlayerAction>`
- Internal has_field_type - InputReader -> KeyMode / 1 refs
  - `H:\Unity\Project-Memories\Assets\Scripts\Player\InputReader.cs:41 / KeyMode`
- Internal has_field_type - PlayerData -> InputReader / 1 refs
  - `H:\Unity\Project-Memories\Assets\Scripts\Player\PlayerData.cs:38 / InputReader`
- Internal has_field_type - PlayerData -> PlayerCharacterData / 1 refs
  - `H:\Unity\Project-Memories\Assets\Scripts\Player\PlayerData.cs:16 / PlayerCharacterData`
- Internal has_property_type - InputReader -> KeyMode / 1 refs
  - `H:\Unity\Project-Memories\Assets\Scripts\Player\InputReader.cs:44 / KeyMode`

## Suggested AI Task

Use the Player / Input context to explain the reading order, likely runtime flow, and risky assumptions. Cite method names, relationship edges, and file references when possible.

