# Audio System

Audio System appears to be an externally connected area around audio, beat, memories, rhythm, action. It contains 2 types, including 2 Unity-facing types.

## Stats

- Types: 2
- Internal relationships: 0
- External relationships: 41
- Entry candidates: 6
- Keywords: `audio`, `beat`, `memories`, `rhythm`, `action`, `player`, `settings`

## Start Here

- `RhythmAudioController.Awake()` - unity_lifecycle / H:\Unity\Project-Memories\Assets\Scripts\Rhythm\RhythmAudioController.cs:71
- `RhythmAudioController.OnEnable()` - unity_lifecycle / H:\Unity\Project-Memories\Assets\Scripts\Rhythm\RhythmAudioController.cs:78
- `RhythmAudioController.OnDisable()` - unity_lifecycle / H:\Unity\Project-Memories\Assets\Scripts\Rhythm\RhythmAudioController.cs:98
- `RhythmAudioController.Update()` - unity_lifecycle / H:\Unity\Project-Memories\Assets\Scripts\Rhythm\RhythmAudioController.cs:117
- `PlayerActionAudioSettings.Load()` - flow_candidate / H:\Unity\Project-Memories\Assets\Scripts\Audio\PlayerActionAudioSettings.cs:42
- `RhythmAudioController.BeginLoopTimeline(StageSoundtrackCatalogSO.Entry, double)` - flow_candidate / H:\Unity\Project-Memories\Assets\Scripts\Rhythm\RhythmAudioController.cs:389

## Core Types

- `RhythmAudioController` - class / Unity / 32 out / 4 in / H:\Unity\Project-Memories\Assets\Scripts\Rhythm\RhythmAudioController.cs:16
- `PlayerActionAudioSettings` - class / Unity / 1 out / 4 in / H:\Unity\Project-Memories\Assets\Scripts\Audio\PlayerActionAudioSettings.cs:7

## Likely Method Flows

- `RhythmAudioController.Update()`
  - `RhythmAudioController.Update()`
  - `RhythmAudioController.ScheduleMusicLookahead()`
  - `RhythmAudioController.IsPlayable(StageSoundtrackCatalogSO.Entry) / terminal`
- `RhythmAudioController.BeginLoopTimeline(StageSoundtrackCatalogSO.Entry, double)`
  - `RhythmAudioController.BeginLoopTimeline(StageSoundtrackCatalogSO.Entry, double)`
  - `RhythmAudioController.ScheduleLoop(long)`
  - `RhythmAudioController.LoopStartDspTime(long)`
  - `RhythmAudioController.CalculateLoopDspTime(double, long, int, float)`
  - `RhythmAudioController.LoopDurationSeconds(int, float) / terminal`
- `RhythmAudioController.Awake()`
  - `RhythmAudioController.Awake()`
  - `RhythmAudioController.EnsureAudioSources()`
  - `RhythmAudioController.ConfigureSource(AudioSource, AudioMixerGroup) / terminal`
- `RhythmAudioController.OnEnable()`
  - `RhythmAudioController.OnEnable()`
  - `RhythmAudioController.HandleStageApplied(StageSO)`
  - `RhythmAudioController.TrySelectCue(StageSO, int, StageSoundtrackCatalogSO.Entry)`
  - `RhythmAudioController.IsPlayable(StageSoundtrackCatalogSO.Entry) / terminal`
- `RhythmAudioController.OnDisable()`
  - `RhythmAudioController.OnDisable()`
  - `RhythmAudioController.StopAllAudio()`
  - `RhythmAudioController.StopMusicImmediately()`
  - `RhythmAudioController.StopMusicSource(int) / terminal`
- `PlayerActionAudioSettings.Load()`
  - `PlayerActionAudioSettings.Load() / terminal`

## Internal Type Relationships

- None detected.

## External Touchpoints

- `RhythmAudioController` -> `StageSoundtrackCatalogSO+Entry` - outgoing / accepts_parameter / 8 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Rhythm\RhythmAudioController.cs:342 / StageSoundtrackCatalogSO.Entry`
- `RhythmAudioController` -> `StageSoundtrackCatalogSO+Entry` - outgoing / has_field_type / 6 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Rhythm\RhythmAudioController.cs:55 / StageSoundtrackCatalogSO.Entry`
- `RhythmAudioController` -> `StageSoundtrackCatalogSO+Entry` - outgoing / uses_local_type / 6 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Rhythm\RhythmAudioController.cs:201 / IReadOnlyList<StageSoundtrackCatalogSO.Entry>`
- `RhythmAudioController` -> `StageSO` - outgoing / accepts_parameter / 2 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Rhythm\RhythmAudioController.cs:228 / StageSO`
- `StageManager` -> `RhythmAudioController` - incoming / calls_member / 2 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Managers\StageManager.cs:101 / rhythmAudio.PrepareCurrentClip()`
- `RhythmAudioController` -> `StageSO` - outgoing / uses_local_type / 2 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Rhythm\RhythmAudioController.cs:193 / StageSO`
- `RhythmAudioController` -> `PhaseSO` - outgoing / accepts_parameter / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Rhythm\RhythmAudioController.cs:127 / PhaseSO`
- `HudView` -> `PlayerActionAudioSettings` - incoming / calls_member / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\View\HudView.cs:256 / PlayerActionAudioSettings.Load()`
- `PhasePresentationController` -> `PlayerActionAudioSettings` - incoming / calls_member / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\View\PhasePresentationController.cs:75 / PlayerActionAudioSettings.Load()`
- `PlayerActionAudioSettings` -> `GameSettings` - outgoing / calls_member / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Audio\PlayerActionAudioSettings.cs:39 / GameSettings.ApplySfxVolume(output.audioMixer)`
- `RhythmAudioController` -> `Conductor` - outgoing / calls_member / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Rhythm\RhythmAudioController.cs:252 / conductor.SetRuntimeTempo(                 hasCue ? selectedCue.Bpm : stage.bpm,                 stage.startDelay)`
- `RhythmAudioController` -> `StageSoundtrackCatalogSO` - outgoing / calls_member / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Rhythm\RhythmAudioController.cs:345 / catalog.TryGetCue(stage, enemyPage, out cue)`
- `HudView` -> `PlayerActionAudioSettings` - incoming / has_field_type / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\View\HudView.cs:28 / PlayerActionAudioSettings`
- `PhasePresentationController` -> `RhythmAudioController` - incoming / has_field_type / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\View\PhasePresentationController.cs:21 / RhythmAudioController`
- `RhythmAudioController` -> `Conductor` - outgoing / has_field_type / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Rhythm\RhythmAudioController.cs:26 / Conductor`
- `RhythmAudioController` -> `RoundManager` - outgoing / has_field_type / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Rhythm\RhythmAudioController.cs:25 / RoundManager`
- `RhythmAudioController` -> `StageSoundtrackCatalogSO` - outgoing / has_field_type / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Rhythm\RhythmAudioController.cs:27 / StageSoundtrackCatalogSO`
- `StageManager` -> `RhythmAudioController` - incoming / has_field_type / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Managers\StageManager.cs:26 / RhythmAudioController`
- `RhythmAudioController` -> `Conductor` - outgoing / unity_find_object / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Rhythm\RhythmAudioController.cs:74 / Conductor`
- `RhythmAudioController` -> `RoundManager` - outgoing / unity_find_object / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Rhythm\RhythmAudioController.cs:73 / RoundManager`
- `PhasePresentationController` -> `PlayerActionAudioSettings` - incoming / uses_local_type / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\View\PhasePresentationController.cs:74 / PlayerActionAudioSettings`

## Internal Method Calls

- `RhythmAudioController.ScheduleLoop(long)` -> `RhythmAudioController.LoopStartDspTime(long)` / 2 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Rhythm\RhythmAudioController.cs:429 / LoopStartDspTime(iteration)`
- `RhythmAudioController.EnsureAudioSources()` -> `RhythmAudioController.ConfigureSource(AudioSource, AudioMixerGroup)` / 2 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Rhythm\RhythmAudioController.cs:635 / ConfigureSource(source, musicOutput)`
- `RhythmAudioController.Awake()` -> `RhythmAudioController.EnsureAudioSources()` / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Rhythm\RhythmAudioController.cs:75 / EnsureAudioSources()`
- `RhythmAudioController.OnEnable()` -> `RhythmAudioController.HandleStageApplied(StageSO)` / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Rhythm\RhythmAudioController.cs:95 / HandleStageApplied(round.CurrentStage)`
- `RhythmAudioController.OnDisable()` -> `RhythmAudioController.StopAllAudio()` / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Rhythm\RhythmAudioController.cs:114 / StopAllAudio()`
- `RhythmAudioController.Update()` -> `RhythmAudioController.ScheduleMusicLookahead()` / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Rhythm\RhythmAudioController.cs:120 / ScheduleMusicLookahead()`
- `RhythmAudioController.Update()` -> `RhythmAudioController.TrySchedulePendingSwitch()` / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Rhythm\RhythmAudioController.cs:119 / TrySchedulePendingSwitch()`
- `RhythmAudioController.PrepareCurrentClip()` -> `RhythmAudioController.CollectCurrentStageClips()` / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Rhythm\RhythmAudioController.cs:145 / CollectCurrentStageClips()`
- `RhythmAudioController.PrepareCurrentClip()` -> `RhythmAudioController.RequestClipLoad(AudioClip)` / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Rhythm\RhythmAudioController.cs:155 / RequestClipLoad(clips[i])`
- `RhythmAudioController.HandleStageApplied(StageSO)` -> `RhythmAudioController.RequestClipLoad(AudioClip)` / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Rhythm\RhythmAudioController.cs:240 / RequestClipLoad(selectedCue.Clip)`
- `RhythmAudioController.HandleStageApplied(StageSO)` -> `RhythmAudioController.StopMusicImmediately()` / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Rhythm\RhythmAudioController.cs:230 / StopMusicImmediately()`
- `RhythmAudioController.HandleStageApplied(StageSO)` -> `RhythmAudioController.TrySelectCue(StageSO, int, StageSoundtrackCatalogSO.Entry)` / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Rhythm\RhythmAudioController.cs:239 / TrySelectCue(stage, 1, out selectedCue)`
- `RhythmAudioController.HandleClockScheduled(double)` -> `RhythmAudioController.BeginLoopTimeline(StageSoundtrackCatalogSO.Entry, double)` / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Rhythm\RhythmAudioController.cs:272 / BeginLoopTimeline(selectedCue, scheduledStartDspTime)`
- `RhythmAudioController.HandleClockScheduled(double)` -> `RhythmAudioController.IsPlayable(StageSoundtrackCatalogSO.Entry)` / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Rhythm\RhythmAudioController.cs:260 / IsPlayable(selectedCue)`
- `RhythmAudioController.HandleClockScheduled(double)` -> `RhythmAudioController.StopMusicImmediately()` / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Rhythm\RhythmAudioController.cs:259 / StopMusicImmediately()`
- `RhythmAudioController.HandleClockScheduled(double)` -> `RhythmAudioController.ValidateLoopLength(StageSoundtrackCatalogSO.Entry)` / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Rhythm\RhythmAudioController.cs:271 / ValidateLoopLength(selectedCue)`
- `RhythmAudioController.HandleClockStopped()` -> `RhythmAudioController.StopAllAudio()` / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Rhythm\RhythmAudioController.cs:277 / StopAllAudio()`
- `RhythmAudioController.HandleEnemyPageTransitionStarted(int, int, int)` -> `RhythmAudioController.RequestClipLoad(AudioClip)` / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Rhythm\RhythmAudioController.cs:303 / RequestClipLoad(pendingPageCue.Clip)`
- `RhythmAudioController.HandleEnemyPageTransitionStarted(int, int, int)` -> `RhythmAudioController.TrySelectCue(StageSO, int, StageSoundtrackCatalogSO.Entry)` / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Rhythm\RhythmAudioController.cs:300 / TrySelectCue(stage, page, out pendingPageCue)`
- `RhythmAudioController.HandlePreparationScheduled(double, double, int)` -> `RhythmAudioController.IsPlayable(StageSoundtrackCatalogSO.Entry)` / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Rhythm\RhythmAudioController.cs:328 / IsPlayable(pendingPageCue)`
- `RhythmAudioController.HandlePreparationScheduled(double, double, int)` -> `RhythmAudioController.SchedulePreparationSnares(double, double, int)` / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Rhythm\RhythmAudioController.cs:320 / SchedulePreparationSnares(startDspTime, endDspTime, beats)`
- `RhythmAudioController.HandlePreparationScheduled(double, double, int)` -> `RhythmAudioController.StopLoopTimelineAt(double)` / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Rhythm\RhythmAudioController.cs:326 / StopLoopTimelineAt(endDspTime)`
- `RhythmAudioController.HandlePreparationScheduled(double, double, int)` -> `RhythmAudioController.TrySchedulePendingSwitch()` / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Rhythm\RhythmAudioController.cs:336 / TrySchedulePendingSwitch()`
- `RhythmAudioController.TrySelectCue(StageSO, int, StageSoundtrackCatalogSO.Entry)` -> `RhythmAudioController.IsPlayable(StageSoundtrackCatalogSO.Entry)` / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Rhythm\RhythmAudioController.cs:346 / IsPlayable(cue)`
- `RhythmAudioController.ValidateLoopLength(StageSoundtrackCatalogSO.Entry)` -> `RhythmAudioController.LoopDurationSeconds(int, float)` / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Rhythm\RhythmAudioController.cs:374 / LoopDurationSeconds(                 cue.LoopBeats,                 cue.Bpm)`
- `RhythmAudioController.BeginLoopTimeline(StageSoundtrackCatalogSO.Entry, double)` -> `RhythmAudioController.FindAssignableMusicSource()` / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Rhythm\RhythmAudioController.cs:393 / FindAssignableMusicSource()`
- `RhythmAudioController.BeginLoopTimeline(StageSoundtrackCatalogSO.Entry, double)` -> `RhythmAudioController.ScheduleLoop(long)` / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Rhythm\RhythmAudioController.cs:408 / ScheduleLoop(nextLoopIteration)`
- `RhythmAudioController.ScheduleMusicLookahead()` -> `RhythmAudioController.IsPlayable(StageSoundtrackCatalogSO.Entry)` / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Rhythm\RhythmAudioController.cs:414 / IsPlayable(loopCue)`
- `RhythmAudioController.ScheduleMusicLookahead()` -> `RhythmAudioController.LoopStartDspTime(long)` / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Rhythm\RhythmAudioController.cs:417 / LoopStartDspTime(nextLoopIteration)`
- `RhythmAudioController.ScheduleMusicLookahead()` -> `RhythmAudioController.ScheduleLoop(long)` / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Rhythm\RhythmAudioController.cs:419 / ScheduleLoop(nextLoopIteration)`
- `RhythmAudioController.LoopStartDspTime(long)` -> `RhythmAudioController.CalculateLoopDspTime(double, long, int, float)` / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Rhythm\RhythmAudioController.cs:465 / CalculateLoopDspTime(                 loopStartDspTime,                 iteration,                 loopCue.LoopBeats,                 loopCue.Bpm)`
- `RhythmAudioController.CalculateLoopDspTime(double, long, int, float)` -> `RhythmAudioController.LoopDurationSeconds(int, float)` / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Rhythm\RhythmAudioController.cs:477 / LoopDurationSeconds(loopBeats, bpm)`
- `RhythmAudioController.StopLoopTimelineAt(double)` -> `RhythmAudioController.StopMusicSource(int)` / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Rhythm\RhythmAudioController.cs:489 / StopMusicSource(i)`
- `RhythmAudioController.TrySchedulePendingSwitch()` -> `RhythmAudioController.FindAssignableMusicSource()` / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Rhythm\RhythmAudioController.cs:547 / FindAssignableMusicSource()`
- `RhythmAudioController.TrySchedulePendingSwitch()` -> `RhythmAudioController.RequestClipLoad(AudioClip)` / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Rhythm\RhythmAudioController.cs:516 / RequestClipLoad(pendingPageCue.Clip)`
- `RhythmAudioController.TrySchedulePendingSwitch()` -> `RhythmAudioController.ScheduleLoop(long)` / 1 refs
  - Evidence: `H:\Unity\Project-Memories\Assets\Scripts\Rhythm\RhythmAudioController.cs:579 / ScheduleLoop(nextLoopIteration)`

## Evidence

- Likely flow - RhythmAudioController.Update() -> RhythmAudioController.ScheduleMusicLookahead() -> RhythmAudioController.IsPlayable(StageSoundtrackCatalogSO.Entry) / terminal
- Likely flow - RhythmAudioController.BeginLoopTimeline(StageSoundtrackCatalogSO.Entry, double) -> RhythmAudioController.ScheduleLoop(long) -> RhythmAudioController.LoopStartDspTime(long) -> RhythmAudioController.CalculateLoopDspTime(double, long, int, float)
- Internal call - RhythmAudioController.ScheduleLoop(long) -> RhythmAudioController.LoopStartDspTime(long)
  - `H:\Unity\Project-Memories\Assets\Scripts\Rhythm\RhythmAudioController.cs:429 / LoopStartDspTime(iteration)`
- Internal call - RhythmAudioController.EnsureAudioSources() -> RhythmAudioController.ConfigureSource(AudioSource, AudioMixerGroup)
  - `H:\Unity\Project-Memories\Assets\Scripts\Rhythm\RhythmAudioController.cs:635 / ConfigureSource(source, musicOutput)`
- Internal call - RhythmAudioController.Awake() -> RhythmAudioController.EnsureAudioSources()
  - `H:\Unity\Project-Memories\Assets\Scripts\Rhythm\RhythmAudioController.cs:75 / EnsureAudioSources()`
- outgoing accepts_parameter - RhythmAudioController -> StageSoundtrackCatalogSO+Entry / 8 refs
  - `H:\Unity\Project-Memories\Assets\Scripts\Rhythm\RhythmAudioController.cs:342 / StageSoundtrackCatalogSO.Entry`
- outgoing has_field_type - RhythmAudioController -> StageSoundtrackCatalogSO+Entry / 6 refs
  - `H:\Unity\Project-Memories\Assets\Scripts\Rhythm\RhythmAudioController.cs:55 / StageSoundtrackCatalogSO.Entry`
- outgoing uses_local_type - RhythmAudioController -> StageSoundtrackCatalogSO+Entry / 6 refs
  - `H:\Unity\Project-Memories\Assets\Scripts\Rhythm\RhythmAudioController.cs:201 / IReadOnlyList<StageSoundtrackCatalogSO.Entry>`

## Suggested AI Task

Use the Audio System context to explain the reading order, likely runtime flow, and risky assumptions. Cite method names, relationship edges, and file references when possible.

