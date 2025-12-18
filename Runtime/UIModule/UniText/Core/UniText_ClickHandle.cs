using System;
using UnityEngine;
using UnityEngine.EventSystems;

public partial class UniText : IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, IPointerMoveHandler
{
    [Header("Click Handling")]
    [SerializeField]
    private float maxClickDistance = 50f;

    [SerializeField]
    private bool openLinksInBrowser = true;

    private TextHitResult lastHoverResult;

    /// <summary>
    /// Вызывается при клике по тексту.
    /// </summary>
    public event Action<TextHitResult> OnTextClick;

    /// <summary>
    /// Вызывается при клике по ссылке.
    /// </summary>
    public event Action<string> OnLinkClick;

    /// <summary>
    /// Вызывается при наведении на ссылку.
    /// </summary>
    public event Action<string> OnLinkEnter;

    /// <summary>
    /// Вызывается при уходе с ссылки.
    /// </summary>
    public event Action OnLinkExit;

    /// <summary>
    /// Последний результат hover.
    /// </summary>
    public TextHitResult LastHoverResult => lastHoverResult;

    /// <summary>
    /// Сейчас наведено на ссылку?
    /// </summary>
    public bool IsHoveringLink => lastHoverResult.hit && LinkModifier.IsLink(lastHoverResult.cluster);

    public void OnPointerClick(PointerEventData eventData)
    {
        var camera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay
            ? canvas.worldCamera
            : null;

        var result = HitTestScreen(eventData.position, camera, maxClickDistance);
        if (!result.hit) return;

        OnTextClick?.Invoke(result);

        if (LinkModifier.IsLink(result.cluster))
        {
            string url = LinkModifier.GetLinkUrl(result.cluster);
            OnLinkClick?.Invoke(url);

            if (openLinksInBrowser && !string.IsNullOrEmpty(url))
            {
                Application.OpenURL(url);
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        UpdateHover(eventData);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (lastHoverResult.hit && LinkModifier.IsLink(lastHoverResult.cluster))
        {
            OnLinkExit?.Invoke();
        }
        lastHoverResult = TextHitResult.None;
    }

    public void OnPointerMove(PointerEventData eventData)
    {
        UpdateHover(eventData);
    }

    private void UpdateHover(PointerEventData eventData)
    {
        var camera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay
            ? canvas.worldCamera
            : null;

        var result = HitTestScreen(eventData.position, camera, maxClickDistance);

        bool wasLink = lastHoverResult.hit && LinkModifier.IsLink(lastHoverResult.cluster);
        bool isLink = result.hit && LinkModifier.IsLink(result.cluster);

        string oldUrl = wasLink ? LinkModifier.GetLinkUrl(lastHoverResult.cluster) : null;
        string newUrl = isLink ? LinkModifier.GetLinkUrl(result.cluster) : null;

        // Ушли с ссылки
        if (wasLink && (!isLink || oldUrl != newUrl))
        {
            OnLinkExit?.Invoke();
        }

        // Вошли на ссылку
        if (isLink && (!wasLink || oldUrl != newUrl))
        {
            OnLinkEnter?.Invoke(newUrl);
        }

        lastHoverResult = result;
    }
}
