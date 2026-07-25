using UnityEngine;

namespace BeatMemories
{
    /// <summary>
    /// 적 종류를 에셋으로 정의하는 데이터 기반 ScriptableObject.
    /// 실제 값은 <see cref="EnemyData"/> 컨테이너에 위임하고,
    /// 액션에 대한 판정 결과 조회를 제공한다.
    /// </summary>
    [CreateAssetMenu(fileName = "Enemy_", menuName = "Beat Memories/Enemy", order = 0)]
    public class Enemy : CharacterData
    {
        [SerializeField] private EnemyData data = new EnemyData();

        /// <summary>원본 데이터 컨테이너.</summary>
        public EnemyData Data => data;

        public string Id => data.id;
        public string DisplayName => data.displayName;
        public Sprite Sprite => data.sprite;
        public PlayerAction Action => data.action;
        public int AttackDamage => data.attackDamage;
        public bool UnblockableAttack => data.unblockableAttack;
        public bool InvulnerableWhileActing => data.invulnerableWhileActing;
        public bool FixedAttackDamage => data.fixedAttackDamage;
        public Enemy ForcedFollowUp => data.forcedFollowUp;
        public Enemy InterruptedFollowUp => data.interruptedFollowUp;
        public Vector2 LaserOriginOffset => data.laserOriginOffset;
        public int MaxHp => data.maxHp;
        public int Armor => data.armor;
        public override string CharacterId => data.id;
        public override string CharacterDisplayName => data.displayName;
        public override Sprite DefaultSprite => data.sprite;

        /// <summary>
        /// 주어진 액션에 대한 판정 결과를 반환한다.
        /// 테이블에 없는 액션이면 <see cref="EnemyData.defaultOutcome"/>을 반환한다.
        /// </summary>
        public ActionOutcome GetOutcome(PlayerAction action)
        {
            var list = data.outcomes;
            if (list != null)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i] != null && list[i].action == action)
                        return list[i];
                }
            }
            return data.defaultOutcome;
        }

        /// <summary>이 적의 대표 '정답' 액션 = 처리(Cleared)를 만드는 첫 액션. 없으면 None.</summary>
        public PlayerAction PrimaryAnswer()
        {
            var list = data.outcomes;
            if (list != null)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i] != null && list[i].type == OutcomeType.Cleared)
                        return list[i].action;
                }
            }
            return PlayerAction.None;
        }
    }
}
