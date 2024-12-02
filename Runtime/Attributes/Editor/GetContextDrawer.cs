using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
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