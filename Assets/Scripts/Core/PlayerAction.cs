namespace BeatMemories
{
    /// <summary>
    /// 플레이어가 한 박(턴)에 취할 수 있는 대응 동작.
    /// 1차 목업에서는 Guard/Attack만 입력·판정에 사용하지만,
    /// 향후 Dodge/Charge 확장을 위해 4개를 모두 정의해 둔다.
    /// </summary>
    public enum PlayerAction
    {
        /// <summary>미입력(무입력 판정용 기본값).</summary>
        None = 0,

        /// <summary>← 가드.</summary>
        Guard = 1,

        /// <summary>→ 공격.</summary>
        Attack = 2,

        /// <summary>↑ 회피 (1차 미사용, 확장 예정).</summary>
        Dodge = 3,

        /// <summary>↓ 차징 (1차 미사용, 확장 예정).</summary>
        Charge = 4,
    }
}
