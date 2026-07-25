using System.Collections.Generic;
using UnityEngine;

namespace BeatMemories
{
    public enum PhaseKind
    {
        Basic,
        Attack,
    }

    /// <summary>정답 대응 액션별 등장 가중치 한 줄.</summary>
    [System.Serializable]
    public class ActionWeight
    {
        [Tooltip("적의 '정답' 대응 액션 (Guard=공세 자세, Attack=무방비 자세)")]
        public PlayerAction answerAction = PlayerAction.Guard;

        [Tooltip("가중치. 클수록 그 자세의 적이 자주 나온다.")]
        [Min(0f)]
        public float weight = 1f;
    }

    /// <summary>
    /// 페이즈 = 적 등장 확률·예고 공개 규칙·준비 연출을 묶는 맥락.
    /// 공격 페이즈는 실제 적/판정을 바꾸지 않고 공세 예고만 차폐한다.
    /// 모든 값은 데이터로 노출 → 밸런싱을 재빌드 없이.
    /// </summary>
    [CreateAssetMenu(fileName = "Phase_", menuName = "Beat Memories/Phase", order = 2)]
    public class PhaseSO : ScriptableObject
    {
        [SerializeField] private string phaseName;

        [Header("규칙")]
        [SerializeField] private PhaseKind kind = PhaseKind.Basic;
        [Tooltip("공격 페이즈에서 예고를 차폐할 실제 자세의 정답 액션")]
        [SerializeField] private PlayerAction hiddenAnswerAction = PlayerAction.Guard;

        [Tooltip("정답 액션별 등장 가중치")]
        [SerializeField] private List<ActionWeight> weights = new List<ActionWeight>();

        [Header("튜토리얼 고정 시퀀스 (교환당 슬롯 순서)")]
        [Tooltip("비우면 가중 생성. 채우면 exchangeInPhase*slotsPerExchange+slot 순서로 사용")]
        [SerializeField] private List<Enemy> authoredSequence = new List<Enemy>();

        [Header("준비·페이즈 연출")]
        [Tooltip("준비 4박 동안 보여줄 대표 자세")]
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
        public Sprite PreparationSprite => preparationSprite;
        public Color CueColor => cueColor;
        public float PreparationTintStrength => preparationTintStrength;
        public float ActiveTintStrength => activeTintStrength;
        public float PreparationSnareVolume => preparationSnareVolume;
        public float ActiveSnareVolume => activeSnareVolume;

        /// <summary>해당 정답 액션의 가중치(미정의 시 0).</summary>
        public float GetWeight(PlayerAction answerAction)
        {
            if (weights != null)
                for (int i = 0; i < weights.Count; i++)
                    if (weights[i] != null && weights[i].answerAction == answerAction)
                        return Mathf.Max(0f, weights[i].weight);
            return 0f;
        }

        /// <summary>이 페이즈의 예고에서 실제 자세를 차폐해야 하는가.</summary>
        public bool ShouldHidePreview(Enemy enemy)
            => IsAttackPhase
               && enemy != null
               && enemy.PrimaryAnswer() == hiddenAnswerAction;

        /// <summary>튜토리얼용 고정 슬롯을 반환. 없으면 호출자가 가중 생성으로 폴백한다.</summary>
        public bool TryGetAuthoredEnemy(int exchangeInPhase, int slot, int slotsPerExchange, out Enemy enemy)
        {
            enemy = null;
            if (authoredSequence == null || slotsPerExchange <= 0 || exchangeInPhase < 0 || slot < 0)
                return false;

            int index = exchangeInPhase * slotsPerExchange + slot;
            if (index < 0 || index >= authoredSequence.Count || authoredSequence[index] == null)
                return false;

            enemy = authoredSequence[index];
            return true;
        }
    }
}
