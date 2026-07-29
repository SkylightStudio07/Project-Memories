# Action Event Pipeline

Action Event Pipeline appears to be an externally connected area around beat, memories, action, effect, aura. It contains 6 types, including 3 Unity-facing types.

## Stats

- Types: 6
- Internal relationships: 4
- External relationships: 58
- Entry candidates: 8
- Keywords: `beat`, `memories`, `action`, `effect`, `aura`, `button`, `charge`, `death`, `enemy`, `explosion`, `outcome`, `player`

## Start Here

- `DeathExplosionEffect.Awake()` - unity_lifecycle / H:\Unity\Project-Memories\Assets\Scripts\View\DeathExplosionEffect.cs:27
- `ChargeAuraEffect.Awake()` - unity_lifecycle / H:\Unity\Project-Memories\Assets\Scripts\View\ChargeAuraEffect.cs:33
- `ActionButton.Awake()` - unity_lifecycle / H:\Unity\Project-Memories\Assets\Scripts\View\ActionButton.cs:34
- `ChargeAuraEffect.LateUpdate()` - unity_lifecycle / H:\Unity\Project-Memories\Assets\Scripts\View\ChargeAuraEffect.cs:39
- `ActionButton.OnEnable()` - unity_lifecycle / H:\Unity\Project-Memories\Assets\Scripts\View\ActionButton.cs:43
- `ChargeAuraEffect.OnDisable()` - unity_lifecycle / H:\Unity\Project-Memories\Assets\Scripts\View\ChargeAuraEffect.cs:44
- `ChargeAuraEffect.Initialize(SpriteRenderer, Color)` - flow_candidate / H:\Unity\Project-Memories\Assets\Scripts\View\ChargeAuraEffect.cs:49
- `ChargeAuraEffect.InitializeRing()` - flow_candidate / H:\Unity\Project-Memories\Assets\Scripts\View\ChargeAuraEffect.cs:251

## Core Types

- `PlayerAction` - enum / 0 out / 40 in / H:\Unity\Project-Memories\Assets\Scripts\Core\PlayerAction.cs:8
- `ActionButton` - class / Unity / 8 out / 0 in / H:\Unity\Project-Memories\Assets\Scripts\View\ActionButton.cs:13
- `ActionOutcome` - class / 2 out / 5 in / H:\Unity\Project-Memories\Assets\Scripts\Enemy\ActionOutcome.cs:11
- `ChargeAuraEffect` - class / Unity / 0 out / 7 in / H:\Unity\Project-Memories\Assets\Scripts\View\ChargeAuraEffect.cs:11
- `ActionWeight` - class / 1 out / 3 in / H:\Unity\Project-Memories\Assets\Scripts\Sequence\PhaseSO.cs:13
- `DeathExplosionEffect` - class / Unity / 0 out / 0 in / H:\Unity\Project-Memories\Assets\Scripts\View\DeathExplosionEffect.cs:12

## Likely Method Flows

- `ChargeAuraEffect.Initialize(SpriteRenderer, Color)`
  - `ChargeAuraEffect.Initialize(SpriteRenderer, Color)`
  - `ChargeAuraEffect.StopImmediate() / terminal`
- `ChargeAuraEffect.Awake()`
  - `ChargeAuraEffect.Awake()`
  - `ChargeAuraEffect.StopImmediate() / terminal`
- `ChargeAuraEffect.LateUpdate()`
  - `ChargeAuraEffect.LateUpdate()`
  - `ChargeAuraEffect.FollowPlayer() / terminal`
- `ActionButton.OnEnable()`
  - `ActionButton.OnEnable()`
  - `ActionButton.RefreshAvailability() / terminal`
- `ChargeAuraEffect.OnDisable()`
  - `ChargeAuraEffect.OnDisable()`
  - `ChargeAuraEffect.StopImmediate() / terminal`
- `ChargeAuraEffect.InitializeRing()`
  - `ChargeAuraEffect.InitializeRing()`
  - `ChargeAuraEffect.SetRingColor(float) / terminal`
- `DeathExplosionEffect.Awake()`
  - `DeathExplosionEffect.Awake() / terminal`
- `ActionButton.Awake()`
  - `ActionButton.Awake() / terminal`

## Internal Type Relationships

- `ActionButton` -> `PlayerAction` - internal / accepts_parameter / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\View\ActionButton.cs:79 / PlayerAction`
- `ActionButton` -> `PlayerAction` - internal / has_field_type / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\View\ActionButton.cs:16 / PlayerAction`
- `ActionOutcome` -> `PlayerAction` - internal / has_field_type / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Enemy\ActionOutcome.cs:14 / PlayerAction`
- `ActionWeight` -> `PlayerAction` - internal / has_field_type / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Sequence\PhaseSO.cs:15 / PlayerAction`

## External Touchpoints

- `InputReader` -> `PlayerAction` - incoming / accepts_parameter / 4 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Player\InputReader.cs:101 / PlayerAction`
- `HudView` -> `ChargeAuraEffect` - incoming / calls_member / 4 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\View\HudView.cs:1488 / chargeAura.Initialize(playerSlot, playerLaserColor)`
- `EnemySequenceProvider` -> `PlayerAction` - incoming / calls_member / 3 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Sequence\EnemySequenceProvider.cs:151 / byAnswer.TryGetValue(ans, out var list)`
- `PlayerData` -> `PlayerAction` - incoming / accepts_parameter / 2 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Player\PlayerData.cs:133 / PlayerAction`
- `RoundManager` -> `PlayerAction` - incoming / accepts_parameter / 2 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Managers\RoundManager.cs:514 / PlayerAction`
- `ActionButton` -> `InputReader` - outgoing / calls_member / 2 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\View\ActionButton.cs:63 / input.IsActionAvailable(action)`
- `EnemyData` -> `ActionOutcome` - incoming / creates / 2 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Enemy\EnemyData.cs:48 / List<ActionOutcome>`
- `EnemySequenceProvider` -> `PlayerAction` - incoming / creates / 2 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Sequence\EnemySequenceProvider.cs:18 / List<PlayerAction>`
- `InputReader` -> `PlayerAction` - incoming / has_event_type / 2 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Player\InputReader.cs:47 / Action<PlayerAction>`
- `EnemyData` -> `ActionOutcome` - incoming / has_field_type / 2 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Enemy\EnemyData.cs:48 / List<ActionOutcome>`
- `EnemySequenceProvider` -> `PlayerAction` - incoming / has_field_type / 2 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Sequence\EnemySequenceProvider.cs:18 / List<PlayerAction>`
- `HudView` -> `ChargeAuraEffect` - incoming / has_field_type / 2 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\View\HudView.cs:141 / ChargeAuraEffect`
- `EnemySequenceProvider` -> `PlayerAction` - incoming / uses_local_type / 2 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Sequence\EnemySequenceProvider.cs:69 / PlayerAction`
- `ActionButton` -> `StageSO` - outgoing / accepts_parameter / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\View\ActionButton.cs:57 / StageSO`
- `Enemy` -> `PlayerAction` - incoming / accepts_parameter / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Enemy\Enemy.cs:39 / PlayerAction`
- `HudView` -> `PlayerAction` - incoming / accepts_parameter / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\View\HudView.cs:849 / PlayerAction`
- `JudgeResult` -> `PlayerAction` - incoming / accepts_parameter / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Judge\JudgeResult.cs:24 / PlayerAction`
- `JudgeSystem` -> `PlayerAction` - incoming / accepts_parameter / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Judge\JudgeSystem.cs:10 / PlayerAction`
- `PhaseSO` -> `PlayerAction` - incoming / accepts_parameter / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Sequence\PhaseSO.cs:68 / PlayerAction`
- `PlayerCharacterData` -> `PlayerAction` - incoming / accepts_parameter / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Character\PlayerCharacterData.cs:38 / PlayerAction`
- `TimedPlayerAction` -> `PlayerAction` - incoming / accepts_parameter / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Player\InputReader.cs:24 / PlayerAction`
- `PhaseSO` -> `ActionWeight` - incoming / creates / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Sequence\PhaseSO.cs:38 / List<ActionWeight>`
- `PlayerData` -> `PlayerAction` - incoming / has_event_type / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Player\PlayerData.cs:71 / Action<PlayerAction, Sprite>`
- `ActionButton` -> `InputReader` - outgoing / has_field_type / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\View\ActionButton.cs:15 / InputReader`
- `ActionButton` -> `RoundManager` - outgoing / has_field_type / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\View\ActionButton.cs:21 / RoundManager`
- `ActionOutcome` -> `OutcomeType` - outgoing / has_field_type / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Enemy\ActionOutcome.cs:17 / OutcomeType`
- `EnemyData` -> `PlayerAction` - incoming / has_field_type / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Enemy\EnemyData.cs:24 / PlayerAction`
- `JudgeResult` -> `PlayerAction` - incoming / has_field_type / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Judge\JudgeResult.cs:10 / PlayerAction`
- `PhaseSO` -> `ActionWeight` - incoming / has_field_type / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Sequence\PhaseSO.cs:38 / List<ActionWeight>`
- `PhaseSO` -> `PlayerAction` - incoming / has_field_type / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Sequence\PhaseSO.cs:37 / PlayerAction`
- `TimedPlayerAction` -> `PlayerAction` - incoming / has_field_type / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Player\InputReader.cs:20 / PlayerAction`
- `Enemy` -> `PlayerAction` - incoming / has_property_type / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Enemy\Enemy.cs:21 / PlayerAction`
- `PhaseSO` -> `ActionWeight` - incoming / has_property_type / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Sequence\PhaseSO.cs:58 / IReadOnlyList<ActionWeight>`
- `PhaseSO` -> `PlayerAction` - incoming / has_property_type / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Sequence\PhaseSO.cs:57 / PlayerAction`
- `Enemy` -> `ActionOutcome` - incoming / returns / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Enemy\Enemy.cs:39 / ActionOutcome`
- `Enemy` -> `PlayerAction` - incoming / returns / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Enemy\Enemy.cs:54 / PlayerAction`

## Internal Method Calls

- `ChargeAuraEffect.PlayReadyRing()` -> `ChargeAuraEffect.SetRingColor(float)` / 2 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\View\ChargeAuraEffect.cs:280 / SetRingColor(alpha)`
- `ChargeAuraEffect.Awake()` -> `ChargeAuraEffect.CacheReferences()` / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\View\ChargeAuraEffect.cs:35 / CacheReferences()`
- `ChargeAuraEffect.Awake()` -> `ChargeAuraEffect.StopImmediate()` / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\View\ChargeAuraEffect.cs:36 / StopImmediate()`
- `ChargeAuraEffect.LateUpdate()` -> `ChargeAuraEffect.FollowPlayer()` / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\View\ChargeAuraEffect.cs:41 / FollowPlayer()`
- `ActionButton.OnEnable()` -> `ActionButton.RefreshAvailability()` / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\View\ActionButton.cs:47 / RefreshAvailability()`
- `ChargeAuraEffect.OnDisable()` -> `ChargeAuraEffect.StopImmediate()` / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\View\ChargeAuraEffect.cs:46 / StopImmediate()`
- `ChargeAuraEffect.Initialize(SpriteRenderer, Color)` -> `ChargeAuraEffect.ApplyColor()` / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\View\ChargeAuraEffect.cs:54 / ApplyColor()`
- `ChargeAuraEffect.Initialize(SpriteRenderer, Color)` -> `ChargeAuraEffect.CacheReferences()` / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\View\ChargeAuraEffect.cs:53 / CacheReferences()`
- `ChargeAuraEffect.Initialize(SpriteRenderer, Color)` -> `ChargeAuraEffect.FollowPlayer()` / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\View\ChargeAuraEffect.cs:56 / FollowPlayer()`
- `ChargeAuraEffect.Initialize(SpriteRenderer, Color)` -> `ChargeAuraEffect.InitializeRing()` / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\View\ChargeAuraEffect.cs:55 / InitializeRing()`
- `ChargeAuraEffect.Initialize(SpriteRenderer, Color)` -> `ChargeAuraEffect.StopImmediate()` / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\View\ChargeAuraEffect.cs:57 / StopImmediate()`
- `ActionButton.OnStageApplied(StageSO)` -> `ActionButton.RefreshAvailability()` / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\View\ActionButton.cs:57 / RefreshAvailability()`
- `ChargeAuraEffect.SetReady(bool)` -> `ChargeAuraEffect.FollowPlayer()` / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\View\ChargeAuraEffect.cs:68 / FollowPlayer()`
- `ChargeAuraEffect.SetReady(bool)` -> `ChargeAuraEffect.PlayGlow()` / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\View\ChargeAuraEffect.cs:70 / PlayGlow()`
- `ChargeAuraEffect.SetReady(bool)` -> `ChargeAuraEffect.PlayParticles()` / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\View\ChargeAuraEffect.cs:69 / PlayParticles()`
- `ChargeAuraEffect.SetReady(bool)` -> `ChargeAuraEffect.PlayReadyRing()` / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\View\ChargeAuraEffect.cs:71 / PlayReadyRing()`
- `ChargeAuraEffect.SetReady(bool)` -> `ChargeAuraEffect.StopGlow()` / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\View\ChargeAuraEffect.cs:76 / StopGlow()`
- `ChargeAuraEffect.SetReady(bool)` -> `ChargeAuraEffect.StopParticles()` / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\View\ChargeAuraEffect.cs:75 / StopParticles()`
- `ChargeAuraEffect.SetReady(bool)` -> `ChargeAuraEffect.StopReadyRing()` / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\View\ChargeAuraEffect.cs:77 / StopReadyRing()`
- `ActionButton.OnPointerDown(PointerEventData)` -> `ActionButton.OnClick()` / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\View\ActionButton.cs:75 / OnClick()`
- `ChargeAuraEffect.CacheReferences()` -> `ChargeAuraEffect.ConfigureRuntimeParticles()` / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\View\ChargeAuraEffect.cs:113 / ConfigureRuntimeParticles()`
- `ChargeAuraEffect.ApplyColor()` -> `ChargeAuraEffect.SetRingColor(float)` / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\View\ChargeAuraEffect.cs:168 / SetRingColor(0f)`
- `ChargeAuraEffect.InitializeRing()` -> `ChargeAuraEffect.SetRingColor(float)` / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\View\ChargeAuraEffect.cs:267 / SetRingColor(0f)`

## Evidence

- Likely flow - ChargeAuraEffect.Initialize(SpriteRenderer, Color) -> ChargeAuraEffect.StopImmediate() / terminal
- Likely flow - ChargeAuraEffect.Awake() -> ChargeAuraEffect.StopImmediate() / terminal
- Internal call - ChargeAuraEffect.PlayReadyRing() -> ChargeAuraEffect.SetRingColor(float)
  - `H:\Unity\Project-Memories\Assets\Scripts\View\ChargeAuraEffect.cs:280 / SetRingColor(alpha)`
- Internal call - ChargeAuraEffect.Awake() -> ChargeAuraEffect.CacheReferences()
  - `H:\Unity\Project-Memories\Assets\Scripts\View\ChargeAuraEffect.cs:35 / CacheReferences()`
- Internal call - ChargeAuraEffect.Awake() -> ChargeAuraEffect.StopImmediate()
  - `H:\Unity\Project-Memories\Assets\Scripts\View\ChargeAuraEffect.cs:36 / StopImmediate()`
- incoming accepts_parameter - InputReader -> PlayerAction / 4 refs
  - `H:\Unity\Project-Memories\Assets\Scripts\Player\InputReader.cs:101 / PlayerAction`
- incoming calls_member - HudView -> ChargeAuraEffect / 4 refs
  - `H:\Unity\Project-Memories\Assets\Scripts\View\HudView.cs:1488 / chargeAura.Initialize(playerSlot, playerLaserColor)`
- incoming calls_member - EnemySequenceProvider -> PlayerAction / 3 refs
  - `H:\Unity\Project-Memories\Assets\Scripts\Sequence\EnemySequenceProvider.cs:151 / byAnswer.TryGetValue(ans, out var list)`
- Internal accepts_parameter - ActionButton -> PlayerAction / 1 refs
  - `H:\Unity\Project-Memories\Assets\Scripts\View\ActionButton.cs:79 / PlayerAction`
- Internal has_field_type - ActionButton -> PlayerAction / 1 refs
  - `H:\Unity\Project-Memories\Assets\Scripts\View\ActionButton.cs:16 / PlayerAction`
- Internal has_field_type - ActionOutcome -> PlayerAction / 1 refs
  - `H:\Unity\Project-Memories\Assets\Scripts\Enemy\ActionOutcome.cs:14 / PlayerAction`
- Internal has_field_type - ActionWeight -> PlayerAction / 1 refs
  - `H:\Unity\Project-Memories\Assets\Scripts\Sequence\PhaseSO.cs:15 / PlayerAction`

## Suggested AI Task

Use the Action Event Pipeline context to explain the reading order, likely runtime flow, and risky assumptions. Cite method names, relationship edges, and file references when possible.

