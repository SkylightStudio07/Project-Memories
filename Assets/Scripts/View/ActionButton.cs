using UnityEngine;
using UnityEngine.UI;

namespace BeatMemories
{
    /// <summary>
    /// 온스크린 액션 버튼. 클릭 시 지정 액션을 <see cref="InputReader.Press"/>로 흘린다.
    /// (키보드와 같은 진입점을 공유하므로 판정 경로가 하나다.)
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class ActionButton : MonoBehaviour
    {
        [SerializeField] private InputReader input;
        [SerializeField] private PlayerAction action = PlayerAction.Guard;

        private void Start()
        {
            var button = GetComponent<Button>();
            if (button != null) button.onClick.AddListener(OnClick);
        }

        public void OnClick()
        {
            if (input != null) input.Press(action);
        }
    }
}
