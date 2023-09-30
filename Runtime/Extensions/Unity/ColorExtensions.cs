using UnityEngine;

namespace LSCore.Extensions
{
    public static class ColorExtensions
    {
        /// <summary>
        /// Returns a new color equal to the given one with changed brightness
        /// </summary>
        /// <param name="color">Color to evaluate</param>
        /// <param name="brightnessFactor">Brightness factor (multiplied by current brightness)</param>
        /// <param name="alpha">If set applies this alpha value</param>
        public static Color CloneAndChangeBrightness(
            this Color color,
            float brightnessFactor,
            float? alpha = null)
        {
            RGBToHSV(color, out var h, out var s, out var v1);
            var v2 = v1 * brightnessFactor;
            if (v2 < 0.0)
                v2 = 0.0f;
            else if (v2 > 1.0)
                v2 = 1f;
            var rgb = HSVToRGB(h, s, v2);
            if (alpha.HasValue)
                rgb.a = alpha.Value;
            return rgb;
        }

        /// <summary>
        /// Returns a new color equal to the given one with changed saturation
        /// </summary>
        /// <param name="color">Color to evaluate</param>
        /// <param name="saturationFactor">Saturation factor (multiplied by current brightness)</param>
        /// <param name="alpha">If set applies this alpha value</param>
        public static Color CloneAndChangeSaturation(
            this Color color,
            float saturationFactor,
            float? alpha = null)
        {
            RGBToHSV(color, out var h, out var s1, out var v);
            var s2 = s1 * saturationFactor;
            if (s2 < 0.0)
                s2 = 0.0f;
            else if (s2 > 1.0)
                s2 = 1f;
            var rgb = HSVToRGB(h, s2, v);
            if (alpha.HasValue)
                rgb.a = alpha.Value;
            return rgb;
        }

        /// <summary>Changes the alpha of this color and returns it</summary>
        public static Color SetAlpha(this Color color, float alpha)
        {
            color.a = alpha;
            return color;
        }

        /// <summary>
        /// Returns a new color equal to the given one with changed alpha
        /// </summary>
        public static Color CloneAndSetAlpha(this Color color, float alpha) =>
            new Color(color.r, color.g, color.b, alpha);

        /// <summary>
        /// Returns a HEX version of the given Unity Color, without the initial #
        /// </summary>
        /// <param name="includeAlpha">If TRUE, also converts the alpha value and returns a hex of 8 characters,
        /// otherwise doesn't and returns a hex of 6 characters</param>
        public static string ToHex(this Color32 color, bool includeAlpha = false)
        {
            var hex = color.r.ToString("x2") + color.g.ToString("x2") + color.b.ToString("x2");
            if (includeAlpha)
                hex += color.a.ToString("x2");
            return hex;
        }

        /// <summary>
        /// Returns a HEX version of the given Unity Color, without the initial #
        /// </summary>
        /// <param name="includeAlpha">If TRUE, also converts the alpha value and returns a hex of 8 characters,
        /// otherwise doesn't and returns a hex of 6 characters</param>
        public static string ToHex(this Color color, bool includeAlpha = false) => ((Color32)color).ToHex(includeAlpha);

        private static void RGBToHSV(Color rgbColor, out float h, out float s, out float v)
        {
            if (rgbColor.b > (double)rgbColor.g && rgbColor.b > (double)rgbColor.r)
                RGBToHSVHelper(4f, rgbColor.b, rgbColor.r, rgbColor.g, out h, out s, out v);
            else if (rgbColor.g > (double)rgbColor.r)
                RGBToHSVHelper(2f, rgbColor.g, rgbColor.b, rgbColor.r, out h, out s, out v);
            else
                RGBToHSVHelper(0.0f, rgbColor.r, rgbColor.g, rgbColor.b, out h, out s, out v);
        }

        private static void RGBToHSVHelper(
            float offset,
            float dominantcolor,
            float colorone,
            float colortwo,
            out float h,
            out float s,
            out float v)
        {
            v = dominantcolor;
            if (v != 0.0)
            {
                var num1 = colorone <= (double)colortwo ? colorone : colortwo;
                var num2 = v - num1;
                if (num2 != 0.0)
                {
                    s = num2 / v;
                    h = offset + (colorone - colortwo) / num2;
                }
                else
                {
                    s = 0.0f;
                    h = offset + (colorone - colortwo);
                }

                h /= 6f;
                if (h >= 0.0)
                    return;
                ++h;
            }
            else
            {
                s = 0.0f;
                h = 0.0f;
            }
        }

        private static Color HSVToRGB(float h, float s, float v, bool hdr = true)
        {
            var white = Color.white;
            if (s == 0.0)
            {
                white.r = v;
                white.g = v;
                white.b = v;
            }
            else if (v == 0.0)
            {
                white.r = 0.0f;
                white.g = 0.0f;
                white.b = 0.0f;
            }
            else
            {
                white.r = 0.0f;
                white.g = 0.0f;
                white.b = 0.0f;
                var f = h * 6.0;
                var num1 = (int)Mathf.Floor((float)f);
                var num2 = (float)f - num1;
                var num3 = v * (1f - s);
                var num4 = v * (float)(1.0 - s * (double)num2);
                var num5 = v * (float)(1.0 - s * (1.0 - num2));
                switch (num1)
                {
                    case -1:
                        white.r = v;
                        white.g = num3;
                        white.b = num4;
                        break;
                    case 0:
                        white.r = v;
                        white.g = num5;
                        white.b = num3;
                        break;
                    case 1:
                        white.r = num4;
                        white.g = v;
                        white.b = num3;
                        break;
                    case 2:
                        white.r = num3;
                        white.g = v;
                        white.b = num5;
                        break;
                    case 3:
                        white.r = num3;
                        white.g = num4;
                        white.b = v;
                        break;
                    case 4:
                        white.r = num5;
                        white.g = num3;
                        white.b = v;
                        break;
                    case 5:
                        white.r = v;
                        white.g = num3;
                        white.b = num4;
                        break;
                    case 6:
                        white.r = v;
                        white.g = num5;
                        white.b = num3;
                        break;
                }

                if (hdr) return white;
                white.r = Mathf.Clamp(white.r, 0.0f, 1f);
                white.g = Mathf.Clamp(white.g, 0.0f, 1f);
                white.b = Mathf.Clamp(white.b, 0.0f, 1f);
            }

            return white;
        }
    }
}
