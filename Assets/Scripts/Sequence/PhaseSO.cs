using System.Collections.Generic;
using UnityEngine;

namespace BeatMemories
{
    public enum PhaseKind
    {
        Basic,
        Attack,
    }

    [System.Serializable]
    public class ActionWeight
    {
        public PlayerAction answerAction = PlayerAction.Guard;

        [Min(0f)]
        public float weight = 1f;
    }

    [System.Serializable]
    public class EnemyWeight
    {
        public Enemy enemy;

        [Min(0f)]
        public float weight = 1f;
    }

    [CreateAssetMenu(fileName = "Phase_", menuName = "Beat Memories/Phase", order = 2)]
    public class PhaseSO : ScriptableObject
    {
        [SerializeField] private string phaseName;

        [Header("Rules")]
        [SerializeField] private PhaseKind kind = PhaseKind.Basic;
        [SerializeField] private PlayerAction hiddenAnswerAction = PlayerAction.Guard;
        [SerializeField] private List<ActionWeight> weights = new List<ActionWeight>();

        [Tooltip("Enemy-specific weights. Empty keeps the legacy answer-action weighting.")]
        [SerializeField] private List<EnemyWeight> enemyWeights = new List<EnemyWeight>();

        [Header("Authored sequence")]
        [SerializeField] private List<Enemy> authoredSequence = new List<Enemy>();

        [Header("Preparation presentation")]
        [SerializeField] private Sprite preparationSprite;
        [SerializeField] private Color cueColor = Color.white;
        [SerializeField, Range(0f, 1f)] private float preparationTintStrength;
        [SerializeField, Range(0f, 1f)] private float activeTintStrength;
        [SerializeField, Range(0f, 1f)] private float preparationSnareVolume = 0.12f;
        [SerializeField, Range(0f, 1f)] private float activeSnareVolume = 0.08f;

        public string PhaseName => phaseName;
        public PhaseKind Kind => kind;
        public bool IsAttackPhase => kind == PhaseKind.Attack;
        public PlayerAction HiddenAnswerAction => hiddenAnswerAction;
        public IReadOnlyList<ActionWeight> Weights => weights;
        public IReadOnlyList<EnemyWeight> EnemyWeights => enemyWeights;
        public bool HasEnemyWeights => enemyWeights != null && enemyWeights.Count > 0;
        public Sprite PreparationSprite => preparationSprite;
        public Color CueColor => cueColor;
        public float PreparationTintStrength => preparationTintStrength;
        public float ActiveTintStrength => activeTintStrength;
        public float PreparationSnareVolume => preparationSnareVolume;
        public float ActiveSnareVolume => activeSnareVolume;

        public float GetWeight(PlayerAction answerAction)
        {
            if (weights != null)
                for (int i = 0; i < weights.Count; i++)
                    if (weights[i] != null && weights[i].answerAction == answerAction)
                        return Mathf.Max(0f, weights[i].weight);
            return 0f;
        }

        public float GetEnemyWeight(Enemy enemy)
        {
            if (enemyWeights != null)
                for (int i = 0; i < enemyWeights.Count; i++)
                    if (enemyWeights[i] != null && enemyWeights[i].enemy == enemy)
                        return Mathf.Max(0f, enemyWeights[i].weight);
            return 0f;
        }

        public bool ShouldHidePreview(Enemy enemy)
            => IsAttackPhase
               && enemy != null
               && enemy.Action == PlayerAction.Attack;

        public bool TryGetAuthoredEnemy(
            int exchangeInPhase,
            int slot,
            int slotsPerExchange,
            out Enemy enemy)
        {
            enemy = null;
            if (authoredSequence == null
                || slotsPerExchange <= 0
                || exchangeInPhase < 0
                || slot < 0)
                return false;

            int index = exchangeInPhase * slotsPerExchange + slot;
            if (index < 0
                || index >= authoredSequence.Count
                || authoredSequence[index] == null)
                return false;

            enemy = authoredSequence[index];
            return true;
        }
    }
}
