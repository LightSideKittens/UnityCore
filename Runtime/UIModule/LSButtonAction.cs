using System;
using LSCore;
using UnityEngine;

[Serializable]
public class LSButtonAction : LSAction
{
    public LSButton button;

    [SerializeReference] public LSAction action;
        
    public override void Invoke()
    {
        button.Clicked += action.Invoke;
    }
}