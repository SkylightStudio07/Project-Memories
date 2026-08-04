using System.Collections.Generic;
using UnityEngine;

namespace BeatMemories
{
    /// <summary>
    /// 대사 한 세트(데이터 에셋). 스테이지가 이 SO를 참조해 프롤로그·전환 대사를 넘긴다.
    /// 텍스트 분량이 적어 별도 매니저 없이 SO 자체가 데이터 전부를 들고 있다.
    /// </summary>
    [CreateAssetMenu(fileName = "Dialogue_", menuName = "Beat Memories/Dialogue", order = 6)]
    public class DialogueSO : ScriptableObject
    {
        public List<DialogueLine> lines = new List<DialogueLine>();

        [Header("Optional Cinematic Background")]
        public Sprite cinematicBackground;
        [Min(0)] public int showBackgroundForLastLines;
        public bool keepBackgroundVisibleAfterDialogue;
    }
}
