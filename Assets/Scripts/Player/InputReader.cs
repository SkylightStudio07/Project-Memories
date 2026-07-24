using System;
using UnityEngine;
using UnityEngine.InputSystem;

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

        private void Update()
        {
            if (!enableKeyboard) return;
            var kb = Keyboard.current;
            if (kb == null) return; // 새 입력 백엔드 비활성 등

            if (kb.leftArrowKey.wasPressedThisFrame) Emit(PlayerAction.Guard);
            if (kb.rightArrowKey.wasPressedThisFrame) Emit(PlayerAction.Attack);
            if (keyMode == KeyMode.ThreeKey && kb.downArrowKey.wasPressedThisFrame) Emit(PlayerAction.Charge);
            // ↑ Dodge는 4키 확장 시
        }

        /// <summary>온스크린 버튼 등 외부 입력 주입(같은 이벤트로 흐른다).</summary>
        public void Press(PlayerAction action) => Emit(action);

        private void Emit(PlayerAction action) => OnAction?.Invoke(action);
    }
}
