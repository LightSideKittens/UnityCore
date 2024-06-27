using UnityEngine;

namespace LSCore
{
    public class SetPivot : LSAction
    {
        public RectTransform rectTransform;
        public Vector2 pivot;
        public bool savePosition = true;
    
        public override void Invoke()
        {
            if (savePosition)
            {
                Vector2 size = rectTransform.rect.size;

                // Вычисляем разницу в pivot
                Vector2 pivotDelta = pivot - rectTransform.pivot;

                // Вычисляем смещение позиции
                Vector2 offset = new Vector2(pivotDelta.x * size.x, pivotDelta.y * size.y);

                // Обновляем позицию
                rectTransform.anchoredPosition += offset;

                // Устанавливаем новый pivot
                rectTransform.pivot = pivot;
            }
            else
            {
                rectTransform.pivot = pivot;
            }
        }
    }

}