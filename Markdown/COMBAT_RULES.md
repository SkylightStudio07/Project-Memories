# Combat Rules

## Action

플레이어와 적은 동일한 행동을 가진다.

Attack
Defense
Charge
Idle

---

## Attack

상대에게 데미지

---

## Defense

공격을 막는다.

Damage = 0

---

## Charge

다음 Attack

x2.5 Damage

Charge 중 피격

x2 Damage

현재 UI에서는 막혀 있음.

코드는 유지한다.

---

## Idle

아무 행동도 하지 않는다.

무방비 상태.

초반 적 패턴에 사용.

Idle Sprite 그대로 사용.

---

## 판정

같은 Beat에서

Attack vs Idle

공격 성공

Attack vs Attack

서로 공격

Attack vs Defense

방어 성공

Defense vs Idle

아무 일 없음

Idle vs Idle

아무 일 없음

...

(표 형태로 정리 가능)