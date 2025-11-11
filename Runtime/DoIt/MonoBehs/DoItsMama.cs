using System;
using System.Collections.Generic;
using LSCore.Attributes;
using Sirenix.OdinInspector;
using UnityEngine;
using Object = UnityEngine.Object;

[ExecuteAlways]
public class DoItsMama : MonoBehaviour
{
    [Serializable]
    public class Call : DoIt
    {
        public string[] names;
        
        public override void Do()
        {
            for (int i = 0; i < names.Length; i++)
            { 
                childrenDict[names[i]].Do();
            }
        }
    }
    
    [Serializable]
    [Unwrap]
    public struct Child
    {
        [BoxGroup] public string name;
        [SerializeReference] public DoIt doIt;
    }
    
    public Child[] children;
    public static Dictionary<string, DoIt> childrenDict = new();
    
    private void OnEnable()
    {
        for (int i = 0; i < children.Length; i++)
        {
            childrenDict[children[i].name] = children[i].doIt;
        }
    }

    private void OnDisable()
    {
        for (int i = 0; i < children.Length; i++)
        {
            childrenDict.Remove(children[i].name);
        }
    }
    
#if UNITY_EDITOR
    private List<string> names = new();
    private void OnValidate()
    {
        for (int i = 0; i < names.Count; i++)
        {
            childrenDict.Remove(names[i]);
        }
        names.Clear();
        for (int i = 0; i < children.Length; i++)
        {
            names.Add(children[i].name);
            childrenDict[children[i].name] = children[i].doIt;
        }
    }
#endif
}