#if UNITY_EDITOR
using System;
using System.Reflection;
using UnityEngine;

namespace LSCore.AnimationsModule
{
    public class LSCurveEditor
    {
        private static Type type;
        private static Type curveWrapperType;
        private static MethodInfo beginViewGUI;
        private static MethodInfo gridGUI;
        private static MethodInfo drawWrapperPopups;
        private static MethodInfo endViewGUI;
        private object instance;
        
        static LSCurveEditor()
        {
            type = Type.GetType("UnityEditor.CurveEditor,UnityEditor");
            curveWrapperType = Type.GetType("UnityEditor.CurveWrapper,UnityEditor");
            
            beginViewGUI = type.GetMethod("BeginViewGUI");
            gridGUI = type.GetMethod("GridGUI");
            drawWrapperPopups = type.GetMethod("DrawWrapperPopups");
            endViewGUI = type.GetMethod("EndViewGUI");
        }
        
        public LSCurveEditor(Rect rect, bool minimalGUI)
        {
            instance = Activator.CreateInstance(type, rect, Array.CreateInstance(curveWrapperType, 0), minimalGUI);
        }

        public void OnGUI()
        {
            beginViewGUI.Invoke(instance, null);
            gridGUI.Invoke(instance, null);
            drawWrapperPopups.Invoke(instance, null);
            endViewGUI.Invoke(instance, null);
        }
    }
}
#endif