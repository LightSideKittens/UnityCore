using DG.DemiEditor;
using UnityEditor;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityToolbarExtender;

[InitializeOnLoad]
public static class LocaleChanger
{
    static LocaleChanger()
    {
        ToolbarExtender.RightToolbarGUI.Add(OnGUI);
    }

    private static void OnGUI()
    {
        if (GUILayout.Button(LocalizationSettings.ProjectLocale.ToString(), GUILayout.MaxWidth(100)))
        {
            PopupWindow.Show(GUILayoutUtility.GetLastRect(), new NavigationPopup());
        }
    }
    
    private class NavigationPopup : PopupWindowContent
    {
        public override Vector2 GetWindowSize()
        {
            var size = base.GetWindowSize();
            size.x += 15;
            size.y += 300;
            return size;
        }

        private Vector2 scrollPosition;
        public override void OnGUI(Rect rect)
        {
            var s = LocalizationSettings.SelectedLocaleAsync;
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            foreach (var locale in LocalizationSettings.AvailableLocales.Locales)
            {
                DrawButton(locale);
            }
            EditorGUILayout.EndScrollView();
        }

        private static void DrawButton(Locale locale)
        {
            var project = LocalizationSettings.ProjectLocale;
            if (GUILayout.Button(project == locale ? $"❤️ {locale}" : locale.ToString(), GUILayout.MaxWidth(200)))
            {
                LocalizationSettings.ProjectLocale = locale;
                LocalizationSettings.SelectedLocale = locale;
                ToolbarExtender.Repaint();
            }
        }
    }
}