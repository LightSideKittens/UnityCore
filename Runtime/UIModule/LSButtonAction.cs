using System;
using LSCore;
using LSCore.Attributes;
using UnityEngine;

[Serializable]
public abstract class LSClickAction : DoIt
{
    public abstract IClickable Clickable { get; }

    [SerializeReference] public DoIt action;
        
    public override void Invoke()
    {
        Clickable.Clicked += action.Invoke;
    }
}

[Serializable]
public class LSButtonAction : LSClickAction
{
    public LSButton button;
    public override IClickable Clickable => button;
}