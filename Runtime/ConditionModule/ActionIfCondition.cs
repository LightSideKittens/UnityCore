using System;
using LSCore.ConditionModule;
using UnityEngine;

[Serializable]
public abstract class ActionsIfCondition<TAction, TCondition> : LSAction where TAction : LSAction where TCondition : BaseCondition
{
    [SerializeReference] public TCondition conditions;
    [SerializeReference] public TAction[] ifActions;
    [SerializeReference] public TAction[] elseActions;

    public override void Invoke()
    {
        if (conditions)
        {
            ifActions?.Invoke();
        }
        else
        {
            elseActions?.Invoke();
        }
    }
}

[Serializable]
public class ActionsIfCondition : ActionsIfCondition<LSAction, BaseCondition> { }