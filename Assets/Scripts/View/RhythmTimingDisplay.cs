using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace BeatMemories
{
    /// <summary>
    /// 기존 PlayerBeatCursor 오브젝트와 슬롯 참조를 재사용하는 BPM 포커스 테두리.
    /// 슬롯 진입 시 Fade-in되고 Perfect 시점에 눌린 뒤 복원되며, 판정 후에는 전투 결과 색상을 유지한다.
    /// </summary>
    public sealed class RhythmTimingDisplay : MonoBehaviour
    {
        [SerializeField] private Conductor conductor;
        [SerializeField] private RoundManager round;
        [Tooltip("기존 Cursor 이미지는 테두리에 사용할 Sprite/Material 참조로만 재사용한다.")]
        [SerializeField] private RectTransform cursor;
        [Tooltip("담당하는 첫 박. 응답 UI는 4.")]
        [SerializeField, Min(0)] private int beatOffset = 0;
        [Tooltip("포커스 테두리를 표시할 슬롯들.")]
        [SerializeField] private RectTransform[] dots = new RectTransform[4];

        [Header("포커스 테두리")]
        [FormerlySerializedAs("showInputWindowProgress")]
        [SerializeField] private bool showTimingFrames;
        [SerializeField, Min(1f)] private float outlineThickness = 4f;
        [SerializeField] private Color focusColor = new Color(1f, 1f, 1f, 0.9f);
        [SerializeField] private Color playerDamageColor = new Color(1f, 0.2f, 0.2f, 0.85f);
        [SerializeField] private Color enemyDamageColor = new Color(0.25f, 1f, 0.35f, 0.85f);
        [SerializeField] private Color restColor = new Color(1f, 0.78f, 0.18f, 0.9f);

        [Header("포커스 모션")]
        [Tooltip("박이 시작될 때 테두리 Scale.")]
        [SerializeField, Min(1f)] private float focusStartScale = 1.05f;
        [Tooltip("정확한 Perfect 시점의 Scale.")]
        [SerializeField, Range(0.8f, 1f)] private float focusBeatScale = 0.95f;
        [Tooltip("정박 후 최종 복원 Scale.")]
        [SerializeField, Min(0.9f)] private float focusRestScale = 1f;
        [SerializeField, Min(0.01f)] private float focusFadeDuration = 0.12f;
        [Tooltip("한 박 길이 중 정박 후 복원에 사용할 비율.")]
        [SerializeField, Range(0.05f, 0.5f)] private float focusRestoreRatio = 0.18f;
        [SerializeField] private Ease focusEase = Ease.InOutSine;
        [SerializeField] private Ease focusRestoreEase = Ease.OutBack;

        private RectTransform[] _frameRoots;
        private CanvasGroup[] _frameGroups;
        private Image[][] _frameEdges;
        private Sequence[] _focusTweens;
        private bool[] _resolved;
        private bool[] _timingMiss;
        private Image _templateImage;
        private bool _responseActive;

        private void Awake()
        {
            if (cursor != null)
            {
                _templateImage = cursor.GetComponent<Image>();
                if (_templateImage != null) _templateImage.enabled = false;
            }
            if (showTimingFrames) CreateFrames();
        }

        private void OnEnable()
        {
            if (!showTimingFrames) return;
            if (round != null)
            {
                round.OnTimingFrameResolved += OnTimingFrameResolved;
                round.OnJudged += OnJudged;
            }
            if (conductor != null)
            {
                conductor.OnBeat += OnBeat;
                conductor.OnPresentMeasureStart += OnPresentMeasureStart;
                conductor.OnResponseMeasureStart += OnResponseMeasureStart;
                conductor.OnResponseMeasureEnd += OnResponseMeasureEnd;
            }
        }

        private void OnDisable()
        {
            if (!showTimingFrames) return;
            if (round != null)
            {
                round.OnTimingFrameResolved -= OnTimingFrameResolved;
                round.OnJudged -= OnJudged;
            }
            if (conductor != null)
            {
                conductor.OnBeat -= OnBeat;
                conductor.OnPresentMeasureStart -= OnPresentMeasureStart;
                conductor.OnResponseMeasureStart -= OnResponseMeasureStart;
                conductor.OnResponseMeasureEnd -= OnResponseMeasureEnd;
            }
            HideFrames();
        }

        private void CreateFrames()
        {
            if (cursor == null || dots == null) return;
            int count = dots.Length;
            _frameRoots = new RectTransform[count];
            _frameGroups = new CanvasGroup[count];
            _frameEdges = new Image[count][];
            _focusTweens = new Sequence[count];
            _resolved = new bool[count];
            _timingMiss = new bool[count];

            for (int i = 0; i < count; i++)
            {
                if (dots[i] == null) continue;
                GameObject frameObject = new GameObject(
                    $"TimingFocus{i}",
                    typeof(RectTransform),
                    typeof(CanvasGroup));
                RectTransform root = (RectTransform)frameObject.transform;
                root.SetParent(cursor.parent, false);
                root.sizeDelta = cursor.sizeDelta;
                root.pivot = cursor.pivot;
                root.position = dots[i].position;
                root.localScale = Vector3.one * focusRestScale;

                _frameRoots[i] = root;
                _frameGroups[i] = frameObject.GetComponent<CanvasGroup>();
                _frameEdges[i] = CreateOutlineEdges(root);
                SetFrameColor(i, focusColor);
                frameObject.SetActive(false);
            }
        }

        private Image[] CreateOutlineEdges(RectTransform root)
        {
            return new[]
            {
                CreateEdge(root, "Top", new Vector2(0f, 1f), new Vector2(1f, 1f),
                    new Vector2(0f, outlineThickness)),
                CreateEdge(root, "Bottom", new Vector2(0f, 0f), new Vector2(1f, 0f),
                    new Vector2(0f, outlineThickness)),
                CreateEdge(root, "Left", new Vector2(0f, 0f), new Vector2(0f, 1f),
                    new Vector2(outlineThickness, 0f)),
                CreateEdge(root, "Right", new Vector2(1f, 0f), new Vector2(1f, 1f),
                    new Vector2(outlineThickness, 0f)),
            };
        }

        private Image CreateEdge(
            RectTransform parent,
            string edgeName,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 sizeDelta)
        {
            GameObject edgeObject = new GameObject(
                edgeName,
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image));
            RectTransform edgeTransform = (RectTransform)edgeObject.transform;
            edgeTransform.SetParent(parent, false);
            edgeTransform.anchorMin = anchorMin;
            edgeTransform.anchorMax = anchorMax;
            edgeTransform.anchoredPosition = Vector2.zero;
            edgeTransform.sizeDelta = sizeDelta;

            Image edge = edgeObject.GetComponent<Image>();
            if (_templateImage != null)
            {
                edge.sprite = _templateImage.sprite;
                edge.material = _templateImage.material;
            }
            edge.type = Image.Type.Simple;
            edge.raycastTarget = false;
            return edge;
        }

        private void Update()
        {
            if (!_responseActive || _frameRoots == null) return;
            for (int i = 0; i < _frameRoots.Length; i++)
                if (_frameRoots[i] != null && dots[i] != null)
                    _frameRoots[i].position = dots[i].position;
        }

        private void OnBeat(int beatInCycle)
        {
            if (!_responseActive) return;
            int slot = beatInCycle - beatOffset;
            if (slot >= 0 && _frameRoots != null && slot < _frameRoots.Length)
                StartFocus(slot);
        }

        private void StartFocus(int slot)
        {
            RectTransform root = _frameRoots[slot];
            CanvasGroup group = _frameGroups[slot];
            if (root == null || group == null || _resolved[slot]) return;

            _focusTweens[slot]?.Kill();
            root.gameObject.SetActive(true);
            root.localScale = Vector3.one * focusStartScale;
            group.alpha = 0f;
            SetFrameColor(slot, focusColor);

            float focusDuration = conductor != null
                ? conductor.SecondsPerBeat
                    - Mathf.Min(
                        conductor.LateOffset,
                        conductor.SecondsPerBeat * 0.45f)
                : 0.3f;
            if (round != null
                && conductor != null
                && round.TryGetTimingFrameTargetTime(slot, out double targetTime))
                focusDuration = Mathf.Max(
                    0.01f,
                    (float)(targetTime - conductor.SongPosition));

            float restoreDuration = conductor != null
                ? Mathf.Max(0.03f, conductor.SecondsPerBeat * focusRestoreRatio)
                : 0.1f;
            Sequence sequence = DOTween.Sequence().SetTarget(root);
            sequence.Append(root.DOScale(focusBeatScale, focusDuration).SetEase(focusEase));
            sequence.Join(group.DOFade(1f, Mathf.Min(focusFadeDuration, focusDuration)));
            sequence.Append(root.DOScale(focusRestScale, restoreDuration)
                .SetEase(focusRestoreEase));
            _focusTweens[slot] = sequence;
        }

        private void OnTimingFrameResolved(
            int slot,
            RhythmTimingResult result,
            double songTime)
        {
            if (_frameRoots == null || slot < 0 || slot >= _frameRoots.Length) return;
            _resolved[slot] = true;
            _timingMiss[slot] = result != RhythmTimingResult.Success;
            _focusTweens[slot]?.Kill();
            _focusTweens[slot] = null;

            RectTransform root = _frameRoots[slot];
            if (root == null) return;
            root.gameObject.SetActive(_responseActive);
            root.localScale = Vector3.one * focusRestScale;
            _frameGroups[slot].alpha = 1f;
            SetFrameColor(slot, _timingMiss[slot] ? restColor : focusColor);
        }

        private void OnJudged(int slot, Enemy enemy, JudgeResult result)
        {
            if (_frameRoots == null || slot < 0 || slot >= _frameRoots.Length) return;
            Color resultColor;
            if (result.PlayerDamage > 0)
                resultColor = playerDamageColor;
            else if (result.Cleared)
                resultColor = enemyDamageColor;
            else if (_timingMiss[slot])
                resultColor = restColor;
            else
                resultColor = focusColor;
            SetFrameColor(slot, resultColor);
        }

        private void OnPresentMeasureStart(int cycleIndex)
        {
            HideFrames();
        }

        private void OnResponseMeasureStart(int cycleIndex)
        {
            ResetFrames();
            StartFocus(0);
        }

        private void OnResponseMeasureEnd(int cycleIndex)
        {
            HideFrames();
        }

        private void ResetFrames()
        {
            if (_frameRoots == null) return;
            _responseActive = true;
            for (int i = 0; i < _frameRoots.Length; i++)
            {
                _focusTweens[i]?.Kill();
                _focusTweens[i] = null;
                _resolved[i] = false;
                _timingMiss[i] = false;
                if (_frameRoots[i] == null) continue;
                _frameRoots[i].localScale = Vector3.one * focusRestScale;
                _frameGroups[i].alpha = 0f;
                SetFrameColor(i, focusColor);
                _frameRoots[i].gameObject.SetActive(false);
            }
        }

        private void HideFrames()
        {
            _responseActive = false;
            if (_frameRoots == null) return;
            for (int i = 0; i < _frameRoots.Length; i++)
            {
                _focusTweens[i]?.Kill();
                _focusTweens[i] = null;
                if (_frameRoots[i] != null) _frameRoots[i].gameObject.SetActive(false);
            }
        }

        private void SetFrameColor(int slot, Color color)
        {
            if (_frameEdges == null || slot < 0 || slot >= _frameEdges.Length) return;
            Image[] edges = _frameEdges[slot];
            if (edges == null) return;
            foreach (Image edge in edges)
                if (edge != null) edge.color = color;
        }

        private void OnDestroy()
        {
            if (_frameRoots == null) return;
            for (int i = 0; i < _frameRoots.Length; i++)
            {
                _focusTweens[i]?.Kill();
                if (_frameRoots[i] != null) Destroy(_frameRoots[i].gameObject);
            }
        }
    }
}
