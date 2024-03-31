using LSCore.Editor;
using UnityEngine;
using UnityEditor;

public class CanvasWindow : EditorWindow
{
    public LSHandles.CameraData camData = new();
    public LSHandles.GridData gridData = new();
    
    [MenuItem("Window/Custom Editor Window")]
    public static void ShowWindow()
    {
        GetWindow<CanvasWindow>("Custom Workspace");
    }

    private void OnGUI()
    {
        var rect = position;
        rect.position = Vector2.zero;
        LSHandles.Begin(rect, camData);

        for (int i = 0; i < 100; i++)
        {
            LSHandles.DrawBezier(Vector2.one * i, new Vector2(0.5f + i, 0f + i), new Vector2(0.5f + i, 1f + i),
                Vector2.one * (i + 1), Color.yellow, null, (i + 1) * 0.01f);
        }

        LSHandles.DrawGrid(gridData);
        LSHandles.End();
    }
}