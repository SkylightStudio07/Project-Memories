using UnityEngine;

namespace BeatMemories
{
    /// <summary>
    /// Immutable character definition referenced by a <see cref="CharacterView"/>.
    /// Runtime state stays on controllers such as PlayerData and RoundManager.
    /// </summary>
    public abstract class CharacterData : ScriptableObject
    {
        public abstract string CharacterId { get; }
        public abstract string CharacterDisplayName { get; }
        public abstract Sprite DefaultSprite { get; }
    }
}
