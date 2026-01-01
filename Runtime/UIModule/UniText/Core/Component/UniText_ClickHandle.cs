using System;
using UnityEngine;
using UnityEngine.EventSystems;

public partial class UniText : IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, IPointerMoveHandler
{
    private const float DefaultMaxClickDistance = 20;
    
    private TextHitResult lastHoverResult;
    public event Action<TextHitResult> OnTextClick;
    public event Action<string> OnLinkClick;
    public event Action<string> OnLinkEnter;
    public event Action OnLinkExit;
    public TextHitResult LastHoverResult => lastHoverResult;
    public bool IsHoveringLink => lastHoverResult.hit && LinkModifier.IsLink(buffers, lastHoverResult.cluster);

    public void OnPointerClick(PointerEventData eventData)
    {
        var camera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay
            ? canvas.worldCamera
            : null;

        var result = HitTestScreen(eventData.position, camera);
        if (!result.hit) return;

        OnTextClick?.Invoke(result);

        if (LinkModifier.IsLink(buffers, result.cluster))
        {
            var url = LinkModifier.GetLinkUrl(buffers, result.cluster);
            OnLinkClick?.Invoke(url);
            Application.OpenURL(url);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        UpdateHover(eventData);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (lastHoverResult.hit && LinkModifier.IsLink(buffers, lastHoverResult.cluster)) OnLinkExit?.Invoke();
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

        var result = HitTestScreen(eventData.position, camera);

        var wasLink = lastHoverResult.hit && LinkModifier.IsLink(buffers, lastHoverResult.cluster);
        var isLink = result.hit && LinkModifier.IsLink(buffers, result.cluster);

        var oldUrl = wasLink ? LinkModifier.GetLinkUrl(buffers, lastHoverResult.cluster) : null;
        var newUrl = isLink ? LinkModifier.GetLinkUrl(buffers, result.cluster) : null;

        if (wasLink && (!isLink || oldUrl != newUrl)) OnLinkExit?.Invoke();

        if (isLink && (!wasLink || oldUrl != newUrl)) OnLinkEnter?.Invoke(newUrl);

        lastHoverResult = result;
    }
    
    public TextHitResult HitTest(Vector2 localPosition, float maxDistance = DefaultMaxClickDistance)
    {
        if (textProcessor == null)
            return TextHitResult.None;

        var glyphs = textProcessor.PositionedGlyphs;
        var glyphCount = glyphs.Length;
        if (glyphCount == 0)
            return TextHitResult.None;

        var localX = localPosition.x;
        var localY = localPosition.y;

        for (var i = 0; i < glyphCount; i++)
        {
            ref readonly var glyph = ref glyphs[i];

            if (localX >= glyph.left && localX <= glyph.right &&
                localY >= glyph.bottom && localY <= glyph.top)
                return new TextHitResult(i, glyph.cluster, new Vector2(glyph.x, glyph.y), 0f);
        }

        if (maxDistance <= 0)
            return TextHitResult.None;

        var closestDistSq = float.MaxValue;
        var closestIndex = -1;

        for (var i = 0; i < glyphCount; i++)
        {
            ref readonly var glyph = ref glyphs[i];

            var centerX = (glyph.left + glyph.right) * 0.5f;
            var centerY = (glyph.top + glyph.bottom) * 0.5f;
            var dx = localX - centerX;
            var dy = localY - centerY;
            var distSq = dx * dx + dy * dy;

            if (distSq < closestDistSq)
            {
                closestDistSq = distSq;
                closestIndex = i;
            }
        }

        if (closestIndex < 0)
            return TextHitResult.None;

        var distance = Mathf.Sqrt(closestDistSq);
        if (distance > maxDistance)
            return TextHitResult.None;

        ref readonly var closestGlyph = ref glyphs[closestIndex];
        return new TextHitResult(closestIndex, closestGlyph.cluster, new Vector2(closestGlyph.x, closestGlyph.y),
            distance);
    }


    public TextHitResult HitTestScreen(Vector2 screenPosition, Camera eventCamera, float maxDistance = DefaultMaxClickDistance)
    {
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rectTransform, screenPosition, eventCamera, out var localPos))
            return TextHitResult.None;

        return HitTest(localPos, maxDistance);
    }
}