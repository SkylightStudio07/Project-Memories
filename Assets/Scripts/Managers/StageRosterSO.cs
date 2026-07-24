using System.Collections.Generic;
using UnityEngine;

namespace BeatMemories
{
    /// <summary>
    /// 게임의 스테이지 순서 목록(로스터). 스테이지 1~4를 순서대로 담는다.
    /// (1↔2는 같은 배경·다이얼로그 후 적 교체, 3은 배경도 교체 — 각 StageSO가 데이터로 정의)
    /// </summary>
    [CreateAssetMenu(fileName = "StageRoster", menuName = "Beat Memories/Stage Roster", order = 4)]
    public class StageRosterSO : ScriptableObject
    {
        public List<StageSO> stages = new List<StageSO>();

        public int Count => stages != null ? stages.Count : 0;

        public StageSO Get(int index)
            => (stages != null && index >= 0 && index < stages.Count) ? stages[index] : null;
    }
}
