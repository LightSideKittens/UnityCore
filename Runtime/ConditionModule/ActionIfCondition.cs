using System;
using LSCore.ConditionModule;
using UnityEngine;

[Serializable]
public abstract class DoIf<TAction, TCondition> : DoIt where TAction : DoIt where TCondition : BaseIf
{
    [SerializeReference] public TCondition conditions;
    [SerializeReference] public TAction[] ifActions;
    [SerializeReference] public TAction[] elseActions;

    public override void Do()
    {
        if (conditions)
        {
            ifActions?.Do();
        }
        else
        {
            elseActions?.Do();
        }
    }
}

[Serializable]
public class DoIf : DoIf<DoIt, BaseIf> { }