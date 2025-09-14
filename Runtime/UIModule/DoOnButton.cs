using System;
using LSCore;
using UnityEngine;

[Serializable]
public abstract class DoOnClick : DoIt
{
    public abstract IUIControl UIControl { get; }

    [SerializeReference] public DoIt[] doIts;
        
    public override void Do()
    {
        UIControl.Activated += doIts.Do;
    }
}

[Serializable]
public class DoOnButton : DoOnClick
{
    public LSButton button;
    public override IUIControl UIControl => button.uiControl;
}