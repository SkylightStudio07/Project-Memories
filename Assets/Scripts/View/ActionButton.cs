using UnityEngine;
using UnityEngine.UI;

namespace BeatMemories
{
    /// <summary>
    /// 온스크린 액션 버튼. 클릭 시 지정 액션을 <see cref="InputReader.Press"/>로 흘린다.
    /// 또한 키보드/클릭 등 <b>해당 액션 입력이 들어오면 눌림 효과</b>(축소+틴트)를 재생한다
    /// (키보드와 온스크린 버튼이 같은 이벤트로 흐르므로 둘 다 시각 피드백을 받는다).
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class ActionButton : MonoBehaviour
    {
        [SerializeField] private InputReader input;
        [SerializeField] private PlayerAction action = PlayerAction.Guard;

        [Header("눌림 효과 (인스펙터 조정)")]
        [SerializeField, Range(0.5f, 1f)] private float pressScale = 0.86f;
        [SerializeField] private Color pressTint = new Color(0.98f, 0.92f, 0.55f);
        [SerializeField, Min(0.03f)] private float pressDuration = 0.14f;

        private Image _img;
        private RectTransform _rt;
        private Vector3 _baseScale;
        private Color _baseColor;
        private float _t;

        private void Awake()
        {
            _img = GetComponent<Image>();
            _rt = (RectTransform)transform;
            _baseScale = _rt.localScale;
            _baseColor = _img != null ? _img.color : Color.white;
        }

        private void OnEnable() { if (input != null) input.OnAction += OnAction; }
        private void OnDisable() { if (input != null) input.OnAction -= OnAction; }

        private void Start()
        {
            var button = GetComponent<Button>();
            if (button != null) button.onClick.AddListener(OnClick);
        }

        public void OnClick() { if (input != null) input.Press(action); }

        // 이 버튼의 액션 입력이 들어오면 눌림 펄스 시작 (키보드·클릭 공통)
        private void OnAction(PlayerAction a) { if (a == action) _t = 1f; }

        private void Update()
        {
            if (_t <= 0f) return;
            _t = Mathf.MoveTowards(_t, 0f, Time.deltaTime / pressDuration);
            _rt.localScale = Vector3.Lerp(_baseScale, _baseScale * pressScale, _t);
            if (_img != null) _img.color = Color.Lerp(_baseColor, pressTint, _t);
        }
    }
}
