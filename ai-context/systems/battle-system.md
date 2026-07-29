# Battle System

Battle System appears to be an externally connected area around beat, memories, enemy, sequence, combat. It contains 13 types, including 4 Unity-facing types.

## Stats

- Types: 13
- Internal relationships: 56
- External relationships: 88
- Entry candidates: 8
- Keywords: `beat`, `memories`, `enemy`, `sequence`, `combat`, `phase`, `rhythm`, `timeline`, `balance`, `banner`, `cue`, `position`

## Start Here

- `PhasePresentationController.Awake()` - unity_lifecycle / H:\Unity\Project-Memories\Assets\Scripts\View\PhasePresentationController.cs:45
- `PhaseBanner.Start()` - unity_lifecycle / H:\Unity\Project-Memories\Assets\Scripts\View\PhaseBanner.cs:45
- `PhasePresentationController.Start()` - unity_lifecycle / H:\Unity\Project-Memories\Assets\Scripts\View\PhasePresentationController.cs:114
- `PhasePresentationController.OnDisable()` - unity_lifecycle / H:\Unity\Project-Memories\Assets\Scripts\View\PhasePresentationController.cs:152
- `EnemySequenceProvider.GenerateCycle(int, int)` - flow_candidate / H:\Unity\Project-Memories\Assets\Scripts\Sequence\EnemySequenceProvider.cs:40
- `EnemySequenceProvider.GenerateCycleWeighted(int, int, PhaseSO)` - flow_candidate / H:\Unity\Project-Memories\Assets\Scripts\Sequence\EnemySequenceProvider.cs:82
- `CombatTimeline.StartsAfterResponse(int, int, int)` - flow_candidate / H:\Unity\Project-Memories\Assets\Scripts\Rhythm\CombatTimeline.cs:111
- `EnemySequenceProvider.BuildGroups()` - flow_candidate / H:\Unity\Project-Memories\Assets\Scripts\Sequence\EnemySequenceProvider.cs:144

## Core Types

- `Enemy` - class / 8 out / 57 in / H:\Unity\Project-Memories\Assets\Scripts\Enemy\Enemy.cs:11
- `EnemySequenceProvider` - class / 38 out / 3 in / H:\Unity\Project-Memories\Assets\Scripts\Sequence\EnemySequenceProvider.cs:12
- `PhaseSO` - class / Unity / 16 out / 25 in / H:\Unity\Project-Memories\Assets\Scripts\Sequence\PhaseSO.cs:31
- `PhasePresentationController` - class / Unity / 11 out / 0 in / H:\Unity\Project-Memories\Assets\Scripts\View\PhasePresentationController.cs:12
- `EnemyData` - class / 7 out / 3 in / H:\Unity\Project-Memories\Assets\Scripts\Enemy\EnemyData.cs:12
- `EnemyWeight` - class / 1 out / 6 in / H:\Unity\Project-Memories\Assets\Scripts\Sequence\PhaseSO.cs:22
- `CombatTimelinePosition` - struct / 2 out / 4 in / H:\Unity\Project-Memories\Assets\Scripts\Rhythm\CombatTimeline.cs:13
- `EnemyPreviewCue` - struct / 2 out / 4 in / H:\Unity\Project-Memories\Assets\Scripts\Sequence\EnemyPreviewCue.cs:7
- `CombatTimeline` - class / 5 out / 0 in / H:\Unity\Project-Memories\Assets\Scripts\Rhythm\CombatTimeline.cs:53
- `CombatSection` - enum / 0 out / 3 in / H:\Unity\Project-Memories\Assets\Scripts\Rhythm\CombatTimeline.cs:5
- `CombatBalanceSettings` - class / Unity / 0 out / 2 in / H:\Unity\Project-Memories\Assets\Scripts\Core\CombatBalanceSettings.cs:6
- `PhaseKind` - enum / 0 out / 2 in / H:\Unity\Project-Memories\Assets\Scripts\Sequence\PhaseSO.cs:6
- `PhaseBanner` - class / Unity / 1 out / 0 in / H:\Unity\Project-Memories\Assets\Scripts\View\PhaseBanner.cs:11

## Likely Method Flows

- `PhasePresentationController.Awake()`
  - `PhasePresentationController.Awake()`
  - `PhasePresentationController.CreateFallbackSnare() / terminal`
- `EnemySequenceProvider.GenerateCycleWeighted(int, int, PhaseSO)`
  - `EnemySequenceProvider.GenerateCycleWeighted(int, int, PhaseSO)`
  - `EnemySequenceProvider.GetWeighted(int, int, PhaseSO)`
  - `EnemySequenceProvider.Get(int, int)`
  - `EnemySequenceProvider.Hash(uint, uint, uint) / terminal`
- `PhaseBanner.Start()`
  - `PhaseBanner.Start()`
  - `PhaseBanner.OnPreparation(int)`
  - `PhaseBanner.Set(Sprite) / terminal`
- `PhasePresentationController.Start()`
  - `PhasePresentationController.Start()`
  - `PhasePresentationController.CalculateBackgroundMusicStartDspTime(double, float) / terminal`
- `PhasePresentationController.OnDisable()`
  - `PhasePresentationController.OnDisable()`
  - `PhasePresentationController.RestoreImmediately() / terminal`
- `EnemySequenceProvider.GenerateCycle(int, int)`
  - `EnemySequenceProvider.GenerateCycle(int, int)`
  - `EnemySequenceProvider.Get(int, int)`
  - `EnemySequenceProvider.Hash(uint, uint, uint) / terminal`
- `CombatTimeline.StartsAfterResponse(int, int, int)`
  - `CombatTimeline.StartsAfterResponse(int, int, int)`
  - `CombatTimeline.Resolve(int, int, int)`
  - `CombatTimeline.BeatsPerPhase(int, int)`
  - `CombatTimeline.Validate(int, int) / terminal`
- `EnemySequenceProvider.BuildGroups()`
  - `EnemySequenceProvider.BuildGroups()`
  - `EnemySequenceProvider.PrimaryAnswer(Enemy) / terminal`

## Internal Type Relationships

- `EnemySequenceProvider` -> `Enemy` - internal / returns / 5 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Sequence\EnemySequenceProvider.cs:33 / Enemy`
- `EnemySequenceProvider` -> `Enemy` - internal / calls_member / 4 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Sequence\EnemySequenceProvider.cs:44 / list.Add(Get(cycleIndex, i))`
- `EnemySequenceProvider` -> `Enemy` - internal / creates / 4 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Sequence\EnemySequenceProvider.cs:19 / Dictionary<PlayerAction, List<Enemy>>`
- `EnemySequenceProvider` -> `PhaseSO` - internal / accepts_parameter / 3 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Sequence\EnemySequenceProvider.cs:55 / PhaseSO`
- `PhaseSO` -> `Enemy` - internal / accepts_parameter / 3 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Sequence\PhaseSO.cs:77 / Enemy`
- `EnemySequenceProvider` -> `Enemy` - internal / uses_local_type / 3 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Sequence\EnemySequenceProvider.cs:89 / Enemy`
- `EnemySequenceProvider` -> `EnemyWeight` - internal / uses_local_type / 3 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Sequence\EnemySequenceProvider.cs:109 / IReadOnlyList<EnemyWeight>`
- `EnemySequenceProvider` -> `Enemy` - internal / accepts_parameter / 2 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Sequence\EnemySequenceProvider.cs:21 / IReadOnlyList<Enemy>`
- `PhasePresentationController` -> `PhaseSO` - internal / accepts_parameter / 2 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\View\PhasePresentationController.cs:184 / PhaseSO`
- `EnemySequenceProvider` -> `PhaseSO` - internal / calls_member / 2 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Sequence\EnemySequenceProvider.cs:64 / phase.GetWeight(answerKeys[i])`
- `CombatTimeline` -> `CombatTimelinePosition` - internal / creates / 2 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Rhythm\CombatTimeline.cs:80 / CombatTimelinePosition`
- `EnemyData` -> `Enemy` - internal / has_field_type / 2 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Enemy\EnemyData.cs:33 / Enemy`
- `EnemySequenceProvider` -> `Enemy` - internal / has_field_type / 2 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Sequence\EnemySequenceProvider.cs:15 / IReadOnlyList<Enemy>`
- `CombatTimelinePosition` -> `CombatSection` - internal / accepts_parameter / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Rhythm\CombatTimeline.cs:32 / CombatSection`
- `EnemyPreviewCue` -> `Enemy` - internal / accepts_parameter / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Sequence\EnemyPreviewCue.cs:13 / Enemy`
- `Enemy` -> `EnemyData` - internal / creates / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Enemy\Enemy.cs:13 / EnemyData`
- `PhaseSO` -> `Enemy` - internal / creates / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Sequence\PhaseSO.cs:44 / List<Enemy>`
- `PhaseSO` -> `EnemyWeight` - internal / creates / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Sequence\PhaseSO.cs:41 / List<EnemyWeight>`
- `CombatTimelinePosition` -> `CombatSection` - internal / has_field_type / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Rhythm\CombatTimeline.cs:22 / CombatSection`
- `Enemy` -> `EnemyData` - internal / has_field_type / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Enemy\Enemy.cs:13 / EnemyData`
- `EnemyPreviewCue` -> `Enemy` - internal / has_field_type / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Sequence\EnemyPreviewCue.cs:10 / Enemy`
- `EnemyWeight` -> `Enemy` - internal / has_field_type / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Sequence\PhaseSO.cs:24 / Enemy`
- `PhasePresentationController` -> `PhaseSO` - internal / has_field_type / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\View\PhasePresentationController.cs:36 / PhaseSO`
- `PhaseSO` -> `Enemy` - internal / has_field_type / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Sequence\PhaseSO.cs:44 / List<Enemy>`
- `PhaseSO` -> `EnemyWeight` - internal / has_field_type / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Sequence\PhaseSO.cs:41 / List<EnemyWeight>`
- `PhaseSO` -> `PhaseKind` - internal / has_field_type / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Sequence\PhaseSO.cs:36 / PhaseKind`
- `Enemy` -> `EnemyData` - internal / has_property_type / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Enemy\Enemy.cs:16 / EnemyData`
- `PhaseSO` -> `EnemyWeight` - internal / has_property_type / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Sequence\PhaseSO.cs:59 / IReadOnlyList<EnemyWeight>`
- `PhaseSO` -> `PhaseKind` - internal / has_property_type / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Sequence\PhaseSO.cs:55 / PhaseKind`
- `CombatTimeline` -> `CombatTimelinePosition` - internal / returns / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Rhythm\CombatTimeline.cs:66 / CombatTimelinePosition`
- `CombatTimeline` -> `CombatSection` - internal / uses_local_type / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Rhythm\CombatTimeline.cs:95 / CombatSection`
- `CombatTimeline` -> `CombatTimelinePosition` - internal / uses_local_type / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Rhythm\CombatTimeline.cs:117 / CombatTimelinePosition`

## External Touchpoints

- `HudView` -> `Enemy` - incoming / accepts_parameter / 4 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\View\HudView.cs:455 / Enemy`
- `RoundManager` -> `PhaseSO` - incoming / uses_local_type / 4 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Managers\RoundManager.cs:236 / PhaseSO`
- `RoundManager` -> `Enemy` - incoming / accepts_parameter / 3 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Managers\RoundManager.cs:514 / Enemy`
- `EnemySequenceProvider` -> `PlayerAction` - outgoing / calls_member / 3 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Sequence\EnemySequenceProvider.cs:151 / byAnswer.TryGetValue(ans, out var list)`
- `RoundManager` -> `Enemy` - incoming / creates / 3 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Managers\RoundManager.cs:35 / List<Enemy>`
- `HudView` -> `Enemy` - incoming / uses_local_type / 3 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\View\HudView.cs:438 / Enemy`
- `RoundManager` -> `Enemy` - incoming / calls_member / 2 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Managers\RoundManager.cs:177 / currentCycle.Clear()`
- `EnemyData` -> `ActionOutcome` - outgoing / creates / 2 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Enemy\EnemyData.cs:48 / List<ActionOutcome>`
- `EnemySequenceProvider` -> `PlayerAction` - outgoing / creates / 2 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Sequence\EnemySequenceProvider.cs:18 / List<PlayerAction>`
- `RoundManager` -> `PhaseSO` - incoming / creates / 2 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Managers\RoundManager.cs:45 / List<PhaseSO>`
- `RoundManager` -> `Enemy` - incoming / has_event_type / 2 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Managers\RoundManager.cs:64 / Action<int, Enemy>`
- `RoundManager` -> `PhaseSO` - incoming / has_event_type / 2 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Managers\RoundManager.cs:69 / Action<int, PhaseSO>`
- `EnemyData` -> `ActionOutcome` - outgoing / has_field_type / 2 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Enemy\EnemyData.cs:48 / List<ActionOutcome>`
- `EnemySequenceProvider` -> `PlayerAction` - outgoing / has_field_type / 2 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Sequence\EnemySequenceProvider.cs:18 / List<PlayerAction>`
- `RoundManager` -> `Enemy` - incoming / has_field_type / 2 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Managers\RoundManager.cs:35 / List<Enemy>`
- `EnemySequenceProvider` -> `PlayerAction` - outgoing / uses_local_type / 2 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Sequence\EnemySequenceProvider.cs:69 / PlayerAction`
- `RoundManager` -> `Enemy` - incoming / uses_local_type / 2 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Managers\RoundManager.cs:317 / Enemy`
- `BossPagePresentationController` -> `EnemyPreviewCue` - incoming / accepts_parameter / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\View\BossPagePresentationController.cs:146 / EnemyPreviewCue`
- `BossPagePresentationController` -> `PhaseSO` - incoming / accepts_parameter / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\View\BossPagePresentationController.cs:132 / PhaseSO`
- `Enemy` -> `PlayerAction` - outgoing / accepts_parameter / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Enemy\Enemy.cs:39 / PlayerAction`
- `HudView` -> `EnemyPreviewCue` - incoming / accepts_parameter / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\View\HudView.cs:435 / EnemyPreviewCue`
- `HudView` -> `PhaseSO` - incoming / accepts_parameter / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\View\HudView.cs:739 / PhaseSO`
- `JudgeSystem` -> `Enemy` - incoming / accepts_parameter / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Judge\JudgeSystem.cs:10 / Enemy`
- `PhasePresentationController` -> `StageSO` - outgoing / accepts_parameter / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\View\PhasePresentationController.cs:207 / StageSO`
- `PhaseSO` -> `PlayerAction` - outgoing / accepts_parameter / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Sequence\PhaseSO.cs:68 / PlayerAction`
- `RhythmAudioController` -> `PhaseSO` - incoming / accepts_parameter / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Rhythm\RhythmAudioController.cs:127 / PhaseSO`
- `RhythmTimingDisplay` -> `Enemy` - incoming / accepts_parameter / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\View\RhythmTimingDisplay.cs:248 / Enemy`
- `PhasePresentationController` -> `PlayerActionAudioSettings` - outgoing / calls_member / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\View\PhasePresentationController.cs:75 / PlayerActionAudioSettings.Load()`
- `RoundManager` -> `CombatBalanceSettings` - incoming / calls_member / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Managers\RoundManager.cs:142 / CombatBalanceSettings.Load()`
- `RoundManager` -> `EnemySequenceProvider` - incoming / calls_member / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Managers\RoundManager.cs:245 / provider.GenerateCycleWeighted(cycleIndex, count, phase)`
- `RoundManager` -> `PhaseSO` - incoming / calls_member / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Managers\RoundManager.cs:318 / CurrentPhase.ShouldHidePreview(enemy)`
- `PhaseSO` -> `ActionWeight` - outgoing / creates / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Sequence\PhaseSO.cs:38 / List<ActionWeight>`
- `RoundManager` -> `EnemyPreviewCue` - incoming / creates / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Managers\RoundManager.cs:319 / EnemyPreviewCue`
- `RoundManager` -> `EnemySequenceProvider` - incoming / creates / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Managers\RoundManager.cs:197 / EnemySequenceProvider`
- `StageSO` -> `Enemy` - incoming / creates / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Managers\StageSO.cs:22 / List<Enemy>`
- `StageSO` -> `PhaseSO` - incoming / creates / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Managers\StageSO.cs:37 / List<PhaseSO>`

## Internal Method Calls

- `EnemySequenceProvider.GetWeighted(int, int, PhaseSO)` -> `EnemySequenceProvider.Get(int, int)` / 2 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Sequence\EnemySequenceProvider.cs:60 / Get(cycleIndex, slotIndex)`
- `EnemySequenceProvider.GetWeighted(int, int, PhaseSO)` -> `EnemySequenceProvider.Hash(uint, uint, uint)` / 2 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Sequence\EnemySequenceProvider.cs:68 / Hash((uint)seed, (uint)cycleIndex, (uint)slotIndex)`
- `EnemySequenceProvider.GetWeighted(int, int, PhaseSO)` -> `PhaseSO.GetWeight(PlayerAction)` / 2 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Sequence\EnemySequenceProvider.cs:64 / phase.GetWeight(answerKeys[i])`
- `EnemySequenceProvider.EnemySequenceProvider(int, IReadOnlyList<Enemy>)` -> `EnemySequenceProvider.BuildGroups()` / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Sequence\EnemySequenceProvider.cs:25 / BuildGroups()`
- `EnemySequenceProvider.Get(int, int)` -> `EnemySequenceProvider.Hash(uint, uint, uint)` / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Sequence\EnemySequenceProvider.cs:36 / Hash((uint)seed, (uint)cycleIndex, (uint)slotIndex)`
- `EnemySequenceProvider.GenerateCycle(int, int)` -> `EnemySequenceProvider.Get(int, int)` / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Sequence\EnemySequenceProvider.cs:44 / Get(cycleIndex, i)`
- `PhaseBanner.Start()` -> `PhaseBanner.OnPreparation(int)` / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\View\PhaseBanner.cs:45 / OnPreparation(0)`
- `PhasePresentationController.Awake()` -> `PhasePresentationController.CreateFallbackSnare()` / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\View\PhasePresentationController.cs:95 / CreateFallbackSnare()`
- `PhaseBanner.OnPreparation(int)` -> `PhaseBanner.Set(Sprite)` / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\View\PhaseBanner.cs:48 / Set(preparationSprite != null ? preparationSprite : enemyActingSprite)`
- `PhaseBanner.OnPresent(int)` -> `PhaseBanner.Set(Sprite)` / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\View\PhaseBanner.cs:49 / Set(enemyActingSprite)`
- `PhaseBanner.OnResponse(int)` -> `PhaseBanner.Set(Sprite)` / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\View\PhaseBanner.cs:50 / Set(playerActingSprite)`
- `EnemySequenceProvider.GetWeighted(int, int, PhaseSO)` -> `EnemySequenceProvider.GetEnemyWeighted(int, int, PhaseSO, bool)` / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Sequence\EnemySequenceProvider.cs:59 / GetEnemyWeighted(cycleIndex, slotIndex, phase, true)`
- `EnemySequenceProvider.GetWeighted(int, int, PhaseSO)` -> `EnemySequenceProvider.Norm(uint)` / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Sequence\EnemySequenceProvider.cs:68 / Norm(Hash((uint)seed, (uint)cycleIndex, (uint)slotIndex))`
- `CombatTimeline.BeatsPerPhase(int, int)` -> `CombatTimeline.Validate(int, int)` / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Rhythm\CombatTimeline.cs:62 / Validate(exchangesPerPhase, preparationBeats)`
- `CombatTimeline.Resolve(int, int, int)` -> `CombatTimeline.BeatsPerPhase(int, int)` / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Rhythm\CombatTimeline.cs:74 / BeatsPerPhase(exchangesPerPhase, preparationBeats)`
- `CombatTimeline.Resolve(int, int, int)` -> `CombatTimeline.Validate(int, int)` / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Rhythm\CombatTimeline.cs:72 / Validate(exchangesPerPhase, preparationBeats)`
- `EnemySequenceProvider.GenerateCycleWeighted(int, int, PhaseSO)` -> `EnemySequenceProvider.GetEnemyWeighted(int, int, PhaseSO, bool)` / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Sequence\EnemySequenceProvider.cs:90 / GetEnemyWeighted(cycleIndex, i, phase, hasRoomForFollowUp)`
- `EnemySequenceProvider.GenerateCycleWeighted(int, int, PhaseSO)` -> `EnemySequenceProvider.GetWeighted(int, int, PhaseSO)` / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Sequence\EnemySequenceProvider.cs:91 / GetWeighted(cycleIndex, i, phase)`
- `EnemySequenceProvider.GetEnemyWeighted(int, int, PhaseSO, bool)` -> `EnemySequenceProvider.Hash(uint, uint, uint)` / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Sequence\EnemySequenceProvider.cs:123 / Hash((uint)seed, (uint)cycleIndex, (uint)slotIndex)`
- `EnemySequenceProvider.GetEnemyWeighted(int, int, PhaseSO, bool)` -> `EnemySequenceProvider.Norm(uint)` / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Sequence\EnemySequenceProvider.cs:123 / Norm(Hash((uint)seed, (uint)cycleIndex, (uint)slotIndex))`
- `CombatTimeline.StartsAfterResponse(int, int, int)` -> `CombatTimeline.Resolve(int, int, int)` / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Rhythm\CombatTimeline.cs:117 / Resolve(totalBeat - 1, exchangesPerPhase, preparationBeats)`
- `PhasePresentationController.Start()` -> `PhasePresentationController.CalculateBackgroundMusicStartDspTime(double, float)` / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\View\PhasePresentationController.cs:121 / CalculateBackgroundMusicStartDspTime(                     conductor.ScheduledStartDspTime,                     backgroundMusicFirstBeatOffset)`
- `EnemySequenceProvider.BuildGroups()` -> `EnemySequenceProvider.PrimaryAnswer(Enemy)` / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Sequence\EnemySequenceProvider.cs:150 / PrimaryAnswer(e)`
- `PhasePresentationController.OnDisable()` -> `PhasePresentationController.RestoreImmediately()` / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\View\PhasePresentationController.cs:164 / RestoreImmediately()`
- `PhasePresentationController.OnPhasePreparing(int, PhaseSO)` -> `PhasePresentationController.ApplyTargets()` / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\View\PhasePresentationController.cs:189 / ApplyTargets()`
- `PhasePresentationController.OnPhaseActive(int, PhaseSO)` -> `PhasePresentationController.ApplyTargets()` / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\View\PhasePresentationController.cs:196 / ApplyTargets()`
- `PhasePresentationController.OnStageApplied(StageSO)` -> `PhasePresentationController.ApplyTargets()` / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\View\PhasePresentationController.cs:216 / ApplyTargets()`
- `PhasePresentationController.OnStageApplied(StageSO)` -> `PhasePresentationController.RestoreImmediately()` / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\View\PhasePresentationController.cs:209 / RestoreImmediately()`

## Evidence

- Likely flow - PhasePresentationController.Awake() -> PhasePresentationController.CreateFallbackSnare() / terminal
- Likely flow - EnemySequenceProvider.GenerateCycleWeighted(int, int, PhaseSO) -> EnemySequenceProvider.GetWeighted(int, int, PhaseSO) -> EnemySequenceProvider.Get(int, int) -> EnemySequenceProvider.Hash(uint, uint, uint) / terminal
- Internal call - EnemySequenceProvider.GetWeighted(int, int, PhaseSO) -> EnemySequenceProvider.Get(int, int)
  - `H:\Unity\Project-Memories\Assets\Scripts\Sequence\EnemySequenceProvider.cs:60 / Get(cycleIndex, slotIndex)`
- Internal call - EnemySequenceProvider.GetWeighted(int, int, PhaseSO) -> EnemySequenceProvider.Hash(uint, uint, uint)
  - `H:\Unity\Project-Memories\Assets\Scripts\Sequence\EnemySequenceProvider.cs:68 / Hash((uint)seed, (uint)cycleIndex, (uint)slotIndex)`
- Internal call - EnemySequenceProvider.GetWeighted(int, int, PhaseSO) -> PhaseSO.GetWeight(PlayerAction)
  - `H:\Unity\Project-Memories\Assets\Scripts\Sequence\EnemySequenceProvider.cs:64 / phase.GetWeight(answerKeys[i])`
- incoming accepts_parameter - HudView -> Enemy / 4 refs
  - `H:\Unity\Project-Memories\Assets\Scripts\View\HudView.cs:455 / Enemy`
- incoming uses_local_type - RoundManager -> PhaseSO / 4 refs
  - `H:\Unity\Project-Memories\Assets\Scripts\Managers\RoundManager.cs:236 / PhaseSO`
- incoming accepts_parameter - RoundManager -> Enemy / 3 refs
  - `H:\Unity\Project-Memories\Assets\Scripts\Managers\RoundManager.cs:514 / Enemy`
- Internal returns - EnemySequenceProvider -> Enemy / 5 refs
  - `H:\Unity\Project-Memories\Assets\Scripts\Sequence\EnemySequenceProvider.cs:33 / Enemy`
- Internal calls_member - EnemySequenceProvider -> Enemy / 4 refs
  - `H:\Unity\Project-Memories\Assets\Scripts\Sequence\EnemySequenceProvider.cs:44 / list.Add(Get(cycleIndex, i))`
- Internal creates - EnemySequenceProvider -> Enemy / 4 refs
  - `H:\Unity\Project-Memories\Assets\Scripts\Sequence\EnemySequenceProvider.cs:19 / Dictionary<PlayerAction, List<Enemy>>`
- Internal accepts_parameter - EnemySequenceProvider -> PhaseSO / 3 refs
  - `H:\Unity\Project-Memories\Assets\Scripts\Sequence\EnemySequenceProvider.cs:55 / PhaseSO`
- Internal accepts_parameter - PhaseSO -> Enemy / 3 refs
  - `H:\Unity\Project-Memories\Assets\Scripts\Sequence\PhaseSO.cs:77 / Enemy`
- Internal uses_local_type - EnemySequenceProvider -> Enemy / 3 refs
  - `H:\Unity\Project-Memories\Assets\Scripts\Sequence\EnemySequenceProvider.cs:89 / Enemy`
- Internal uses_local_type - EnemySequenceProvider -> EnemyWeight / 3 refs
  - `H:\Unity\Project-Memories\Assets\Scripts\Sequence\EnemySequenceProvider.cs:109 / IReadOnlyList<EnemyWeight>`
- Internal accepts_parameter - EnemySequenceProvider -> Enemy / 2 refs
  - `H:\Unity\Project-Memories\Assets\Scripts\Sequence\EnemySequenceProvider.cs:21 / IReadOnlyList<Enemy>`

## Suggested AI Task

Use the Battle System context to explain the reading order, likely runtime flow, and risky assumptions. Cite method names, relationship edges, and file references when possible.

