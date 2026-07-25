using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Image), typeof(Button))]
public class TitleButton : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerDownHandler,
    IPointerUpHandler,
    ISelectHandler,
    IDeselectHandler
{
    [SerializeField] private Image targetImage;
    [SerializeField] private Sprite normalSprite;
    [SerializeField] private Sprite hoverSprite;
    [SerializeField, Min(1f)] private float hoverScale = 1.05f;
    [SerializeField, Min(1f)] private float pressedScale = 1.09f;
    [SerializeField, Min(0.01f)] private float scaleDuration = 0.12f;

    private Button button;
    private Vector3 baseScale;
    private Tween scaleTween;
    private bool isPrepared;
    private bool isHovered;
    private bool isSelected;
    private bool interactionEnabled = true;

    public Graphic Graphic => targetImage;

    private void Awake()
    {
        Prepare();
        ApplySprite(false);
    }

    private void OnDestroy()
    {
        scaleTween?.Kill();
    }

    public void Prepare()
    {
        if (isPrepared)
        {
            return;
        }

        targetImage ??= GetComponent<Image>();
        button = GetComponent<Button>();
        normalSprite ??= targetImage.sprite;
        baseScale = transform.localScale;
        isPrepared = true;
    }

    public void SetInteractionEnabled(bool value)
    {
        Prepare();
        interactionEnabled = value;
        button.interactable = value;
        isHovered = false;
        isSelected = false;
        ApplySprite(false);

        if (!value)
        {
            scaleTween?.Kill();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!interactionEnabled)
        {
            return;
        }

        isHovered = true;
        AnimateState(true, hoverScale);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!interactionEnabled)
        {
            return;
        }

        isHovered = false;
        AnimateState(isSelected, isSelected ? hoverScale : 1f);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!interactionEnabled || eventData.button != PointerEventData.InputButton.Left)
        {
            return;
        }

        AnimateState(true, pressedScale);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!interactionEnabled || eventData.button != PointerEventData.InputButton.Left)
        {
            return;
        }

        bool highlighted = isHovered || isSelected;
        AnimateState(highlighted, highlighted ? hoverScale : 1f);
    }

    public void OnSelect(BaseEventData eventData)
    {
        if (!interactionEnabled)
        {
            return;
        }

        isSelected = true;
        AnimateState(true, hoverScale);
    }

    public void OnDeselect(BaseEventData eventData)
    {
        if (!interactionEnabled)
        {
            return;
        }

        isSelected = false;
        AnimateState(isHovered, isHovered ? hoverScale : 1f);
    }

    private void AnimateState(bool highlighted, float scaleMultiplier)
    {
        ApplySprite(highlighted);
        scaleTween?.Kill();
        scaleTween = transform
            .DOScale(baseScale * scaleMultiplier, scaleDuration)
            .SetEase(Ease.OutCubic)
            .SetUpdate(true);
    }

    private void ApplySprite(bool highlighted)
    {
        if (targetImage == null)
        {
            return;
        }

        Sprite sprite = highlighted ? hoverSprite : normalSprite;
        if (sprite != null)
        {
            targetImage.sprite = sprite;
        }
    }
}
