using System;
using System.Reflection;
using UnityEngine;

public class GameViewExt : BaseWindowExtender
{
    public static event Action<Rect> PostGUI;
    public static Rect ViewRect { get; private set; }
    public static Vector2 ScreenSize { get; private set; }
    private static PropertyInfo viewRectMethod;
    private static MethodInfo screenSizeMethod;
    protected override Type GetWindowType()
    {
        var type = Type.GetType("UnityEditor.GameView,UnityEditor");
        viewRectMethod = type.GetProperty("targetInView", BindingFlags.NonPublic | BindingFlags.Instance);
        screenSizeMethod = type.GetMethod("GetSizeOfMainGameView", BindingFlags.NonPublic | BindingFlags.Static);
        return Type.GetType("UnityEditor.GameView,UnityEditor");
    }

    public override void OnPreGUI() { }

    public override void OnPostGUI()
    {
        ViewRect = (Rect)viewRectMethod.GetMethod.Invoke(window, null);
        ScreenSize = (Vector2)screenSizeMethod.Invoke(null, null);
        PostGUI?.Invoke(Rect);
    }
}