# cyber Punk-Punk 타이틀 개발 기록

## 2026-07-25

### 구현 목표

타이틀 씬의 주요 아트 요소가 DOTween 시퀀스에 따라 순서대로 등장하도록 구성한다.

1. 좌우 Woofer 동시 등장
2. Character 등장
3. Text Art 등장

### 구현 내용

- `Assets/Scripts/UI/Title.cs`
  - 기존 빈 스크립트를 타이틀 인트로 연출 컨트롤러로 구현했다.
  - 각 요소의 원래 위치, 크기, 알파값을 런타임에 저장한 뒤 인트로 시작 상태를 적용한다.
  - 좌우 Woofer는 화면 바깥 방향에서 중앙으로 슬라이드하며 페이드/팝인한다.
  - Character는 아래쪽에서 올라오며 페이드/팝인한다.
  - Text Art는 큰 크기에서 원래 크기로 축소되며 페이드인한다.
  - `Time.timeScale`과 무관하게 타이틀 연출이 재생되도록 `SetUpdate(true)`를 사용했다.
  - 오브젝트 파괴 시 실행 중인 시퀀스를 정리한다.
  - `PlayIntro()`를 공개해 필요할 때 동일한 인트로를 다시 재생할 수 있다.

- `Assets/Scenes/Title.unity`
  - Canvas에 `Title` 컴포넌트를 추가했다.
  - `Woofer_Left`, `Woofer_Right`, `Character`, `Text Art`의 Image 컴포넌트를 직렬화 참조로 연결했다.
  - 기본 연출 시간과 이동 거리 값을 씬에 저장했다.

### 기본 연출 값

| 항목 | 값 |
| --- | ---: |
| 최초 대기 | 0.15초 |
| Woofer 등장 | 0.5초 |
| Character 등장 | 0.5초 |
| Text Art 등장 | 0.45초 |
| 요소 간 간격 | 0.08초 |
| Woofer 시작 오프셋 | 좌우 220px |
| Character 시작 오프셋 | 아래 100px |

모든 값은 Canvas의 `Title` 컴포넌트 Inspector에서 조정할 수 있다.

### 검증

- `dotnet build Project-Memories.slnx` 성공
- 신규 컴파일 오류 0개
- 기존 코드의 미사용 이벤트/필드 경고 3개만 확인

### 참고

- 저장소에는 현재 `Architecture.md`가 존재하지 않는다.
- 연결된 Notion의 `Project Memories` 메인 페이지를 확인했으나 `Architecture.md`에 해당하는 문서는 메인 페이지 본문에서 확인되지 않았다.

## 2026-07-25 — 타이틀 버튼 연출 및 상호작용

### 구현 내용

- 재세팅된 `Start`, `Options`, `Exit` 오브젝트의 위치와 크기를 유지했다.
- 세 오브젝트에 Unity UI `Button`과 공통 `TitleButton` 컴포넌트를 연결했다.
- Text Art 등장 완료 후 1.25초 대기한 다음 아래 순서로 등장한다.
  1. Start
  2. Options
  3. Exit
- 각 버튼은 아래에서 24px 올라오면서 페이드/팝인한다.
- 버튼의 등장 연출이 끝나기 전에는 상호작용을 비활성화한다.
- 마우스 hover 또는 키보드 선택 시:
  - 원래 크기의 1.05배로 확대
  - 해당 `UI_Title_*Button_Hover_v2` 스프라이트로 변경
- 마우스 클릭을 누르는 동안:
  - 원래 크기의 1.09배로 확대
  - Hover v2 스프라이트 유지
- 포인터가 벗어나거나 선택이 해제되면 Normal v2 스프라이트와 원래 크기로 복원한다.
- 모든 확대/복원은 DOTween으로 0.12초 동안 재생한다.

### 추가 파일

- `Assets/Scripts/UI/TitleButton.cs`
  - Start, Options, Exit에서 공통 사용하는 포인터/선택 상호작용 컴포넌트

### 검증

- `dotnet build Project-Memories.slnx --no-restore` 성공
- 신규 컴파일 오류 0개
- 기존 미사용 이벤트/필드 경고 3개만 확인

## 2026-07-25 — Options 화면 전환

### 구현 내용

- `Options` 버튼의 `OnClick`을 `Title.ShowOptions()`에 연결했다.
- Options 진입 시 `Text Art`, `Start`, `Options`, `Exit`가 DOTween으로 페이드아웃되며 0.94배로 축소된다.
- 메인 UI가 사라진 뒤 `OptionPanel`, `BackButton`이 오른쪽 화면 밖에서 원래 위치로 슬라이드 인한다.
- `BackButton`에 Unity UI `Button`과 기존 공통 `TitleButton` 컴포넌트를 적용했다.
- BackButton 상호작용:
  - Normal: `UI_Options_BackButton_Normal_v1`
  - Hover/클릭: `UI_Options_BackButton_Hover_v1`
  - Hover 1.05배, 클릭 중 1.09배 확대
- `BackButton`의 `OnClick`을 `Title.HideOptions()`에 연결했다.
- Back 클릭 시 OptionPanel과 BackButton이 오른쪽으로 퇴장한 뒤 메인 UI가 원래 알파와 크기로 복원된다.
- 화면 전환 중 모든 관련 버튼의 입력을 차단해 중복 전환을 방지한다.
- Options 화면의 기존 최종 위치와 크기는 변경하지 않고 런타임 시작 위치만 오른쪽으로 오프셋했다.

### 기본 전환 값

| 항목 | 값 |
| --- | ---: |
| 메인 UI 퇴장/복원 | 0.25초 |
| 옵션 UI 진입/퇴장 | 0.45초 |
| OptionPanel/BackButton 간격 | 0.08초 |
| 오른쪽 시작 오프셋 | 1800px |

### 검증

- `dotnet build Project-Memories.slnx --no-restore` 성공
- 씬 MonoBehaviour fileID 중복 없음
- Options → `ShowOptions`, BackButton → `HideOptions` 직렬화 이벤트 연결 확인
- 신규 컴파일 오류 0개

## 2026-07-26 — Start 씬 전환

### 구현 내용

- `Start` 버튼의 `OnClick`을 `Title.StartGame()`에 연결했다.
- 클릭 시 중복 입력과 실행 중인 타이틀 DOTween 시퀀스를 정리한 뒤 `BeatMemories` 씬을 로드한다.
- `Assets/Scenes/BeatMemories.unity`가 Build Settings에 활성화된 상태로 등록되어 있음을 확인했다.

### 검증

- `dotnet build Project-Memories.slnx` 성공
- 신규 컴파일 오류 0개

## 2026-07-26 — 옵션 설정 기능

### 구현 내용

- BGM/SFX 슬라이더는 이번 작업 범위에서 제외했다.
- `Input Offset`
  - 좌우 버튼으로 10ms씩 조절한다.
  - 범위는 -200ms부터 +200ms까지이며 `PlayerPrefs`에 저장한다.
  - 저장값을 `RoundManager`의 실제 입력 판정 시각에 반영한다.
- `Resolution`
  - 현재 모니터가 지원하는 해상도 목록을 좌우 버튼으로 순환한다.
  - 선택 즉시 `Screen.SetResolution`으로 적용하고 저장한다.
- `Window Mode`
  - `Fullscreen`, `Borderless`, `Windowed`를 순환한다.
  - 해상도와 함께 즉시 적용하고 저장한다.
- `VSYNC`
  - `Off`, `On`을 전환하고 `QualitySettings.vSyncCount`에 즉시 반영한다.
- 저장된 `InputOffsetNum`, `ResolutionOffsetNum`, `WindowModeOffsetText`,
  `VSyncModeOffsetText` TMP 오브젝트를 `OptionsSettingsController`에 직접 연결했다.
- 설정은 타이틀 씬 재진입 및 게임 재실행 후에도 유지된다.

### 추가 파일

- `Assets/Scripts/Core/GameSettings.cs`
- `Assets/Scripts/UI/OptionsSettingsController.cs`

### 검증

- `dotnet build Project-Memories.slnx --no-restore` 성공
- 신규 컴파일 오류 0개
- 기존 `HudView` 미사용 필드 경고 2개만 확인

## 2026-07-26 — 타이틀 BGM 및 전역 BGM 음량

### 구현 내용

- `Soul Funk Blues by Audio Library Beats (No Copyright Background Music) Dreamscape.mp3`를
  타이틀의 반복 BGM으로 연결하고 긴 음원에 맞게 Streaming으로 임포트한다.
- 타이틀 BGM과 Stage 1~5·Boss BGM을 `Dayeon_BGM_Mixer/Music` 그룹으로 통일했다.
- `BGMSlider`는 `0..1`의 선형 음량을 `settings.bgmVolume`에 저장하고,
  `MusicVolumeDb`로 변환해 Music 그룹에 즉시 적용한다.
- 저장값이 없는 첫 실행의 기본 음량은 `1.0`(100%)이다.
- `Start` 클릭 시 공유 Music 그룹은 유지하고 타이틀 AudioSource의 로컬 음량만
  0.3초 동안 페이드아웃한 뒤 `BeatMemories_Dayeon` 씬을 로드한다.
- SFX 슬라이더와 Metronome 그룹 음량은 이번 작업에서 변경하지 않는다.
