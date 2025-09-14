using System;
using UnityEngine;

[Serializable]
public class SetActive : DoIt
{
    public bool active;
    [SerializeReference] public Get<GameObject> go;
    
    public override void Do()
    {
        go.Data.SetActive(active);
    }
}