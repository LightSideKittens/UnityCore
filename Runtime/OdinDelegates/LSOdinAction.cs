using System;

[Serializable]
public abstract class LSAction
{
    public abstract void Action();
}

[Serializable]
public abstract class LSAction<T>
{
    public abstract void Action(T value);
}

[Serializable]
public abstract class LSFunc<TReturn>
{
    public abstract TReturn Action();
}

[Serializable]
public abstract class LSFunc<TReturn, TArg>
{
    public abstract TReturn Action(TArg arg);
}
