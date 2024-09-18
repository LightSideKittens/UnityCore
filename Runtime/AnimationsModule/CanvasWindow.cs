#if UNITY_EDITOR
using LSCore.Editor;
using UnityEngine;
using UnityEditor;

public class CanvasWindow : EditorWindow
{
    private Vector2 startTangent = new Vector2(0.5f, 0f);
    private Vector2 endTangent = new Vector2(0.5f, 1f);
    
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

        LSHandles.DrawBezier(Vector2.zero, startTangent, endTangent,
            Vector2.one, Color.yellow, null, (1) * 0.01f);

        LSHandles.DrawGrid(gridData);
        LSHandles.End();

        startTangent = EditorGUILayout.Vector2Field("Start Tangent", startTangent);
        endTangent = EditorGUILayout.Vector2Field("End Tangent", endTangent);
    }
}


#endif