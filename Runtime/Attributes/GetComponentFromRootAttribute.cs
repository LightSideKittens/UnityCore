using System;
using System.Diagnostics;
using UnityEngine;

#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor;
#endif

namespace LSCore.Attributes
{
    [Conditional("UNITY_EDITOR")]
    public class GetComponentAttribute : Attribute { }
}
#if UNITY_EDITOR
public class GetComponentFromRootAttributeDrawer : OdinAttributeDrawer<LSCore.Attributes.GetComponentAttribute>
{
    protected override void Initialize()
    {
        base.Initialize();
        
        var target = Property.Tree.WeakTargets[0] as MonoBehaviour;
        if (target != null)
        {
            var componentType = Property.ValueEntry.TypeOfValue;
            var component = target.GetComponent(componentType);

            if (component != null && Property.ValueEntry.WeakSmartValue == null)
            {
                Property.ValueEntry.WeakSmartValue = component;
            }
        }
    }

    protected override void DrawPropertyLayout(GUIContent label)
    {
        CallNextDrawer(label);
    }
}
#endif