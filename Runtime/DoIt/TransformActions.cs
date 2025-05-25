using System;
using UnityEngine;

[Serializable]
public class SetParentAction : DoIt
{
    [SerializeReference] public Get<Transform> parent;
    [SerializeReference] public Get<Transform> child;
    public bool worldPositionStays = true;
    
    public override void Invoke()
    {
        ((Transform)child).SetParent(parent, worldPositionStays);
    }
}