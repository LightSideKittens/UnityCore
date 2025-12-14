using System;
using UnityEngine;

[Serializable]
public class ModRegister
{
    [SerializeReference] public BaseModifier modifier;
    [SerializeReference] public IParseRule rule;
    public bool IsValid => modifier != null && rule != null;

    public void Register(AttributeParser parser) => parser.Register(rule, modifier);
}