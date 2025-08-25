using System;
using LSCore.ConditionModule;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public abstract class DoIf<TAction, TCondition> : DoIt where TAction : DoIt where TCondition : BaseIf
{
    [SerializeReference] public TCondition conditions;
    [LabelText("If")] [SerializeReference] public TAction[] ifActions;
    [LabelText("Else")] [SerializeReference] public TAction[] elseActions;

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