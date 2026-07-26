using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace BeatMemories
{
    /// <summary>
    /// 스테이지(Act) 클리어 시 해당 Act 배너를 커지며 살짝 회전·페이드하는 연출로 잠깐 표시한다.
    /// (카운트다운과 같은 톤의 연출) StageManager가 전환 연출 중 <see cref="PlayActClear"/>로 호출한다.
    /// 최종 스테이지(게임 클리어)는 GameOverView가 배너+버튼을 재사용해 담당한다.
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class StageClearBanner : MonoBehaviour
    {
        [Tooltip("Act 배너(인덱스 0 = Act1 …). stageNumber로 선택")]
        [SerializeField] private Sprite[] actBanners = new Sprite[4];

        [Header("연출 (인스펙터 조정)")]
        [Tooltip("작게 시작해 원래 크기로 커지는 시간(초)")]
        [SerializeField, Min(0.05f)] private float growDuration = 0.35f;
        [Tooltip("완전히 보이는 유지 시간(초)")]
        [SerializeField, Min(0f)] private float holdSeconds = 1.1f;
        [Tooltip("사라지는 페이드 시간(초)")]
        [SerializeField, Min(0.05f)] private float fadeDuration = 0.45f;
        [Tooltip("등장 시작 배율(작게 → 1)")]
        [SerializeField, Range(0.1f, 1f)] private float startScale = 0.6f;
        [Tooltip("등장 시 기울기(도) → 0으로 정렬")]
        [SerializeField] private float startRotate = 8f;
        [SerializeField, Min(1f)] private float impactStartScale = 1.45f;
        [SerializeField, Range(0.6f, 1f)] private float impactSquashScale = 0.88f;
        [SerializeField, Range(0f, 0.3f)] private float punchStrength = 0.08f;
        [SerializeField, Min(1f)] private float displayScaleMultiplier = 2f;
        [Header("Dim Background")]
        [SerializeField] private Image dimBackground;
        [SerializeField] private Color dimBackgroundColor =
            new Color(0f, 0f, 0f, 225f / 255f);

        private Image _img;
        private RectTransform _rt;
        private RectTransform _dimRt;
        private Vector3 _baseScale = Vector3.one;

        private void Awake()
        {
            _img = GetComponent<Image>();
            _rt = (RectTransform)transform;
            _baseScale = _rt.localScale;
            _img.enabled = false;
            EnsureDimBackground();
            if (dimBackground != null) dimBackground.enabled = false;
        }

        /// <summary>Act 번호(1~4) 배너를 연출과 함께 표시하고, 끝날 때까지 대기한다.</summary>
        public IEnumerator PlayActClear(int actNumber)
        {
            int idx = actNumber - 1;
            Sprite sp = (actBanners != null && idx >= 0 && idx < actBanners.Length) ? actBanners[idx] : null;
            if (_img == null || sp == null) yield break;

            _img.sprite = sp;
            _img.enabled = true;
            EnsureDimBackground();
            if (dimBackground != null)
            {
                dimBackground.enabled = true;
                dimBackground.raycastTarget = true;
                SetDimAlpha(0f);
                _dimRt.SetAsLastSibling();
            }
            _rt.SetAsLastSibling();

            _rt.DOKill();
            _img.DOKill();
            dimBackground?.DOKill();
            Vector3 displayScale = _baseScale * displayScaleMultiplier;
            _rt.localScale = displayScale * impactStartScale;
            _rt.localRotation = Quaternion.Euler(0f, 0f, -startRotate);
            SetAlpha(0f);

            float squashScale = Mathf.Max(startScale, impactSquashScale);
            float impactDuration = Mathf.Max(0.05f, growDuration * 0.38f);
            float settleDuration = Mathf.Max(0.05f, growDuration - impactDuration);
            Sequence impact = DOTween.Sequence().SetUpdate(true);
            impact.Append(_img.DOFade(1f, impactDuration * 0.45f));
            if (dimBackground != null)
                impact.Join(dimBackground
                    .DOFade(dimBackgroundColor.a, impactDuration * 0.75f)
                    .SetEase(Ease.OutQuad));
            impact.Join(_rt.DOScale(
                    displayScale * squashScale,
                    impactDuration)
                .SetEase(Ease.InExpo));
            impact.Join(_rt.DORotate(Vector3.zero, impactDuration)
                .SetEase(Ease.OutQuad));
            impact.Append(_rt.DOScale(displayScale, settleDuration)
                .SetEase(Ease.OutBack, 2.2f));
            if (punchStrength > 0f)
                impact.Append(_rt.DOPunchScale(
                    displayScale * punchStrength,
                    0.16f,
                    7,
                    0.65f));
            yield return impact.WaitForCompletion();

            if (holdSeconds > 0f) yield return new WaitForSecondsRealtime(holdSeconds);

            Sequence outro = DOTween.Sequence().SetUpdate(true);
            outro.Append(_img.DOFade(0f, fadeDuration).SetEase(Ease.InQuad));
            if (dimBackground != null)
                outro.Join(dimBackground.DOFade(0f, fadeDuration)
                    .SetEase(Ease.InQuad));
            outro.Join(_rt.DOScale(displayScale * 1.08f, fadeDuration)
                .SetEase(Ease.InQuad));
            yield return outro.WaitForCompletion();
            Hide();
        }

        public void Hide()
        {
            if (_rt != null) _rt.DOKill();
            if (_img != null) _img.DOKill();
            if (dimBackground != null) dimBackground.DOKill();
            if (_img != null) { _img.enabled = false; SetAlpha(1f); }
            if (dimBackground != null)
            {
                dimBackground.enabled = false;
                SetDimAlpha(0f);
            }
            if (_rt != null) { _rt.localScale = _baseScale; _rt.localRotation = Quaternion.identity; }
        }

        private void EnsureDimBackground()
        {
            if (dimBackground != null)
            {
                _dimRt = dimBackground.rectTransform;
                dimBackground.color = dimBackgroundColor;
                return;
            }
            if (_rt == null || _rt.parent == null) return;

            GameObject dimObject = new GameObject(
                "StageClearDimBackground",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image));
            dimObject.layer = gameObject.layer;
            _dimRt = (RectTransform)dimObject.transform;
            _dimRt.SetParent(_rt.parent, false);
            _dimRt.anchorMin = Vector2.zero;
            _dimRt.anchorMax = Vector2.one;
            _dimRt.offsetMin = Vector2.zero;
            _dimRt.offsetMax = Vector2.zero;
            _dimRt.localScale = Vector3.one;
            dimBackground = dimObject.GetComponent<Image>();
            dimBackground.color = dimBackgroundColor;
            dimBackground.raycastTarget = true;
        }

        private void SetAlpha(float a)
        {
            if (_img == null) return;
            var c = _img.color; c.a = Mathf.Clamp01(a); _img.color = c;
        }

        private void SetDimAlpha(float a)
        {
            if (dimBackground == null) return;
            Color color = dimBackgroundColor;
            color.a = Mathf.Clamp01(a);
            dimBackground.color = color;
        }
    }
}
