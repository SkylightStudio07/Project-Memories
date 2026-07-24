using UnityEngine;
using UnityEngine.UI;

namespace BeatMemories
{
    /// <summary>
    /// 시작 카운트인(3-2-1)을 스프라이트로 표시하고, 각 숫자마다
    /// 커지고 살짝 회전하며 페이드아웃하는 연출을 준다.
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class CountdownDisplay : MonoBehaviour
    {
        [SerializeField] private Conductor conductor;
        [SerializeField] private Sprite three;
        [SerializeField] private Sprite two;
        [SerializeField] private Sprite one;

        [Header("연출 (인스펙터 조정)")]
        [Tooltip("숫자 표시 동안 커지는 최대 배율")]
        [SerializeField] private float endScale = 1.5f;
        [Tooltip("최대 회전각(도) — 좌→우")]
        [SerializeField] private float rotate = 8f;
        [Tooltip("이 진행도(0~1) 이후부터 페이드아웃 시작")]
        [SerializeField, Range(0f, 1f)] private float fadeStart = 0.5f;

        private Image _img;
        private RectTransform _rt;
        private Vector3 _baseScale = Vector3.one;

        private void Awake()
        {
            _img = GetComponent<Image>();
            _rt = (RectTransform)transform;
            _baseScale = _rt.localScale;
        }

        private void OnDisable() { ResetVisual(); }

        private void Update()
        {
            if (conductor == null || _img == null) return;
            double t = conductor.TimeUntilStart;
            int n = Mathf.CeilToInt((float)t);
            if (t <= 0.001 || n < 1 || n > 3) { _img.enabled = false; return; }

            Sprite sp = n >= 3 ? three : (n == 2 ? two : one);
            if (sp == null) { _img.enabled = false; return; }
            _img.enabled = true;
            _img.sprite = sp;

            float p = Mathf.Clamp01(n - (float)t); // 0(등장) → 1(다음 직전)
            float easeOut = 1f - (1f - p) * (1f - p);
            _rt.localScale = _baseScale * Mathf.Lerp(1f, endScale, easeOut);
            _rt.localRotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(-rotate, rotate, p));

            float a = p < fadeStart ? 1f : 1f - (p - fadeStart) / Mathf.Max(1e-4f, 1f - fadeStart);
            var c = _img.color; c.a = Mathf.Clamp01(a); _img.color = c;
        }

        private void ResetVisual()
        {
            if (_img != null) { _img.enabled = false; var c = _img.color; c.a = 1f; _img.color = c; }
            if (_rt != null) { _rt.localScale = _baseScale; _rt.localRotation = Quaternion.identity; }
        }
    }
}
