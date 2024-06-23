#if UNITY_EDITOR
using System.Reflection;
using Sirenix.OdinInspector.Editor;

public static class InspectorPropertyExtensions
{
    public static object CallMethod(this InspectorProperty property, params object[] parameters)
    {
        var method = (MethodInfo)property.Info.GetMemberInfo();
        return method.Invoke(property.Parent.ValueEntry.WeakSmartValue, parameters);
    }
}
#endif

