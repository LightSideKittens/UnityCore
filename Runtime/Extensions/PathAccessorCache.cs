using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

public class TypedPathAccessor<TValue>
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

public class ObjectPathAccessor
{
    public readonly Func<object, object?> Get;
    public readonly Action<object, object?> Set;

    internal ObjectPathAccessor(
        Func<object, object?> get,
        Action<object, object?> set)
    {
        Get = get;
        Set = set;
    }
}

public static class PathAccessorCache
{
    public static TypedPathAccessor<TValue> Get<TValue>(object rootObj, string path)
        => Cache<TValue>.Get(rootObj, path);

    public static ObjectPathAccessor GetRef(object rootObj, string path)
        => RefCache.Get(rootObj, path);


    private static class Cache<TValue>
    {
        private static readonly Dictionary<(Type, string), TypedPathAccessor<TValue>> dic = new();

        public static TypedPathAccessor<TValue> Get(object rootObj, string path)
        {
            var rootType = rootObj.GetType();
            if (dic.TryGetValue((rootType, path), out var acc))
                return acc;

            acc = Build(rootObj, path);
            dic[(rootType, path)] = acc;
            return acc;
        }

        private static TypedPathAccessor<TValue> Build(object rootObj, string path)
        {
            var (expr, valueType, rootPar, canRead, canWrite) = BuildExpression(rootObj, path);

            Func<object, TValue> getter = null;

            if (canRead)
            {
                getter = Expression.Lambda<Func<object, TValue>>(
                    Expression.Convert(expr, typeof(TValue)),
                    rootPar).Compile();
            }
            
            Action<object, TValue> setter = null;

            if (canWrite)
            {
                var valPar = Expression.Parameter(typeof(TValue), "val");
                var assign = Expression.Assign(
                    expr,
                    Expression.Convert(valPar, valueType));

                setter = Expression.Lambda<Action<object, TValue>>(
                    assign, rootPar, valPar).Compile();

            }
            
            return new TypedPathAccessor<TValue>(getter, setter);
        }
    }

    private static class RefCache
    {
        private static readonly Dictionary<(Type, string), ObjectPathAccessor> dic = new();

        public static ObjectPathAccessor Get(object rootObj, string path)
        {
            var rootType = rootObj.GetType();
            if (dic.TryGetValue((rootType, path), out var acc))
                return acc;

            acc = Build(rootObj, path);
            dic[(rootType, path)] = acc;
            return acc;
        }

        private static ObjectPathAccessor Build(object rootObj, string path)
        {
            var (expr, valueType, rootPar, canRead, canWrite) = BuildExpression(rootObj, path);
            
            Func<object, object> getter = null;
            
            if (canRead)
            {
                getter = Expression.Lambda<Func<object, object>>(
                    Expression.TypeAs(expr, typeof(object)),
                    rootPar).Compile();
            }

            Action<object, object> setter = null;

            if (canWrite)
            {
                var valPar = Expression.Parameter(typeof(object), "val");
                var assign  = Expression.Assign(
                    expr,
                    Expression.Convert(valPar, valueType));
                setter = Expression.Lambda<Action<object, object>>(assign, rootPar, valPar).Compile();
            }
            
            return new ObjectPathAccessor(getter, setter);
        }
    }

    private static (Expression expr, Type valueType, ParameterExpression rootPar, bool canRead, bool canWrite) BuildExpression(object rootInstance, string path)
    {
        ReadOnlySpan<char> input = path.AsSpan();
        var tokens = ArrayPool<Token>.Shared.Rent(32);
        var length = Tokenize(input, tokens);
        var rootPar = Expression.Parameter(typeof(object), "root");
        Expression curExpr = Expression.Convert(rootPar, rootInstance.GetType());

        object curObj = rootInstance;
        Type curTyp = curObj.GetType();
        
        for (int i = 0; i < length; i++)
        {
            var t = tokens[i];

            if (!t.IsIndex)
            {
                var name = t.GetName(input);
                var member =
                    (MemberInfo)curTyp.GetField(name, BF) ??
                    (MemberInfo)curTyp.GetProperty(name, BF)
                    ?? throw new MissingMemberException(curTyp.Name, name);

                Expression memberExpr = Expression.PropertyOrField(curExpr, name);

                curObj = member switch
                {
                    FieldInfo fi1 => fi1.GetValue(curObj),
                    PropertyInfo pi1 => pi1.GetValue(curObj),
                    _ => null
                };

                curTyp = curObj?.GetType() ??
                         (member is FieldInfo fi ? fi.FieldType : ((PropertyInfo)member).PropertyType);
                curExpr = i == length - 1 ? memberExpr : Expression.Convert(memberExpr, curTyp);
            }
            else
            {
                var index = t.Index;
                var idxC = Expression.Constant(index);

                Expression itemExpr;
                object itemObj;
                Type itemTyp;

                if (curTyp.IsArray)
                {
                    itemExpr = Expression.ArrayAccess(curExpr, idxC);
                    itemObj = ((Array)curObj)?.GetValue(index);
                    itemTyp = itemObj?.GetType() ?? curTyp.GetElementType()!;
                }
                else
                {
                    var itemProp = curTyp.GetProperty(
                                       "Item", BF, null, null,
                                       new[] { typeof(int) }, null)
                                   ?? throw new MissingMemberException(curTyp.Name, "Item[int]");

                    itemExpr = Expression.MakeIndex(curExpr, itemProp, new[] { idxC });
                    itemObj = itemProp.GetValue(curObj, new object[] { index });
                    itemTyp = itemObj?.GetType() ?? itemProp.PropertyType;
                }

                curObj = itemObj;
                curTyp = itemTyp;
                curExpr = i == length - 1 ? itemExpr : Expression.Convert(itemExpr, itemTyp);
            }
        }

        bool canRead  = false;
        bool canWrite = false;

        switch (curExpr)
        {
            case MemberExpression me:
                switch (me.Member)
                {
                    case PropertyInfo prop:
                        canRead  = prop.CanRead  && prop.GetMethod != null;
                        canWrite = prop.CanWrite && prop.SetMethod != null;
                        break;

                    case FieldInfo field:
                        canRead = true;
                        canWrite = !field.IsInitOnly && !field.IsLiteral;
                        break;
                }
                break;
            
            case IndexExpression ie:
                if (ie.Indexer == null)
                {
                    canRead = true;
                    canWrite = true;
                }
                else
                {
                    canRead  = ie.Indexer.CanRead  && ie.Indexer.GetMethod != null;
                    canWrite = ie.Indexer.CanWrite && ie.Indexer.SetMethod != null;
                }
                break;
        }
        
        ArrayPool<Token>.Shared.Return(tokens);
        return (curExpr, curTyp, rootPar, canRead, canWrite);
    }

    private readonly struct Token
    {
        private readonly int Start;
        private readonly short Length;
        public readonly int Index;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Token(int idx)
        {
            Start = 0; Length = 0;
            Index = idx;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Token(int start, int length)
        {
            Start = start;
            Length = (short)length;
            Index = -1;
        }
        
        public bool IsIndex => Index >= 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string GetName(ReadOnlySpan<char> src) => new(src.Slice(Start, Length));
    }

    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int Tokenize(ReadOnlySpan<char> input, Span<Token> dst)
    {
        int len   = input.Length;
        int i     = 0;
        int count = 0;

        while (i < len)
        {
            switch (input[i])
            {
                case '.':
                    i++;
                    continue;

                case '[':
                    int val = 0;
                    for (i++; i < len && input[i] != ']'; i++)
                        val = val * 10 + (input[i] - '0');
                    i++;

                    dst[count++] = new Token(val);
                    continue;
            }

            int start = i;
            int rel   = input[i..].IndexOfAny('.', '[');
            i         = rel >= 0 ? i + rel : len;

            dst[count++] = new Token(start, i - start);
        }

        return count;
    }

    private const BindingFlags BF =
        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
}

