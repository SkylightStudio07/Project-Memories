# TASK

## 6. TimingFrame Scale 유지 방식 변경

현재 TimingFrame은 플레이어가 입력을 완료하면 즉시 사라지고, 다음 박자의 TimingFrame만 표시된다.

이를 아래와 같이 변경한다.

### 구현 요구사항

* 플레이어가 해당 슬롯에 입력한 순간 **또는** 해당 박자의 입력 가능 시간이 종료되어 박자를 놓친 순간,

  * **그 시점의 TimingFrame Scale을 그대로 고정(Freeze)** 한다.
* 이후 해당 슬롯의 TimingFrame은 더 이상 Scale이 변경되지 않는다.
* 다음 슬롯(다음 박)으로 즉시 진행한다.
* 즉, 입력한 타이밍이 TimingFrame의 크기로 그대로 남아 플레이어가 자신의 입력 타이밍을 직관적으로 확인할 수 있어야 한다.

예시

```text
Beat 1
Scale 0.82에서 입력
→ TimingFrame Scale = 0.82로 고정

Beat 2 진행
→ Beat1 TimingFrame은 계속 0.82 유지
→ Beat2 TimingFrame만 새롭게 애니메이션
```

### 판정 결과 표시

TimingFrame이 고정될 때 판정 결과를 색상으로 표시한다.

* 입력이 EarlyOffset ~ LateOffset 범위 안이었다면

  * **초록색(반투명)** 으로 유지
* 범위를 벗어났다면

  * **빨간색(반투명)** 으로 유지

이를 통해 플레이어가 각 박자의 입력 결과를 한눈에 확인할 수 있어야 한다.

---

## 7. 다음 페이즈 진입 시 TimingFrame 초기화

한 페이즈(플레이어 응답 4박)가 종료되면,

다음 **적 행동 표시 페이즈(전반 4박)** 에서는 TimingFrame을 표시하지 않는다.

### 구현 요구사항

* 플레이어 응답 Phase 종료 시

  * 모든 TimingFrame을 숨긴다.
* 적 행동 표시 Phase에서는 TimingFrame이 하나도 보이지 않아야 한다.
* 다음 플레이어 응답 Phase가 시작될 때

  * TimingFrame을 다시 초기 상태로 표시한다.
  * Scale과 색상도 모두 초기화한다.

TimingFrame은 **플레이어 입력 구간에서만 표시되는 UI**여야 한다.

---

## 8. TimingFrame Scale 계산 방식 변경

현재 `TryGetTimingFrameScale()`에서는 Late 판정 구간에서

```csharp
scale = 1f + conductor.LateOffset * lateProgress;
```

를 사용하고 있다.

이 방식은 `LateOffset` 값을 크게 조정할수록 TimingFrame의 Scale도 함께 커지는 문제가 있다.

### 변경 요구사항

`EarlyOffset`과 `LateOffset`은 **판정 시간 범위**만 결정해야 하며,

TimingFrame의 실제 Scale 범위는 별도의 설정값으로 관리한다.

예를 들어

```text
ScaleRange
Min = 0
Max = 1.25
```

이라면

* Early 시작
  → Scale = 0

* Perfect
  → Scale = 1

* Late 종료
  → Scale = 1.25

가 되도록 한다.

즉,

판정 시간은

```text
EarlyOffset ~ Perfect ~ LateOffset
```

을 그대로 사용하지만,

UI Scale은

```text
ScaleRange.Min ~ 1.0 ~ ScaleRange.Max
```

으로 선형 보간하여 계산한다.

또한 **TimingFrame의 ScaleRange를 별도로 조정할 수 있도록 구현**하여, 판정 범위가 리듬게임 특유의 쫀득한 타이밍 감각으로 표현될 수 있도록 한다.

단순히 판정 시간을 표시하는 수준이 아니라, **UI만 보더라도 언제 입력해야 하는지 직관적으로 느껴질 수 있는 연출**을 목표로 한다. 필요하다면 일반적인 리듬게임들의 Timing UI를 참고하여 가장 적합한 표현 방식을 적용한다.

### 구현 요구사항

* `LateOffset` 값을 Scale 계산에 직접 사용하지 않는다.
* TimingFrame의 Scale 범위는 별도의 설정값(예: `ScaleRange`)을 따른다.
* Early/Late 판정 시간이 변경되어도 TimingFrame의 최대 크기는 항상 일정해야 한다.
* 판정 시간과 UI 표현을 명확히 분리하여 관리한다.

---

## 9. 메트로놈(Tick/Tack) 재생 지연 검토 및 개선

현재 메트로놈 효과음(`Tick → Tack → Tack → Tack`)이

* 간헐적으로 끊기는 느낌이 있으며,
* 플레이어 입력보다 약간 늦게 들리는 것처럼 느껴진다.

### 원인 검토

현재 구현을 분석하여 아래 사항을 확인한다.

* AudioSource를 매 Beat마다 새로 생성하고 있지는 않은지
* `Play()` 호출이 프레임 타이밍의 영향을 받고 있지는 않은지
* Beat 이벤트 자체가 실제 Beat보다 늦게 발생하고 있지는 않은지
* Update 이후에 재생되어 한 프레임 늦어지는 구조는 아닌지
* GC Allocation이나 불필요한 객체 생성으로 인해 오디오 재생이 지연되는 부분은 없는지

### 개선 요구사항

가능한 경우 현재 구조를 유지하면서 아래 방향으로 개선한다.

* Beat와 동일한 기준 시점에서 효과음을 재생한다.
* 오디오 재생 타이밍 오차를 최소화한다.
* 프레임 드랍의 영향을 최대한 받지 않도록 구현한다.
* AudioSource와 AudioClip을 재사용하여 불필요한 할당을 방지한다.
* 필요하다면 `AudioSettings.dspTime` 또는 `PlayScheduled()` 등 Unity의 정밀 오디오 재생 방식을 검토하여 적용한다.

### 목표

플레이어가 메트로놈 소리만 듣고도 입력할 수 있을 정도로,

**Tick/Tack 효과음이 캐릭터 애니메이션, TimingFrame, 입력 판정과 모두 동일한 Beat 기준으로 정확하게 동기화**되도록 구현한다.
