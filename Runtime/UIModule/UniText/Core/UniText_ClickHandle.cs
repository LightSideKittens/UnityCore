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
    
    
    public int GetCharacterIndexAtPosition(Vector2 localPosition)
    {
        var result = HitTest(localPosition);
        return result.hit ? result.glyphIndex : -1;
    }

    /// <summary>
    /// Выполняет bounds-based hit test по локальной позиции.
    /// Проверяет попадание в визуальные границы глифов.
    /// </summary>
    /// <param name="localPosition">Позиция в локальных координатах RectTransform</param>
    /// <param name="maxDistance">Максимальное расстояние для fallback (0 = без fallback)</param>
    public TextHitResult HitTest(Vector2 localPosition, float maxDistance = 0)
    {
        if (processor == null)
            return TextHitResult.None;

        var glyphs = processor.PositionedGlyphs;
        int glyphCount = glyphs.Length;
        if (glyphCount == 0)
            return TextHitResult.None;

        float localX = localPosition.x;
        float localY = localPosition.y;

        // First pass: check if click is inside any glyph bounds
        for (int i = 0; i < glyphCount; i++)
        {
            ref readonly var glyph = ref glyphs[i];

            // Bounds check (left <= x <= right, bottom <= y <= top)
            if (localX >= glyph.left && localX <= glyph.right &&
                localY >= glyph.bottom && localY <= glyph.top)
            {
                return new TextHitResult(i, glyph.cluster, new Vector2(glyph.x, glyph.y), 0f);
            }
        }

        // No direct hit - optionally find closest glyph center within maxDistance
        if (maxDistance <= 0)
            return TextHitResult.None;

        float closestDistSq = float.MaxValue;
        int closestIndex = -1;

        for (int i = 0; i < glyphCount; i++)
        {
            ref readonly var glyph = ref glyphs[i];

            // Distance to glyph center
            float centerX = (glyph.left + glyph.right) * 0.5f;
            float centerY = (glyph.top + glyph.bottom) * 0.5f;
            float dx = localX - centerX;
            float dy = localY - centerY;
            float distSq = dx * dx + dy * dy;

            if (distSq < closestDistSq)
            {
                closestDistSq = distSq;
                closestIndex = i;
            }
        }

        if (closestIndex < 0)
            return TextHitResult.None;

        float distance = Mathf.Sqrt(closestDistSq);
        if (distance > maxDistance)
            return TextHitResult.None;

        ref readonly var closestGlyph = ref glyphs[closestIndex];
        return new TextHitResult(closestIndex, closestGlyph.cluster, new Vector2(closestGlyph.x, closestGlyph.y), distance);
    }

    /// <summary>
    /// Выполняет hit test по screen position.
    /// </summary>
    public TextHitResult HitTestScreen(Vector2 screenPosition, Camera eventCamera, float maxDistance = 0)
    {
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rectTransform, screenPosition, eventCamera, out var localPos))
            return TextHitResult.None;

        return HitTest(localPos, maxDistance);
    }
}
