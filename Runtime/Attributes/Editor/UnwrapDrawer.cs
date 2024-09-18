using System.Linq;
using LSCore.Attributes;
using Sirenix.OdinInspector.Editor;
using UnityEngine;

public class UnwrapDrawer : OdinAttributeDrawer<UnwrapAttribute>
{
    protected override void DrawPropertyLayout(GUIContent label)
    {
        Property.Children.First().Draw(label);
    }
}