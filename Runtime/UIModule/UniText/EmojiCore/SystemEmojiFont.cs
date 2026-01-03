using UnityEngine;
using System.IO;
using System.Collections.Generic;

#if UNITY_ANDROID && !UNITY_EDITOR
using System.Xml;
#endif

public static class SystemEmojiFont
{
    public static string GetDefaultEmojiFont()
    {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
        return GetWindowsEmojiFont();
#elif UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
        return GetMacOSEmojiFont();
#elif UNITY_IOS
        return GetiOSEmojiFont();
#elif UNITY_ANDROID
        return GetAndroidEmojiFont();
#elif UNITY_STANDALONE_LINUX
        return GetLinuxEmojiFont();
#else
        return null;
#endif
    }

    // ==================== WINDOWS ====================
    static string GetWindowsEmojiFont()
    {
        var paths = new[]
        {
            Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Fonts), "seguiemj.ttf"),
            @"C:\Windows\Fonts\seguiemj.ttf"
        };

        foreach (var path in paths)
        {
            if (File.Exists(path)) return path;
        }
        return null;
    }

    // ==================== macOS ====================
    static string GetMacOSEmojiFont()
    {
        var paths = new[]
        {
            "/System/Library/Fonts/Apple Color Emoji.ttc",
            "/Library/Fonts/Apple Color Emoji.ttc"
        };

        foreach (var path in paths)
        {
            if (File.Exists(path)) return path;
        }
        return null;
    }

    // ==================== iOS ====================
    static string GetiOSEmojiFont()
    {
        var paths = new[]
        {
            "/System/Library/Fonts/Core/AppleColorEmoji.ttc",
            "/System/Library/Fonts/Apple Color Emoji.ttc",
            "/System/Library/Fonts/AppleColorEmoji.ttf"
        };

        foreach (var path in paths)
        {
            if (File.Exists(path)) return path;
        }
        return null;
    }

    // ==================== LINUX ====================
    static string GetLinuxEmojiFont()
    {
        var paths = new[]
        {
            "/usr/share/fonts/truetype/noto/NotoColorEmoji.ttf",
            "/usr/share/fonts/google-noto-emoji/NotoColorEmoji.ttf",
            "/usr/share/fonts/noto-emoji/NotoColorEmoji.ttf",
            "/usr/share/fonts/truetype/ancient-scripts/Symbola_hint.ttf",
            "/usr/share/fonts/TTF/Symbola.ttf"
        };

        foreach (var path in paths)
        {
            if (File.Exists(path)) return path;
        }
        return null;
    }

    // ==================== ANDROID ====================
    static string GetAndroidEmojiFont()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        // Сначала пробуем парсить fonts.xml для точного определения
        var fontFromXml = ParseAndroidFontsXml();
        if (fontFromXml != null) return fontFromXml;
#endif
        // Fallback: пробуем известные пути производителей
        var paths = new[]
        {
            // Samsung
            "/system/fonts/SamsungColorEmoji.ttf",
            // Google/Stock Android
            "/system/fonts/NotoColorEmoji.ttf",
            // Older Android
            "/system/fonts/NotoColorEmojiLegacy.ttf",
            // HTC
            "/system/fonts/HTCColorEmoji.ttf",
            // LG
            "/system/fonts/LGColorEmoji.ttf",
            // Huawei
            "/system/fonts/HuaweiColorEmoji.ttf",
            // Xiaomi
            "/system/fonts/MiColorEmoji.ttf",
            // OnePlus
            "/system/fonts/OnePlusEmoji.ttf",
            // Generic fallbacks
            "/system/fonts/ColorEmoji.ttf",
            "/system/fonts/Emoji.ttf",
        };

        foreach (var path in paths)
        {
            if (File.Exists(path)) return path;
        }

        // Последняя попытка: сканируем папку /system/fonts/
        return ScanForEmojiFont("/system/fonts/");
    }

#if UNITY_ANDROID && !UNITY_EDITOR
    static string ParseAndroidFontsXml()
    {
        var xmlPaths = new[]
        {
            "/system/etc/fonts.xml",
            "/vendor/etc/fonts.xml"
        };

        foreach (var xmlPath in xmlPaths)
        {
            if (!File.Exists(xmlPath)) continue;

            try
            {
                var xml = new XmlDocument();
                xml.Load(xmlPath);

                // Ищем familyset/family с name="sans-serif" и ищем emoji fallback
                // Или ищем напрямую font с именем содержащим "emoji"
                var fontNodes = xml.SelectNodes("//font");
                if (fontNodes != null)
                {
                    foreach (XmlNode node in fontNodes)
                    {
                        var fontName = node.InnerText?.Trim();
                        if (string.IsNullOrEmpty(fontName)) continue;

                        if (fontName.ToLower().Contains("emoji") && 
                            fontName.ToLower().Contains("color"))
                        {
                            var fullPath = Path.Combine("/system/fonts", fontName);
                            if (File.Exists(fullPath)) return fullPath;
                        }
                    }

                    // Если не нашли ColorEmoji, ищем просто Emoji
                    foreach (XmlNode node in fontNodes)
                    {
                        var fontName = node.InnerText?.Trim();
                        if (string.IsNullOrEmpty(fontName)) continue;

                        if (fontName.ToLower().Contains("emoji"))
                        {
                            var fullPath = Path.Combine("/system/fonts", fontName);
                            if (File.Exists(fullPath)) return fullPath;
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Failed to parse {xmlPath}: {e.Message}");
            }
        }

        return null;
    }
#endif

    static string ScanForEmojiFont(string directory)
    {
        if (!Directory.Exists(directory)) return null;

        try
        {
            var files = Directory.GetFiles(directory, "*.ttf");
            
            // Приоритет: ColorEmoji > Emoji
            foreach (var file in files)
            {
                var name = Path.GetFileName(file).ToLower();
                if (name.Contains("coloremoji")) return file;
            }

            foreach (var file in files)
            {
                var name = Path.GetFileName(file).ToLower();
                if (name.Contains("emoji")) return file;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Failed to scan {directory}: {e.Message}");
        }

        return null;
    }
}