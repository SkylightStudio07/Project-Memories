namespace BeatMemories
{
    /// <summary>
    /// 플레이어의 한 대응이 만들어내는 판정 결과의 유형.
    /// 액션-결과 테이블(<see cref="ActionOutcome"/>)에서 각 액션에 부여된다.
    /// </summary>
    public enum OutcomeType
    {
        /// <summary>적을 처리함(성공적인 대응).</summary>
        Cleared = 0,

        /// <summary>피해 없이 흘림. 처리는 못했지만 안전.</summary>
        Safe = 1,

        /// <summary>불리한 대응 → 플레이어가 피해를 입음.</summary>
        Punished = 2,
    }
}
