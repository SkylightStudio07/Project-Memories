namespace BeatMemories
{
    /// <summary>
    /// 한 턴의 판정 결과. <see cref="JudgeSystem"/>이 생성하고
    /// 매니저/뷰가 소비한다 (HP 반영, 피드백 표시 등).
    /// </summary>
    public readonly struct JudgeResult
    {
        /// <summary>플레이어가 입력한 액션.</summary>
        public readonly PlayerAction Input;

        /// <summary>판정 결과 유형.</summary>
        public readonly OutcomeType Type;

        /// <summary>플레이어가 받는 피해.</summary>
        public readonly int PlayerDamage;

        /// <summary>이 대응으로 적을 처리했는가.</summary>
        public readonly bool Cleared;

        /// <summary>표시용 피드백 문구.</summary>
        public readonly string Feedback;

        public JudgeResult(PlayerAction input, OutcomeType type, int playerDamage, bool cleared, string feedback)
        {
            Input = input;
            Type = type;
            PlayerDamage = playerDamage;
            Cleared = cleared;
            Feedback = feedback;
        }
    }
}
