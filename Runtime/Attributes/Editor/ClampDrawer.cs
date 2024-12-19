using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEngine;

public class ClampDrawer : OdinAttributeDrawer<ClampAttribute>
{
    protected override void DrawPropertyLayout(GUIContent label)
    {
        var property = Property;

        if (property.ValueEntry.TypeOfValue != typeof(float) &&
            property.ValueEntry.TypeOfValue != typeof(int))
        {
            SirenixEditorGUI.ErrorMessageBox("ClampDrawer can only be used on float or int");
            CallNextDrawer(label);
            return;
        }

        var clampAttribute = Attribute;
        var value = property.ValueEntry.WeakSmartValue;

        switch (value)
        {
            case float floatValue:
            {
                var clampedValue = Mathf.Clamp(floatValue, clampAttribute.min, clampAttribute.max);
                property.ValueEntry.WeakSmartValue = clampedValue;
                break;
            }
            case int intValue:
            {
                var clampedValue = (int)Mathf.Clamp(intValue, clampAttribute.min, clampAttribute.max);
                property.ValueEntry.WeakSmartValue = clampedValue;
                break;
            }
        }

        CallNextDrawer(label);
    }
}