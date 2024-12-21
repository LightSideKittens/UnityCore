#if UNITY_EDITOR
using System.Runtime.InteropServices;
using UnityEngine;
using WindowsInput;
using WindowsInput.Native;

public static class CursorPoint
{
    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public int X;
        public int Y;
    }

    [DllImport("user32.dll")]
    public static extern bool GetCursorPos(out POINT point);

    public static Vector2 Position
    {
        get
        {
            if (GetCursorPos(out POINT point))
            {
                return new Vector2(point.X, Screen.currentResolution.height - point.Y);
            }
            return Vector2.zero;
        }
    }
}

public static class KeyWatcher
{
    private static readonly InputSimulator input = new ();
    private static bool[] states = new bool[256];
    
    public static bool IsKeyPressed(LSKeyCode key)
    {
        if (!states[(int)key] && input.InputDeviceState.IsKeyDown((VirtualKeyCode)key))
        {
            states[(int)key] = true;
            return true;
        }

        return false;
    }

    public static bool IsKeyHeld(LSKeyCode key)
    {
        return input.InputDeviceState.IsKeyDown((VirtualKeyCode)key);
    }

    public static bool IsKeyReleased(LSKeyCode key)
    {
        if (states[(int)key] && input.InputDeviceState.IsKeyUp((VirtualKeyCode)key))
        {
            states[(int)key] = false;
            return true;
        }

        return false;
    }
}
#endif