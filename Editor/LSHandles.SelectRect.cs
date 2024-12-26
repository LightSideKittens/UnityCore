using UnityEngine;

namespace LSCore.Editor
{
    public static partial class LSHandles
    {
        public static class SelectRect
        {
            public static Color selectionColor = new Color(0.62f, 0.4f, 1f, 0.35f);
            private static Vector2 startPos;
            private static bool isSelecting = false;
            
            public static void Start()
            {
                isSelecting = true;
                startPos = MouseInWorldPoint;
            }

            public static void Draw()
            {
                if(eventType != EventType.Repaint) return;
                if (!isSelecting) return;
                
                var rect = CreateRectInArea(startPos, MouseInWorldPoint);
                
                var center = rect.center;
                rect.size *= currentMatrix.lossyScale;
                rect.size /= ScaleMultiplier;
                rect.center = center;
                
                DrawSquare(rect, selectionColor, false);
                GUI.changed = true;
            }

            public static Rect End()
            {
                if (!isSelecting) return default;
                isSelecting = false;
                return CreateRectInArea(startPos, MouseInWorldPoint);
            }
            
            public static Rect CreateRectInArea(Vector2 a, Vector2 b)
            {
                float xMin = Mathf.Min(a.x, b.x);
                float yMin = Mathf.Min(a.y, b.y);
                float xMax = Mathf.Max(a.x, b.x);
                float yMax = Mathf.Max(a.y, b.y);
                
                float width = xMax - xMin;
                float height = yMax - yMin;

                return new Rect(xMin, yMin, width, height);
            }

            public static Rect CreateRect(Vector2 min, Vector2 end)
            {
                float xMin = min.x;
                float yMin = min.y;
                float xMax = end.x;
                float yMax = end.y;
                
                float width = xMax - xMin;
                float height = yMax - yMin;

                return new Rect(xMin, yMin, width, height);
            }
        }
    }
}