using System;
using UnityEditor;
using UnityEngine;

public class Popup : PopupWindowContent
{
    public Action onGui;
    public Action onClose;
    public Vector2 position;
    public Vector2 size;

    public Popup()
    {
        size = new Vector2(200f, 200f);
    }
    
    public static Popup Draw(Rect position, Action onGui)
    {
        Popup popup = new Popup();
        popup.onGui = onGui;
        PopupWindow.Show(position, popup);
        return popup;
    }
    
    public static Popup Draw(Action onGui)
    {
        Popup popup = new Popup();
        popup.onGui = onGui;
        PopupWindow.Show(new Rect(Event.current.mousePosition, new Vector2(10, 10)), popup);
        return popup;
    }
    
    public Popup(Vector2 position, Vector2 size)
    {
        this.position = position;
        this.size = size;
    }

    public void OnGUIInArea()
    {
        GUILayout.BeginArea(new Rect(position, size));
        onGui();
        GUILayout.EndArea();
    }
    
    public void OnGUI()
    {
        onGui();
    }
    
    public override void OnGUI(Rect rect)
    {
        onGui();
    }
    
    public void DrawFoldout(string name, Action gui, bool show = false)
    {
        EditorUtils.DrawInBoxFoldout(new GUIContent(name), gui, name, show);
    }

    public bool DrawButton(string name)
    {
        return DrawButton(name, GUILayout.MaxWidth(200f));
    }
    
    public bool DrawButton(string name, params GUILayoutOption[] options)
    {
        if (GUILayout.Button(name, options))
        {
            editorWindow.Close();
            return true;
        }

        return false;
    }

    public override void OnClose()
    {
        base.OnClose();
        onClose?.Invoke();
    }

    public override Vector2 GetWindowSize()
    {
        return size;
    }

    public void Close()
    {
        if (editorWindow != null) editorWindow.Close();
    }

    public void Repaint() => editorWindow.Repaint();

    public void Show(Vector2 mousePos)
    {
        PopupWindow.Show(new Rect(mousePos, new Vector2(10, 10)), this);
    }
}