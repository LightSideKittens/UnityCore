#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace LSCore
{
    public class PricesDrawer : OdinValueDrawer<Prices>
    {
        protected override void DrawPropertyLayout(GUIContent label)
        {
            var prices = ValueEntry.SmartValue;

            var rect = GUILayoutUtility.GetRect(new GUIContent("Add Prices"), GUI.skin.button, GUILayout.MaxWidth(100));
            if (GUI.Button(rect, "Add Prices"))
            {
                PopupWindow.Show(rect, prices.Popup);
            }

            var count = 0;
            
            foreach (var price in prices)
            {
                if (count == 0)
                {
                    EditorGUILayout.BeginHorizontal();
                }
                
                if (price.Editor_Draw())
                {
                    this.SetDirtyParent();
                }
                
                count++;
                if (count == 2)
                {
                    EditorGUILayout.EndHorizontal();
                    count = 0;
                }
            }
            
            if (count == 1)
            {
                EditorGUILayout.EndHorizontal();
            }
        }
    }
}
#endif
