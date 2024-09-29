using System.Collections.Generic;
using System.Linq;
using LSCore.Attributes;
using Sirenix.OdinInspector.Editor;
using UnityEngine;

public class UnwrapDrawer : OdinAttributeDrawer<UnwrapAttribute>
{
    private HashSet<InspectorProperty> set = new();
    
    protected override void Initialize()
    {
        base.Initialize();

        if (Property.Children.Count == 1)
        {
            set.Add(Property.Children[0]);
        }
        else
        {
            foreach (var child in Property.Children)
            {
                if (child.Attributes.Any(x => x.GetType() == typeof(UnwrapTargetAttribute)))
                {
                    set.Add(child);
                }
            }
        }
    }

    protected override void DrawPropertyLayout(GUIContent label)
    {
        foreach (var child in Property.Children)
        {
            if (set.Contains(child))
            {
                child.Draw(label);
                continue;
            }
            
            child.Draw();
        }
    }
}