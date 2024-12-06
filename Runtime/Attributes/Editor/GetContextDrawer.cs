using System.Reflection;
using LSCore.Attributes;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;

public class GetContextDrawer : OdinDrawer
{
    private object context;

    public override bool CanDrawProperty(InspectorProperty property)
    {
        return property.Attributes.HasAttribute<GetContextAttribute>();
    }

    protected override void Initialize()
    {
        context = Property.SerializationRoot.ValueEntry.WeakSmartValue;
        Property.Info.GetMemberInfo().SetMemberValue(Property.Parent.BaseValueEntry.WeakSmartValue, context);
    }

    protected override void DrawPropertyLayout(GUIContent label)
    {
    }
}

public class SceneGUIDrawer : OdinDrawer
{
    public override bool CanDrawProperty(InspectorProperty property)
    {
        return property.Info.PropertyType == PropertyType.Method && property.Attributes.HasAttribute<SceneGUIAttribute>();
    }
    
    private MethodInfo sceneGUIMethod;
    private bool ignoreFirst;
    
    protected override void Initialize()
    {
        sceneGUIMethod = (MethodInfo)Property.Info.GetMemberInfo();
        ignoreFirst = true;
        SceneView.duringSceneGui += OnSceneGUI;
        Selection.selectionChanged += OnSelectionChanged;
    }

    private void OnSelectionChanged()
    {
        if (ignoreFirst)
        {
            ignoreFirst = false;
            return;
        }
        
        var obj = Property.SerializationRoot.ValueEntry.WeakSmartValue;
        var selection = Selection.activeObject;
        
        if (selection == null) goto unsub;


        if (obj is Component component)
        {
            var tr = component.transform;
            
            if (selection is GameObject go)
            {
                var tr2 = go.transform;

                if (tr2 == tr || !tr2.IsChildOf(tr))
                {
                    goto unsub;
                }
                
                return;
            }
        }
        
        unsub:
        SceneView.duringSceneGui -= OnSceneGUI;
        Selection.selectionChanged -= OnSelectionChanged;
    }

    private void OnSceneGUI(SceneView sceneView)
    {
        var obj = Property?.Parent?.ValueEntry?.WeakSmartValue;
        
        if (obj == null)
        {
            SceneView.duringSceneGui -= OnSceneGUI;
            Selection.selectionChanged -= OnSelectionChanged;
            return;
        }
        
        sceneGUIMethod.Invoke(obj, null);
    }

    protected override void DrawPropertyLayout(GUIContent label)
    {
        
    }
}