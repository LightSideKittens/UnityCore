using System;
using UnityEngine;

[Serializable]
public abstract class BaseWallet
{
    public int value;
    public abstract void Earn();
    public abstract bool TrySpend();
}