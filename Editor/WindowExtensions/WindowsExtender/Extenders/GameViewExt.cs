using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public class GameViewExt : BaseWindowExtender
{
    public static event Action<Rect> PostGUI;
    public static Rect ViewRect { get; private set; }
    public static Vector2 ScreenSize { get; private set; }
    private static PropertyInfo viewRectMethod;
    private static MethodInfo screenSizeMethod;
    public static Type Type { get; private set; }
    
    protected override Type GetWindowType()
    {
        Type = Type.GetType("UnityEditor.GameView,UnityEditor");
        viewRectMethod = Type.GetProperty("targetInView", BindingFlags.NonPublic | BindingFlags.Instance);
        screenSizeMethod = Type.GetMethod("GetSizeOfMainGameView", BindingFlags.NonPublic | BindingFlags.Static);
        return Type;
    }

    public override void OnPreGUI() { }

    public override void OnPostGUI()
    {
        ViewRect = (Rect)viewRectMethod.GetMethod.Invoke(window, null);
        ScreenSize = (Vector2)screenSizeMethod.Invoke(null, null);
        PostGUI?.Invoke(Rect);
    }

    public static void Repaint()
    {
        EditorWindow.GetWindow(Type, false, null, false).Repaint();
    }
}