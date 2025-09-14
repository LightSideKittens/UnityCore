using System;
using LSCore.Async;
using UnityEngine;

[Serializable]
public class Delay : DoIt
{
    public float delay;
    [SerializeReference] public DoIt doIt;
    
    public override void Do()
    {
        Wait.Delay(delay, doIt.Do);
    }
}