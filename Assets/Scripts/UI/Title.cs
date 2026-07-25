using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Title : MonoBehaviour
{
    private const string BeatMemoriesSceneName = "BeatMemories_Dayeon";

    [Header("Title Elements")]
    [SerializeField] private Graphic wooferLeft;
    [SerializeField] private Graphic wooferRight;
    [SerializeField] private Graphic character;
    [SerializeField] private Graphic textArt;
    [SerializeField] private TitleButton startButton;
    [SerializeField] private TitleButton optionsButton;
    [SerializeField] private TitleButton exitButton;
    [SerializeField] private Graphic optionPanel;
    [SerializeField] private TitleButton backButton;

    [Header("Intro Timing")]
    [SerializeField, Min(0f)] private float initialDelay = 0.15f;
    [SerializeField, Min(0.01f)] private float wooferDuration = 0.5f;
    [SerializeField, Min(0.01f)] private float characterDuration = 0.5f;
    [SerializeField, Min(0.01f)] private float textArtDuration = 0.45f;
    [SerializeField, Min(0f)] private float elementGap = 0.08f;
    [SerializeField, Range(1f, 2f)] private float buttonDelay = 1.25f;
    [SerializeField, Min(0.01f)] private float buttonDuration = 0.35f;
    [SerializeField, Min(0f)] private float buttonGap = 0.12f;

    [Header("Intro Motion")]
    [SerializeField, Min(0f)] private float wooferHorizontalOffset = 220f;
    [SerializeField, Min(0f)] private float characterVerticalOffset = 100f;
    [SerializeField, Range(0.01f, 1f)] private float wooferStartScale = 0.82f;
    [SerializeField, Range(0.01f, 1f)] private float characterStartScale = 0.9f;
    [SerializeField, Min(1f)] private float textArtStartScale = 1.18f;
    [SerializeField, Range(0.01f, 1f)] private float buttonStartScale = 0.78f;
    [SerializeField, Min(0f)] private float buttonVerticalOffset = 24f;

    [Header("Options Transition")]
    [SerializeField, Min(0.01f)] private float mainUiTransitionDuration = 0.25f;
    [SerializeField, Min(0.01f)] private float optionsTransitionDuration = 0.45f;
    [SerializeField, Min(0f)] private float optionsElementGap = 0.08f;
    [SerializeField, Min(0f)] private float optionsHorizontalOffset = 1800f;
    [SerializeField, Range(0.01f, 1f)] private float hiddenMainUiScale = 0.94f;

    private ElementState wooferLeftState;
    private ElementState wooferRightState;
    private ElementState characterState;
    private ElementState textArtState;
    private ElementState startButtonState;
    private ElementState optionsButtonState;
    private ElementState exitButtonState;
    private ElementState optionPanelState;
    private ElementState backButtonState;
    private Sequence introSequence;
    private Sequence viewSequence;
    private bool optionsOpen;
    private bool isTransitioning;

    private void Awake()
    {
        if (!TryCacheElementStates())
        {
            enabled = false;
            return;
        }

        ApplyIntroState();
    }

    private void Start()
    {
        PlayIntro();
    }

    private void OnDestroy()
    {
        introSequence?.Kill();
        viewSequence?.Kill();
    }

    public void PlayIntro()
    {
        if (!enabled)
        {
            return;
        }

        introSequence?.Kill();
        viewSequence?.Kill();
        optionsOpen = false;
        isTransitioning = false;
        RestoreFinalState();
        ApplyIntroState();

        introSequence = DOTween.Sequence()
            .SetTarget(this)
            .SetUpdate(true);

        float stageStart = initialDelay;
        AddWooferTween(wooferLeftState, stageStart);
        AddWooferTween(wooferRightState, stageStart);

        stageStart += wooferDuration + elementGap;
        AddCharacterTween(stageStart);

        stageStart += characterDuration + elementGap;
        AddTextArtTween(stageStart);

        stageStart += textArtDuration + buttonDelay;
        AddButtonTween(startButtonState, startButton, stageStart);

        stageStart += buttonDuration + buttonGap;
        AddButtonTween(optionsButtonState, optionsButton, stageStart);

        stageStart += buttonDuration + buttonGap;
        AddButtonTween(exitButtonState, exitButton, stageStart);
    }

    public void ShowOptions()
    {
        if (!enabled || optionsOpen || isTransitioning)
        {
            return;
        }

        introSequence?.Kill();
        SetMainButtonsInteraction(false);
        backButton.SetInteractionEnabled(false);
        isTransitioning = true;

        viewSequence?.Kill();
        viewSequence = DOTween.Sequence()
            .SetTarget(this)
            .SetUpdate(true);

        AddMainUiTweens(viewSequence, 0f, false);

        float optionsStart = mainUiTransitionDuration;
        viewSequence.Insert(
            optionsStart,
            optionPanelState.RectTransform
                .DOAnchorPos(optionPanelState.AnchoredPosition, optionsTransitionDuration)
                .SetEase(Ease.OutCubic));
        viewSequence.Insert(
            optionsStart + optionsElementGap,
            backButtonState.RectTransform
                .DOAnchorPos(backButtonState.AnchoredPosition, optionsTransitionDuration)
                .SetEase(Ease.OutBack));
        viewSequence.InsertCallback(
            optionsStart + optionsElementGap + optionsTransitionDuration,
            () =>
            {
                optionsOpen = true;
                isTransitioning = false;
                backButton.SetInteractionEnabled(true);
            });
    }

    public void StartGame()
    {
        if (!enabled || isTransitioning)
        {
            return;
        }

        introSequence?.Kill();
        viewSequence?.Kill();
        SetMainButtonsInteraction(false);
        SceneManager.LoadScene(BeatMemoriesSceneName);
    }

    public void HideOptions()
    {
        if (!enabled || !optionsOpen || isTransitioning)
        {
            return;
        }

        backButton.SetInteractionEnabled(false);
        isTransitioning = true;

        viewSequence?.Kill();
        viewSequence = DOTween.Sequence()
            .SetTarget(this)
            .SetUpdate(true);

        Vector2 hiddenOffset = Vector2.right * optionsHorizontalOffset;
        viewSequence.Insert(
            0f,
            backButtonState.RectTransform
                .DOAnchorPos(backButtonState.AnchoredPosition + hiddenOffset, optionsTransitionDuration)
                .SetEase(Ease.InCubic));
        viewSequence.Insert(
            optionsElementGap,
            optionPanelState.RectTransform
                .DOAnchorPos(optionPanelState.AnchoredPosition + hiddenOffset, optionsTransitionDuration)
                .SetEase(Ease.InCubic));

        float mainUiStart = optionsElementGap + optionsTransitionDuration;
        AddMainUiTweens(viewSequence, mainUiStart, true);
        viewSequence.InsertCallback(
            mainUiStart + mainUiTransitionDuration,
            () =>
            {
                optionsOpen = false;
                isTransitioning = false;
                SetMainButtonsInteraction(true);
            });
    }

    private bool TryCacheElementStates()
    {
        if (wooferLeft == null
            || wooferRight == null
            || character == null
            || textArt == null
            || startButton == null
            || optionsButton == null
            || exitButton == null
            || optionPanel == null
            || backButton == null)
        {
            Debug.LogError(
                $"{nameof(Title)} requires all title art and button references.",
                this);
            return false;
        }

        startButton.Prepare();
        optionsButton.Prepare();
        exitButton.Prepare();
        backButton.Prepare();

        wooferLeftState = new ElementState(wooferLeft);
        wooferRightState = new ElementState(wooferRight);
        characterState = new ElementState(character);
        textArtState = new ElementState(textArt);
        startButtonState = new ElementState(startButton.Graphic);
        optionsButtonState = new ElementState(optionsButton.Graphic);
        exitButtonState = new ElementState(exitButton.Graphic);
        optionPanelState = new ElementState(optionPanel);
        backButtonState = new ElementState(backButton.Graphic);
        return true;
    }

    private void ApplyIntroState()
    {
        ApplyHiddenState(
            wooferLeftState,
            new Vector2(-wooferHorizontalOffset, 0f),
            wooferStartScale);
        ApplyHiddenState(
            wooferRightState,
            new Vector2(wooferHorizontalOffset, 0f),
            wooferStartScale);
        ApplyHiddenState(
            characterState,
            new Vector2(0f, -characterVerticalOffset),
            characterStartScale);
        ApplyHiddenState(textArtState, Vector2.zero, textArtStartScale);
        ApplyButtonIntroState(startButtonState, startButton);
        ApplyButtonIntroState(optionsButtonState, optionsButton);
        ApplyButtonIntroState(exitButtonState, exitButton);
        ApplyOptionsHiddenState();
    }

    private void ApplyButtonIntroState(ElementState state, TitleButton titleButton)
    {
        titleButton.SetInteractionEnabled(false);
        ApplyHiddenState(
            state,
            new Vector2(0f, -buttonVerticalOffset),
            buttonStartScale);
    }

    private static void ApplyHiddenState(ElementState state, Vector2 positionOffset, float scaleMultiplier)
    {
        state.RectTransform.anchoredPosition = state.AnchoredPosition + positionOffset;
        state.RectTransform.localScale = state.LocalScale * scaleMultiplier;
        SetAlpha(state.Graphic, 0f);
    }

    private void RestoreFinalState()
    {
        RestoreElement(wooferLeftState);
        RestoreElement(wooferRightState);
        RestoreElement(characterState);
        RestoreElement(textArtState);
        RestoreElement(startButtonState);
        RestoreElement(optionsButtonState);
        RestoreElement(exitButtonState);
        RestoreElement(optionPanelState);
        RestoreElement(backButtonState);
    }

    private void ApplyOptionsHiddenState()
    {
        Vector2 hiddenOffset = Vector2.right * optionsHorizontalOffset;
        optionPanelState.RectTransform.anchoredPosition = optionPanelState.AnchoredPosition + hiddenOffset;
        backButtonState.RectTransform.anchoredPosition = backButtonState.AnchoredPosition + hiddenOffset;
        backButton.SetInteractionEnabled(false);
    }

    private void SetMainButtonsInteraction(bool value)
    {
        startButton.SetInteractionEnabled(value);
        optionsButton.SetInteractionEnabled(value);
        exitButton.SetInteractionEnabled(value);
    }

    private void AddMainUiTweens(Sequence sequence, float startTime, bool visible)
    {
        AddMainUiTween(sequence, textArtState, startTime, visible);
        AddMainUiTween(sequence, startButtonState, startTime, visible);
        AddMainUiTween(sequence, optionsButtonState, startTime, visible);
        AddMainUiTween(sequence, exitButtonState, startTime, visible);
    }

    private void AddMainUiTween(Sequence sequence, ElementState state, float startTime, bool visible)
    {
        float targetAlpha = visible ? state.Alpha : 0f;
        Vector3 targetScale = visible
            ? state.LocalScale
            : state.LocalScale * hiddenMainUiScale;
        Ease ease = visible ? Ease.OutCubic : Ease.InCubic;

        sequence.Insert(
            startTime,
            state.Graphic.DOFade(targetAlpha, mainUiTransitionDuration).SetEase(ease));
        sequence.Insert(
            startTime,
            state.RectTransform.DOScale(targetScale, mainUiTransitionDuration).SetEase(ease));
    }

    private static void RestoreElement(ElementState state)
    {
        state.RectTransform.anchoredPosition = state.AnchoredPosition;
        state.RectTransform.localScale = state.LocalScale;
        SetAlpha(state.Graphic, state.Alpha);
    }

    private void AddWooferTween(ElementState state, float startTime)
    {
        RectTransform rectTransform = state.RectTransform;

        introSequence.Insert(
            startTime,
            rectTransform.DOAnchorPos(state.AnchoredPosition, wooferDuration).SetEase(Ease.OutCubic));
        introSequence.Insert(
            startTime,
            rectTransform.DOScale(state.LocalScale, wooferDuration).SetEase(Ease.OutBack));
        introSequence.Insert(
            startTime,
            state.Graphic.DOFade(state.Alpha, wooferDuration * 0.7f).SetEase(Ease.OutQuad));
    }

    private void AddCharacterTween(float startTime)
    {
        RectTransform rectTransform = characterState.RectTransform;

        introSequence.Insert(
            startTime,
            rectTransform.DOAnchorPos(characterState.AnchoredPosition, characterDuration).SetEase(Ease.OutCubic));
        introSequence.Insert(
            startTime,
            rectTransform.DOScale(characterState.LocalScale, characterDuration).SetEase(Ease.OutBack));
        introSequence.Insert(
            startTime,
            characterState.Graphic.DOFade(characterState.Alpha, characterDuration * 0.7f).SetEase(Ease.OutQuad));
    }

    private void AddTextArtTween(float startTime)
    {
        introSequence.Insert(
            startTime,
            textArtState.RectTransform.DOScale(textArtState.LocalScale, textArtDuration).SetEase(Ease.OutBack));
        introSequence.Insert(
            startTime,
            textArtState.Graphic.DOFade(textArtState.Alpha, textArtDuration * 0.75f).SetEase(Ease.OutQuad));
    }

    private void AddButtonTween(ElementState state, TitleButton titleButton, float startTime)
    {
        RectTransform rectTransform = state.RectTransform;

        introSequence.Insert(
            startTime,
            rectTransform.DOAnchorPos(state.AnchoredPosition, buttonDuration).SetEase(Ease.OutCubic));
        introSequence.Insert(
            startTime,
            rectTransform.DOScale(state.LocalScale, buttonDuration).SetEase(Ease.OutBack));
        introSequence.Insert(
            startTime,
            state.Graphic.DOFade(state.Alpha, buttonDuration * 0.75f).SetEase(Ease.OutQuad));
        introSequence.InsertCallback(
            startTime + buttonDuration,
            () => titleButton.SetInteractionEnabled(true));
    }

    private static void SetAlpha(Graphic graphic, float alpha)
    {
        Color color = graphic.color;
        color.a = alpha;
        graphic.color = color;
    }

    private readonly struct ElementState
    {
        public ElementState(Graphic graphic)
        {
            Graphic = graphic;
            RectTransform = graphic.rectTransform;
            AnchoredPosition = RectTransform.anchoredPosition;
            LocalScale = RectTransform.localScale;
            Alpha = graphic.color.a;
        }

        public Graphic Graphic { get; }
        public RectTransform RectTransform { get; }
        public Vector2 AnchoredPosition { get; }
        public Vector3 LocalScale { get; }
        public float Alpha { get; }
    }
}
