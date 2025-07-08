using System;
using LSCore;
using UnityEngine;

[Serializable]
public abstract class DoOnClick : DoIt
{
    public abstract ISubmittable Submittable { get; }

    [SerializeReference] public DoIt[] doIts;
        
    public override void Do()
    {
        Submittable.Submitted += doIts.Do;
    }
}

[Serializable]
public class DoOnButton : DoOnClick
{
    public LSButton button;
    public override ISubmittable Submittable => button.submittable;
}