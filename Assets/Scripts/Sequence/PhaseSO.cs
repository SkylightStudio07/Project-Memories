using System.Collections.Generic;
using UnityEngine;

namespace BeatMemories
{
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
    /// 페이즈 = 적 등장 확률을 편향시키는 '맥락'. (공격 페이즈/수비 페이즈 등)
    /// 정답 액션별 가중치를 담고, 결정론적 가중 선택에 쓰인다(재현성 유지).
    /// 모든 값은 데이터로 노출 → 밸런싱을 재빌드 없이.
    /// </summary>
    [CreateAssetMenu(fileName = "Phase_", menuName = "Beat Memories/Phase", order = 2)]
    public class PhaseSO : ScriptableObject
    {
        [SerializeField] private string phaseName;

        [Tooltip("정답 액션별 등장 가중치")]
        [SerializeField] private List<ActionWeight> weights = new List<ActionWeight>();

        public string PhaseName => phaseName;
        public IReadOnlyList<ActionWeight> Weights => weights;

        /// <summary>해당 정답 액션의 가중치(미정의 시 0).</summary>
        public float GetWeight(PlayerAction answerAction)
        {
            if (weights != null)
                for (int i = 0; i < weights.Count; i++)
                    if (weights[i] != null && weights[i].answerAction == answerAction)
                        return Mathf.Max(0f, weights[i].weight);
            return 0f;
        }
    }
}
