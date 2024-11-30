using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using UnityEngine;

public class GetContextDrawer : OdinAttributeDrawer<GetContextAttribute, Object>
{
    private object context;
    
    protected override void Initialize()
    {
        base.Initialize();
        context = Property.SerializationRoot.ValueEntry.WeakSmartValue;
        Property.Info.GetMemberInfo().SetMemberValue(Property.Parent.BaseValueEntry.WeakSmartValue, context);
    }

    protected override void DrawPropertyLayout(GUIContent label)
    {
    }
}