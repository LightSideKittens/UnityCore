using System;
using Newtonsoft.Json;

[JsonConverter(typeof(ReactPropConverter))]
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

public class ReactPropConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        // Проверяем, что это тип ReactProp<>
        return objectType.IsGenericType && objectType.GetGenericTypeDefinition() == typeof(ReactProp<>);
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        var prop = value.GetType().GetProperty("Value").GetValue(value);
        serializer.Serialize(writer, prop);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        var valueType = objectType.GetGenericArguments()[0];
        var deserializedValue = serializer.Deserialize(reader, valueType);
        var propInstance = Activator.CreateInstance(objectType);
        objectType.GetProperty("Value").SetValue(propInstance, deserializedValue);
        return propInstance;
    }
}