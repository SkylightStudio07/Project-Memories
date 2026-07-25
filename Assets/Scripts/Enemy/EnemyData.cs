using System.Collections.Generic;
using UnityEngine;

namespace BeatMemories
{
    /// <summary>
    /// 적 한 종류의 순수 데이터 컨테이너 (MonoBehaviour 아님).
    /// SO(<see cref="Enemy"/>)에 담기거나 런타임에서 값으로 전달된다.
    /// 판정은 단일 정답이 아니라 <see cref="outcomes"/> 액션-결과 테이블로 표현한다.
    /// </summary>
    [System.Serializable]
    public class EnemyData
    {
        [Tooltip("식별용 ID (스폰/디버그/로깅용)")]
        public string id;

        [Tooltip("표시 이름")]
        public string displayName;

        [Tooltip("적 실루엣 스프라이트")]
        public Sprite sprite;

        [Tooltip("이 적이 현재 박자에 수행하는 행동")]
        public PlayerAction action = PlayerAction.None;

        [Header("Enemy Combat Traits")]
        [Min(0)]
        public int attackDamage = 1;

        public bool unblockableAttack;
        public bool invulnerableWhileActing;
        public bool fixedAttackDamage;
        public Enemy forcedFollowUp;
        public Enemy interruptedFollowUp;

        [Tooltip("EnemyActor 기준 레이저 발사 위치의 로컬 오프셋")]
        public Vector2 laserOriginOffset;

        [Tooltip("체력 — 공격 위력이 이 값 이상이어야 처리된다")]
        [Min(1)]
        public int maxHp = 1;

        [Tooltip("방어력 — 일반 공격 위력을 이만큼 깎는다 (강공격은 무시)")]
        [Min(0)]
        public int armor = 0;

        [Tooltip("액션별 판정 결과 테이블. 여기 정의되지 않은 액션은 defaultOutcome을 사용")]
        public List<ActionOutcome> outcomes = new List<ActionOutcome>();

        [Tooltip("테이블에 없는 액션(부적절한 대응 등)에 대한 기본 결과")]
        public ActionOutcome defaultOutcome = new ActionOutcome
        {
            action = PlayerAction.None,
            type = OutcomeType.Punished,
            playerDamage = 1,
            feedback = "잘못된 대응",
        };
    }
}
