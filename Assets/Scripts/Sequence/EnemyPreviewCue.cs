namespace BeatMemories
{
    /// <summary>
    /// 뷰에 전달되는 정제된 적 예고. 차폐된 경우 실제 적 참조를 전달하지 않아
    /// 이름·스프라이트·자세색이 UI에서 우회 노출되지 않게 한다.
    /// </summary>
    public readonly struct EnemyPreviewCue
    {
        public readonly int Slot;
        public readonly Enemy VisibleEnemy;
        public readonly bool IsHidden;

        public EnemyPreviewCue(int slot, Enemy visibleEnemy, bool isHidden)
        {
            Slot = slot;
            VisibleEnemy = isHidden ? null : visibleEnemy;
            IsHidden = isHidden;
        }
    }
}
