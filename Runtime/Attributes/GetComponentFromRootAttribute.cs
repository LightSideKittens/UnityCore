using System;
using Sirenix.OdinInspector.Editor;
using UnityEngine;

namespace LSCore.Attributes
{
    public class GetComponentAttribute : Attribute { }
}

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



