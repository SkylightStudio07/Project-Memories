# Beat Memories — 개발 아키텍처 (P0 / 1차 목업)

> **기획 SOT(Source of Truth): Notion "박자게임" 페이지.**
> (구 `GGD-Draft.MD`는 폐기 — 콜앤리스폰스 구조로 정정됨.)
> 이 문서는 그 기획을 코드 구조로 옮긴 **개발 설계 기준**이다. 진척은 Notion 진척 페이지에 별도 기록.
>
> **상태: P0 코어 목업 완성(2026-07-25).** 씬 `Assets/Scenes/BeatMemories.unity` 플레이 가능. 남은 것은 밸런싱·아트·확장(§8 미정 항목, 4액션/P1).

## 0. 코어 루프 (콜앤리스폰스)

- **1 사이클 = 8박 = 2마디.** **제시 마디(4박)** → **응답 마디(4박)**.
- **제시**: 적들이 자세(실루엣)를 드러낸다. **응답**: 제시된 **순서 그대로** 되받아친다.
- **제시 순서 = 응답 순서**, **제시 밀도 = 응답 밀도**. 판단은 제시 마디에서 끝나고, 응답 마디는 손이 따라가는 구간.
- 판정: 맞으면 적 처리, 틀리면 피격·체력 감소. 체력 0 종료.
- **판정 축 = 자세**. 색·옷·종족은 판정에 안 걸리는 노이즈(의도된 방해물).

## 1. 확정된 P0 범위

| 항목 | 결정 |
|---|---|
| 액션 | **2/3키 토글**(`InputReader.keyMode`): 2키 `←가드`/`→공격` · 3키 +`↓차징` |
| 자세(적) | **4종**: `공세`(→가드) / `무방비`(→공격) / `충전유발`(→차징) / `장갑무방비`(→차징→강공격) |
| 전투 | 적 `maxHp`·`armor`. 일반 공격은 방어력에 막힘 → **차징하면 강공격(방어 관통·고위력)** 으로 처리 |
| 타이밍 | **비트 클록 포함** — BPM 메트로놈 + 8박 사이클 + 온-비트 판정 |
| 리듬 패턴 | 안1 `다-다-다-딴!`(입력 1) / 안2 `딴!딴!딴!딴!`(입력 4) — **둘 다 데이터로 구현, P0 시작 패턴은 미정(§9)** |
| 시퀀스 | **결정론적 난수(시드)** — 재현성·독립성 보장 |
| 연출 | **P0는 모션 없이 색 점멸**로 타이밍 먼저 검증 |
| 수치 | 전부 잠정 → **데이터 파일/SO로 분리**, 테스트로 결정 |

## 2. 아키텍처 원칙

3계층 데이터 지향:
1. **데이터 컨테이너** — 비 MonoBehaviour `[Serializable]` (`EnemyData`, `ActionOutcome` …)
2. **데이터 기반 SO** — `ScriptableObject` 에셋 (`Enemy`, `RhythmPatternSO` …)
3. **매니저** — 런타임 흐름 (`Conductor`, `RoundManager` …)

추가:
- 잠정 수치는 코드에 박지 않고 SO/데이터로 노출(§8).
- 시퀀스 난수는 **전용 시드 스트림**만 사용(전역 `UnityEngine.Random` 금지) → 재현성·독립성.
- 플레이어·적은 단순 스프라이트, 십자키/HUD는 Canvas. 하이어라키/인스펙터는 Unity CLI(Pipeline)로.

## 3. 폴더 구조

```
Assets/
  Scripts/
    Core/       # PlayerAction, OutcomeType
    Enemy/      # EnemyData, ActionOutcome, Enemy(SO)
    Judge/      # JudgeResult, JudgeSystem
    Rhythm/     # RhythmPatternSO, (Conductor/BeatClock)
    Sequence/   # EnemySequenceProvider (시드 기반)
    Player/     # PlayerHealth, InputReader
    Managers/   # RoundManager
    View/       # 적 슬롯(색 점멸), 8박 인디케이터, HUD, 피드백
  Data/
    Enemies/    # Enemy SO 에셋
    Rhythm/     # RhythmPatternSO 에셋
```

## 4. 모듈 분해 (한 번에 한 모듈씩)

| # | 모듈 | 산출물 | 상태 |
|---|---|---|---|
| 1 | 데이터: 액션/결과 | `PlayerAction`, `OutcomeType`, `ActionOutcome` | ✅ 완료 |
| 2 | 데이터: Enemy(자세) | `EnemyData`, `Enemy`(SO), 공세/무방비 샘플 | ✅ 완료 |
| 3 | 판정 | `JudgeResult`, `JudgeSystem` | ✅ 완료 |
| 4 | 리듬 패턴 | `RhythmPatternSO` (안1/안2 스포트라이트·입력수 데이터) | ✅ 완료 |
| 5 | 결정론적 시퀀스 | `EnemySequenceProvider` (시드, 재현성/독립성) | ✅ 완료 |
| 5b | 페이즈(맥락) | `PhaseSO`/`ActionWeight`, 결정론적 가중 선택, 공격/수비 에셋 | ✅ 완료 |
| 6 | 비트 클록 | `Conductor` (BPM, 8박 사이클, 제시/응답 마디 이벤트) | ✅ 완료(플레이 검증) |
| 7 | 플레이어 | `PlayerHealth`(HP), `InputReader`(←/→) | ✅ 완료(플레이 검증) |
| 8 | 라운드 매니저 | 사이클 오케스트레이션(제시→응답, 판정·HP 반영) | ✅ 완료(플레이 검증) |
| 9 | UI(Canvas) | 적 슬롯(색 점멸), 8박 인디케이터, HP 하트, 페이즈·키 가이드·피드백, 온스크린 버튼 | ✅ 완료(시각 검증) |
| 10 | 씬 조립 | `BeatMemories.unity` 기능 씬 배선 (Unity CLI) | ✅ 완료(스모크 테스트) |

## 5. 데이터 계약 (구현 완료분)

### PlayerAction (enum)
`None=0, Guard=1(←), Attack=2(→), Dodge=3(↑,미사용), Charge=4(↓,미사용)`

### OutcomeType (enum)
`Cleared=0`(처리) · `Safe=1`(무피해·미처리) · `Punished=2`(피해)

### ActionOutcome (컨테이너)
`action` / `type` / `playerDamage` / `feedback` / `ClearsEnemy(get)`

### EnemyData (컨테이너) · Enemy (SO)
- `id`, `displayName`, `sprite`, `outcomes: List<ActionOutcome>`, `defaultOutcome`
- `Enemy.GetOutcome(action)` → 테이블 조회, 없으면 defaultOutcome
- **개념 정렬**: P0에서 Enemy 1개 = 자세 유형 1개(공세/무방비). 물량(자세 N × 노이즈 M) 변형 시스템은 P1+에서 `poseType` + 스킨으로 확장.

### JudgeResult (struct) · JudgeSystem (static)
- `JudgeSystem.Judge(Enemy, PlayerAction)` → `JudgeResult(Input, Type, PlayerDamage, Cleared, Feedback)`. 상태 없음.

## 6. 데이터 계약 (예정)

### RhythmPatternSO (SO) — 모듈 4
- `spotlightBeats: bool[4]` (제시/응답 마디에서 어느 박이 스포트라이트인지)
  - 안1: `[F,F,F,T]` (4박째 1회) / 안2: `[T,T,T,T]` (매 박)
- `inputsPerCycle` = spotlight 개수.

### EnemySequenceProvider — 모듈 5
- 생성자 `(int seed, IReadOnlyList<Enemy> pool)`.
- `Get(cycleIndex, slotIndex)` = **(seed, cycle, slot)의 순수 함수** → 호출 순서와 무관하게 재현·독립.
- `GenerateCycle(cycleIndex, count)` → 제시 순서 리스트(응답이 이 순서를 재생).

### PhaseSO / ActionWeight — 모듈 5b (페이즈 = 맥락)
- `ActionWeight`: `answerAction`(정답 액션) + `weight`.
- `PhaseSO`: `phaseName` + `List<ActionWeight>`. 정답 액션별 등장 가중치.
- **가중 선택**: `EnemySequenceProvider.GetWeighted(cycle, slot, phase)` — 적을 정답 액션(자세)으로 그룹핑 후, 해시를 [0,1)로 정규화해 가중 추첨 → 그룹 내 재해시 선택. **결정론적**(재현성 유지).
- **매핑(잠정, §8)**: 공격 페이즈=공세(가드)↑, 수비 페이즈=무방비(공격)↑. 값은 SO/인스펙터로 즉시 조정.
- **진행(잠정)**: `RoundManager`가 `phases` 리스트를 `cyclesPerPhase` 사이클마다 순환(결정론적). 둘 다 인스펙터 노출.

## 7. 잠정 수치 (전부 데이터화, 테스트로 결정)

| 항목 | 잠정 기본값 | 근거/비고 |
|---|---|---|
| BPM | **90** | GDD: 초반 저속. 고정/가속 미정 |
| 히로인 체력 | **8** | UI 와이어의 하트 8개. 체력제는 확정 |
| 오답 피해 | **1** | |
| 판정 입력 구간 | **행동별 한 박 전체** | `[박 시작, 다음 박 시작)`의 첫 입력으로 즉시 판정 |
| 무입력 처리 | **오답과 동일(피해)** | 미정 — §9 |
| 한 판 길이 | ~90초 | 시연 고려 |
| 카운트인(준비) | **3초** | `Conductor.startDelay`. 시작 전 여유. 인스펙터 조정 |

## 8. 미정 — 진행하며 사용자 확정 필요

1. **P0 시작 리듬 패턴**: 안1(참는·입력1)로 타이밍 먼저 검증 vs 안2(몰아치는·입력4). (둘 다 데이터로 구현하되 첫 통합 대상)
2. **패턴 배분/전환 규칙**: 고정/혼합/전환 트리거(구간·곡·적). — 후반.
3. **무입력 처리**: 오답과 동일한가 별도인가.
4. ~~**판정 피드백 시점**~~: **응답 행동마다 즉시**로 확정. 한 박 전체에서 첫 입력을 판정하고, 미입력도 해당 박 종료 즉시 처리.
5. **페이즈 매핑 방향(잠정)**: 공격=공세↑/수비=무방비↑ 로 구현. 뒤집을지 확인 필요.
6. **페이즈 진행(잠정)**: `cyclesPerPhase`마다 교대. 확률 전환/구간 연동 등은 후반.

## 9. 변경 이력

- 2026-07-24: 초안(카드형·턴제 가정 — **오독**).
- 2026-07-24: Module 1~3 구현(액션/판정 데이터 + JudgeSystem), 액션-결과 테이블로 자유도 표현.
- 2026-07-24: **기획 SOT를 Notion "박자게임" 페이지로 교체.** 코어 루프를 **콜앤리스폰스(8박 2마디)** 로 정정, 판정 축=자세, P0 자세 2종(공세/무방비), 비트 클록·리듬 2종·결정론적 시퀀스 포함으로 모듈 재계획. 샘플 적을 공세/무방비로 정렬.
- 2026-07-24: **Module 4·5 완료.** `RhythmPatternSO`(안1 `Rhythm_Hold`[F,F,F,T] / 안2 `Rhythm_Rush`[T,T,T,T]), `EnemySequenceProvider`(시드 기반 순수 해시). 라이브 검증: 재현성·독립성·시드민감성 True, 분포 397/403 균형. 컴파일 에러 0.
- 2026-07-24: **Module 6·7·8 코드 작성**(컴파일 에러 0). `Conductor`(Time 기반 BPM 클록, 8박 사이클, 제시/응답 이벤트), `PlayerHealth`(HP, 인스펙터 조정), `InputReader`(키보드 ←/→ + 온스크린 `Press()` 공유), `RoundManager`(제시→응답 오케스트레이션, 스포트라이트 박마다 버퍼 입력 판정, HP 반영, 무입력=오답 옵션). **플레이모드 실동작 검증은 M9 UI + M10 씬 조립 단계에서 수행 예정.** 모든 튜닝 수치 `[SerializeField]` 노출(사용자 원칙).
- 2026-07-25: **판정을 타이밍 고정으로 강화** (이후 같은 날 한 박 전체 입력 구간으로 대체됨). 각 응답 박에 **±판정창**(`hitWindowRatio`, 기본 ±35%)을 두고, 창에 먼저 들어온 입력 하나로 그 박을 확정했다. `Conductor.SongPosition`/`BeatToTime` 추가. 컴파일 0, 예외 0.
- 2026-07-25: **플레이어 idle = 키프레임 스텝(림버스식)**. `KeyframeAnimator`(SR 필수): 스프라이트 배열을 트위닝 없이 순환 — **비트 동기**(박마다/`beatsPerFrame`) 또는 시간(fps) 선택. 행동 스프라이트 표시 중 `Pause()`, idle 복귀 시 `Resume()`(HudView 연동; 프레임 없으면 기존 단일 idle 폴백). PlayerActor 부착·배선(기본: 비트 동기 1박/프레임). **키프레임 스프라이트는 인스펙터 frames 배열에 채우면 됨.** 컴파일 0.
- 2026-07-25: **StageRoster + StageManager (시작 스테이지 인스펙터 관리)**. 기획 확정: 스테이지 1=튜토리얼이고 1↔2는 같은 배경(다이얼로그 후 적만 교체), 3은 배경도 교체. `StageSO`에 `backgroundSprite`/`floorSprite` 추가(씬 '백그라운드'/'바닥' SR에 바꿔끼움, 비우면 유지). `StageRosterSO`(스테이지 목록), `StageManager`(DefaultExecutionOrder -100): 로스터에서 `startStageIndex`(인스펙터, 적/에셋 테스트용) 스테이지를 골라 배경 스왑 + `RoundManager.SetStage`. 에셋: Stage_2(본편1)/Stage_3(배경교체)/Stage_4 생성(기본값 Stage_1 상속, 스프라이트는 인스펙터에서 채움), `StageRoster`=[1,2,3,4]. 씬 배선·검증. 컴파일 0.
- 2026-07-25: **UI 최신화(에셋 기반)**. ① 플레이어 HP: 하트 → `UI_HP_Player_Frame`+Fill **세그먼트 7칸**(자유 배치, `hearts[i].enabled = i<HP`). ② 적 HP: `UI_HP_Enemy_Bar`+`UI_HP_Enemy_Fill` **8칸**(HudView.enemyCells, 구동은 임시 '처리 시 1감소' — 의미(보스HP/남은적수) 미정). ③ 비트 타임라인: 점=위치 마커화, **BeatCursor 2개**(`UI_BeatTimeline_CenterCursor`) — 플레이어 커서 offset0+PresentDot0~3 / 적 커서 offset4+ResponseDot0~3, 박마다 이동+펄스(`beatOffset` 리팩토링). ④ 카운트다운: 텍스트 → `UI_Countdown_3/2/1` 에셋 + 연출(확대·회전·페이드아웃, `CountdownDisplay`). ⑤ 임포트 이슈: 일부 스프라이트 Multiple 모드라 로드 실패 → Single 전환. 컴파일 0.
- 2026-07-25: **배너 미교체 버그 수정**. 원인: 첫 배선 때 컴포넌트 추가가 도메인 리로드로 **저장 유실**돼 `PlayerEnemyPhaseBanner`에 `PhaseBanner`가 없었음(had=False 확인). 플레이 종료 상태에서 재부착·저장(dirtyAfter=False). 플레이 로그로 교체 확인: 제시→`WatchLeft_ActionRight`, 응답→`ActionLeft_WatchRight` 정상 교대. (교훈: 컴포넌트 추가+저장이 리로드와 겹치면 유실될 수 있음 → 플레이 오프 상태에서 저장 후 재검증.)
- 2026-07-25: **차지 봉인 + 마디 배너**. (1) 차지(↓)는 잠정 미사용 — 코드/데이터는 남겨두고(혹시 몰라) InputReader를 2키로 고정, Stage 1도 2키라 게임에선 공/방만. (2) **`PhaseBanner`**: `Conductor`의 제시/응답 마디 이벤트로 `PlayerEnemyPhaseBanner`(HUD Image) 스프라이트 교체 — 제시(적 행동)=`UI_Phase_WatchLeft_ActionRight`, 응답(플레이어 행동)=`UI_Phase_ActionLeft_WatchRight`. 배선·검증(in-place). 컴파일 0.
- 2026-07-25: **스테이지 시스템(StageSO) + Stage 1**. 게임을 **스테이지 1~4** 로 구성(스테이지마다 배경·적·능력 다름). `StageSO`(SO): stageNumber, keyMode(공방=2키/+차징=3키), enemyPool, pattern, phases, cyclesPerPhase, bpm, playerMaxHp, backgroundPrefab(옵션). `RoundManager.stage` 지정 시 Awake에서 `ApplyStage` — 적 풀·키모드(InputReader)·bpm(Conductor)·HP(PlayerData)·페이즈를 스테이지로 덮고, backgroundPrefab 있으면 인스턴스화. `PlayerData.SetMaxHp` 추가. **Stage_1**(2키, 공세/무방비, bpm90, hp8, 배경=씬 기존) 생성·현재 씬 RoundManager에 연결(in-place). 컴파일 0. 배경 전환 방식(씬 분리 vs 프리팹)은 미정.
- 2026-07-25: **그림자 각도 고정 + 버튼 눌림 효과**. (1) 드롭섀도가 액터 자식이라 스프라이트 회전을 상속 → 같이 기욺. `DropShadow2D`가 그림자 **월드 회전을 `groundAngle`(기본 0=수평)로 고정** → 부모 회전과 무관하게 바닥에 평평. (2) `ActionButton`에 **눌림 효과**(축소+틴트 펄스): 해당 액션 입력(`InputReader.OnAction`) 시 재생 → 키보드·온스크린 클릭 모두 시각 피드백. 컴파일 0.
- 2026-07-25: **적 축소 버그 수정 + 드롭섀도**. (1) 버그: `HudView.FitSprite`가 적 등장 시 스프라이트를 `worldHeight`로 자동 리스케일 → 사용자가 준 EnemyActor 스케일(0.40)이 덮여 작아짐(aggressive/armored SO의 sprite가 로봇이라 발동). **자동 리스케일 제거 → 크기는 액터 트랜스폼(수동) 존중.** (2) **`DropShadow2D`**(ExecuteAlways): 자식 "Shadow" SpriteRenderer로 발밑(스프라이트 바운드 하단)에 소프트 타원 그림자, 캐릭터 정렬 아래, 월드 크기/오프셋(부모 스케일 보정), 색·α 조정. 소프트 원형 `shadow_blob.png` 생성. Player/Enemy 액터에 부착(씬 재오픈 없이 in-place). 컴파일 0.
- 2026-07-25: **NeonLight2D 컴포넌트 + 5색 라이트 배치** (사용자: 트랜스폼에 붙여 색 골라 쓰는 라이트 요청, 초록/청색/흰색/하늘색/적색). `NeonLight2D`(ExecuteAlways, Light2D 래핑): 색 프리셋 5종+커스텀, 세기/반경, 옵션 `aimTarget`(콘 조준). 씬 `Lights/` 하위에 Green/Blue/White/Sky/Red 배치(네온 요소 위치). **사용자가 편집 중이라 씬 재오픈 없이 현재 열린 씬(dirty=False 확인)에 in-place 작업 후 저장.** 컴파일 0.
- 2026-07-25: **URP 2D 라이팅 + 파랄랙스** (사용자: 배경/바닥 레이어 넣음, 2D 라이팅·파랄랙스 요청). 프로젝트가 이미 URP 2D 템플릿(`UniversalRP.asset`이 `Renderer2D` 사용) → 카메라 **Orthographic**(2D 라이팅 기준)으로 전환, 스프라이트에 **Sprite-Lit-Default** 머티리얼, **Light2D**(전역 앰비언트 + 시안 무드 포인트), 정렬(배경<바닥<캐릭터). **파랄랙스**(`Parallax` 컴포넌트, 카메라 상대 factor) + **`CameraSway`**(미세 흔들림으로 파랄랙스 구동). `HudView` 색 틴트를 **흰색 기준**으로 바꿔 실제 캐릭터 아트가 자연색으로 표시. 플레이 스크린샷 확인: 사이버펑크 배경+2D 라이팅+캐릭터 정상. 모든 값 인스펙터 조정. 컴파일 0.
- 2026-07-25: **2.5D 대비 — 캐릭터를 월드 SpriteRenderer로 이전** (사용자: 2.5D 분위기 준비, 캐릭터가 UI Canvas 위였음). 원래 prompt 의도(캐릭터=스프라이트, UI만 Canvas)로 바로잡음. `HudView.enemySlot/playerSlot`을 `Image`→`SpriteRenderer`(월드)로 변경, `EnemyActor`/`PlayerActor` 월드 오브젝트 생성(placeholder 흰 사각형 `Assets/Art/Placeholder/white_square.png`), 카메라 Perspective, 구 UI 슬롯·전체화면 BG 이미지 제거(월드 가림 방지). SpriteRenderer는 원본 비율 자동 유지 → 비율 문제 함께 해소. HUD(하트·인디케이터·라벨·버튼)만 Canvas Overlay. 플레이 스크린샷으로 월드 렌더 확인(URP 정상). 컴파일 0. **실제 2.5D 룩(각도·깊이·조명)은 아트 단계에서.**
- 2026-07-25: **스프라이트 원본 비율 처리** (사용자 지적: 고정 슬롯이라 찌그러짐). `HudView.FitSprite` — `preserveAspect=true` + 슬롯 rect를 원본 픽셀 비율에 맞춰 최대 박스(`enemySlotBox`/`playerSlotBox`, 인스펙터) 안에서 스케일. 스프라이트 없으면 박스 크기로 복원(색 폴백). 컴파일 0.
- 2026-07-25: **idle 스프라이트 자리 정리** (사용자 지적: 플레이어/적 idle 둘 곳이 애매). 최소 방식 — 필드만 추가하고 구조는 분산 유지. `PlayerData.idleSprite`(+ guard/attack/charge 배열), `HudView.enemyIdleSprite`, `actionSpriteHold`(복귀 시간). 행동/자세 스프라이트 표시 후 idle 복귀, 아트 없으면 색 폴백(현재 동작 불변). 적 슬롯은 아트가 있으면 `EnemyData.sprite`(자세)를 실제로 표시. 컴파일 0.
- 2026-07-25: **3키(차징) + 적 HP/방어력 전투 + PlayerData/스프라이트** (사용자 요청, 대규모).
    - `InputReader`: `KeyMode` 2/3키 인스펙터 토글, 3키에서 `↓차징`.
    - `EnemyData`: `maxHp`·`armor` 추가. `RoundManager.ApplyJudge`가 판정표 위에 전투 해석 — 공격이 정답이어도 `위력<HP`거나 방어에 막히면 미처리(Safe), **차징 후 강공격(방어 관통·고위력)** 이면 처리.
    - `PlayerHealth` → **`PlayerData`** 리팩터: HP + 공격력/강공격 + 차징 상태 + **행동 시 액션별 스프라이트 배열에서 랜덤 픽**(`OnSpriteChanged`, 아트 없으면 색 폴백). `InputReader` 구독.
    - `HudView`: PlayerData 연동, 주인공 슬롯 스프라이트 표시, `⚡충전` 표시.
    - 샘플 적 `충전유발형`(Charge=Cleared)·`장갑무방비형`(armor 2), 페이즈에 차징 가중치. 씬 풀 4종, 3키 모드.
    - **검증(AutoSolver)**: 예외 0. 충전유발+차징→Cleared, 장갑+일반공격→Safe(방어막힘) 로그 확인. 강공격 관통은 수치로 확정(armor2/hp1 → 일반 dmg0 미처리, 강공격 dmg3 처리). 검증 후 AutoSolver 제거, 컴파일 0, SampleScene 복원.
- 2026-07-25: **버그 수정 — 응답 첫 슬롯 확정 실패.** 원인: `OnResponseMeasureStart`가 `OnBeat`보다 먼저 실행돼 입력 버퍼를 리셋 → 같은 프레임의 첫 응답 박 판정이 항상 None(무입력). 해결: `RoundManager`를 **입력 기반 판정**으로 재작성(버퍼 폐기, 응답 마디 중 누른 순서대로 슬롯 즉시 판정, 마감 시 미답 슬롯만 무입력 처리). 임시 `AutoSolver`로 자동 정답 플레이 → 13사이클+ HP 8 유지(전 슬롯 Cleared) 확인 후 제거. (정밀 타이밍창 ±%는 여전히 §8.)
- 2026-07-25: **카운트인(준비) 추가** (사용자 피드백: 바로 시작해 정신없음). `Conductor.startDelay`(기본 3초) 동안 박 미진행, `Conductor.TimeUntilStart`/`IsCountingDown` 노출. `HudView`가 "준비 3/2/1" 표시. 시작 전 페이즈/피드백 라벨 비움. 플레이 스크린샷으로 "준비 2" 화면 확인. 컴파일 0.
- 2026-07-25: **Module 9(UI) 완료 → P0 코어 목업 완성.** `HudView`/`ActionButton` + Canvas 조립(HP 하트 8, 8박 인디케이터[제시◉◉◉◉▶응답○○○○], 적/주인공 색 점멸 슬롯, 페이즈·피드백 라벨, ←가드/→공격 버튼·키가이드). 플레이모드 스크린샷 2장으로 시각 검증(초반: 풀 HP·공격 페이즈·공세형 등장·제시 박 하이라이트 / 종료: GAME OVER). 씬 기본값 정상화(BPM 90, cyclesPerPhase 2). 컴파일 에러 0.
- 2026-07-25: **씬 조립 + 플레이모드 스모크 테스트 완료 (M6·7·8·10 실동작 검증).** `Assets/Scenes/BeatMemories.unity` 조립(Conductor/Player/InputReader/RoundManager, pattern=Rush, pool=공세/무방비, phases=공격/수비). BPM 120·안2·cyclesPerPhase=1로 플레이 → 로그 확인: 사이클0 공격 페이즈(공세 3/무방비 1 편향), 사이클1 수비 페이즈 전환, 무입력=Punished, HP 8→0, `GAME OVER`+클록 정지. **예외 0.** 테스트 후 SampleScene 복원.
- 2026-07-25: **Module 5b(페이즈=맥락) 완료.** `PhaseSO`/`ActionWeight`, `EnemySequenceProvider.GetWeighted`(결정론적 가중 선택), `RoundManager` 페이즈 순환 연결. 에셋 `Phase_Assault`(가드3:공격1)/`Phase_Defense`(가드1:공격3). 라이브 검증: 편향 73%/74%(목표 75%), 균등 50/50, **재현성 유지 True**. 컴파일 에러 0. (매핑 방향·진행은 잠정 §8-5,6.)
- 2026-07-25: **응답 입력 구간·HP 반영 시점 확정.** 응답 마디의 각 행동은 대응하는 박 전체 `[박 시작, 다음 박 시작)`에서 첫 입력 하나를 판정한다. 구간 종료까지 입력이 없으면 해당 행동만 즉시 무입력 판정하며, 피해는 `PlayerData.TakeDamage`와 `OnHealthChanged`를 통해 행동별로 즉시 반영한다. 네 행동 종료 후 피해를 일괄 적용하지 않는다.
- 2026-07-25: **게임오버 UI + 씬 이동.** `GameOverView`가 `RoundManager.OnGameOver`를 구독해 에셋 기반 배너·끊어진 타임라인·스캔라인/글리치·Retry/Title 버튼을 표시한다. 게임오버에도 배경·캐릭터·양측 HP는 유지하고 전투 조작 UI만 숨긴다. Retry는 현재 전투 씬을 재로드하고 Title은 최소 `Title.unity` 씬으로 이동한다. Build Settings는 Title→BeatMemories 순서.
