namespace BeatMemories
{
    /// <summary>
    /// 순수 판정 로직 (상태 없음). 적과 플레이어 입력을 받아
    /// 액션-결과 테이블을 조회해 <see cref="JudgeResult"/>을 만든다.
    /// MonoBehaviour가 아니므로 EditMode 테스트가 쉽다.
    /// </summary>
    public static class JudgeSystem
    {
        public static JudgeResult Judge(Enemy enemy, PlayerAction input)
        {
            if (enemy == null)
                return new JudgeResult(input, OutcomeType.Safe, 0, false, "적 없음");

            ActionOutcome outcome = enemy.GetOutcome(input);
            if (outcome == null)
                return new JudgeResult(input, OutcomeType.Punished, 1, false, "판정 결과 없음");

            return new JudgeResult(
                input,
                outcome.type,
                outcome.playerDamage,
                outcome.ClearsEnemy,
                outcome.feedback);
        }
    }
}
