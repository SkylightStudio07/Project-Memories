using UnityEngine;

namespace BeatMemories
{
    [CreateAssetMenu(menuName = "Beat Memories/Combat Balance Settings")]
    public sealed class CombatBalanceSettings : ScriptableObject
    {
        public const string ResourceName = "CombatBalanceSettings";

        [SerializeField, Min(1f)] private float chargedAttackDamageMultiplier = 3f;
        [SerializeField, Min(1f)] private float chargedLaserWidthMultiplier = 3f;

        public float ChargedAttackDamageMultiplier =>
            Mathf.Max(1f, chargedAttackDamageMultiplier);
        public float ChargedLaserWidthMultiplier =>
            Mathf.Max(1f, chargedLaserWidthMultiplier);

        public static CombatBalanceSettings Load() =>
            Resources.Load<CombatBalanceSettings>(ResourceName);
    }
}
