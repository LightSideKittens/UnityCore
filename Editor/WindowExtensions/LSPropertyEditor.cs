using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Object = UnityEngine.Object;


public class LSPropertyEditor
{
    private static Type type;
    private static MethodInfo getInspectedObject;
    private static FieldInfo getPropertyEditors;
    private static MethodInfo hoveredPropertyEditor;
    private static MethodInfo focusedPropertyEditor;
    private static MethodInfo openPropertyEditor;
    private object window;
    private static LSPropertyEditor hovered = new();

    public static bool TryGetHovered(out LSPropertyEditor editor)
    {
        hovered.window = hoveredPropertyEditor.Invoke(null, null);
        editor = hovered;
        return editor.window != null;
    }
    
    public static bool TryGetFocused(out LSPropertyEditor editor)
    {
        hovered.window = focusedPropertyEditor.Invoke(null, null);
        editor = hovered;
        return editor.window != null;
    }

    static LSPropertyEditor()
    {
        type = Type.GetType("UnityEditor.PropertyEditor,UnityEditor");
        getInspectedObject = type.GetMethod("GetInspectedObject", BindingFlags.Instance | BindingFlags.NonPublic);
        getPropertyEditors = type.GetField("m_AllPropertyEditors", BindingFlags.Static | BindingFlags.NonPublic);

        var types = new[] { typeof(Object), typeof(bool)};
        openPropertyEditor = type.GetMethod("OpenPropertyEditor", BindingFlags.Static | BindingFlags.NonPublic, null, types, null);

        hoveredPropertyEditor = type.GetProperty("HoveredPropertyEditor", BindingFlags.Static | BindingFlags.NonPublic).GetGetMethod(true);
        focusedPropertyEditor = type.GetProperty("FocusedPropertyEditor", BindingFlags.Static | BindingFlags.NonPublic).GetGetMethod(true);
    }
    

    private LSPropertyEditor(){}

    public static List<LSPropertyEditor> GetAll()
    {
        var list = (IList)getPropertyEditors.GetValue(null);
        var result = new List<LSPropertyEditor>();

        for (int i = 0; i < list.Count; i++)
        {
            result.Add(new LSPropertyEditor(){window = list[i]});
        }

        return result;
    }

    public Object InspectedObject => (Object)getInspectedObject.Invoke(window, null);

    public static void Show(Object obj, bool showWindow = true)
    {
        openPropertyEditor.Invoke(null, new object[]{obj, showWindow});
    }
    
    public static bool TryGetInspectedObjectFromHoveredEditor<T>(out T obj) where T : Object
    {
        if (TryGetHovered(out var editor))
        {
            if (editor.InspectedObject is T result)
            {
                obj = result;
                return true;
            }
        }

        obj = null;
        return false;
    }
    
    public static class AllEditors
    {
        public static bool TryGetInspectedObject<T>(out T obj) where T : Object
        {
            foreach (var propertyEditor in GetAll())
            {
                if (propertyEditor.InspectedObject is T result)
                {
                    obj = result;
                    return true;
                }
            }

            obj = null;
            return false;
        }
        
        public static bool TryGetInspectedObject(out Object obj)
        {
            foreach (var propertyEditor in GetAll())
            {
                obj = propertyEditor.InspectedObject; 
                return true;
            }

            obj = null;
            return false;
        }
    }
    
    public static class Hovered
    {
        public static bool TryGetInspectedObject<T>(out T obj) where T : Object
        {
            if (TryGetHovered(out var editor))
            {
                if (editor.InspectedObject is T result)
                {
                    obj = result;
                    return true;
                }
            }

            obj = null;
            return false;
        }
    }
}


