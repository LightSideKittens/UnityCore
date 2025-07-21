using System;
using LSCore.Attributes;
using Sirenix.OdinInspector;

[Serializable]
public abstract class BaseToggleData
{
    public bool? lastIsOn;
    
    public bool IsOn
    {
        get
        {
            lastIsOn = Get;
            return lastIsOn.Value;
        }
        set
        {
            if (lastIsOn == value) return;
            lastIsOn = value;
            Set = value;
            valueChanged?.Invoke(value);
        }
    }

    protected abstract bool Get { get; }
    protected abstract bool Set { set; }

    public Action<bool> valueChanged;
        
    public static implicit operator bool(BaseToggleData data) => data.IsOn;
}

[Serializable]
[HideReferenceObjectPicker]
[Unwrap]
public class DefaultToggleData : BaseToggleData
{
    public bool isOn;
    protected override bool Get => isOn;

    protected override bool Set
    {
        set => isOn = value;
    }
}