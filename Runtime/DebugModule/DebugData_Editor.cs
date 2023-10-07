#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityToolbarExtender;

namespace LSCore
{
    [InitializeOnLoad]
    public partial class LSDebugData
    {
        private static NavigationPopup popup;
        static LSDebugData()
        {
            ToolbarExtender.RightToolbarGUI.Add(OnToolbarRightGUI);
            popup = new NavigationPopup();
        }

        private static void OnToolbarRightGUI()
        {
            var rect = GUILayoutUtility.GetRect(new GUIContent(Environment), GUI.skin.button, GUILayout.MaxWidth(100));
            
            if (GUI.Button(rect, Environment))
            {
                PopupWindow.Show(rect, popup);
            }
            
            GUI.changed = true;
        }

        public class NavigationPopup : PopupWindowContent
        {
            public override void OnGUI(Rect rect)
            {
                foreach (var environment in LSConsts.Env.Environments)
                {
                    var isSelected = environment == Environment;

                    if (GUILayout.Button(environment + (isSelected ? " ❤️" : "")))
                    {
                        Environment = environment;
                        Save();
                    }
                }
            }
        }
    }
}

#endif