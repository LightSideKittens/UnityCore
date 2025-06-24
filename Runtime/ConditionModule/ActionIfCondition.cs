using System;
using LSCore.ConditionModule;
using UnityEngine;

[Serializable]
public abstract class ActionsIfCondition<TAction, TCondition> : DoIt where TAction : DoIt where TCondition : BaseCondition
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
public class ActionsIfCondition : ActionsIfCondition<DoIt, BaseCondition> { }