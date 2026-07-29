# UI Layer

UI Layer appears to be an externally connected area around ui, title, beat, memories, button. It contains 5 types, including 4 Unity-facing types.

## Stats

- Types: 5
- Internal relationships: 44
- External relationships: 57
- Entry candidates: 8
- Keywords: `ui`, `title`, `beat`, `memories`, `button`, `element`, `hud`, `options`, `settings`

## Start Here

- `OptionsSettingsController.Awake()` - unity_lifecycle / H:\Unity\Project-Memories\Assets\Scripts\UI\OptionsSettingsController.cs:66
- `HudView.OnDisable()` - unity_lifecycle / H:\Unity\Project-Memories\Assets\Scripts\View\HudView.cs:289
- `HudView.Start()` - unity_lifecycle / H:\Unity\Project-Memories\Assets\Scripts\View\HudView.cs:358
- `HudView.Update()` - unity_lifecycle / H:\Unity\Project-Memories\Assets\Scripts\View\HudView.cs:385
- `Title.StartGame()` - flow_candidate / H:\Unity\Project-Memories\Assets\Scripts\UI\Title.cs:203
- `HudView.InitializeQueues()` - flow_candidate / H:\Unity\Project-Memories\Assets\Scripts\View\HudView.cs:999
- `HudView.InitializeLaser(LineRenderer, LineRenderer, Light2D, Light2D)` - flow_candidate / H:\Unity\Project-Memories\Assets\Scripts\View\HudView.cs:1164
- `HudView.InitializeChargeEffects()` - flow_candidate / H:\Unity\Project-Memories\Assets\Scripts\View\HudView.cs:1484

## Core Types

- `HudView` - class / Unity / 52 out / 3 in / H:\Unity\Project-Memories\Assets\Scripts\View\HudView.cs:16
- `Title` - class / Unity / 44 out / 0 in / H:\Unity\Project-Memories\Assets\Scripts\UI\Title.cs:6
- `Title+ElementState` - struct / 0 out / 25 in / H:\Unity\Project-Memories\Assets\Scripts\UI\Title.cs:594
- `TitleButton` - class / Unity / 0 out / 19 in / H:\Unity\Project-Memories\Assets\Scripts\UI\TitleButton.cs:7
- `OptionsSettingsController` - class / Unity / 2 out / 0 in / H:\Unity\Project-Memories\Assets\Scripts\UI\OptionsSettingsController.cs:10

## Likely Method Flows

- `HudView.OnDisable()`
  - `HudView.OnDisable()`
  - `HudView.StopEffectLight(Light2D) / terminal`
- `HudView.Start()`
  - `HudView.Start()`
  - `HudView.InitializeLaser(LineRenderer, LineRenderer, Light2D, Light2D)`
  - `HudView.InitializeLaserLine(LineRenderer, float) / terminal`
- `OptionsSettingsController.Awake()`
  - `OptionsSettingsController.Awake()`
  - `OptionsSettingsController.HasRequiredReferences() / terminal`
- `HudView.Update()`
  - `HudView.Update()`
  - `HudView.DamageFlash(Color, float) / terminal`
- `HudView.InitializeLaser(LineRenderer, LineRenderer, Light2D, Light2D)`
  - `HudView.InitializeLaser(LineRenderer, LineRenderer, Light2D, Light2D)`
  - `HudView.InitializeLaserLine(LineRenderer, float) / terminal`
- `HudView.InitializeChargeEffects()`
  - `HudView.InitializeChargeEffects() / terminal`
- `Title.StartGame()`
  - `Title.StartGame()`
  - `Title.LoadGameScene() / terminal`
- `HudView.InitializeQueues()`
  - `HudView.InitializeQueues()`
  - `HudView.QueueSlot(Image[], int) / terminal`

## Internal Type Relationships

- `Title` -> `TitleButton` - internal / calls_member / 13 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\UI\Title.cs:172 / backButton.SetInteractionEnabled(false)`
- `Title` -> `Title+ElementState` - internal / creates / 9 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\UI\Title.cs:421 / ElementState`
- `Title` -> `Title+ElementState` - internal / has_field_type / 9 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\UI\Title.cs:63 / ElementState`
- `Title` -> `Title+ElementState` - internal / accepts_parameter / 7 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\UI\Title.cs:368 / ElementState`
- `Title` -> `TitleButton` - internal / has_field_type / 4 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\UI\Title.cs:15 / TitleButton`
- `Title` -> `TitleButton` - internal / accepts_parameter / 2 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\UI\Title.cs:454 / TitleButton`

## External Touchpoints

- `HudView` -> `CharacterView` - outgoing / calls_member / 5 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\View\HudView.cs:1210 / view.GetComponent<KeyframeAnimator>()`
- `HudView` -> `Enemy` - outgoing / accepts_parameter / 4 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\View\HudView.cs:455 / Enemy`
- `HudView` -> `JudgeResult` - outgoing / accepts_parameter / 4 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\View\HudView.cs:455 / JudgeResult`
- `HudView` -> `ChargeAuraEffect` - outgoing / calls_member / 4 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\View\HudView.cs:1488 / chargeAura.Initialize(playerSlot, playerLaserColor)`
- `HudView` -> `CharacterView` - outgoing / uses_local_type / 4 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\View\HudView.cs:1208 / CharacterView`
- `HudView` -> `Enemy` - outgoing / uses_local_type / 3 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\View\HudView.cs:438 / Enemy`
- `HudView` -> `StageSO` - outgoing / accepts_parameter / 2 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\View\HudView.cs:706 / StageSO`
- `HudView` -> `KeyframeAnimator` - outgoing / calls_member / 2 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\View\HudView.cs:852 / playerIdleAnim.Pause()`
- `HudView` -> `RoundManager` - outgoing / calls_member / 2 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\View\HudView.cs:1880 / round.CommitScore(points)`
- `OptionsSettingsController` -> `GameSettings` - outgoing / calls_member / 2 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\UI\OptionsSettingsController.cs:315 / GameSettings.BgmVolumeToDecibels(bgmVolume)`
- `HudView` -> `ChargeAuraEffect` - outgoing / has_field_type / 2 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\View\HudView.cs:141 / ChargeAuraEffect`
- `HudView` -> `StageSO` - outgoing / uses_local_type / 2 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\View\HudView.cs:714 / StageSO`
- `HudView` -> `EnemyPreviewCue` - outgoing / accepts_parameter / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\View\HudView.cs:435 / EnemyPreviewCue`
- `HudView` -> `PhaseSO` - outgoing / accepts_parameter / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\View\HudView.cs:739 / PhaseSO`
- `HudView` -> `PlayerAction` - outgoing / accepts_parameter / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\View\HudView.cs:849 / PlayerAction`
- `HudView` -> `RhythmTimingResult` - outgoing / accepts_parameter / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\View\HudView.cs:598 / RhythmTimingResult`
- `HudView` -> `PlayerActionAudioSettings` - outgoing / calls_member / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\View\HudView.cs:256 / PlayerActionAudioSettings.Load()`
- `StageManager` -> `HudView` - incoming / calls_member / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Managers\StageManager.cs:197 / hud.WaitForEnemyExit()`
- `HudView` -> `CameraSway` - outgoing / has_field_type / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\View\HudView.cs:25 / CameraSway`
- `HudView` -> `Conductor` - outgoing / has_field_type / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\View\HudView.cs:22 / Conductor`
- `HudView` -> `Enemy` - outgoing / has_field_type / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\View\HudView.cs:226 / Enemy[]`
- `HudView` -> `KeyframeAnimator` - outgoing / has_field_type / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\View\HudView.cs:34 / KeyframeAnimator`
- `HudView` -> `PlayerActionAudioSettings` - outgoing / has_field_type / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\View\HudView.cs:28 / PlayerActionAudioSettings`
- `HudView` -> `PlayerData` - outgoing / has_field_type / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\View\HudView.cs:23 / PlayerData`
- `HudView` -> `RoundManager` - outgoing / has_field_type / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\View\HudView.cs:21 / RoundManager`
- `HudView` -> `StageManager` - outgoing / has_field_type / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\View\HudView.cs:24 / StageManager`
- `StageManager` -> `HudView` - incoming / has_field_type / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Managers\StageManager.cs:24 / HudView`
- `HudView` -> `CharacterView` - outgoing / returns / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\View\HudView.cs:1809 / CharacterView`
- `HudView` -> `ChargeAuraEffect` - outgoing / unity_add_component / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\View\HudView.cs:1502 / ChargeAuraEffect`
- `HudView` -> `StageManager` - outgoing / unity_find_object / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\View\HudView.cs:1196 / StageManager`
- `StageManager` -> `HudView` - incoming / unity_find_object / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Managers\StageManager.cs:75 / HudView`
- `HudView` -> `KeyframeAnimator` - outgoing / unity_get_component / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\View\HudView.cs:1210 / KeyframeAnimator`
- `HudView` -> `KeyframeAnimator` - outgoing / uses_local_type / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\View\HudView.cs:1209 / KeyframeAnimator`

## Internal Method Calls

- `Title.RestoreFinalState()` -> `Title.RestoreElement(ElementState)` / 9 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\UI\Title.cs:472 / RestoreElement(wooferLeftState)`
- `HudView.OnDisable()` -> `HudView.StopEffectLight(Light2D)` / 4 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\View\HudView.cs:340 / StopEffectLight(playerLaserMuzzleGlow)`
- `Title.TryCacheElementStates()` -> `TitleButton.Prepare()` / 4 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\UI\Title.cs:416 / startButton.Prepare()`
- `Title.ApplyIntroState()` -> `Title.ApplyHiddenState(ElementState, Vector2, float)` / 4 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\UI\Title.cs:435 / ApplyHiddenState(             wooferLeftState,             new Vector2(-wooferHorizontalOffset, 0f),             wooferStartScale)`
- `Title.AddMainUiTweens(Sequence, float, bool)` -> `Title.AddMainUiTween(Sequence, ElementState, float, bool)` / 4 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\UI\Title.cs:500 / AddMainUiTween(sequence, textArtState, startTime, visible)`
- `HudView.FlipSprite(SpriteRenderer, Sprite, Vector3, bool)` -> `HudView.ScaleSpriteWidth(SpriteRenderer, Vector3, float, float, float)` / 4 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\View\HudView.cs:951 / ScaleSpriteWidth(slot, baseScale, 1f, 0f, quarterDuration)`
- `HudView.PlayLaser(LineRenderer, LineRenderer, Light2D, Light2D, SpriteRenderer, Vector3, Vector3, Color, bool, float)` -> `HudView.SetLaserColor(LineRenderer, Color, float)` / 4 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\View\HudView.cs:1286 / SetLaserColor(laser, color, alpha)`
- `Title.PlayIntro()` -> `Title.AddButtonTween(ElementState, TitleButton, float)` / 3 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\UI\Title.cs:153 / AddButtonTween(startButtonState, startButton, stageStart)`
- `Title.PlayBeatPulse()` -> `Title.CreateBeatPulse(ElementState, float, Sequence)` / 3 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\UI\Title.cs:342 / CreateBeatPulse(             wooferLeftState,             wooferBeatScale,             wooferLeftPulseSequence)`
- `Title.ApplyIntroState()` -> `Title.ApplyButtonIntroState(ElementState, TitleButton)` / 3 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\UI\Title.cs:448 / ApplyButtonIntroState(startButtonState, startButton)`
- `Title.SetMainButtonsInteraction(bool)` -> `TitleButton.SetInteractionEnabled(bool)` / 3 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\UI\Title.cs:493 / startButton.SetInteractionEnabled(value)`
- `HudView.PlayPlayerJudgementVoice(JudgeResult)` -> `HudView.PlayPlayerVoice(AudioClip[])` / 3 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\View\HudView.cs:540 / PlayPlayerVoice(playerActionAudioSettings.MistakeVoices)`
- `HudView.PlayResolvedActionEffect(Enemy, JudgeResult)` -> `HudView.PlayPlayerVoice(AudioClip[])` / 3 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\View\HudView.cs:576 / PlayPlayerVoice(playerActionAudioSettings.ParryEffect)`
- `Title.PlayIntro()` -> `Title.AddWooferTween(ElementState, float)` / 2 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\UI\Title.cs:140 / AddWooferTween(wooferLeftState, stageStart)`
- `Title.ShowOptions()` -> `TitleButton.SetInteractionEnabled(bool)` / 2 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\UI\Title.cs:172 / backButton.SetInteractionEnabled(false)`
- `HudView.OnDisable()` -> `HudView.RestorePresentation(SpriteRenderer, Vector3, Vector3)` / 2 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\View\HudView.cs:320 / RestorePresentation(enemySlot, enemyBaseScale, ref enemyShakeOffset)`
- `HudView.OnDisable()` -> `HudView.StopGuardShield(SpriteRenderer)` / 2 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\View\HudView.cs:345 / StopGuardShield(playerGuardShield)`
- `HudView.OnDisable()` -> `HudView.StopLaser(LineRenderer, LineRenderer)` / 2 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\View\HudView.cs:324 / StopLaser(playerLaser, playerLaserOuter)`
- `HudView.Start()` -> `HudView.InitializeLaser(LineRenderer, LineRenderer, Light2D, Light2D)` / 2 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\View\HudView.cs:378 / InitializeLaser(playerLaser, playerLaserOuter, playerLaserMuzzleGlow, playerLaserHitFlash)`
- `HudView.Update()` -> `HudView.DamageFlash(Color, float)` / 2 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\View\HudView.cs:403 / DamageFlash(Color.white, ref enemyDamageFlashTimer)`
- `HudView.LateUpdate()` -> `HudView.UpdateDspIdleBounce(SpriteRenderer, Vector3, bool)` / 2 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\View\HudView.cs:425 / UpdateDspIdleBounce(                 playerSlot,                 playerBaseScale,                 playerIdleBounceEnabled)`
- `HudView.OnJudged(int, Enemy, JudgeResult)` -> `HudView.PlayGuardShield(SpriteRenderer, SpriteRenderer, SpriteRenderer)` / 2 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\View\HudView.cs:514 / PlayGuardShield(playerGuardShield, playerSlot, enemySlot)`
- `HudView.OnJudged(int, Enemy, JudgeResult)` -> `HudView.PlayLaser(LineRenderer, LineRenderer, Light2D, Light2D, SpriteRenderer, Vector3, Vector3, Color, bool, float)` / 2 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\View\HudView.cs:478 / PlayLaser(                     playerLaser,                     playerLaserOuter,                     playerLaserMuzzleGlow,                     playerLaserHitFlash,                     enemySlot,                     ResolveLaserOrigi`
- `HudView.OnJudged(int, Enemy, JudgeResult)` -> `HudView.ResolveHitPosition(bool, SpriteRenderer)` / 2 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\View\HudView.cs:485 / ResolveHitPosition(false, enemySlot)`
- `HudView.OnJudged(int, Enemy, JudgeResult)` -> `HudView.ResolveLaserOrigin(bool, LineRenderer)` / 2 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\View\HudView.cs:484 / ResolveLaserOrigin(true, playerLaser)`
- `HudView.OnBeat(int)` -> `HudView.PlayIdleBeatBounce(SpriteRenderer, Vector3, bool, Tween)` / 2 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\View\HudView.cs:765 / PlayIdleBeatBounce(                 playerSlot,                 playerBaseScale,                 playerIdleBounceEnabled,                 ref playerIdleBounce)`
- `HudView.UpdateDspIdleBounce(SpriteRenderer, Vector3, bool)` -> `HudView.RestoreIdleScaleY(SpriteRenderer, Vector3)` / 2 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\View\HudView.cs:785 / RestoreIdleScaleY(slot, baseScale)`
- `HudView.InitializeQueues()` -> `HudView.QueueSlot(Image[], int)` / 2 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\View\HudView.cs:1003 / QueueSlot(enemyQueueSlots, i)`
- `HudView.InitializeLaser(LineRenderer, LineRenderer, Light2D, Light2D)` -> `HudView.InitializeEffectLight(Light2D)` / 2 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\View\HudView.cs:1177 / InitializeEffectLight(muzzleGlow)`
- `HudView.InitializeLaser(LineRenderer, LineRenderer, Light2D, Light2D)` -> `HudView.InitializeLaserLine(LineRenderer, float)` / 2 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\View\HudView.cs:1171 / InitializeLaserLine(laser, laserWidth)`
- `HudView.ResolveCurrentLaserOrigin(LineRenderer, Vector3)` -> `HudView.ResolveLaserOrigin(bool, LineRenderer)` / 2 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\View\HudView.cs:1246 / ResolveLaserOrigin(true, laser)`
- `HudView.PlayLaser(LineRenderer, LineRenderer, Light2D, Light2D, SpriteRenderer, Vector3, Vector3, Color, bool, float)` -> `HudView.SetLaserPositions(LineRenderer, LineRenderer, Vector3, Vector3)` / 2 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\View\HudView.cs:1293 / SetLaserPositions(laser, outerLaser, origin, origin)`
- `HudView.InitializeGuardShields()` -> `HudView.CreateGuardShield(SpriteRenderer, string)` / 2 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\View\HudView.cs:1513 / CreateGuardShield(                 playerGuardShield,                 "PlayerGuardShield")`
- `TitleButton.Awake()` -> `TitleButton.ApplySprite(bool)` / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\UI\TitleButton.cs:36 / ApplySprite(false)`
- `TitleButton.Awake()` -> `TitleButton.Prepare()` / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\UI\TitleButton.cs:35 / Prepare()`
- `TitleButton.SetInteractionEnabled(bool)` -> `TitleButton.ApplySprite(bool)` / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\UI\TitleButton.cs:65 / ApplySprite(false)`

## Evidence

- Likely flow - HudView.OnDisable() -> HudView.StopEffectLight(Light2D) / terminal
- Likely flow - HudView.Start() -> HudView.InitializeLaser(LineRenderer, LineRenderer, Light2D, Light2D) -> HudView.InitializeLaserLine(LineRenderer, float) / terminal
- Internal call - Title.RestoreFinalState() -> Title.RestoreElement(ElementState)
  - `H:\Unity\Project-Memories\Assets\Scripts\UI\Title.cs:472 / RestoreElement(wooferLeftState)`
- Internal call - HudView.OnDisable() -> HudView.StopEffectLight(Light2D)
  - `H:\Unity\Project-Memories\Assets\Scripts\View\HudView.cs:340 / StopEffectLight(playerLaserMuzzleGlow)`
- Internal call - Title.TryCacheElementStates() -> TitleButton.Prepare()
  - `H:\Unity\Project-Memories\Assets\Scripts\UI\Title.cs:416 / startButton.Prepare()`
- outgoing calls_member - HudView -> CharacterView / 5 refs
  - `H:\Unity\Project-Memories\Assets\Scripts\View\HudView.cs:1210 / view.GetComponent<KeyframeAnimator>()`
- outgoing accepts_parameter - HudView -> Enemy / 4 refs
  - `H:\Unity\Project-Memories\Assets\Scripts\View\HudView.cs:455 / Enemy`
- outgoing accepts_parameter - HudView -> JudgeResult / 4 refs
  - `H:\Unity\Project-Memories\Assets\Scripts\View\HudView.cs:455 / JudgeResult`
- Internal calls_member - Title -> TitleButton / 13 refs
  - `H:\Unity\Project-Memories\Assets\Scripts\UI\Title.cs:172 / backButton.SetInteractionEnabled(false)`
- Internal creates - Title -> Title+ElementState / 9 refs
  - `H:\Unity\Project-Memories\Assets\Scripts\UI\Title.cs:421 / ElementState`
- Internal has_field_type - Title -> Title+ElementState / 9 refs
  - `H:\Unity\Project-Memories\Assets\Scripts\UI\Title.cs:63 / ElementState`
- Internal accepts_parameter - Title -> Title+ElementState / 7 refs
  - `H:\Unity\Project-Memories\Assets\Scripts\UI\Title.cs:368 / ElementState`
- Internal has_field_type - Title -> TitleButton / 4 refs
  - `H:\Unity\Project-Memories\Assets\Scripts\UI\Title.cs:15 / TitleButton`
- Internal accepts_parameter - Title -> TitleButton / 2 refs
  - `H:\Unity\Project-Memories\Assets\Scripts\UI\Title.cs:454 / TitleButton`

## Suggested AI Task

Use the UI Layer context to explain the reading order, likely runtime flow, and risky assumptions. Cite method names, relationship edges, and file references when possible.

