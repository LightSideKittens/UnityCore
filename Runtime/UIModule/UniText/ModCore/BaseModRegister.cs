using System;
using UnityEngine;

[Serializable]
public abstract class BaseModRegister
{
    public abstract IParseRule ParseRule { get; }
    public abstract void Register(AttributeParser parser);
}

[Serializable]
public abstract class BaseRendererModRegister: BaseModRegister
{
    public abstract IRenderModifier Modifier { get; }
    
    public override void Register(AttributeParser parser) => parser.Register(ParseRule, Modifier);
}

[Serializable]
public class RendererModRegister : BaseRendererModRegister
{
    [SerializeReference] public IRenderModifier modifier;
    [SerializeReference] public IParseRule rule;
    public override IParseRule ParseRule => rule;
    public override IRenderModifier Modifier => modifier;
}