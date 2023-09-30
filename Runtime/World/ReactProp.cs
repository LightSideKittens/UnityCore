using System;

public class ReactProp<T> : IDisposable
{
    public event Action<T> Changed;
    protected T value;

    public T Value
    {
        get => value;
        set
        {
            if (!this.value.Equals(value))
            {
                this.value = value;
                Changed?.Invoke(value);
            }
        }
    }
    
    public static implicit operator ReactProp<T>(T value) => new() {value = value};
    public static implicit operator T(ReactProp<T> prop) => prop.value;
    public override string ToString() => value.ToString();

    public void Dispose()
    {
        Changed = null;
    }
}

public class IntReact : ReactProp<int>
{
    public static implicit operator IntReact(in int value) => new() {value = value};
    public static implicit operator int(in IntReact value) => value.value;

    public void pp() => Value++;
    public void mm() => Value--;

    public static IntReact operator +(IntReact a, in int b)
    {
        a.Value += b;
        return a;
    }
    
    public static int operator +(in int a, IntReact b) => a + b.value;
}


public class FloatReact : ReactProp<float>
{
    public static implicit operator FloatReact(in float value) => new() {value = value};
    public static implicit operator float(in FloatReact value) => value.value;

    public static FloatReact operator +(FloatReact a, in float b)
    {
        a.Value += b;
        return a;
    }
    
    public static float operator +(in float a, FloatReact b) => a + b.value;
}
