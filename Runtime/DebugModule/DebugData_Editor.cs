#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityToolbarExtender;

namespace LSCore
{
    [InitializeOnLoad]
    public partial class LSDebugData
    {
        static LSDebugData()
        {
            ToolbarExtender.RightToolbarGUI.Add(OnToolbarRightGUI);
        }

        private static void OnToolbarRightGUI()
        {
            var rect = GUILayoutUtility.GetRect(new GUIContent(Environment), GUI.skin.button, GUILayout.MaxWidth(100));
            if (GUI.Button(rect, Environment))
            {
                PopupWindow.Show(rect, new NavigationPopup());
            }
        }

        public class NavigationPopup : PopupWindowContent
        {
            public override void OnGUI(Rect rect)
            {
                foreach (var environment in LSConsts.Env.Environments)
                {
                    if (GUILayout.Button(environment))
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