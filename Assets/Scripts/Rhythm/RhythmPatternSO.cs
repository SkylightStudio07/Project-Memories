using System.Collections.Generic;
using UnityEngine;

namespace BeatMemories
{
    /// <summary>
    /// 한 마디(4박)의 리듬 패턴을 정의하는 데이터 기반 SO.
    /// 제시 구간에서 어느 박이 '스포트라이트'(적 등장)인지와 사이클당 행동 수를 담는다.
    ///  - 안1(참는): [F,F,F,T]  → 4박째 1회
    ///  - 안2(몰아치는): [T,T,T,T] → 매 박
    /// 현재 Rush 패턴의 응답은 네 번의 제시가 끝난 다음 박부터 행동 수만큼 연속으로 진행한다.
    /// </summary>
    [CreateAssetMenu(fileName = "Rhythm_", menuName = "Beat Memories/Rhythm Pattern", order = 1)]
    public class RhythmPatternSO : ScriptableObject
    {
        public const int BeatsPerMeasure = 4;

        [SerializeField] private string patternName;

        [Tooltip("제시 4박에서 스포트라이트(적 등장) 박 여부")]
        [SerializeField] private bool[] spotlightBeats = new bool[BeatsPerMeasure];

        public string PatternName => patternName;
        public IReadOnlyList<bool> SpotlightBeats => spotlightBeats;

        /// <summary>스포트라이트 박 개수 = 사이클당 입력 수 = 제시 마디의 적 수.</summary>
        public int SpotlightCount
        {
            get
            {
                int c = 0;
                if (spotlightBeats != null)
                    for (int i = 0; i < spotlightBeats.Length; i++)
                        if (spotlightBeats[i]) c++;
                return c;
            }
        }

        public bool IsSpotlight(int beatInMeasure)
            => spotlightBeats != null
               && beatInMeasure >= 0 && beatInMeasure < spotlightBeats.Length
               && spotlightBeats[beatInMeasure];

        /// <summary>스포트라이트가 켜진 박 인덱스를 순서대로 반환(제시 순서 = 응답 순서).</summary>
        public List<int> SpotlightBeatIndices()
        {
            var list = new List<int>();
            if (spotlightBeats != null)
                for (int i = 0; i < spotlightBeats.Length; i++)
                    if (spotlightBeats[i]) list.Add(i);
            return list;
        }

        private void OnValidate()
        {
            if (spotlightBeats == null || spotlightBeats.Length != BeatsPerMeasure)
            {
                var resized = new bool[BeatsPerMeasure];
                if (spotlightBeats != null)
                    for (int i = 0; i < spotlightBeats.Length && i < BeatsPerMeasure; i++)
                        resized[i] = spotlightBeats[i];
                spotlightBeats = resized;
            }
        }
    }
}
