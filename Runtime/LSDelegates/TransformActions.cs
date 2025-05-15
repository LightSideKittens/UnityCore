using System;
using UnityEngine;

[Serializable]
public class SetParentAction : DoIt
{
    [SerializeReference] public DataProvider<Transform> parent;
    [SerializeReference] public DataProvider<Transform> child;
    public bool worldPositionStays = true;
    
    public override void Invoke()
    {
        ((Transform)child).SetParent(parent, worldPositionStays);
    }
}