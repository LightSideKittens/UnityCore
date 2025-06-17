using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class Commit
{
    [SerializeReference] public List<BaseChange> changes;
    [NonSerialized] private string branch;
    [NonSerialized] private int index;

    public string Branch
    {
        get => branch;
        set
        {
            branch = value;
            for (int i = 0; i < changes.Count; i++)
            {
                changes[i].Branch = value;
            }
        }
    }

    public int Index
    {
        get => index;
        set
        {
            index = value;
            for (int i = 0; i < changes.Count; i++)
            {
                changes[i].Commit = value;
                changes[i].Index = i;
            }
        }
    }

    public void Do()
    {
        Do(changes.Select(x => (Action<Action>)x.Do));
    }
    
    public static void Do(IEnumerable<Action<Action>> actions)
    {
        Action continuation = () => { };
        foreach (var action in actions.Reverse())
        {
            var next = continuation;
            continuation = () => action(next);
        }
        continuation();
    }

    public void Undo()
    {
        Do(changes.Select(x => (Action<Action>)x.Undo));
    }

    public void Load()
    {
        foreach (var change in changes)
        {
            change.Branch = branch;
            change.Index = index;
            change.Preload();
        }
    }

    public void Unload()
    {
        foreach (var change in changes)
        {
            change.Unload();
        }
    }
}