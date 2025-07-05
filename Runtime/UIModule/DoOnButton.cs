using System;
using LSCore;
using UnityEngine;

[Serializable]
public abstract class DoOnClick : DoIt
{
    public abstract IClickable Clickable { get; }

    [SerializeReference] public DoIt[] doIts;
        
    public override void Do()
    {
        Clickable.Submitted += doIts.Do;
    }
}

[Serializable]
public class DoOnButton : DoOnClick
{
    public LSButton button;
    public override IClickable Clickable => button;
}