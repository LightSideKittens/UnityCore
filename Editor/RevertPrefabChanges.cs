using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public static class RevertPrefabChanges
{
    private static HashSet<string> properties = new()
    {
        "m_AnchoredPosition",
        "m_SizeDelta"
    };
    
    [MenuItem("CONTEXT/RectTransform/Revert All")]
    public static void RevertChanges()
    {
        RevertChanges(null);
    }
    
    [MenuItem("CONTEXT/RectTransform/Revert Except Pos and Size")]
    public static void RevertChangesExceptPosAndSize()
    {
        RevertChanges(x => !properties.Contains(x));
    }
    
    public static void RevertChanges(Func<string, bool> predicate)
    {
        var selectedObjects = Selection.gameObjects;

        if (selectedObjects == null || selectedObjects.Length == 0)
        {
            return;
        }

        foreach (var selectedObject in selectedObjects)
        {
            var rect = selectedObject.GetComponent<RectTransform>();

            if (PrefabUtility.IsPartOfPrefabInstance(rect))
            {
                SerializedObject serializedObject = new SerializedObject(rect);
            
                SerializedProperty property = serializedObject.GetIterator();
            
                if (property.NextVisible(true))
                {
                    do
                    {
                        if (predicate == null || predicate.Invoke(property.name))
                        {
                            PrefabUtility.RevertPropertyOverride(property, InteractionMode.UserAction);
                        }
                    }
                    while (property.NextVisible(false));
                }
            }
        }
    }
}