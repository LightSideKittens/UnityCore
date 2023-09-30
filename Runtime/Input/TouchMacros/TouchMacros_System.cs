using System.Runtime.InteropServices;
using UnityEngine;

public partial class TouchMacros
{
    [DllImport("user32.dll")]
    private static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);
    
    // Flags for mouse events
    private const int MOUSEEVENTF_LEFTDOWN = 0x02;
    private const int MOUSEEVENTF_LEFTUP = 0x04;

    [DllImport("user32.dll")]
    private static extern bool SetCursorPos(int x, int y);

    private static bool SetCursorPos(Vector2 pos) => SetCursorPos((int)pos.x, (int)pos.y);

    private static void ProceedOneInput(Vector2 pos, int id)
    {
        SetCursorPos(pos);
        mouse_event(id, 0, 0, 0, 0);
    }

    public static void Down(Vector2 pos) => ProceedOneInput(pos, MOUSEEVENTF_LEFTDOWN);
    public static void Up(Vector2 pos) => ProceedOneInput(pos, MOUSEEVENTF_LEFTUP);
    
    public static void Click(Vector2 pos)
    {
        SetCursorPos(pos);
        mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
        mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
    }
}