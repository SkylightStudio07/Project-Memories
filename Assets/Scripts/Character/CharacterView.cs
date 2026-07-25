using System;
using System.Collections.Generic;
using UnityEngine;

namespace BeatMemories
{
    public enum CharacterAnchorType
    {
        LaserMuzzle = 0,
        Hit = 1,
        Effect = 2,
        ProjectileOrigin = 3,
        Ground = 4,
    }

    [Serializable]
    public struct CharacterAnchorBinding
    {
        public CharacterAnchorType type;
        public Transform anchor;
    }

    /// <summary>
    /// Prefab-side character presentation. The prefab owns spatial information;
    /// its ScriptableObject owns immutable character data.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class CharacterView : MonoBehaviour
    {
        [SerializeField] private CharacterData characterData;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Animator animator;

        [Header("Common Anchors")]
        [SerializeField] private Transform laserMuzzle;
        [SerializeField] private Transform hitAnchor;
        [SerializeField] private Transform effectAnchor;

        [Header("Additional Anchors")]
        [SerializeField] private List<CharacterAnchorBinding> additionalAnchors =
            new List<CharacterAnchorBinding>();

        public CharacterData Data => characterData;
        public SpriteRenderer Renderer =>
            spriteRenderer != null ? spriteRenderer : GetComponentInChildren<SpriteRenderer>();
        public Animator Animator =>
            animator != null ? animator : GetComponentInChildren<Animator>();
        public Transform LaserMuzzle => GetAnchor(CharacterAnchorType.LaserMuzzle);
        public Transform HitAnchor => GetAnchor(CharacterAnchorType.Hit);
        public Transform EffectAnchor => GetAnchor(CharacterAnchorType.Effect);

        public Transform GetAnchor(CharacterAnchorType type)
        {
            switch (type)
            {
                case CharacterAnchorType.LaserMuzzle:
                    if (laserMuzzle != null) return laserMuzzle;
                    break;
                case CharacterAnchorType.Hit:
                    if (hitAnchor != null) return hitAnchor;
                    break;
                case CharacterAnchorType.Effect:
                    if (effectAnchor != null) return effectAnchor;
                    break;
            }

            if (additionalAnchors != null)
            {
                for (int i = 0; i < additionalAnchors.Count; i++)
                {
                    CharacterAnchorBinding binding = additionalAnchors[i];
                    if (binding.type == type && binding.anchor != null)
                        return binding.anchor;
                }
            }

            return transform;
        }

        public Vector3 GetAnchorPosition(CharacterAnchorType type)
        {
            Transform anchor = GetAnchor(type);
            if (anchor != transform) return anchor.position;

            SpriteRenderer renderer = Renderer;
            return renderer != null ? renderer.bounds.center : transform.position;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (spriteRenderer == null)
                spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            if (animator == null)
                animator = GetComponentInChildren<Animator>();
        }
#endif
    }
}
