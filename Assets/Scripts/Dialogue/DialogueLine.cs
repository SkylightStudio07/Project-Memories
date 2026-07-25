using UnityEngine;

namespace BeatMemories
{
    public enum DialogueSpeaker
    {
        Enemy,
        Player,
    }

    /// <summary>대사 한 줄. 화자(창 위치 결정)·이름·포트레잇·본문을 담는다.</summary>
    [System.Serializable]
    public class DialogueLine
    {
        public DialogueSpeaker speaker = DialogueSpeaker.Enemy;
        [Tooltip("이름표에 표시할 화자 이름. 비우면 이전 줄의 이름 유지")]
        public string speakerName;
        [Tooltip("이 대사에서 표시할 포트레잇. 비우면 이전 줄의 포트레잇 유지")]
        public Sprite portrait;
        [Tooltip("줄바꿈: 인스펙터에서 Enter로 직접 개행하거나, 텍스트에 \\n을 적어도 줄바꿈된다")]
        [TextArea(2, 4)]
        public string text;
    }
}
