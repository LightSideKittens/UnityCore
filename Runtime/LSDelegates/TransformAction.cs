using System;
using LSCore.Extensions.Unity;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public abstract class TransformAction : LSAction { }

[Serializable]
public class SetParentAction : LSAction
{
    public bool usePath = true;
        
    [LabelText("$LabelText")]
    public Transform root;
        
    [ShowIf("usePath")] 
    public string pathToObject;
    public bool worldPositionStays = true;
        
    private string LabelText => usePath ? "Root" : "Parent";
        
    public override void Invoke()
    {
        Transform value = DataBuffer<Transform>.value;
        var parent = root;
        if (!string.IsNullOrEmpty(pathToObject) && usePath)
        {
            parent = root.FindComponent<Transform>(pathToObject);
        }
        value.SetParent(parent, worldPositionStays);
    }
}