/*using System;
using UnityEditor;

public class AnimationWindowExt : BaseWindowExtender
{
    public static event Action PostGUI;
    protected override Type GetWindowType()
    {
        var type = typeof(AnimationWindow);
        return type;
    }

    public override void OnPreGUI() { }

    public override void OnPostGUI()
    {
        PostGUI?.Invoke();
    }
}*/