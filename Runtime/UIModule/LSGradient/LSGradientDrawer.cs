using System.Reflection;
using DG.DemiEditor;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;
using UnityToolbarExtender;
using Object = UnityEngine.Object;

namespace LSCore
{
    [CustomPropertyDrawer(typeof(LSGradient))]
    public class LSGradientDrawer : PropertyDrawer
    {
        private Gradient gradient = new Gradient();
        private bool isInited;
        private LSGradient lsGradient;
        private SerializedProperty lsGradientProp;
        private bool isHold;
        private bool isDirty;
        private int selected;
        private double lastClickTime;
        private MethodInfo onValidate;
        private Object target;
        private const float DoubleClickTime = 0.3f;

        public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
        {
            if (!isInited || lsGradientProp != property)
            {
                lsGradientProp = property;
                lsGradient = property.CastTo<LSGradient>();
                lsGradient.FillLegacy(gradient);
                target = property.serializedObject.targetObject;
                onValidate = target.GetType().GetMethod("OnValidate", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                isInited = true;
            }
            
            var e = Event.current;
            var mousePos = e.mousePosition;
            rect.width -= 10;

            if (isHold)
            {
                GUI.changed = true;
                
                if (selected != -1 && e.type == EventType.MouseDrag)
                {
                    if (e.button == 0)
                    {
                        lsGradient.SetPostion(selected, Mathf.Clamp01((mousePos.x - rect.xMin) / rect.width));
                        isDirty = true;
                    }
                }
                
                if (e.type == EventType.MouseUp)
                {
                    isHold = false;
                    GUI.changed = false;
                }
            }

            if (e.type == EventType.ExecuteCommand)
            {
                if (e.commandName == "ColorPickerChanged")
                {
                    lsGradient.SetColor(selected, LSColorPicker.Color);
                    isDirty = true;
                }
            }
            
            rect.yMax += EditorGUILayout.GetControlRect().height;
            
            if (rect.Contains(mousePos))
            {
                if (e.type == EventType.MouseDown)
                {
                    e.type = EventType.Ignore;
                }
            }
            
            gradient = EditorGUI.GradientField(rect, gradient);
            rect = EditorGUILayout.GetControlRect();
            rect.width -= 10;
            
            var xOffset = rect.min.x - 5;
            var btnSize = new Rect(xOffset, rect.min.y, 10, 15);
            
            for (int i = 0; i < lsGradient.Count; i++)
            {
                var data = lsGradient[i];
                btnSize.x = data.position * rect.width + xOffset;
                Box(btnSize, data.color, i == selected);
                
                if (!btnSize.Contains(mousePos)) continue;

                switch (e.type)
                {
                    case EventType.MouseDown:
                        if (e.button == 0)
                        {
                            selected = i;
                            isHold = true;
                            double timeSinceLastClick = EditorApplication.timeSinceStartup - lastClickTime;

                            if (timeSinceLastClick < DoubleClickTime)
                            {
                                LSColorPicker.Show(c => {}, data.color);
                            }

                            lastClickTime = (float)EditorApplication.timeSinceStartup;
                        }
                        else if(e.button == 1)
                        {
                            lsGradient.Remove(i);
                            isDirty = true;
                        }
                        return;
                }
            }
            
            if (rect.Contains(mousePos))
            {
                switch (e.type)
                {
                    case EventType.MouseDown:
                        if (e.button == 0)
                        {
                            var pos = Mathf.Clamp01((mousePos.x - rect.xMin) / rect.width);
                            lsGradient.Add(pos, lsGradient.Evaluate(pos));
                            selected = lsGradient.Count - 1;
                            isHold = true;
                            isDirty = true;
                        }
                        break;
                }
            }

            if (isDirty)
            {
                lsGradient.FillLegacy(gradient);
                onValidate?.Invoke(target, null);
                EditorUtility.SetDirty(target);
                isDirty = false;
            }
        }

        private void Box(Rect rect, in Color color, bool isSelected)
        {
            if (isSelected)
            {
                var selectedStyle = new GUIStyle();
                selectedStyle.normal.background = EditorUtils.GetTextureByColor(Color.black);
                var newRect = rect;
                newRect.position -= Vector2.one * 2;
                newRect.size += Vector2.one * 4;
                GUI.Box(newRect, string.Empty, selectedStyle);
            }
            
            var style = new GUIStyle();
            style.normal.background = EditorUtils.GetTextureByColor(color); 
            GUI.Box(rect, string.Empty, style);
            rect.TakeFromTop(12);
            var alpha = color.a;
            style.normal.background = EditorUtils.GetTextureByColor(new Color(alpha, alpha, alpha, 1)); 
            GUI.Box(rect, string.Empty, style);
        }
    }
}