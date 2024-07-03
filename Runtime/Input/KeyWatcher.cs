#if UNITY_EDITOR
using WindowsInput;
using WindowsInput.Native;

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