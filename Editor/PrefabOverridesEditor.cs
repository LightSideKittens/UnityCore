using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
using Sirenix.Utilities.Editor;
using static LSCore.Sirenix.Utilities.Editor.LSGenericMenuExtensions;

public class GlobalContextMenuProcessor : OdinAttributeProcessor<object>
{
    public override void ProcessSelfAttributes(InspectorProperty property, List<Attribute> attributes)
    {
        attributes.Add(new GlobalContextMenuAttribute());
    }
}
public class GlobalContextMenuAttribute : Attribute { }
public class GlobalContextMenuDrawer<T> : OdinAttributeDrawer<GlobalContextMenuAttribute, T>, IDefinesGenericMenuItems
{
    protected override void DrawPropertyLayout(GUIContent label)
    {
        CallNextDrawer(label);

        SerializedProperty sproperty = Property.Tree.GetUnityPropertyForPath(Property.Path);
        if (sproperty is { prefabOverride: true })
        {
            Rect rect = GUILayoutUtility.GetLastRect();
            Rect blueRect = new Rect(rect.x-3, rect.y, 3, rect.height);
            EditorGUI.DrawRect(blueRect, new Color(1f, 0.88f, 0f));
        }

    }

    public void PopulateGenericMenu(InspectorProperty property, GenericMenu genericMenu)
    {
        GameObject selectedObject = Selection.activeGameObject;
        if (selectedObject == null)
        {
            Debug.LogError("Please select a GameObject in the hierarchy.");
            return;
        }

        GameObject prefabRoot = selectedObject;
        if (prefabRoot == null)
        {
            Debug.LogError("Selected GameObject is not part of a prefab instance.");
            return;
        }
        
        SerializedProperty sproperty = property.Tree.GetUnityPropertyForPath(property.Path);

        if (sproperty == null)
        {
            return;
        }
        
        if (sproperty.prefabOverride)
        {
            List<GameObject> prefabs = new List<GameObject>();
            
            GameObject prefab = selectedObject;

            while (true)
            {
                prefab = PrefabUtility.GetCorrespondingObjectFromSource(prefab);
                if (prefab != null)
                {
                    prefabs.Add(prefab);
                }
                else
                {
                    break;
                }
            }

            var name = prefabs.Count > 1 ? "Apply to/{0}" : "Apply to {0}";
            
            foreach (var target in prefabs)
            {
                var odinName = $"Apply value to prefab '{target.name}'";
                genericMenu.RemoveItems(odinName);
                genericMenu.AddItem(new GUIContent(string.Format(name, target.name)),false, Apply);
                
                void Apply()
                { 
                    PrefabUtility.ApplyPropertyOverride(sproperty, AssetDatabase.GetAssetPath(target), InteractionMode.UserAction);
                }
                
            }
            
            genericMenu.RemoveItems("Revert to prefab value");
            genericMenu.AddItem(new GUIContent("Revert"), false, Revert);
        }

        return;
        
        void Revert()
        {
            PrefabUtility.RevertPropertyOverride(sproperty, InteractionMode.UserAction);
        }
    }

    public static bool IsPropertyModified(GameObject prefabRoot, SerializedProperty sproperty)
    {
        PropertyModification[] modifications = PrefabUtility.GetPropertyModifications(prefabRoot);

        bool isModified = false;
        string propPath = sproperty.propertyPath;

        foreach (var modification in modifications)
        {
            var targetPath = modification.propertyPath;

            if (propPath == targetPath)
            {
                isModified = true;
                break;
            }
        }

        return isModified;
    }

    private static string ResolveManagedReferencePath(string targetPath, Dictionary<long, string> managedReferencePaths)
    {
        foreach (var kvp in managedReferencePaths)
        {
            string managedReference = $"managedReferences[{kvp.Key}]";
            if (targetPath.Contains(managedReference))
            {
                return targetPath.Replace(managedReference, kvp.Value);
            }
        }

        return targetPath;
    }
}


