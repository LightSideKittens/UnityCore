using System;
using UnityEngine;
using UnityEditor;

public class CanvasWindow : EditorWindow
{
    private GridGUI grid;
    private Vector2 scroll;
    private Rect windowRect = new Rect(Vector2.zero, new Vector2(800, 600));

    [MenuItem("Window/Custom Editor Window")]
    public static void ShowWindow()
    {
        GetWindow<CanvasWindow>("Custom Workspace");
    }

    private void Awake()
    {
        grid = new GridGUI();
    }

    private void OnGUI()
    {
        grid.OnGUI(new Rect(50, 50, 800, 800));

        if (grid.NeedRepaint)
        {
            Repaint();
        }
    }

    /*void WindowFunction(int windowID)
    {
        var area = windowRect;
        area.position = Vector2.zero;
        grid.OnGUI(area);

        if (grid.NeedRepaint)
        {
            Repaint();
        }
        
        /#1#/ Делает окно перемещаемым
        GUI.DragWindow();#1#
    }*/
}
