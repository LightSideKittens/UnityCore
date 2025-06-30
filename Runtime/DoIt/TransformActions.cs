using System;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class SetParentAction : DoIt
{
    [SerializeReference] public Get<Transform> parent;
    [SerializeReference] public Get<Transform> child;
    public bool worldPositionStays = true;
    
    public override void Do()
    {
        ((Transform)child).SetParent(parent, worldPositionStays);
    }
}

[Serializable]
public class GetSiblingIndex
{
#if UNITY_EDITOR
    public bool useTransform;
#endif
    
    [ShowIf("useTransform")]
    [SerializeReference] public Get<Transform> target;
    [HideIf("useTransform")] public int index;
    public int offset;
    
    public int Index => target != null ? ((Transform)target).GetSiblingIndex() + offset : index;
    
    public static implicit operator int(GetSiblingIndex self) => self.Index;
}

[Serializable]
public class SetSiblingIndex : DoIt
{
    [SerializeReference] public Get<Transform> target;
    public GetSiblingIndex index;
    
    public override void Do()
    {
        ((Transform)target).SetSiblingIndex(index);
    }
}