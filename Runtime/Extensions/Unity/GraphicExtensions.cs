using UnityEngine.UI;

namespace LSCore.Extensions.Unity
{
    public static class GraphicExtensions
    {
        public static void A(this Graphic target, float alpha)
        {
            var color = target.color;
            color.a = alpha;
            target.color = color;
        }
    }
}
