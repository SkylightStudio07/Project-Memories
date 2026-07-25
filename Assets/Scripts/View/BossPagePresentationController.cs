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
        private bool originalsCaptured;
        private StageSO currentStage;
        private Image slashImage;
        private Coroutine slashRoutine;
        private bool transitionActive;
        private bool previewCutActive;
        private int transitionBeatCount;

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
            transitionActive = false;
            transitionBeatCount = 0;
            SetPreviewCutActive(false);
            HideSlash();
        }

        private void OnEnemyPageTransitionStarted(int page, int pageCount, int preparationBeats)
        {
            if (currentStage == null
                || !currentStage.cutPreviewBottomHalfOnSecondPage
                || page < 2)
                return;

            transitionActive = true;
            transitionBeatCount = Mathf.Max(0, preparationBeats);
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
                SetPreviewCutActive(true);
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
            if (!transitionActive) return;
            transitionActive = false;
            HideSlash();
        }

        private void OnCycleStarted(int cycleIndex)
        {
            if (!previewCutActive) return;
            for (int i = 0; i < previewSlots.Length; i++)
                ApplyBottomHalf(i);
        }

        private void OnEnemyPreviewed(EnemyPreviewCue cue)
        {
            if (!previewCutActive) return;
            if (cue.IsHidden) RestoreSlot(cue.Slot);
            else ApplyBottomHalf(cue.Slot);
        }

        private void SetPreviewCutActive(bool active)
        {
            CaptureOriginalSlotState();
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

        private void ApplyBottomHalf(int index)
        {
            Image slot = Slot(index);
            if (slot == null) return;
            slot.type = Image.Type.Filled;
            slot.fillMethod = Image.FillMethod.Vertical;
            slot.fillOrigin = (int)Image.OriginVertical.Bottom;
            slot.fillAmount = 0.5f;
            slot.fillClockwise = false;
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
                typeof(Image));
            RectTransform rect = slashObject.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(0f, 0.5f);
            rect.anchorMax = new Vector2(1f, 0.5f);
            rect.pivot = new Vector2(0f, 0.5f);
            rect.offsetMin = new Vector2(0f, -slashThickness * 0.5f);
            rect.offsetMax = new Vector2(0f, slashThickness * 0.5f);
            rect.SetAsLastSibling();

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
