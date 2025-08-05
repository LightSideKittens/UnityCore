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
            if (lastIsOn == null) Init();
            lastIsOn = Get;
            return lastIsOn.Value;
        }
        set
        {
            if (lastIsOn == value) return;
            Set = value;
            lastIsOn = Get;
            if (lastIsOn == value)
            { 
                valueChanged?.Invoke(value);
            }
        }
    }

    protected abstract bool Get { get; }
    protected abstract bool Set { set; }

    public Action<bool> valueChanged;
    
    public static implicit operator bool(BaseToggleData data) => data.IsOn;
    protected virtual void Init() {}
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