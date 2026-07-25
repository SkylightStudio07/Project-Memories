# TASK - Charge System 개선 및 Charge Aura VFX 구현

## Overview

현재 차징 시스템의 입력 문제를 수정하고, Charge Ready 상태를 명확하게 전달할 수 있는 시각적 피드백을 추가한다.

이번 작업의 목표는 다음과 같다.

1. 차징 입력 문제 해결
2. Charge Aura Particle 관리 구조 개선
3. Charge Ready 상태 전용 Stylized Aura VFX 구현

---

# 1. 차징 키 입력 문제 수정

## Current Issue

현재 아래 방향키를 입력해도 차징 행동이 실행되지 않는다.

- 차징 키: 아래 방향키
- 기대 동작: Charging 상태 진입

입력 처리 또는 상태 전환 로직을 확인하여 원인을 해결한다.

---

## Requirements

- 아래 방향키 입력 시 정상적으로 Charging 상태에 진입해야 한다.
- 기존 Input System 구조를 최대한 유지한다.
- 차징 성공 이후 Charge Ready 상태로 정상 전환되는지 확인한다.
- 기존 이동 및 공격 입력에 영향을 주지 않는다.

---

# 2. Charge Aura Particle 관리 방식 개선

## Current Issue

현재 Particle System을 런타임에서 직접 생성하고 있다.

이 방식은 다음 문제가 있다.

- Inspector에서 디자인 수정이 어렵다.
- 파티클 세부 조정 시 코드 수정이 필요하다.
- VFX 작업자가 직접 수정하기 어렵다.

---

## Change Direction

런타임 생성 방식을 제거하고, Scene에 미리 배치된 Particle System을 사용한다.

---

## Requirements

### Charge Ready 진입

```

ParticleSystem.Play()

```

호출

### Charge Ready 종료

```

ParticleSystem.Stop()

```

호출

---

## Goal

- Unity Inspector에서 Particle 설정을 직접 수정 가능하게 한다.
- 파티클 디자인과 코드 로직을 분리한다.
- 이후 VFX 조정 작업이 쉽게 가능하도록 한다.

---

# 3. Charge Aura VFX 구현

## Goal

플레이어가 차징을 성공하여 **Charge Ready 상태**에 진입했음을 즉시 인식할 수 있도록 카툰 스타일의 에너지 오오라를 추가한다.

중요:

이 오오라는 **Charging 상태를 표현하는 이펙트가 아니다.**

차징 성공 이후,
강화 공격을 사용할 수 있는 상태인 **Charge Ready 상태(Status Effect)** 를 표현한다.

---

# State Flow

```

Normal

↓

Charging
(차징 입력 중)
(별도 Aura 없음)

↓

Charge Ready
(차징 성공)
(Aura 활성화)

↓

Charged Attack
(강화 레이저 발사)

↓

Normal
(강화 상태 소비)
(Aura 제거)

```

---

# Charge Aura Rules

## Activation

Aura는 다음 조건에서만 활성화한다.

- Charge Ready 상태 진입

---

## Deactivation

Aura는 다음 상황에서 제거한다.

- 강화 공격 사용 완료
- Charge Ready 상태 취소
- 플레이어 상태 초기화

---

## Important

- Charging 중에는 Aura를 표시하지 않는다.
- Charge Ready 상태 동안 Aura는 계속 유지한다.
- 단순한 차징 효과가 아니라 "강화 공격 가능 상태"를 표현하는 Status Effect이다.

---

# VFX Design

Charge Ready 상태는 두 가지 효과를 조합한다.

---

# 1. Rim Glow Shader

## Purpose

캐릭터 자체가 에너지를 머금고 있는 느낌을 표현한다.

---

## Requirements

- 캐릭터 외곽선에 Glow 적용
- 레이저 색상과 동일한 색상 사용
- 지속적으로 유지
- Pulse 효과 적용

---

## Pulse Direction

Glow는 고정된 밝기가 아니라 에너지가 순환하는 느낌으로 변화한다.

Example:

```

Low Glow

↓

Increase

↓

Maximum Glow

↓

Decrease

↓

Repeat

```

---

## Implementation Direction

가능하면 다음 방식을 사용한다.

- Fresnel 기반 Rim Glow
- Sprite Outline Glow Shader
- Emission Intensity Pulse

Glow는 강한 번개 효과가 아니라,
캐릭터 주변에 은은하게 흐르는 에너지 느낌을 목표로 한다.

---

# 2. Energy Wisps Particle

## Purpose

캐릭터 내부에 축적된 에너지가 조금씩 밖으로 새어나오는 느낌을 표현한다.

---

## Visual Direction

캐릭터 주변에 작은 빛 입자가 생성된다.

입자는:

```

Spawn

↓

Grow Slightly

↓

Move Upward

↓

Fade Out

↓

Disappear

```

흐름을 가진다.

---

## Particle Behavior

### Spawn Position

- 캐릭터 주변 전체
- 캐릭터를 감싸는 영역
- 몸 표면에서 생성되는 느낌

---

### Movement

- 천천히 위쪽으로 상승
- 약간의 좌우 흔들림
- 부드러운 움직임
- 너무 빠른 속도는 피한다

---

### Shape

추천:

- Soft Circle
- Glow Orb
- Energy Particle

피해야 하는 방향:

- 강한 번개
- 날카로운 스파크
- 폭발적인 파편

전체적으로 둥글고 부드러운 에너지 입자 느낌을 유지한다.

---

# Color

Aura의 모든 색상은 레이저 색상을 기준으로 한다.

Example:

```

Cyan Laser

↓

Cyan Aura

Pink Laser

↓

Pink Aura

```

---

## Requirement

레이저 색상이 변경되면 Aura 색상도 자동으로 변경되어야 한다.

색상을 별도로 관리하지 않고,
레이저 색상 데이터를 참조하는 구조를 권장한다.

---

# State Transition Behavior

## Charge Ready Enter

Execute:

- Rim Glow 활성화
- Glow Pulse 시작
- Particle System Play()

---

## Charge Ready Update

Maintain:

- Glow Pulse 지속
- Particle 지속 재생

---

## Charge Ready Exit

Execute:

- Glow Fade Out
- Particle System Stop()

기존 Particle은 Lifetime 종료 후 자연스럽게 사라진다.

즉시 제거하지 않는다.

---

# Implementation Structure

## Requirements

- Charge Aura는 독립적인 Effect Component로 관리한다.
- Charge Ready On / Off 이벤트로 제어 가능해야 한다.
- Glow와 Particle은 서로 독립적으로 관리한다.
- 이후 Bloom, Distortion, 추가 Shader 효과 확장이 가능해야 한다.

---

# Final Goal

이 Aura는 단순히 "차징 중"임을 표현하는 효과가 아니다.

플레이어에게:

> "현재 강화 공격을 사용할 준비가 완료되었다."

라는 정보를 전달하는 전투 피드백이다.

---

# Visual Direction

Target:

- Stylized Cartoon VFX
- Sci-fi Energy
- Clean Readability
- Anime Game Style

Reference:

- NIKKE
- Zenless Zone Zero
- Blue Archive

Keywords:

- Stylized Energy Aura
- Energy Wisps
- Floating Energy Motes
- Anime Charge Aura
- Sci-fi Energy Charge
- Stylized Rim Glow
- Soft Pulse Glow
- Energy Shedding
```
