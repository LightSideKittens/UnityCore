using UnityEngine;

namespace LSCore.Runtime
{
    public static class AndroidUtils
    {
        public static int ToARGB(this Color color)
        {
            return ((Color32)color).ToARGB();
        }
        
        public static int ToARGB(this Color32 color)
        {
            int argbColor = (color.a << 24) |
                            (color.r << 16) |
                            (color.g << 8) |
                            color.b;
            return argbColor;
        }
    }
}