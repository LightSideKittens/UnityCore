#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

public class MoveItCurveWindow : EditorWindow
{
    public MoveItMultiCurveEditor editor;
    public event Action Edited {add => editor.Edited += value; remove => editor.Edited -= value; }
    
    public static MoveItCurveWindow ShowWindow(MoveItCurve curve)
    {
        var window = GetWindow<MoveItCurveWindow>();
        window.editor = new MoveItMultiCurveEditor(new MoveItCurveEditor(curve, window));
        window.Edited += window.Repaint;
        return window;
    }
    
    public static MoveItCurveWindow CreateAndShowWindow(MoveItCurve curve)
    {
        var window = CreateWindow<MoveItCurveWindow>();
        window.editor = new MoveItMultiCurveEditor(new MoveItCurveEditor(curve, window));
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
        editor.OnGUI(rect, true);
        
        if (GUI.changed)
        {
            Repaint();
        }
    }
}
#endif