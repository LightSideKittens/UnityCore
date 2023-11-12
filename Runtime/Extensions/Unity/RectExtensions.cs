using UnityEngine;

namespace LSCore.Extensions.Unity
{
    public static class RectExtensions
    {
        public static float NormalizeX(in this Rect rect, float x) => (x - rect.xMin) / rect.width;
        public static float NormalizeY(in this Rect rect, float y) => (y - rect.yMin) / rect.height;
        public static Vector2 BottomLeft(in this Rect rect) => new(rect.xMin, rect.yMin);
        public static Vector2 BottomRight(in this Rect rect) => new(rect.xMax, rect.yMin);
        public static Vector2 TopLeft(in this Rect rect) => new(rect.xMin, rect.yMax);
        public static Vector2 TopRight(in this Rect rect) => new(rect.xMax, rect.yMax);
        public static Vector2[] Corners(in this Rect rect) => new[] { rect.TopLeft(), rect.TopRight(), rect.BottomLeft(), rect.BottomRight() };
        public static Vector2[] CornersRelativeCenter(in this Rect rect) => new[] { rect.TopLeft() - rect.center, rect.TopRight() - rect.center, rect.BottomLeft() - rect.center, rect.BottomRight() - rect.center };
        public static float CircumscribedCircleRadius(in this Rect rect)
        {
            return Mathf.Sqrt(Mathf.Pow(rect.width / 2, 2) + Mathf.Pow(rect.height / 2, 2));
        }

        private delegate Vector2 GeneratePointDelegate(Rect rect, float distance);

        private static readonly GeneratePointDelegate[] pointGenerators =
        {
            GenerateLeftSidePoint,
            GenerateRightSidePoint,
            GenerateTopSidePoint,
            GenerateBottomSidePoint
        };

        public static Vector2 RandomPointAroundRect(this Rect rect, float distance = 1.0f)
        {
            return pointGenerators[Random.Range(0, 4)](rect, distance);
        }

        private static Vector2 GenerateLeftSidePoint(Rect rect, float distance)
        {
            return new Vector2(rect.xMin - distance, Random.Range(rect.yMin, rect.yMax));
        }

        private static Vector2 GenerateRightSidePoint(Rect rect, float distance)
        {
            return new Vector2(rect.xMax + distance, Random.Range(rect.yMin, rect.yMax));
        }

        private static Vector2 GenerateTopSidePoint(Rect rect, float distance)
        {
            return new Vector2(Random.Range(rect.xMin, rect.xMax), rect.yMin - distance);
        }

        private static Vector2 GenerateBottomSidePoint(Rect rect, float distance)
        {
            return new Vector2(Random.Range(rect.xMin, rect.xMax), rect.yMax + distance);
        }
    }
}
