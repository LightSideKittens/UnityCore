//-----------------------------------------------------------------------
// <copyright file="ColorValueResolverCreator.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#if UNITY_EDITOR
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
[assembly: Sirenix.OdinInspector.Editor.ValueResolvers.RegisterDefaultValueResolverCreator(typeof(Sirenix.OdinInspector.Editor.ValueResolvers.ColorValueResolverCreator), -20)]

namespace Sirenix.OdinInspector.Editor.ValueResolvers
{
#pragma warning disable

    using System.Collections.Generic;
    using System.Text;
    using UnityEditor;
    using UnityEngine;

    public class ColorValueResolverCreator : ValueResolverCreator
    {
        private static Dictionary<string, Color> _colorMap;
        private static string _colorStrings;

        private static Dictionary<string, Color> colorMap
        {
            get
            {
                if (_colorMap == null)
                {
                    _colorMap = new Dictionary<string, Color>()
                    {
                        { "white", Color.white },
                        { "black", Color.black },
                        { "clear", Color.clear },
                        { "transparent", new Color(0,0,0,0) },
                        { "transparentBlack", new Color(0,0,0,0) },
                        { "transparentWhite", new Color(1,1,1,0) },
                    };

                    var darkSkin = EditorGUIUtility.isProSkin;
                    var blue = darkSkin ? new Color(0.184f, 0.593f, 1f, 1f) : new Color(0f, 0.301f, 1f, 1f);
                    var green = darkSkin ? new Color(0.223f, 0.83f, 0.223f, 1f) : new Color(0f, 0.642f, 0f, 1f);
                    var purple = darkSkin ? new Color(0.74f, 0.46f, 0.73f, 1f) : new Color(0.613f, 0f, 0.588f, 1f);
                    var red = darkSkin ? new Color(1f, 0.222f, 0.245f, 1f) : new Color(1f, 0.111f, 0.125f, 1f);
                    var orange = darkSkin ? new Color(0.97f, 0.55f, 0.02f, 1f) : new Color(1f, 0.391f, 0f, 1f);
                    var brightColors = new (string name, Color color)[]
                    {
                        ( "red", red),
                        ( "yellow", new Color(0.888f, 0.837f, 0.19f, 1f) ),
                        ( "green", green ),
                        ( "blue", blue ),
                        ( "gray", Color.gray ),
                        ( "grey", Color.grey ),
                        ( "cyan", Color.cyan ),
                        ( "magenta", Color.magenta ),
                        ( "orange", orange ),
                        ( "purple", purple ),
                    };

                    foreach (var color in brightColors)
                    {
                        var col = color.color;
                        Color.RGBToHSV(color.color, out var h, out var s, out var v);
                        _colorMap.Add("dark" + color.name, Color.HSVToRGB(h, s, v * 0.7f));
                        _colorMap.Add(color.name, col);
                        _colorMap.Add("light" + color.name, Color.HSVToRGB(h, s, v + 0.7f));
                    }
                }

                return _colorMap;
            }
        }

        private static string colorStrings
        {
            get
            {
                if (_colorStrings == null)
                {
                    List<string> colors = new List<string>();

                    colors.AddRange(colorMap.Keys);
                    colors.Sort();

                    var sb = new StringBuilder();

                    foreach (var colorStr in colors)
                    {
                        if (sb.Length > 0) sb.Append(", ");
                        var color = colorMap[colorStr];

                        sb.Append("<color=#");
                        sb.Append(ColorUtility.ToHtmlStringRGB(color).ToLower());
                        sb.Append("ff>");
                        sb.Append(colorStr);
                        sb.Append("</color>");
                    }

                    _colorStrings = sb.ToString();
                }

                return _colorStrings;
            }
        }

        public override string GetPossibleMatchesString(ref ValueResolverContext context)
        {
            if (context.ResultType == typeof(Color) || context.ResultType == typeof(Color?))
            {
                return 
                    "rgba(1, 1, 1, 1)\n" +
                    "rgb(1, 1, 1)\n" +
                    "#FFFFFFFF\n" +
                    "#FFFFFF\n" +
                    "One of these color names (note: transparency not displayed here): " + colorStrings;
            }

            return null;
        }

        public override ValueResolverFunc<TResult> TryCreateResolverFunc<TResult>(ref ValueResolverContext context)
        {
            if (string.IsNullOrWhiteSpace(context.ResolvedString) ||
                (context.ResultType != typeof(Color) && context.ResultType != typeof(Color?)))
            {
                return null;
            }

            var trimmed = context.ResolvedString.Trim();
            if (trimmed.Length == 0) return null;

            {
                if (colorMap.TryGetValue(trimmed.ToLower(), out var result))
                {
                    return GetResultFunc<TResult>(ref context, result);
                }
            }

            if (trimmed[0] == '#')
            {
                if (trimmed.Length == 7)
                {
                    if (!TryParseHex(trimmed[1], trimmed[2], out var r))
                    {
                        context.ErrorMessage = $"Invalid red color hex code '{trimmed}'; '{trimmed.Substring(1, 2)}' is not a hex number. Valid hex number characters are 0-9, a-f and A-F.";
                        return GetFailedResolverFunc<TResult>();
                    }

                    if (!TryParseHex(trimmed[3], trimmed[4], out var g))
                    {
                        context.ErrorMessage = $"Invalid green color hex code '{trimmed}'; '{trimmed.Substring(3, 2)}' is not a hex number. Valid hex number characters are 0-9, a-f and A-F.";
                        return GetFailedResolverFunc<TResult>();
                    }

                    if (!TryParseHex(trimmed[5], trimmed[6], out var b))
                    {
                        context.ErrorMessage = $"Invalid blue color hex code '{trimmed}'; '{trimmed.Substring(5, 2)}' is not a hex number. Valid hex number characters are 0-9, a-f and A-F.";
                        return GetFailedResolverFunc<TResult>();
                    }

                    Color result = new Color((float)r / 255, (float)g / 255, (float)b / 255, 1f);
                    return GetResultFunc<TResult>(ref context, result);
                }
                else if (trimmed.Length == 9)
                {

                    if (!TryParseHex(trimmed[1], trimmed[2], out var r))
                    {
                        context.ErrorMessage = $"Invalid red color hex code '{trimmed}'; '{trimmed.Substring(1, 2)}' is not a hex number. Valid hex number characters are 0-9, a-f and A-F.";
                        return GetFailedResolverFunc<TResult>();
                    }

                    if (!TryParseHex(trimmed[3], trimmed[4], out var g))
                    {
                        context.ErrorMessage = $"Invalid green color hex code '{trimmed}'; '{trimmed.Substring(3, 2)}' is not a hex number. Valid hex number characters are 0-9, a-f and A-F.";
                        return GetFailedResolverFunc<TResult>();
                    }

                    if (!TryParseHex(trimmed[5], trimmed[6], out var b))
                    {
                        context.ErrorMessage = $"Invalid blue color hex code '{trimmed}'; '{trimmed.Substring(5, 2)}' is not a hex number. Valid hex number characters are 0-9, a-f and A-F.";
                        return GetFailedResolverFunc<TResult>();
                    }

                    if (!TryParseHex(trimmed[7], trimmed[8], out var a))
                    {
                        context.ErrorMessage = $"Invalid alpha color hex code '{trimmed}'; '{trimmed.Substring(7, 2)}' is not a hex number. Valid hex number characters are 0-9, a-f and A-F.";
                        return GetFailedResolverFunc<TResult>();
                    }

                    Color result = new Color((float)r / 255, (float)g / 255, (float)b / 255, (float)a / 255);
                    return GetResultFunc<TResult>(ref context, result);

                }
                else
                {
                    context.ErrorMessage = $"Invalid color hex code '{trimmed}'; expected 6 or 8 hex characters.";
                    return GetFailedResolverFunc<TResult>();
                }
            }

            if (FastStartsWithIgnoreCase(trimmed, "rgba("))
            {
                if (trimmed[trimmed.Length - 1] != ')')
                {
                    context.ErrorMessage = $"Expected rgba statement '{trimmed}' to end with ')' (for example, 'rgba(1,1,1,1)').";
                    return GetFailedResolverFunc<TResult>();
                }

                var colorsStr = trimmed.Substring(5, trimmed.Length - 6);
                var colors = colorsStr.Split(',');

                if (colors.Length != 4)
                {
                    context.ErrorMessage = $"Expected rgba statement '{trimmed}' to contain four color components numbers separated by commas (for example, 'rgba(1,1,1,1)').";
                    return GetFailedResolverFunc<TResult>();
                }

                Color result = default;

                if (!float.TryParse(colors[0], out result.r))
                {
                    context.ErrorMessage = $"Could not parse red '{colors[0]}' to a float value when parsing rgba statement '{trimmed}'.";
                    return GetFailedResolverFunc<TResult>();
                }

                if (!float.TryParse(colors[1], out result.g))
                {
                    context.ErrorMessage = $"Could not parse green '{colors[1]}' to a float value when parsing rgba statement '{trimmed}'.";
                    return GetFailedResolverFunc<TResult>();
                }

                if (!float.TryParse(colors[2], out result.b))
                {
                    context.ErrorMessage = $"Could not parse blue '{colors[2]}' to a float value when parsing rgba statement '{trimmed}'.";
                    return GetFailedResolverFunc<TResult>();
                }

                if (!float.TryParse(colors[3], out result.a))
                {
                    context.ErrorMessage = $"Could not parse alpha '{colors[3]}' to a float value when parsing rgba statement '{trimmed}'.";
                    return GetFailedResolverFunc<TResult>();
                }

                return GetResultFunc<TResult>(ref context, result);

            }
            else if (FastStartsWithIgnoreCase(trimmed, "rgb("))
            {
                if (trimmed[trimmed.Length - 1] != ')')
                {
                    context.ErrorMessage = $"Expected rgb statement '{trimmed}' to end with ')'. (for example, 'rgba(1,1,1,1)')";
                    return GetFailedResolverFunc<TResult>();
                }

                var colorsStr = trimmed.Substring(4, trimmed.Length - 5);
                var colors = colorsStr.Split(',');

                if (colors.Length != 3)
                {
                    context.ErrorMessage = $"Expected rgb statement '{trimmed}' to contain three color components numbers separated by commas (for example, 'rgb(1,1,1)').";
                    return GetFailedResolverFunc<TResult>();
                }

                Color result = default;

                if (!float.TryParse(colors[0], out result.r))
                {
                    context.ErrorMessage = $"Could not parse red '{colors[0]}' to a float value when parsing rgb statement '{trimmed}'.";
                    return GetFailedResolverFunc<TResult>();
                }

                if (!float.TryParse(colors[1], out result.g))
                {
                    context.ErrorMessage = $"Could not parse green '{colors[1]}' to a float value when parsing rgb statement '{trimmed}'.";
                    return GetFailedResolverFunc<TResult>();
                }

                if (!float.TryParse(colors[2], out result.b))
                {
                    context.ErrorMessage = $"Could not parse blue '{colors[2]}' to a float value when parsing rgb statement '{trimmed}'.";
                    return GetFailedResolverFunc<TResult>();
                }

                result.a = 1f;
                return GetResultFunc<TResult>(ref context, result);
            }

            return null;
        }

        private static ValueResolverFunc<TResult> GetResultFunc<TResult>(ref ValueResolverContext context, Color result)
        {
            if (context.ResultType == typeof(Color))
            {
                return (ValueResolverFunc<TResult>)(object)CreateColorResultFunc(result);
            }
            else
            {
                return (ValueResolverFunc<TResult>)(object)CreateNullableColorResultFunc(result);
            }
        }

        private static bool TryParseHex(char a, char b, out uint result)
        {
            if (TryParseSingleChar(a, 16, out var aResult) && TryParseSingleChar(b, 1, out var bResult))
            {
                result = aResult + bResult;
                return true;
            }

            result = 0;
            return false;
        }

        private static bool TryParseSingleChar(char c, uint multiplier, out uint result)
        {
            if (c >= '0' && c <= '9')
            {
                result = (uint)(c - '0') * multiplier;
                return true;
            }
            else if (c >= 'A' && c <= 'F')
            {
                result = (uint)((c - 'A') + 10) * multiplier;
                return true;
            }
            else if (c >= 'a' && c <= 'f')
            {
                result = (uint)((c - 'a') + 10) * multiplier;
                return true;
            }

            result = 0;
            return false;
        }

        private static ValueResolverFunc<Color> CreateColorResultFunc(Color color)
        {
            return (ref ValueResolverContext context, int selectionIndex) => color;
        }

        private static ValueResolverFunc<Color?> CreateNullableColorResultFunc(Color? color)
        {
            return (ref ValueResolverContext context, int selectionIndex) => color;
        }

        private static bool FastStartsWithIgnoreCase(string str, string startsWithLower)
        {
            if (startsWithLower.Length > str.Length) return false;
            for (int i = 0; i < startsWithLower.Length; i++)
            {
                if (char.ToLower(str[i]) != startsWithLower[i]) return false;
            }
            return true;
        }
    }
}
#endif