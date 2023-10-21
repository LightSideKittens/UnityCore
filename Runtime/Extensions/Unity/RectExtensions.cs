using UnityEngine;

namespace LSCore.Extensions.Unity
{
    public static class RectExtensions
    {
        public static float NormalizeX(in this Rect rect, float x) => (x - rect.xMin) / rect.width;
        public static float NormalizeY(in this Rect rect, float y) => (y - rect.yMin) / rect.height;


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
