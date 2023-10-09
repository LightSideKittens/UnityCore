using System;
using UnityEngine;

[Serializable]
public abstract class BasePrice
{
    public int value;
    public abstract void Earn();
    public abstract bool TrySpend();
}