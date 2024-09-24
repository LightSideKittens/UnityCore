using System.Linq;
using LSCore.Attributes;
using Sirenix.OdinInspector.Editor;
using UnityEngine;

public class UnwrapDrawer : OdinAttributeDrawer<UnwrapAttribute>
{
    protected override void DrawPropertyLayout(GUIContent label)
    {
        foreach (var child in Property.Children)
        {
            child.Draw();
        }
    }
}