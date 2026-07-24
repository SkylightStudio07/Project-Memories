using UnityEngine;

namespace BeatMemories
{
    /// <summary>
    /// "이 적에게 이 액션을 쓰면 무슨 일이 일어나는가"를 정의하는 데이터 컨테이너.
    /// 하나의 <see cref="Enemy"/>는 여러 개의 ActionOutcome을 테이블로 가진다.
    /// 여러 액션이 상황별로 유효하도록 설계 → 플레이어 자유도를 데이터로 표현한다.
    /// </summary>
    [System.Serializable]
    public class ActionOutcome
    {
        [Tooltip("이 결과가 대응하는 플레이어 액션")]
        public PlayerAction action = PlayerAction.Guard;

        [Tooltip("판정 결과 유형")]
        public OutcomeType type = OutcomeType.Punished;

        [Tooltip("이 대응으로 플레이어가 받는 피해")]
        [Min(0)]
        public int playerDamage = 0;

        [Tooltip("판정 피드백 문구(선택)")]
        [TextArea]
        public string feedback;

        /// <summary>이 결과가 적을 처리(클리어)하는가.</summary>
        public bool ClearsEnemy => type == OutcomeType.Cleared;
    }
}
