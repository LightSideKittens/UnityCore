using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

public readonly struct TypedPathAccessor<TValue>
{
    public readonly Func<object, TValue>   Get;
    public readonly Action<object, TValue> Set;

    internal TypedPathAccessor(
        Func<object, TValue> get,
        Action<object, TValue> set)
    {
        Get = get;
        Set = set;
    }
}


public readonly struct ObjectPathAccessor
{
    public readonly Func<object, object?> Get;
    public readonly Action<object, object?> Set;
    public readonly Type ValueType;

    internal ObjectPathAccessor(
        Func<object, object?> get,
        Action<object, object?> set,
        Type valueType)
    {
        Get = get;
        Set = set;
        ValueType = valueType;
    }
}

public readonly struct EnumPathAccessor
{
    public readonly Func<object, object> GetRaw;
    public readonly Action<object, object> SetRaw;
    public readonly Type EnumType;

    internal EnumPathAccessor(
        Func<object, object> get,
        Action<object, object> set,
        Type enumType)
    {
        GetRaw   = get;
        SetRaw   = set;
        EnumType = enumType;
    }

    public int GetInt(object root)  => Convert.ToInt32(GetRaw(root));
    public void SetInt(object root, int value)
        => SetRaw(root, Enum.ToObject(EnumType, value));
}


public static class PathAccessorCache
{
    public static TypedPathAccessor<TValue> Get<TValue>(Type rootType, string path)
        => Cache<TValue>.Get(rootType, path);

    public static ObjectPathAccessor GetRef(Type rootType, string path)
        => RefCache.Get(rootType, path);

    public static EnumPathAccessor GetEnum(Type rootType, string path)
        => EnumCache.Get(rootType, path);

    private static class EnumCache
    {
        private static readonly Dictionary<(Type, string), EnumPathAccessor> dic = new();

        public static EnumPathAccessor Get(Type rootType, string path)
        {
            if (dic.TryGetValue((rootType, path), out var acc))
                return acc;

            acc = Build(rootType, path);
            dic[(rootType, path)] = acc;
            return acc;
        }

        private static EnumPathAccessor Build(Type rootType, string path)
        {
            var (expr, valueType, rootPar) = BuildExpression(rootType, path);

            if (!valueType.IsEnum)
                throw new InvalidOperationException(
                    $"Path '{path}' в '{rootType.Name}' ведёт к '{valueType.Name}', " +
                    $"который не является enum. Для value/ref‑типов используйте " +
                    $"Get<TValue>() или GetRef().");

            Type baseType = Enum.GetUnderlyingType(valueType);

            var getter = Expression.Lambda<Func<object, object>>(
                    Expression.Convert(expr, typeof(object)), rootPar)
                .Compile();

            var rootP = rootPar;
            var valObj = Expression.Parameter(typeof(object), "val");

            var assign = Expression.Assign(
                expr,
                Expression.Convert(
                    Expression.Convert(valObj, baseType),
                    valueType));

            var setter = Expression.Lambda<Action<object, object>>(
                assign, rootP, valObj).Compile();

            return new EnumPathAccessor(getter, setter, baseType);
        }
    }

    private static class Cache<TValue>
    {
        private static readonly Dictionary<(Type, string), TypedPathAccessor<TValue>> dic = new();

        public static TypedPathAccessor<TValue> Get(Type rootType, string path)
        {
            if (dic.TryGetValue((rootType, path), out var acc))
                return acc;

            acc = Build(rootType, path);
            dic[(rootType, path)] = acc;
            return acc;
        }

        private static TypedPathAccessor<TValue> Build(Type rootType, string path)
        {
            var (expr, valueType, rootPar) = BuildExpression(rootType, path);

            if (!typeof(TValue).IsAssignableFrom(valueType) &&
                !valueType.IsAssignableFrom(typeof(TValue)))
            {
                throw new InvalidOperationException(
                    $"Path '{path}' в '{rootType.Name}' даёт тип '{valueType.Name}', " +
                    $"не совместимый с '{typeof(TValue).Name}'.");
            }

            var getter = Expression.Lambda<Func<object, TValue>>(
                Expression.Convert(expr, typeof(TValue)),
                rootPar).Compile();

            var valPar = Expression.Parameter(typeof(TValue), "val");
            var assign = Expression.Assign(
                expr, 
                Expression.Convert(valPar, valueType));

            var setter = Expression.Lambda<Action<object, TValue>>(
                assign, rootPar, valPar).Compile();

            return new TypedPathAccessor<TValue>(getter, setter);
        }
    }

    private static class RefCache
    {
        private static readonly Dictionary<(Type, string), ObjectPathAccessor> dic = new();

        public static ObjectPathAccessor Get(Type rootType, string path)
        {
            if (dic.TryGetValue((rootType, path), out var acc))
                return acc;

            acc = Build(rootType, path);
            dic[(rootType, path)] = acc;
            return acc;
        }

        private static ObjectPathAccessor Build(Type rootType, string path)
        {
            var (expr, valueType, rootPar) = BuildExpression(rootType, path);

            if (!typeof(object).IsAssignableFrom(valueType))
                throw new InvalidOperationException(
                    $"Path '{path}' в '{rootType.Name}' приводит к нессылочному типу '{valueType.Name}'. " +
                    $"Для значимых/произвольных типов используйте generic‑метод Get<TValue>().");

            Func<object, object> getter = null;
            Action<object, object> setter = null;
            
            try
            {
                getter = Expression.Lambda<Func<object, object?>>(
                    Expression.TypeAs(expr, typeof(object)),
                    rootPar).Compile();

                var valPar = Expression.Parameter(typeof(object), "val");
                var assign = Expression.Assign(
                    expr,
                    Expression.TypeAs(valPar, valueType));

                setter = Expression.Lambda<Action<object, object?>>(
                    assign, rootPar, valPar).Compile();
            }
            catch { }
            
            return new ObjectPathAccessor(getter, setter, valueType);
        }
    }

    private static (Expression expr, Type valueType, ParameterExpression rootPar) BuildExpression(Type rootType,
        string path)
    {
        var tokens = Tokenize(path);
        var rootPar = Expression.Parameter(typeof(object), "root");

        Expression cur = Expression.Convert(rootPar, rootType);
        Type type = rootType;

        foreach (var t in tokens)
        {
            switch (t)
            {
                case MemberToken m:
                    var member = (MemberInfo?)type.GetField(m.name, BF)
                                 ?? type.GetProperty(m.name, BF);
                    cur = member switch
                    {
                        FieldInfo fi => Expression.Field(cur, fi),
                        PropertyInfo pi => Expression.Property(cur, pi),
                        _ => throw new MissingMemberException(type.Name, m.name)
                    };
                    type = member switch
                    {
                        FieldInfo fi => fi.FieldType,
                        PropertyInfo pi => pi.PropertyType,
                        _ => type
                    };
                    break;

                case IndexToken idx:
                    var idxC = Expression.Constant(idx.index);
                    if (type.IsArray)
                    {
                        cur = Expression.ArrayAccess(cur, idxC);
                        type = type.GetElementType()!;
                    }
                    else
                    {
                        var itemProp = type.GetProperty(
                                           "Item", BF, null, null, new[] { typeof(int) }, null)
                                       ?? throw new MissingMemberException(type.Name, "Item[int]");

                        cur = Expression.MakeIndex(cur, itemProp, new[] { idxC });
                        type = itemProp.PropertyType;
                    }

                    break;
            }
        }

        return (cur, type, rootPar);
    }

    private abstract record Token;

    private record MemberToken : Token
    {
        public MemberToken(string Name)
        {
            name = Name;
        }

        public string name;
    }

    private record IndexToken : Token
    {
        public IndexToken(int Index)
        {
            this.index = Index;
        }

        public int index;
    }

    private static IEnumerable<Token> Tokenize(string path)
    {
        int i = 0;
        while (i < path.Length)
        {
            if (path[i] == '.')
            {
                i++;
                continue;
            }

            if (path[i] == '[')
            {
                int close = path.IndexOf(']', i);
                yield return new IndexToken(
                    int.Parse(path.Substring(i + 1, close - i - 1)));
                i = close + 1;
                continue;
            }

            int j = i;
            while (j < path.Length && path[j] != '.' && path[j] != '[') j++;
            yield return new MemberToken(path.Substring(i, j - i));
            i = j;
        }
    }

    private const BindingFlags BF =
        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
}

