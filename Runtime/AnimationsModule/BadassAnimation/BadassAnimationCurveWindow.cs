#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

public class BadassAnimationCurveWindow : EditorWindow
{
    public BadassAnimationMultiCurveEditor editor;
    public event Action Edited {add => editor.First.Edited += value; remove => editor.First.Edited -= value; }
    
    public static BadassAnimationCurveWindow ShowWindow(BadassAnimationCurve curve)
    {
        var window = GetWindow<BadassAnimationCurveWindow>();
        window.editor = new BadassAnimationMultiCurveEditor(new BadassAnimationCurveEditor(curve, window));
        return window;
    }
    
    public static BadassAnimationCurveWindow CreateAndShowWindow(BadassAnimationCurve curve)
    {
        var window = CreateWindow<BadassAnimationCurveWindow>();
        window.editor = new BadassAnimationMultiCurveEditor(new BadassAnimationCurveEditor(curve, window));
        return window;
    }

    private void OnEnable()
    {
        editor?.OnEnable();
    }

    private void OnDisable()
    {
        editor.OnDisable();
    }

    private void OnGUI()
    {
        editor.OnGUI(position);
        
        if (GUI.changed)
        {
            Repaint();
        }
    }
}
#endif