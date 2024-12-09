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
    
    public static InspectorProperty GetPropertyByPath(this InspectorProperty property, string path)
    {
        if (property == null) return null;
        if (string.IsNullOrEmpty(path)) return property;

        var segments = path.Split('/');
        var currentProperty = property;

        foreach (var segment in segments)
        {
            if (segment == "..")
            {
                currentProperty = currentProperty.Parent;
                if (currentProperty == null)
                {
                    return null;
                }
            }
            else
            {
                currentProperty = currentProperty.Children[segment];
                if (currentProperty == null)
                {
                    return null;
                }
            }
        }

        return currentProperty;
    }
}
#endif

