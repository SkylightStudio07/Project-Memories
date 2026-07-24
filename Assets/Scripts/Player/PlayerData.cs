using System;
using UnityEngine;

namespace BeatMemories
{
    /// <summary>
    /// 주인공 데이터 허브 (구 PlayerHealth 확장).
    /// 체력 · 공격력/강공격 · 차징 상태 · 행동 스프라이트를 관리한다.
    /// 행동(입력) 시 해당 액션의 스프라이트 배열에서 임의로 하나를 뽑아 리드미컬한 변화를 준다.
    /// (idle 스프라이트/애니메이션은 별도.)
    /// </summary>
    public class PlayerData : MonoBehaviour
    {
        [Header("체력 (인스펙터 조정)")]
        [SerializeField, Min(1)] private int maxHp = 8;
        [SerializeField] private int currentHp = 8;

        [Header("공격력 (인스펙터 조정)")]
        [SerializeField, Min(0)] private int attackPower = 1;
        [Tooltip("차징 후 강공격 위력")]
        [SerializeField, Min(0)] private int chargedAttackPower = 3;
        [Tooltip("강공격은 방어력을 무시")]
        [SerializeField] private bool chargedPiercesArmor = true;

        [Header("스프라이트 (배열에서 랜덤, 비면 색 폴백)")]
        [Tooltip("기본(idle) 스프라이트. 행동이 끝나면 이 스프라이트로 복귀 (없으면 색 폴백)")]
        [SerializeField] private Sprite idleSprite;
        [SerializeField] private Sprite[] guardSprites;
        [SerializeField] private Sprite[] attackSprites;
        [SerializeField] private Sprite[] chargeSprites;
        [Tooltip("여기 입력을 구독해 행동 시 스프라이트를 뽑는다")]
        [SerializeField] private InputReader input;

        // ── 체력 ──
        public int MaxHp => maxHp;
        public int CurrentHp => currentHp;
        public bool IsDead => currentHp <= 0;
        public event Action<int, int> OnHealthChanged;
        public event Action OnDied;

        // ── 공격/차징 ──
        public int AttackPower => attackPower;
        public int ChargedAttackPower => chargedAttackPower;
        public bool ChargedPiercesArmor => chargedPiercesArmor;
        public bool IsCharged { get; private set; }
        public event Action<bool> OnChargedChanged;

        // ── 스프라이트 ──
        /// <summary>기본(idle) 스프라이트. 행동이 끝나면 뷰가 이걸로 복귀시킨다.</summary>
        public Sprite IdleSprite => idleSprite;
        public Sprite CurrentSprite { get; private set; }
        public event Action<Sprite> OnSpriteChanged;

        // 게임플레이 시드와 독립적인 연출용 난수
        private readonly System.Random spriteRng = new System.Random();

        private void Awake() => ResetState();

        private void OnEnable() { if (input != null) input.OnAction += HandleAction; }
        private void OnDisable() { if (input != null) input.OnAction -= HandleAction; }

        public void ResetState()
        {
            currentHp = maxHp;
            IsCharged = false;
            OnHealthChanged?.Invoke(currentHp, maxHp);
            OnChargedChanged?.Invoke(IsCharged);
        }

        /// <summary>최대 체력을 바꾸고 가득 채운다(스테이지 적용 등).</summary>
        public void SetMaxHp(int value)
        {
            maxHp = Mathf.Max(1, value);
            ResetState();
        }

        public void TakeDamage(int amount)
        {
            if (amount <= 0 || IsDead) return;
            currentHp = Mathf.Max(0, currentHp - amount);
            OnHealthChanged?.Invoke(currentHp, maxHp);
            if (currentHp == 0) OnDied?.Invoke();
        }

        public void SetCharged(bool value)
        {
            if (IsCharged == value) return;
            IsCharged = value;
            OnChargedChanged?.Invoke(IsCharged);
        }

        /// <summary>차징 소모. 소모 전 상태를 반환.</summary>
        public bool ConsumeCharge()
        {
            bool was = IsCharged;
            if (was) SetCharged(false);
            return was;
        }

        // 행동 시 스프라이트 랜덤 픽 (모든 입력 = 행동)
        private void HandleAction(PlayerAction action)
        {
            Sprite[] arr = SpritesFor(action);
            if (arr == null || arr.Length == 0) return; // 아트 없으면 색 폴백(HudView)
            CurrentSprite = arr[spriteRng.Next(arr.Length)];
            OnSpriteChanged?.Invoke(CurrentSprite);
        }

        private Sprite[] SpritesFor(PlayerAction action)
        {
            switch (action)
            {
                case PlayerAction.Guard: return guardSprites;
                case PlayerAction.Attack: return attackSprites;
                case PlayerAction.Charge: return chargeSprites;
                default: return null;
            }
        }
    }
}
