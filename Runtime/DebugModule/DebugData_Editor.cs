#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityToolbarExtender;

namespace LSCore
{
    [InitializeOnLoad]
    public partial class LSDebugData
    {
        private static NavigationPopup popupContent;
        
        static LSDebugData()
        {
            ToolbarExtender.RightToolbarGUI.Add(OnToolbarRightGUI);
            popupContent = new NavigationPopup();
        }

        private static void OnToolbarRightGUI()
        {
            var rect = GUILayoutUtility.GetRect(new GUIContent(Config.Environment), GUI.skin.button, GUILayout.MaxWidth(100));
            
            if (GUI.Button(rect, Config.Environment))
            {
                PopupWindow.Show(rect, popupContent);
            }
        }

        public class NavigationPopup : PopupWindowContent
        {
            public override void OnGUI(Rect rect)
            {
                foreach (var environment in LSConsts.Env.Environments)
                {
                    var isSelected = environment == Config.Environment;

                    if (GUILayout.Button(environment + (isSelected ? " ❤️" : "")))
                    {
                        Config.Environment = environment;
                        Config.Save();
                        ToolbarExtender.Repaint();
                    }
                }
            }
        }
    }
}

#endif