#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

public class BadassAnimationCurveWindow : EditorWindow
{
    public BadassMultiCurveEditor editor;
    public event Action Edited {add => editor.First.Edited += value; remove => editor.First.Edited -= value; }
    
    public static BadassAnimationCurveWindow ShowWindow(BadassCurve curve)
    {
        var window = GetWindow<BadassAnimationCurveWindow>();
        window.editor = new BadassMultiCurveEditor(new BadassCurveEditor(curve, window));
        window.Edited += window.Repaint;
        return window;
    }
    
    public static BadassAnimationCurveWindow CreateAndShowWindow(BadassCurve curve)
    {
        var window = CreateWindow<BadassAnimationCurveWindow>();
        window.editor = new BadassMultiCurveEditor(new BadassCurveEditor(curve, window));
        window.Edited += window.Repaint;
        return window;
    }

    private void OnEnable()
    {
        if (editor != null)
        {
            Edited -= Repaint;
            Edited += Repaint;
            editor.OnEnable();
        }
    }

    private void OnDisable()
    {
        Edited -= Repaint;
        editor.OnDisable();
    }

    private void OnGUI()
    {
        var rect = position;
        rect.position = Vector2.zero;
        editor.OnGUI(rect);
        
        if (GUI.changed)
        {
            Repaint();
        }
    }
}
#endif