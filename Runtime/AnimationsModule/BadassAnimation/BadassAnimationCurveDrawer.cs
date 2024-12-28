#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;

[ResolverPriority(100)]
public class BadassAnimationCurveResolver : ProcessedMemberPropertyResolver<BadassAnimationCurve>
{
    public override bool IsCollection => false;
}

[CustomPropertyDrawer(typeof(BadassAnimationCurve))]
[DrawerPriority(DrawerPriorityLevel.SuperPriority)]
public class BadassAnimationCurveDrawer : PropertyDrawer
{
    private BadassAnimation window;
    
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var targetObject = property.serializedObject.targetObject;
        var serializedObject = property.serializedObject;
        var jsonProperty = property.FindPropertyRelative("json");
        var jsonPropertyPath = jsonProperty.propertyPath;
        EditorGUI.BeginProperty(position, label, jsonProperty);
        var curve = (BadassAnimationCurve)property.boxedValue;
        
        var labelRect = position.TakeFromLeft(position.width / 3);
        GUI.Label(labelRect, label);
        if (GUI.Button(position, "Edit"))
        {
            if (window != null)
            {
                window.Close();
                window = null;
            }
            
            window = BadassAnimation.ShowWindow(curve);
            
            window.OnEdited += () =>
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
                    jsonProperty = serializedObject.FindProperty(jsonPropertyPath);
                    serializedObject.Update();
                }
                
                var newJson = curve.GetJson();
                var oldJson = jsonProperty.stringValue;
                if (newJson != oldJson)
                {
                    jsonProperty.stringValue = newJson;
                    serializedObject.ApplyModifiedPropertiesWithoutUndo();
                    EditorUtility.SetDirty(serializedObject.targetObject);
                }
            };
        }
        
        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return 50;
    }
}
#endif