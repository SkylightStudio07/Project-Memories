namespace BeatMemories
{
    /// <summary>
    /// 순수 판정 로직 (상태 없음). 적과 플레이어의 행동을 받아
    /// 대칭 전투 규칙으로 <see cref="JudgeResult"/>을 만든다.
    /// MonoBehaviour가 아니므로 EditMode 테스트가 쉽다.
    /// </summary>
    public static class JudgeSystem
    {
        public static JudgeResult Judge(Enemy enemy, PlayerAction input, bool chargedAttack = false)
        {
            if (enemy == null)
                return new JudgeResult(input, OutcomeType.Safe, 0, false, "적 없음");

            PlayerAction enemyAction = enemy.Action;
            bool playerAttacks = input == PlayerAction.Attack;
            bool enemyAttacks = enemyAction == PlayerAction.Attack;
            bool playerGuards = input == PlayerAction.Guard;
            bool enemyGuards = enemyAction == PlayerAction.Guard;

            // 공격은 상대가 가드일 때만 무효화된다. 공격/쉼/차징은 방어 상태가 아니다.
            bool attackBlocked = playerGuards && !enemy.UnblockableAttack;
            int playerDamage = enemyAttacks && !attackBlocked
                ? System.Math.Max(0, enemy.AttackDamage)
                : 0;
            if (playerDamage > 0 && input == PlayerAction.Charge && !enemy.FixedAttackDamage)
                playerDamage *= 2;
            bool enemyDamaged = playerAttacks
                && !enemy.InvulnerableWhileActing
                && (!enemyGuards || chargedAttack);

            OutcomeType type = enemyDamaged
                ? OutcomeType.Cleared
                : (playerDamage > 0 ? OutcomeType.Punished : OutcomeType.Safe);

            string feedback;
            if (enemyDamaged && playerDamage > 0) feedback = "상호 공격";
            else if (enemyDamaged) feedback = chargedAttack ? "차징 공격 성공" : "공격 성공";
            else if (playerDamage > 0) feedback = "피격";
            else if (enemyAttacks && playerGuards) feedback = "가드 성공";
            else if (playerAttacks && enemyGuards) feedback = "적 가드에 막힘";
            else feedback = "피해 없음";

            return new JudgeResult(input, type, playerDamage, enemyDamaged, feedback);
        }
    }
}
