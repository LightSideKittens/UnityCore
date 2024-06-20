using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
using static GlobalContextMenuAttribute;
using static LSCore.Sirenix.Utilities.Editor.LSGenericMenuExtensions;

public class GlobalContextMenuProcessor : OdinAttributeProcessor<object>
{
    public override void ProcessSelfAttributes(InspectorProperty property, List<Attribute> attributes)
    {
        attributes.Add(new GlobalContextMenuAttribute());
    }
}

public class GlobalContextMenuAttribute : Attribute
{
    internal static Color changeColor = new Color(0.43f, 1f, 0.39f);
    internal static FontStyle defaulLabelFontStyle;
    internal static Color defaulLabelColor;
    private static bool isInited;
    
    internal static void Init()
    {
        if (isInited)
        {
            return;
        }

        isInited = true;
        defaulLabelColor = EditorStyles.label.normal.textColor;
        defaulLabelFontStyle = FontStyle.Normal;
        
    }
}

public class GlobalContextMenuDrawer<T> : OdinAttributeDrawer<GlobalContextMenuAttribute, T>, IDefinesGenericMenuItems
{
    protected override void DrawPropertyLayout(GUIContent label)
    {
        Init();
        ResetStyle();
        SerializedProperty sproperty = Property.Tree.GetUnityPropertyForPath(Property.Path);
        bool hasOverrides = sproperty is { prefabOverride: true };

        if (hasOverrides)
        {
            EditorStyles.label.fontStyle = FontStyle.Bold;
            EditorStyles.label.normal.textColor = changeColor;
            CallNextDrawer(label);
            Rect rect = GUILayoutUtility.GetLastRect();
            Rect blueRect = new Rect(rect.x-3, rect.y, 2, rect.height);
            EditorGUI.DrawRect(blueRect, changeColor);
        }
        else
        {
            CallNextDrawer(label);
        }
        
        ResetStyle();
        void ResetStyle()
        {
            EditorStyles.label.fontStyle = defaulLabelFontStyle;
            EditorStyles.label.normal.textColor = defaulLabelColor;
        }
    }

    public void PopulateGenericMenu(InspectorProperty property, GenericMenu genericMenu)
    {
        GameObject selectedGameObject = Selection.activeGameObject;
        if (selectedGameObject == null)
        {
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
            
            GameObject prefab = selectedGameObject;

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


