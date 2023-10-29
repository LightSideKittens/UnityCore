using LSCore.Extensions;
using Attributes;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.OdinInspector.Editor.Drawers;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Attributes.Editor.Drawers
{
    public partial class ColoredFieldPropertyDrawer : OdinGroupDrawer<ColoredFieldAttribute>
    {
        private bool isOpen;
        private GUIStyle style;
        private Texture2D blueHeartTexture;
        private Texture2D heartTexture;
        private string labelName;
        private float labelWidth;
        private Color normalColor;
        private Color hoverColor;

        protected override void DrawPropertyLayout(GUIContent label)
        {
            if (Property.Children[0].ValueEntry.WeakSmartValue != null)
            {
                style.normal.background = EditorUtils.GetTextureByColor(Color.gray * 1.2f);
                BaseDraw(label);
            }
            else
            {
                for (int i = 0; i < Property.Children.Count; i++)
                {
                    Property.Children[i].Draw();
                }
            }
        }

        private bool Foldout(bool isVisible, GUIContent label, GUIStyle style = null)
        {
            float fieldWidth = EditorGUIUtility.fieldWidth;
            EditorGUIUtility.fieldWidth = 10f;
            Rect controlRect = EditorGUILayout.GetControlRect(false);
            EditorGUIUtility.fieldWidth = fieldWidth;
            return Foldout(controlRect, isVisible, label, style);
        }

        private bool Foldout(Rect rect, bool isVisible, GUIContent label, GUIStyle style = null)
        {
            style ??= SirenixGUIStyles.Foldout;
            style.normal.textColor = normalColor;
            style.focused.textColor = normalColor;
            style.active.textColor = normalColor;
            style.hover.textColor = normalColor;
            EventType type = Event.current.type;
            bool flag = false;
            if (type != EventType.Layout)
                flag = rect.Contains(Event.current.mousePosition);

            if (flag)
            {
                GUIHelper.PushLabelColor(hoverColor);
            }
            else if (isOpen)
            {
                GUIHelper.PushLabelColor(normalColor);
            }
            
            if (flag && type == EventType.MouseMove)
                GUIHelper.RequestRepaint();
            if (type == EventType.MouseDown & flag && Event.current.button == 0)
            {
                isVisible = !isVisible;
                GUIHelper.RequestRepaint();
                GUIHelper.PushGUIEnabled(true);
                Event.current.Use();
                GUIHelper.PopGUIEnabled();
                GUIHelper.RemoveFocusControl();
            }
            isVisible = EditorGUI.Foldout(rect, isVisible, label, style);
            if (flag || isOpen)
                GUIHelper.PopLabelColor();
            return isVisible;
        }

        private void SetupHeader(Rect rect)
        {
            var prop = Property.Children[0];
            PropertyContextMenuDrawer.AddRightClickArea(prop, rect);
            
            var val = prop.ValueEntry.WeakSmartValue;
            var dragAndDropZone = rect;
            dragAndDropZone = dragAndDropZone.AlignCenter(18, 18);
            SdfIcons.DrawIcon(dragAndDropZone, SdfIconType.List, hoverColor);

            prop.ValueEntry.WeakSmartValue = DragAndDropUtilities.DragAndDropZone(dragAndDropZone, val, val.GetType(), true, true);
        }

        private Rect BeginBoxHeader()
        {
            GUILayout.Space(-3f);
            Rect rect = EditorGUILayout.BeginHorizontal(SirenixGUIStyles.BoxHeaderStyle, GUILayoutOptions.ExpandWidth());
            if (Event.current.type == EventType.Repaint)
            {
                rect.x -= 3f;
                rect.width += 6f;
                
                EventType type = Event.current.type;
                bool flag = false;
                if (type != EventType.Layout)
                    flag = rect.Contains(Event.current.mousePosition);
                
                var color = Attribute.colorData.color;
                if (flag)
                {
                    color = color.CloneAndChangeSaturation(1.2f);
                    color = color.CloneAndChangeBrightness(1.2f);
                    
                    GUIHelper.PushColor(color);
                }
                else
                {
                    GUIHelper.PushColor(color);
                }

                GUI.DrawTexture(rect, Texture2D.whiteTexture);
                GUIHelper.PopColor();
                SirenixEditorGUI.DrawBorders(rect, 0, 0, 0, 1, new Color(0.0f, 0.0f, 0.0f, 0.4f));
            }
            GUIHelper.PushLabelWidth(GUIHelper.BetterLabelWidth - 4f);

            return rect;
        }
    }
}