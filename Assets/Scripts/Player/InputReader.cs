using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace BeatMemories
{
    /// <summary>입력 키 모드. 테스트용으로 인스펙터에서 전환.</summary>
    public enum KeyMode
    {
        /// <summary>2키: ←가드 / →공격.</summary>
        TwoKey,
        /// <summary>3키: ←가드 / →공격 / ↓차징.</summary>
        ThreeKey,
    }

    /// <summary>Input System이 기록한 실제 발생 시각을 보존하는 플레이어 입력.</summary>
    public readonly struct TimedPlayerAction
    {
        public readonly PlayerAction Action;
        public readonly double Realtime;
        public readonly int Frame;

        public TimedPlayerAction(PlayerAction action, double realtime, int frame)
        {
            Action = action;
            Realtime = realtime;
            Frame = frame;
        }
    }

    /// <summary>
    /// 플레이어 입력을 <see cref="PlayerAction"/>으로 변환해 방출한다.
    /// 키보드와 온스크린 버튼(<see cref="Press"/>)이 같은 진입점을 공유.
    /// 2키/3키 모드는 테스트용으로 인스펙터에서 전환한다.
    /// </summary>
    public class InputReader : MonoBehaviour
    {
        [Header("입력 모드 (테스트용, 인스펙터 조정)")]
        [Tooltip("2키: ←가드/→공격  ·  3키: +↓차징")]
        [SerializeField] private KeyMode keyMode = KeyMode.TwoKey;
        [SerializeField] private bool enableKeyboard = true;

        public KeyMode Mode { get => keyMode; set => keyMode = value; }

        /// <summary>액션 입력 시 발생.</summary>
        public event Action<PlayerAction> OnAction;
        /// <summary>실제 입력 이벤트 시각을 포함한 판정용 액션.</summary>
        public event Action<TimedPlayerAction> OnTimedAction;
        /// <summary>RoundManager가 현재 행동 구간에 소비한 입력.</summary>
        public event Action<PlayerAction> OnActionAccepted;

        private InputAction guardAction;
        private InputAction attackAction;
        private InputAction chargeAction;

        private void Awake()
        {
            guardAction = new InputAction("Guard", InputActionType.Button, "<Keyboard>/leftArrow");
            attackAction = new InputAction("Attack", InputActionType.Button, "<Keyboard>/rightArrow");
            chargeAction = new InputAction("Charge", InputActionType.Button, "<Keyboard>/downArrow");

            guardAction.performed += OnGuard;
            attackAction.performed += OnAttack;
            chargeAction.performed += OnCharge;
        }

        private void OnEnable()
        {
            if (!enableKeyboard) return;
            guardAction?.Enable();
            attackAction?.Enable();
            chargeAction?.Enable();
        }

        private void OnDisable()
        {
            guardAction?.Disable();
            attackAction?.Disable();
            chargeAction?.Disable();
        }

        private void OnDestroy()
        {
            guardAction?.Dispose();
            attackAction?.Dispose();
            chargeAction?.Dispose();
        }

        private void OnGuard(InputAction.CallbackContext context) => Emit(PlayerAction.Guard, context.time);
        private void OnAttack(InputAction.CallbackContext context) => Emit(PlayerAction.Attack, context.time);
        private void OnCharge(InputAction.CallbackContext context)
        {
            if (keyMode == KeyMode.ThreeKey) Emit(PlayerAction.Charge, context.time);
        }

        /// <summary>현재 키 모드에서 사용할 수 있는 액션인가(2키 스테이지에선 차징 불가).</summary>
        public bool IsActionAvailable(PlayerAction action)
            => action != PlayerAction.Charge || keyMode == KeyMode.ThreeKey;

        /// <summary>온스크린 버튼 등 외부 입력 주입(같은 이벤트로 흐른다).
        /// 키보드와 동일하게 키 모드 제한을 적용한다 — 2키 스테이지에서 차징 버튼을 눌러도 무시.</summary>
        public void Press(PlayerAction action)
        {
            if (!IsActionAvailable(action)) return;
            Emit(action, InputState.currentTime);
        }

        private void Emit(PlayerAction action, double realtime)
        {
            OnAction?.Invoke(action);
            OnTimedAction?.Invoke(new TimedPlayerAction(action, realtime, Time.frameCount));
        }

        internal void NotifyAccepted(PlayerAction action) => OnActionAccepted?.Invoke(action);
    }
}
