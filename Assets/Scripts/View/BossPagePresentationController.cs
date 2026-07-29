using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace BeatMemories
{
    /// <summary>
    /// 보스 페이지 전환 검격과 2페이지 예측창 절단을 담당한다.
    /// 실제 행동 데이터는 건드리지 않고 UI Image의 표시 영역만 줄인다.
    /// </summary>
    [DisallowMultipleComponent]
    public class BossPagePresentationController : MonoBehaviour
    {
        [Header("참조")]
        [SerializeField] private RoundManager round;
        [SerializeField] private Conductor conductor;
        [SerializeField] private StageManager stageManager;
        [SerializeField] private SpriteRenderer enemyActor;
        [SerializeField] private Image[] previewSlots = new Image[Conductor.BeatsPerMeasure];
        [SerializeField] private RectTransform previewContainer;

        [Header("검격")]
        [SerializeField] private Color slashColor = new Color(0.15f, 0.95f, 1f, 1f);
        [SerializeField, Min(1f)] private float slashThickness = 12f;
        [SerializeField, Min(0.01f)] private float slashGrowDuration = 0.12f;
        [SerializeField, Min(0.01f)] private float slashFadeDuration = 0.18f;

        private Image.Type[] originalTypes;
        private Image.FillMethod[] originalFillMethods;
        private int[] originalFillOrigins;
        private float[] originalFillAmounts;
        private bool[] originalFillClockwise;
        private RectMask2D[] previewMasks;
        private RectTransform[] previewMaskRects;
        private bool originalsCaptured;
        private bool masksInitialized;
        private StageSO currentStage;
        private Image slashImage;
        private Coroutine slashRoutine;
        private bool transitionActive;
        private bool previewCutActive;
        private int transitionBeatCount;
        private int currentPage = 1;

        public bool IsPreviewCutActive => previewCutActive;

        private void Awake()
        {
            if (round == null) round = FindFirstObjectByType<RoundManager>();
            if (conductor == null) conductor = FindFirstObjectByType<Conductor>();
            if (stageManager == null)
                stageManager = FindFirstObjectByType<StageManager>();
            CaptureOriginalSlotState();
        }

        private void OnEnable()
        {
            if (round != null)
            {
                round.OnStageApplied += OnStageApplied;
                round.OnEnemyPageTransitionStarted += OnEnemyPageTransitionStarted;
                round.OnEnemyPreviewed += OnEnemyPreviewed;
                round.OnCycleStarted += OnCycleStarted;
                round.OnPhaseChanged += OnPhaseChanged;
            }
            if (conductor != null) conductor.OnPreparationBeat += OnPreparationBeat;
        }

        private void Start()
        {
            if (round != null && round.CurrentStage != null)
                OnStageApplied(round.CurrentStage);
        }

        private void OnDisable()
        {
            if (round != null)
            {
                round.OnStageApplied -= OnStageApplied;
                round.OnEnemyPageTransitionStarted -= OnEnemyPageTransitionStarted;
                round.OnEnemyPreviewed -= OnEnemyPreviewed;
                round.OnCycleStarted -= OnCycleStarted;
                round.OnPhaseChanged -= OnPhaseChanged;
            }
            if (conductor != null) conductor.OnPreparationBeat -= OnPreparationBeat;
            transitionActive = false;
            SetPreviewCutActive(false);
            HideSlash();
        }

        private void OnStageApplied(StageSO appliedStage)
        {
            currentStage = appliedStage;
            currentPage = round != null ? round.CurrentEnemyPage : 1;
            transitionActive = false;
            transitionBeatCount = 0;
            SetPreviewCutActive(ShouldCutCurrentPage());
            HideSlash();
        }

        private void OnEnemyPageTransitionStarted(int page, int pageCount, int preparationBeats)
        {
            currentPage = Mathf.Max(1, page);
            if (currentStage == null
                || !currentStage.cutPreviewBottomHalfOnSecondPage
                || page < 2)
                return;

            transitionActive = true;
            transitionBeatCount = Mathf.Max(0, preparationBeats);
            // 준비 비트 이벤트가 누락되거나 대사 뒤 클록이 재시작되더라도
            // 페이지 상태만으로 절단이 유지되어야 한다.
            SetPreviewCutActive(true);
            SpriteRenderer activeEnemyActor = stageManager != null
                ? stageManager.EnemyActor
                : enemyActor;
            if (activeEnemyActor != null
                && currentStage.enemyPageTransitionSprite != null)
            {
                activeEnemyActor.sprite =
                    currentStage.enemyPageTransitionSprite;
            }

            if (transitionBeatCount <= 0)
            {
                PlaySlash();
            }
        }

        private void OnPreparationBeat(int beat)
        {
            if (!transitionActive) return;
            int slashBeat = Mathf.Min(1, Mathf.Max(0, transitionBeatCount - 1));
            if (beat != slashBeat) return;

            SetPreviewCutActive(true);
            PlaySlash();
        }

        private void OnPhaseChanged(int cycleIndex, PhaseSO phase)
        {
            transitionActive = false;
            SetPreviewCutActive(ShouldCutCurrentPage());
            HideSlash();
        }

        private void OnCycleStarted(int cycleIndex)
        {
            if (round != null) currentPage = round.CurrentEnemyPage;
            SetPreviewCutActive(ShouldCutCurrentPage());
            if (!previewCutActive) return;
            for (int i = 0; i < previewSlots.Length; i++)
                ApplyBottomHalf(i);
        }

        private void OnEnemyPreviewed(EnemyPreviewCue cue)
        {
            if (!previewCutActive) return;
            // 숨김 공격의 노이즈 스프라이트도 2페이지 기믹의 대상이다.
            // 여기서 전체 높이로 복원하면 가장 중요한 공격 슬롯만 절단이 풀린다.
            ApplyBottomHalf(cue.Slot);
        }

        private bool ShouldCutCurrentPage()
            => currentStage != null
               && currentStage.cutPreviewBottomHalfOnSecondPage
               && currentPage >= 2;

        private void SetPreviewCutActive(bool active)
        {
            CaptureOriginalSlotState();
            EnsurePreviewMasks();
            previewCutActive = active;
            for (int i = 0; i < previewSlots.Length; i++)
            {
                if (active) ApplyBottomHalf(i);
                else RestoreSlot(i);
            }
        }

        private void CaptureOriginalSlotState()
        {
            if (originalsCaptured || previewSlots == null) return;

            int count = previewSlots.Length;
            originalTypes = new Image.Type[count];
            originalFillMethods = new Image.FillMethod[count];
            originalFillOrigins = new int[count];
            originalFillAmounts = new float[count];
            originalFillClockwise = new bool[count];
            for (int i = 0; i < count; i++)
            {
                Image slot = previewSlots[i];
                if (slot == null) continue;
                originalTypes[i] = slot.type;
                originalFillMethods[i] = slot.fillMethod;
                originalFillOrigins[i] = slot.fillOrigin;
                originalFillAmounts[i] = slot.fillAmount;
                originalFillClockwise[i] = slot.fillClockwise;
            }
            originalsCaptured = true;
        }

        // 슬롯 안쪽 아이콘의 Image.fillAmount만 바꾸면 바깥 프레임은 온전히
        // 남는다. 각 슬롯 컨테이너 위에 RectMask2D 래퍼를 런타임 생성해
        // 프레임과 아이콘을 함께 절단한다.
        private void EnsurePreviewMasks()
        {
            if (masksInitialized || previewSlots == null) return;

            previewMasks = new RectMask2D[previewSlots.Length];
            previewMaskRects = new RectTransform[previewSlots.Length];
            for (int i = 0; i < previewSlots.Length; i++)
            {
                Image slot = previewSlots[i];
                RectTransform slotContainer =
                    slot != null ? slot.rectTransform.parent as RectTransform : null;
                RectTransform parent =
                    slotContainer != null ? slotContainer.parent as RectTransform : null;
                if (slotContainer == null || parent == null) continue;

                int siblingIndex = slotContainer.GetSiblingIndex();
                var wrapperObject = new GameObject(
                    $"BossPreviewMask_{i}",
                    typeof(RectTransform),
                    typeof(RectMask2D),
                    typeof(LayoutElement));
                wrapperObject.layer = slotContainer.gameObject.layer;

                RectTransform wrapper = wrapperObject.GetComponent<RectTransform>();
                wrapper.SetParent(parent, false);
                wrapper.SetSiblingIndex(siblingIndex);
                wrapper.anchorMin = slotContainer.anchorMin;
                wrapper.anchorMax = slotContainer.anchorMax;
                wrapper.anchoredPosition = slotContainer.anchoredPosition;
                wrapper.sizeDelta = slotContainer.sizeDelta;
                wrapper.pivot = slotContainer.pivot;
                wrapper.localRotation = slotContainer.localRotation;
                wrapper.localScale = slotContainer.localScale;

                var layout = wrapperObject.GetComponent<LayoutElement>();
                layout.preferredWidth = Mathf.Abs(slotContainer.rect.width);
                layout.preferredHeight = Mathf.Abs(slotContainer.rect.height);

                Vector2 containerSize = slotContainer.sizeDelta;
                slotContainer.SetParent(wrapper, false);
                slotContainer.anchorMin = new Vector2(0.5f, 0.5f);
                slotContainer.anchorMax = new Vector2(0.5f, 0.5f);
                slotContainer.anchoredPosition = Vector2.zero;
                slotContainer.sizeDelta = containerSize;
                slotContainer.localRotation = Quaternion.identity;
                slotContainer.localScale = Vector3.one;

                previewMaskRects[i] = wrapper;
                previewMasks[i] = wrapperObject.GetComponent<RectMask2D>();
            }
            masksInitialized = true;
        }

        private void ApplyBottomHalf(int index)
        {
            Image slot = Slot(index);
            if (slot == null) return;
            slot.type = Image.Type.Filled;
            slot.fillMethod = Image.FillMethod.Vertical;
            slot.fillOrigin = (int)Image.OriginVertical.Bottom;
            slot.fillAmount = 0.5f;
            slot.fillClockwise = false;
            SetMaskCut(index, true);
        }

        private void RestoreSlot(int index)
        {
            Image slot = Slot(index);
            if (slot == null || !originalsCaptured) return;
            slot.type = originalTypes[index];
            slot.fillMethod = originalFillMethods[index];
            slot.fillOrigin = originalFillOrigins[index];
            slot.fillAmount = originalFillAmounts[index];
            slot.fillClockwise = originalFillClockwise[index];
            SetMaskCut(index, false);
        }

        private void SetMaskCut(int index, bool cut)
        {
            if (previewMasks == null
                || previewMaskRects == null
                || index < 0
                || index >= previewMasks.Length
                || previewMasks[index] == null
                || previewMaskRects[index] == null)
                return;

            float height = Mathf.Abs(previewMaskRects[index].rect.height);
            if (height <= 0f)
                height = Mathf.Abs(previewMaskRects[index].sizeDelta.y);
            previewMasks[index].padding = cut
                ? new Vector4(0f, 0f, 0f, height * 0.5f)
                : Vector4.zero;
        }

        private Image Slot(int index)
            => previewSlots != null && index >= 0 && index < previewSlots.Length
                ? previewSlots[index]
                : null;

        private void PlaySlash()
        {
            EnsureSlashImage();
            if (slashImage == null) return;
            if (slashRoutine != null) StopCoroutine(slashRoutine);
            slashRoutine = StartCoroutine(AnimateSlash());
        }

        private void EnsureSlashImage()
        {
            if (slashImage != null) return;
            RectTransform parent = previewContainer;
            if (parent == null && previewSlots != null)
            {
                for (int i = 0; i < previewSlots.Length; i++)
                {
                    if (previewSlots[i] == null) continue;
                    parent = previewSlots[i].rectTransform.parent as RectTransform;
                    break;
                }
            }
            if (parent == null) return;

            var slashObject = new GameObject(
                "BossPreviewSlash",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image),
                typeof(LayoutElement));
            RectTransform rect = slashObject.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(0f, 0.5f);
            rect.anchorMax = new Vector2(1f, 0.5f);
            rect.pivot = new Vector2(0f, 0.5f);
            rect.offsetMin = new Vector2(0f, -slashThickness * 0.5f);
            rect.offsetMax = new Vector2(0f, slashThickness * 0.5f);
            rect.SetAsLastSibling();
            slashObject.GetComponent<LayoutElement>().ignoreLayout = true;

            slashImage = slashObject.GetComponent<Image>();
            slashImage.raycastTarget = false;
            slashImage.color = slashColor;
            slashObject.SetActive(false);
        }

        private IEnumerator AnimateSlash()
        {
            slashImage.gameObject.SetActive(true);
            RectTransform rect = slashImage.rectTransform;
            rect.localScale = new Vector3(0f, 1f, 1f);
            Color color = slashColor;
            color.a = 1f;
            slashImage.color = color;

            for (float elapsed = 0f; elapsed < slashGrowDuration; elapsed += Time.unscaledDeltaTime)
            {
                float t = Mathf.Clamp01(elapsed / slashGrowDuration);
                rect.localScale = new Vector3(t, 1f, 1f);
                yield return null;
            }
            rect.localScale = Vector3.one;

            for (float elapsed = 0f; elapsed < slashFadeDuration; elapsed += Time.unscaledDeltaTime)
            {
                float t = Mathf.Clamp01(elapsed / slashFadeDuration);
                color.a = 1f - t;
                slashImage.color = color;
                yield return null;
            }

            slashImage.gameObject.SetActive(false);
            slashRoutine = null;
        }

        private void HideSlash()
        {
            if (slashRoutine != null)
            {
                StopCoroutine(slashRoutine);
                slashRoutine = null;
            }
            if (slashImage != null) slashImage.gameObject.SetActive(false);
        }
    }
}
