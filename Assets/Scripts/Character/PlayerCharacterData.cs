using UnityEngine;

namespace BeatMemories
{
    [CreateAssetMenu(
        fileName = "PlayerCharacter_",
        menuName = "Beat Memories/Player Character",
        order = 1)]
    public sealed class PlayerCharacterData : CharacterData
    {
        [Header("Identity")]
        [SerializeField] private string id = "player";
        [SerializeField] private string displayName = "Player";

        [Header("Combat")]
        [SerializeField, Min(1)] private int maxHp = 8;
        [SerializeField, Min(0)] private int attackPower = 1;
        [SerializeField, Min(1f)] private float chargedAttackMultiplier = 2.5f;
        [SerializeField] private bool chargedPiercesArmor = true;

        [Header("Presentation")]
        [SerializeField] private Sprite idleSprite;
        [SerializeField] private Sprite[] guardSprites;
        [SerializeField] private Sprite[] attackSprites;
        [SerializeField] private Sprite[] chargeSprites;
        [SerializeField] private Sprite timingMistakeSprite;

        public override string CharacterId => id;
        public override string CharacterDisplayName => displayName;
        public override Sprite DefaultSprite => idleSprite;
        public int MaxHp => maxHp;
        public int AttackPower => attackPower;
        public float ChargedAttackMultiplier => chargedAttackMultiplier;
        public bool ChargedPiercesArmor => chargedPiercesArmor;
        public Sprite IdleSprite => idleSprite;
        public Sprite TimingMistakeSprite => timingMistakeSprite;

        public Sprite[] SpritesFor(PlayerAction action)
        {
            switch (action)
            {
                case PlayerAction.Guard: return guardSprites;
                case PlayerAction.Attack: return attackSprites;
                case PlayerAction.Charge: return chargeSprites;
                default: return null;
            }
        }
    }
}
