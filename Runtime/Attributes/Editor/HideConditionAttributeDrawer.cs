using System.Collections;
using Sirenix.OdinInspector.Editor;
using UnityEngine;

namespace LSCore.Attributes
{
    public class HideConditionAttributeDrawer : OdinAttributeDrawer<HideConditionAttribute>
    {
        protected override void DrawPropertyLayout(GUIContent label)
        {
            var property = this.Property.Parent;
            var parent = property.Parent;
            
            if (parent != null)
            {
                if (parent.ValueEntry.WeakSmartValue is IList list)
                {
                    if (list.IndexOf(property.ValueEntry.WeakSmartValue) != 0)
                    {
                        CallNextDrawer(label);
                    }
                }
            }
        }
    }

}