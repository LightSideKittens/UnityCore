using System;
using LSCore;
using LSCore.Attributes;
using UnityEngine;

[Serializable]
public abstract class LSClickAction : DoIt
{
    public abstract ISubmittable Submittable { get; }

    [SerializeReference] public DoIt action;
        
    public override void Do()
    {
        Submittable.Submitted += action.Do;
    }
}

[Serializable]
public class LSButtonAction : LSClickAction
{
    public LSButton button;
    public override ISubmittable Submittable => button;
}