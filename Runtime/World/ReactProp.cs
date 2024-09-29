using System;
using Newtonsoft.Json;

[JsonConverter(typeof(ReactPropConverter<>))]
public class ReactProp<T> : IDisposable
{
    public void SubOnChangedAndCall(Action<T> action)
    {
        Changed += action;
        action(value);
    }
    
    public event Action<T> Changed;
    protected T value;

    public T Value
    {
        get => value;
        set
        {
            if (!this.value?.Equals(value) ?? value != null)
            {
                this.value = value;
                Changed?.Invoke(value);
            }
        }
    }

    public static explicit operator ReactProp<T>(T value) => new() { Value = value };
    public static explicit operator T(ReactProp<T> prop) => prop.Value;
    public override string ToString() => Value.ToString();

    public void Dispose()
    {
        Changed = null;
    }
}


public class IntReact : ReactProp<int>
{
    public static explicit operator IntReact(in int value) => new() {value = value};
    public static explicit operator int(in IntReact value) => value.value;

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
    public static explicit operator FloatReact(in float value) => new() {value = value};
    public static explicit operator float(in FloatReact value) => value.value;

    public static FloatReact operator +(FloatReact a, in float b)
    {
        a.Value += b;
        return a;
    }
    
    public static float operator +(in float a, FloatReact b) => a + b.value;
}

public class ReactPropConverter<T> : JsonConverter<ReactProp<T>>
{
    public override void WriteJson(JsonWriter writer, ReactProp<T> value, JsonSerializer serializer)
    {
        serializer.Serialize(writer, value.Value);
    }

    public override ReactProp<T> ReadJson(JsonReader reader, Type objectType, ReactProp<T> existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        T value = serializer.Deserialize<T>(reader);
        return new ReactProp<T> { Value = value };
    }
}
