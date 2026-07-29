# Beat Memories — 개발 아키텍처 (P0 / 1차 목업)

> **기획 SOT(Source of Truth): Notion "박자게임" 페이지.**
> (구 `GGD-Draft.MD`는 폐기 — 콜앤리스폰스 구조로 정정됨.)
> 이 문서는 그 기획을 코드 구조로 옮긴 **개발 설계 기준**이다. 진척은 Notion 진척 페이지에 별도 기록.
>
> **상태: P0 코어 목업 완성(2026-07-25).** 씬 `Assets/Scenes/BeatMemories.unity` 플레이 가능. 남은 것은 밸런싱·아트·확장(§8 미정 항목, 4액션/P1).

## 0. 코어 루프 (콜앤리스폰스)

- **1 사이클 = 8박.** **제시 4박** 직후 **응답 4박**이 이어지며 별도의 휴식 박은 없다.
- **제시**: 적들이 자세(실루엣)를 드러낸다. **응답**: 제시된 **순서 그대로** 되받아친다.
- **제시 순서 = 응답 순서**, **제시 밀도 = 응답 밀도**. 네 번째 적 예고 박이 끝난 다음 박에 아군 첫 행동을 시작한다.
- 네 번째 아군 행동의 입력·미입력 판정은 응답 종료 프레임의 `LateUpdate`에서 먼저 처리하고, 다음 프레임에 새 사이클의 첫 적 예고를 표시한다. 비트 수는 늘어나지 않으며 피해가 적 예고에 귀속되어 보이지 않게 한다.
- 판정: 맞으면 적 처리, 틀리면 피격·체력 감소. 체력 0 종료.
- **판정 축 = 자세**. 색·옷·종족은 판정에 안 걸리는 노이즈(의도된 방해물).

## 1. 확정된 P0 범위

| 항목 | 결정 |
|---|---|
| 액션 | **2/3키 토글**(`InputReader.keyMode`): 2키 `←가드`/`→공격` · 3키 +`↓차징` |
| 자세(적) | **4종**: `공세`(→가드) / `무방비`(→공격) / `충전유발`(→차징) / `장갑무방비`(→차징→강공격) |
| 전투 | 적 `maxHp`·`armor`. 일반 공격은 방어력에 막힘 → **차징하면 강공격(방어 관통·고위력)** 으로 처리 |
| 타이밍 | **비트 클록 포함** — BPM 메트로놈 + 8박 연속 사이클 + 온-비트 판정 |
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
    View/       # 적 슬롯(색 점멸), 8박 타임라인, HUD, 피드백
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
| 6 | 비트 클록 | `Conductor` (DSP 고정 시계, 템포 맵, 선예약 메트로놈, 8박 제시/응답 이벤트) | ✅ 완료(플레이 검증) |
| 7 | 플레이어 | `PlayerHealth`(HP), `InputReader`(←/→) | ✅ 완료(플레이 검증) |
| 8 | 라운드 매니저 | 사이클 오케스트레이션(제시→응답, 판정·HP 반영) | ✅ 완료(플레이 검증) |
| 9 | UI(Canvas) | 적 슬롯(색 점멸), 8박 타임라인, HP 하트, 페이즈·키 가이드·피드백, 온스크린 버튼 | ✅ 완료(시각 검증) |
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
- `spotlightBeats: bool[4]` (제시 구간에서 어느 박이 스포트라이트인지)
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
| BPM | **96** | BGM 실측 템포에 맞춘 고정값. 가속 미정 |
| 히로인 체력 | **8** | UI 와이어의 하트 8개. 체력제는 확정 |
| 오답 피해 | **1** | |
| 판정 입력 구간 | **행동별 한 박 전체** | `[박 시작, 다음 박 시작)`의 첫 입력으로 즉시 판정 |
| 입력 경계 처리 | **발생 시각 기준 + 프레임 말 마감** | Input System 타임스탬프로 박을 찾고, 같은 프레임 입력 큐를 모두 소비한 뒤 미입력을 판정 |
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
- 2026-07-25: **스프라이트 크기 널뛰기 수정 — PPU 정규화 규칙**. idle 아트(465px)가 원본(1123px)보다 저해상도인데 PPU가 같은 100 → idle 전환 시 캐릭터가 41%로 축소. **규칙: 새 캐릭터 아트는 PPU를 `픽셀높이 × 100 / 1123`(= 원본 월드높이 11.23 기준)로 설정.** Idle_1/2/3.psd에 PPU 41.4 적용(단일 값 → 프레임 간 비율 유지) → 월드높이 11.23/11.09/11.09로 일치. 앞으로 가드/공격 스프라이트 추가 시에도 동일 규칙. (화질 자체는 원본 해상도 차이 — 고해상도로 재추출해야 개선.)
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
- 2026-07-25: **Module 9(UI) 완료 → P0 코어 목업 완성.** `HudView`/`ActionButton` + Canvas 조립(HP 하트 8, 8박 인디케이터[제시◉◉◉◉▶응답○○○○], 적/주인공 색 점멸 슬롯, 페이즈·피드백 라벨, ←가드/→공격 버튼·키가이드). 플레이모드 스크린샷 2장으로 시각 검증(초반: 풀 HP·공격 페이즈·공세형 등장·제시 박 하이라이트 / 종료: GAME OVER). 씬 기본값 정상화(BPM 96, cyclesPerPhase 2). 컴파일 에러 0.
- 2026-07-25: **씬 조립 + 플레이모드 스모크 테스트 완료 (M6·7·8·10 실동작 검증).** `Assets/Scenes/BeatMemories.unity` 조립(Conductor/Player/InputReader/RoundManager, pattern=Rush, pool=공세/무방비, phases=공격/수비). BPM 120·안2·cyclesPerPhase=1로 플레이 → 로그 확인: 사이클0 공격 페이즈(공세 3/무방비 1 편향), 사이클1 수비 페이즈 전환, 무입력=Punished, HP 8→0, `GAME OVER`+클록 정지. **예외 0.** 테스트 후 SampleScene 복원.
- 2026-07-25: **Module 5b(페이즈=맥락) 완료.** `PhaseSO`/`ActionWeight`, `EnemySequenceProvider.GetWeighted`(결정론적 가중 선택), `RoundManager` 페이즈 순환 연결. 에셋 `Phase_Assault`(가드3:공격1)/`Phase_Defense`(가드1:공격3). 라이브 검증: 편향 73%/74%(목표 75%), 균등 50/50, **재현성 유지 True**. 컴파일 에러 0. (매핑 방향·진행은 잠정 §8-5,6.)
- 2026-07-25: **응답 입력 구간·HP 반영 시점 확정.** 응답 마디의 각 행동은 대응하는 박 전체 `[박 시작, 다음 박 시작)`에서 첫 입력 하나를 판정한다. 구간 종료까지 입력이 없으면 해당 행동만 즉시 무입력 판정하며, 피해는 `PlayerData.TakeDamage`와 `OnHealthChanged`를 통해 행동별로 즉시 반영한다. 네 행동 종료 후 피해를 일괄 적용하지 않는다.
- 2026-07-25: **게임오버 UI + 씬 이동.** `GameOverView`가 `RoundManager.OnGameOver`를 구독해 에셋 기반 배너·끊어진 타임라인·스캔라인/글리치·Retry/Title 버튼을 표시한다. 게임오버에도 배경·캐릭터·양측 HP는 유지하고 전투 조작 UI만 숨긴다. Retry는 현재 전투 씬을 재로드하고 Title은 최소 `Title.unity` 씬으로 이동한다. Build Settings는 Title→BeatMemories 순서.
- 2026-07-25: **적 예고→아군 행동 전환 확정.** 7박 겹침 해석을 취소하고 `적 예고 4박 → 아군 행동 4박`의 8박 연속 사이클로 확정했다. `Conductor.ResponseStartBeat=4`, `BeatsPerCycle=8`이며 아군 네 번째 행동 종료 직후 다음 사이클을 시작한다.
- 2026-07-26: **Dayeon 실행 씬 DSP 오디오·모션 동기화.** Build Settings의 `BeatMemories_Dayeon`이 공용 Stage 1~5와 `StageSoundtrackCatalogSO`·`RhythmAudioController`를 사용한다. BGM 루프는 `AudioSource.loop` 대신 A/B 소스에 절대 DSP 시각으로 예약하고, Stage 5 페이지 2는 4박 준비 경계에서 95→92 BPM 템포 맵·메트로놈·BGM을 함께 전환한다. `HudView`는 `ClockBeatPosition`을 매 프레임 샘플링해 프레임 드롭 뒤에도 위상에 재합류한다. 기존 `BeatMemories_Dayeon_BGM` 씬과 전용 Stage 복사본은 참고용으로만 유지한다.
- 2026-07-25: **응답 마지막 박 입력 경계 수정.** 키보드·UI 입력을 실제 발생 시각과 프레임 번호를 포함한 큐로 수집하고, `RoundManager.LateUpdate`에서 입력을 모두 소비한 뒤 미입력을 마감한다. 응답 종료와 같은 프레임에 전달된 입력은 네 번째 행동에 우선 귀속하며, 새 사이클 첫 예고는 판정이 끝난 다음 프레임에 표시한다. 행동 스프라이트와 버튼 펄스도 실제 소비된 입력에만 반응한다.
- 2026-07-25: **머지 복구(위상-준비 이벤트 이식).** origin/main의 PR #1 머지(`8b60419`)가 `Conductor`/`RoundManager`의 이벤트 정의 5개를 유실한 채 그것을 쓰는 뷰 스크립트만 남겨 **컴파일 불가**였음(정상본은 `2a38c65`). 정식 아키텍처 통째 복원 대신, 점수·세션 UI를 보존하려 **이벤트만 현재 코드에 이식**: `Conductor`에 `OnClockBeat`(매 박)·`OnPreparationMeasureStart`(선언)·`ScheduledStartDspTime`(StartClock에서 DSP 예약), `RoundManager`에 `OnPhasePreparing`(전환 시 발생)·`OnStageCleared`(선언). 준비 마디 연출은 현재 비트 모델에 준비 마디가 없어 폴백(간략 동작). 컴파일 0.
- 2026-07-26: **버그 수정 — 보스 사망 중 스테이지 전환 시 조작 불가(사망 연출 코루틴 예외).** 증상(사용자 보고): 1스테이지 보스가 죽을 때 공격 버튼을 연타하면 확률적으로 승리 판정이 꼬이고, 다음 스테이지로 넘어간 뒤 **아무 조작도 안 됨**. **원인**: `HudView.PlayDeathSequence`는 히트스톱→카메라 포커스→흔들림→폭발→비행까지 여러 `yield`에 걸친 긴 코루틴인데, 그 도중 스테이지 전환이 일어나면 `StageManager.ApplyCharacterPrefabs`가 적 `CharacterView` 인스턴스를 `Destroy`한다 → 다음 프레임에 코루틴이 파괴된 Transform의 `position`에 접근해 **MissingReferenceException**으로 중단(`HudView.cs:1314`, 콘솔에서 확인). 코루틴 말미의 `deathPresentationActive = false`와 `FadeCombatUi(1f)`가 실행되지 못해, 시작부에서 부른 `HideCombatUi()` 상태가 그대로 남아 **전투 UI가 숨겨진 채 고착**됐다(= 조작 불가). 연타할수록 사망 연출과 전환 타이밍이 겹칠 확률이 올라가 "확률적"으로 보였다. **해결**: 연출 본문을 `try`로 감싸고 각 `yield` 뒤에 `if (actor == null) yield break`로 파괴를 감지해 안전 종료, 정리는 `finally`에 두어 **어떤 경로로 끝나도 복구**되게 했다. finally에 넣은 항목: `deathPresentationActive = false`(UI 숨김 플래그), `FadeCombatUi(1f)`(전투 UI 복구), **`cameraSway.RestoreFocus()`(사망 줌 해제)**. 특히 카메라 복구는 1차 수정에서 빠뜨려 **줌인이 다음 스테이지 전투까지 그대로 이어지는** 후속 증상이 남았고, 정상 경로에 있던 호출을 finally로 옮겨 해결했다. 컴파일 0. **교훈: 파괴 가능한 오브젝트를 다루는 다단계 코루틴은 매 재개 지점에서 생존을 확인하고, 상태 복구는 finally로 보장할 것.**
- 2026-07-26: **다이얼로그 확장 — 보스 페이지 돌입 / 스테이지 클리어 후.** 협업자 요청: 5스테이지 보스 2페이즈 돌입 전과 최종 클리어 후에도 대사가 필요. `StageSO`에 `outroDialogue`(적 처치 직후)와 `pageTransitionDialogues`(List, 인덱스 0 = 2페이지 진입) 추가. **(1) 페이지 돌입**: `BeginNextEnemyPage`가 해당 페이지 대사 유무를 확인해, 있으면 `PauseForStageTransition()`으로 **클록을 멈추고** `OnEnemyPageDialogueRequested(page)` 발생 후 반환(준비 비트 큐잉 생략) → `StageManager`가 대사를 재생하고 `StartRound()`로 **카운트인부터 재개**. 클록이 리셋되므로 페이즈 계산 기준인 `stageStartCycleIndex`도 0으로 함께 맞춤(안 맞추면 `localCycle`이 고정되어 페이즈가 안 넘어감). **(2) 클리어 후**: `StageTransition` 첫머리에서 `outroDialogue`를 재생 → 이후 Act 배너·암전·다음 스테이지, 마지막 스테이지면 게임 클리어 화면. 최종 대사가 `StopAtStageClear()`(=`OnFinalStageCleared` → GameOverView 패널)보다 먼저 나오도록 순서를 잡음. 대사 중 비트 정지 원칙은 기존과 동일. **주의: 이 작업이 GitHub Desktop 자동 stash에 밀려난 뒤 협업자의 DSP 오디오 게이팅(`rhythmAudio`/`gateClockUntilAudioReady`, `Start()` 코루틴화)이 먼저 병합됨 → stash를 통째로 pop하지 않고 대사 훅만 골라 현재 버전에 재적용했다.** 컴파일 0. **대사 SO 연결은 인스펙터에서 사용자가 채움.**
- 2026-07-26: **다이얼로그 보강 — 이름표·줄바꿈·창 배타성 강화.** `DialogueLine.speakerName` 추가(비우면 이전 줄 이름 유지, 포트레잇과 동일 규칙). 각 창에 `SpeakerName` Text 신설(대사 텍스트 바로 위, 볼드+강조색) — **프리팹 소스**(`Assets/Prefabs/UI/DialogueViewer.prefab`)를 `PrefabUtility.LoadPrefabContents`로 직접 열어 추가·배선 후 저장 → 씬을 열지 않고도 연결된 모든 인스턴스에 반영. 본문에 리터럴 `\n`(백슬래시+n) 입력 시 실제 줄바꿈으로 치환(인스펙터 Enter 개행은 원래 지원됨). 창 배타성은 `ShowLine`이 이미 한쪽만 활성화했으나 **Awake 시점엔 두 창 다 활성 기본값**이라 프리팹 편집 중 겹쳐 보일 수 있었음 — Awake·재생 종료 시 양쪽 다 명시적으로 비활성화, `ShowLine`도 반대쪽을 먼저 끄고 켜는 순서로 강화. 컴파일 0.
- 2026-07-26: **버그 수정 — 격파된 적의 '발악' 반격, 그리고 클리어가 게임오버로 덮이는 문제.** 증상(사용자 보고): 적 HP를 다 깎았는데 같은 응답 마디의 다음 슬롯이 공격이면 적이 한 번 더 때리고 죽음. **원인**: `DamageEnemy()`로 `CurrentEnemyHp==0`이 되어도 `stageClearPending`만 세워질 뿐, 스테이지 클리어 처리는 마디 끝(`FinalizeResponseEnd`)에서야 일어남 → 그 사이 `notes`에 남은 슬롯들이 계속 `ApplyJudge`를 타고 `result.PlayerDamage > 0`이면 `player.TakeDamage()` 실행. **파생 위험(더 심각)**: 그 발악 피해로 플레이어가 사망하면 `isOver=true`가 되어 `bool stageCleared = stageClearPending && !isOver;`가 **false로 뒤집혀 스테이지 클리어가 취소되고 게임오버가 뜸** — 마지막 스테이지면 게임 클리어 대신 게임오버. (기존 `ReplaceInterruptedFollowUp`은 차지 콤보 차단용이라 바로 다음 슬롯 1개·`ForcedFollowUp` 설정 적에만 적용되어 이걸 못 막음.) **해결**: `ApplyJudge`에서 판정 직전 `enemyDefeated = stageClearPending || CurrentEnemyHp <= 0`이면 `PlayerDamage`를 0으로 덮어씀("적 격파 — 반격 무효") — 쓰러진 적은 더 이상 반격 못 하되 입력·비트·슬롯 판정은 그대로라 리듬이 안 깨짐. HudView의 적 레이저 연출도 `PlayerDamage>0` 조건이라 자동으로 함께 사라짐. **같은 슬롯에서의 상호 공격(플레이어와 적이 동시에 쓰러짐)은 정상 동작으로 유지** — 그 경우엔 적 HP가 아직 0이 아니므로 피해가 정상 적용되고 게임오버가 뜬다. 컴파일 0.
- 2026-07-26: **다이얼로그 텍스트 TMP 전환 + 타자기 연출.** `DialogueViewer`의 이름/본문 필드를 `UnityEngine.UI.Text` → `TMPro.TMP_Text`로 교체(4개: enemy/player × name/text). 대사는 `charsPerSecond`(인스펙터, 기본 40) 속도로 한 글자씩 타이핑되며, 타이핑 중 클릭하면 즉시 전체 텍스트로 완성(`SkipTyping`)하고 완성된 상태에서 한 번 더 클릭해야 다음 줄로 진행 — 실수로 대사를 건너뛰지 않게. `Assets/Prefabs/UI/DialogueViewer.prefab`을 `LoadPrefabContents`로 직접 열어 레거시 Text→TextMeshProUGUI 교체(폰트=`TMP_Settings.defaultFontAsset`), 필드 재배선. 플레이어 쪽 본문은 사용자가 이미 만들어둔 `Text (TMP)` 오브젝트를 그대로 활용해 스타일만 통일(중복/유령 오브젝트 없음 확인). 컴파일 0.
- 2026-07-26: **버그 수정 — StageManager Awake 순서 경쟁으로 다이얼로그 영구 대기.** 증상: Stage 1에 실제 다이얼로그(SO) 연결 후 타이틀→전투 진입 시 그대로 멈춤. **원인**: `StageManager`(`DefaultExecutionOrder(-100)`)의 `Awake()`가 다른 오브젝트보다 먼저 실행되며 `StartCoroutine(PrologueThenStart)`를 동기적으로 `DialogueViewer.PlayRoutine`까지 파고들어 `root.SetActive(true)`+첫 줄 표시를 이미 끝냄 → 그 직후 뒤늦게 실행된 `DialogueViewer.Awake()`가 (직전 커밋에서 추가한) 방어 코드로 root/두 창을 도로 비활성화 → 코루틴은 `IsPlaying=true`로 계속 대기하는데 UI가 안 보여 클릭 불가, 영구 대기. `introDialogue`가 항상 null이던 동안은 이 경로를 안 타서 잠복해 있다가 실제 콘텐츠 연결로 처음 발현. **해결**: `DialogueViewer.Awake()`에서 활성 상태 조작 코드 전부 제거(실행 순서 의존 없앰) — 초기 비활성 상태는 **프리팹 저장값**(root/enemyWindow/playerWindow 전부 false, `LoadPrefabContents`로 고정)에만 의존. 라이브 진단으로 확인: `IsPlaying=true`인데 `root.activeSelf=false`, 반대쪽 창은 `activeSelf=true` — 특정 줄에서 멈춘 게 아니라 활성 상태 자체가 코루틴 로직과 어긋난 상황임을 확정. 컴파일 0. **교훈: 다른 컴포넌트의 Awake에서 코루틴을 즉시 시작할 수 있는 구조라면(DefaultExecutionOrder로 선행 실행), 대상 컴포넌트 자신의 Awake가 그 상태를 되돌리는 로직을 넣지 말 것 — 초기값은 에셋/프리팹 데이터로 고정.**
- 2026-07-26: **버그 수정 — 4/5스테이지 페이즈 연출(적색광)·보스 예고 차폐 미작동.** 증상: Stage 4/5의 2페이즈(숨김 공격) 진입 시 맵이 적색으로 물드는 연출과 예고 차폐가 전혀 나오지 않음. **원인: 데이터·코드는 전부 정상인데 연출을 실행할 컴포넌트가 `BeatMemories_Dayeon` 씬에만 누락**(원본 `BeatMemories.unity`엔 존재) — 씬 이관 과정에서 `PhasePresentationController`·`BossPagePresentationController` 둘 다 빠짐(씬 내 개수 0). 확인된 정상 요소: `Phase_Stage4/5_HiddenAttack`(kind=Attack, cueColor 적색 0.95/0.05/0.05, tint 0.85), `RoundManager`가 `ShouldHidePreview`로 `hidden` 계산 후 `OnEnemyPreviewed` 발생·`OnEnemyRevealed` 억제, `HudView.OnPreview`가 차폐 시 빈 슬롯 표시. **해결**: 두 컨트롤러를 원본 씬과 동일하게 `RoundManager` 오브젝트에 부착·배선(PPC: background=백그라운드, globalLight=Global Light 2D, targetCamera=Main Camera / BPC: enemyActor, previewSlots=ResponseDot0~3, previewContainer=Response Container). 카메라가 orthographic이라 공격 페이즈 준비 줌도 동작. 저장 후 재검증(dirty=False). **교훈: 씬을 갈아탈 때 매니저·연출 컴포넌트 누락 여부를 컴포넌트 단위로 대조할 것.**
- 2026-07-26: **다이얼로그 리팩터 — SO 컨테이너 + 프리팹 렌더러.** 사용자 피드백: 대사를 StageSO에 직접 리스트로 두지 말고 전용 SO로 분리, 렌더러는 프리팹으로. `DialogueSO`(SO, `lines: List<DialogueLine>`) 신설 → `StageSO.introDialogue`가 `List<DialogueLine>` 대신 `DialogueSO` 참조로 변경(패턴/페이즈 SO 참조와 동일한 결). `DialogueController`를 `DialogueViewer`로 개명(파일+meta 함께 이동해 GUID 보존 → 씬 컴포넌트 참조 안 깨짐), `PlayRoutine(DialogueSO)`로 시그니처 변경. 씬의 `DialogueRoot`를 `Assets/Prefabs/UI/DialogueViewer.prefab`로 추출(`PrefabUtility.SaveAsPrefabAssetAndConnect`) — 씬 인스턴스는 연결된 프리팹 인스턴스로 유지, 다른 씬에서도 재사용 가능. 컴파일 0.
- 2026-07-26: **다이얼로그 시스템(스테이지 시작 전).** 3계층: `DialogueLine`(데이터 컨테이너 — speaker/portrait/text) → `StageSO.introDialogue`(리스트, 인스펙터 입력) → `DialogueController`(View, 우측=적 창/좌측=플레이어 창, 화면 클릭으로 다음 줄) + `StageManager`(오케스트레이션). 프롤로그(스테이지1 진입)와 스테이지 전환(클리어→다음 스테이지 공개) 두 지점 모두 대사 재생 후에만 `round.StartRound()` 호출 — **대사 중 카운트인/비트 진행 없음**. 이를 위해 `Conductor.playOnStart=false`로 바꾸고 시작 시점을 StageManager가 전담. 다이얼로그 창 에셋(`UI_Dialogue_EnemyWindow/PlayerWindow/EnemyFrame/PlayerFrame_v1`) Single 재임포트. `BeatMemories_Dayeon`(최신 통합 씬)에 배선·저장. 포트레잇은 줄마다 교체 가능(비우면 이전 줄 유지). 컴파일 0. **스테이지별 실제 대사 텍스트/포트레잇은 인스펙터에서 사용자가 채워야 함.**
- 2026-07-25: **스테이지 진행(적 처치→다음 스테이지) + 적 보스 HP + 차징 재해금.** (1) **적 보스 HP를 RoundManager로 승격** — `enemyBarHp`(인스펙터, 기본 7), 성공 처치(Cleared)마다 1 감소, `OnEnemyBarChanged`(HudView가 셀 반영)·0이면 `OnStageCleared` 발생. HudView의 임시 로컬 감소 제거. (2) **스테이지 전환** — `RoundManager.RestartForStage(StageSO)`가 풀·패턴·페이즈·provider·적HP·클록을 새 스테이지로 재구성·재시작(카운트인부터). `StageManager`가 `OnStageCleared` 구독 → **암전 페이드(`blackout` CanvasGroup) → 다음 스테이지 적용 → 페이드 인**, 로스터 끝이면 **게임 클리어**(`OnGameCleared` + `gameClearScreen`). 클리어 중 입력·클록 정지 가드(`_stageTransitioning`), 처치로 노트가 비워질 때 `ConsumeInput` 슬롯 캡처로 안전화. (3) **적 자세 다양성** — 전 스테이지 풀이 1종이라 한 자세만 반복하던 것을 Stage_1=[공세·무방비](2키), **Stage_2~5=[공세·무방비·차징](3키, 차징 stage2 해금)** 으로 채움. **주의: 플레이어 HP는 `SetMaxHp`→`ResetState`로 스테이지마다 풀 회복.** 컴파일 0. **연출 배선(`blackout`/`gameClearScreen`/`enemyActor`)·플레이 검증은 미완.**
- 2026-07-29: **Stage 5 페이지 전환 통합 수정.** `enemyPage`와 사이클 기반 `PhaseSO`가 독립적으로 움직여 1페이지에서 숨김 페이즈가 먼저 나오고 2페이지에서 공개 페이즈로 되돌아가던 문제를 `StageSO.phasesFollowEnemyPages`로 분리·정렬했다(Stage 5: 페이지 1=공개, 페이지 2=숨김 공격). 페이지 대사가 있는 경로는 기존 클록을 먼저 정지한 뒤 페이지 이벤트를 발생시키고, `Conductor.StartClock`이 정지 중 예약된 준비 종료 템포를 보존하도록 변경해 대사 후에도 **4박 준비 경계에서 Boss 1 95 BPM → Boss 2 92 BPM** 전환이 유지된다. 카탈로그의 잘못된 `boss_2phase.mp3`/120 BPM 참조를 `Boss_2_92BPM_Loop.wav`/92 BPM으로 교체했다. 보스 예고 절단은 안쪽 아이콘의 `Image.fillAmount`만 줄이던 방식에서 슬롯 컨테이너 전체를 감싸는 런타임 `RectMask2D`로 보강해 프레임과 자세 이미지를 함께 아래 절반만 표시한다. 절단 활성 조건은 준비 이벤트가 아니라 현재 보스 페이지를 기준으로 매 페이즈·사이클마다 재확인하며, 숨김 공격의 노이즈 스프라이트를 전체 높이로 되돌리던 예외도 제거해 2페이지의 모든 예고 슬롯에 동일하게 적용한다. Stage 5 idle/전환/준비 스프라이트의 잘못된 서브에셋 fileID도 실제 스프라이트 ID로 복구했다. 검증: C# 런타임·EditMode 테스트 어셈블리 컴파일 성공, Unity EditMode 53개 중 Stage 5/리듬 오디오 관련 전부 통과(전체 51 통과, 기존 무관 실패 2), PlayMode 2/2 통과.
- 2026-07-29: **적 차지 공격 6 피해 중첩 수정.** Stage 3~5의 차지 공격 에셋에 이미 기본 피해 2가 들어간 상태에서 `RoundManager`의 전역 차지 배율 3이 다시 곱해져 최종 6 피해가 되던 문제를 수정했다. 차지 공격 에셋의 원본 피해를 일반 공격과 같은 1로 통일해 **최종 피해를 1×3=3**으로 확정했다. 플레이어 강공격의 3 피해는 변경하지 않았다. 원본 피해와 RoundManager 최종 피해를 각각 검증하는 EditMode 회귀 테스트를 추가했다.
- 2026-07-29: **WebGL 선행 입력 버퍼 추가.** 판정 성공 범위는 한 박 전체였지만 `RoundManager`가 응답 박 경계 전에 들어온 입력을 프레임 기준으로 바로 폐기해, 음악을 따라 조금 일찍 누르거나 웹 프레임이 흔들릴 때 키가 씹힌 것처럼 느껴졌다. 공용 `RhythmTimingSettings.inputBufferSeconds`(기본 0.15초)를 추가해 응답 시작 직전 입력을 시간 기준으로 보존하고, 이미 처리된 슬롯 다음 행동을 0.15초 이내로 선입력하면 가장 이른 성공 시각으로 보정해 한 슬롯에 한 번만 적용한다. WebGL 빌드에서는 `WebGLInput.captureAllKeyboardInput`도 명시적으로 활성화해 임베디드 캔버스의 방향키가 호스트 페이지로 빠지는 것을 방지한다.
