using UnityEngine;

namespace LSCore.Editor
{
    public static class Blink
    {
        public static void Start(RectInt rect, int loops = 4, Color color = default)
        {
            rect.y += 20;
            PythonRunner.Run("blink", $"{rect.x} {rect.y} {rect.width} {rect.height} {loops} #{ColorUtility.ToHtmlStringRGB(color)} {color.a}");
        }
        
        public static void Start(Rect rect, int loops = 4, Color color = default)
        {
            var intRect = new RectInt((int)rect.x, (int)rect.y, (int)rect.width, (int)rect.height);
            Start(intRect, loops, color);
        }
    }
}