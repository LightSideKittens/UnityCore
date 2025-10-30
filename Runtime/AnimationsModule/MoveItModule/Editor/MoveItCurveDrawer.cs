#if UNITY_EDITOR
using LSCore;
using Newtonsoft.Json;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;

[ResolverPriority(100)]
public class MoveItCurveResolver : ProcessedMemberPropertyResolver<MoveItCurve>
{
    public override bool IsCollection => false;
}

[CustomPropertyDrawer(typeof(MoveItCurve))]
[DrawerPriority(DrawerPriorityLevel.SuperPriority)]
public class MoveItCurveDrawer : PropertyDrawer
{
    private MoveItCurveWindow window;
    
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var targetObject = property.serializedObject.targetObject;
        var serializedObject = property.serializedObject;
        var propertyPath = property.propertyPath;
        EditorGUI.BeginProperty(position, label, property);
        var curve = (MoveItCurve)property.boxedValue;
        var oldJson = JsonConvert.SerializeObject(curve, SerializationSettings.Default.settings);
        
        var labelRect = position.TakeFromLeft(position.width / 3);
        GUI.Label(labelRect, label);
        if (GUI.Button(position, "Edit"))
        {
            if (window != null)
            {
                window.Close();
                window = null;
            }
            
            window = MoveItCurveWindow.ShowWindow(curve);
            window.editor.editors[0].IsFocused = true;
            window.editor.snapping = false;
            
            window.Edited += () =>
            {
                if (targetObject == null)
                {
                    window.Close();
                    return;
                }

                try
                {
                    serializedObject.Update();
                }
                catch
                {
                    serializedObject = new SerializedObject(targetObject);
                    property = serializedObject.FindProperty(propertyPath);
                    serializedObject.Update();
                }
                
                var newJson = JsonConvert.SerializeObject(curve, SerializationSettings.Default.settings);
                if (newJson != oldJson)
                {
                    oldJson = newJson;
                    property.boxedValue = curve;
                    serializedObject.ApplyModifiedPropertiesWithoutUndo();
                    EditorUtility.SetDirty(serializedObject.targetObject);
                }
            };
        }
        
        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return 40;
    }
}
#endif