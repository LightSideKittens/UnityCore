using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

public readonly struct ObjectReferenceAccessor
{
    public readonly Func<UnityEngine.Object, UnityEngine.Object> Get;
    public readonly Action<UnityEngine.Object, UnityEngine.Object> Set;
    public ObjectReferenceAccessor(
        Func<UnityEngine.Object, UnityEngine.Object> get,
        Action<UnityEngine.Object, UnityEngine.Object> set)
    { Get = get; Set = set; }
}

public static class PathAccessorCache
{
    private static readonly Dictionary<(Type, string), ObjectReferenceAccessor> cache = new();

    public static ObjectReferenceAccessor Get(Type rootType, string path)
        => cache.TryGetValue((rootType, path), out var acc)
            ? acc
            : cache[(rootType, path)] = Build(rootType, path);

    private static ObjectReferenceAccessor Build(Type rootType, string path)
    {
        var tokens = Tokenize(path);
        var rootObj = Expression.Parameter(typeof(UnityEngine.Object), "root");
        Expression cur = Expression.Convert(rootObj, rootType);
        Type curType = rootType;

        foreach (var t in tokens)
        {
            switch (t)
            {
                case MemberToken m:
                    var fld = curType.GetField(m.name, BF) as MemberInfo
                              ?? curType.GetProperty(m.name, BF);
                    cur = fld switch
                    {
                        FieldInfo fi => Expression.Field(cur, fi),
                        PropertyInfo pi => Expression.Property(cur, pi),
                        _ => throw new MissingMemberException(curType.Name, m.name)
                    };
                    curType = fld switch
                    {
                        FieldInfo fi => fi.FieldType,
                        PropertyInfo pi => pi.PropertyType,
                        _ => curType
                    };
                    break;

                case IndexToken idx:
                    var idxExpr = Expression.Constant(idx.index);

                    if (curType.IsArray)
                    {
                        cur = Expression.ArrayAccess(cur, idxExpr);
                        curType = curType.GetElementType()!;
                    }
                    else
                    {
                        var itemProp = curType.GetProperty("Item");
                        cur = Expression.MakeIndex(cur, itemProp!, new[] { idxExpr });
                        curType = itemProp!.PropertyType;
                    }

                    break;
            }
        }

        var getterBody = Expression.Convert(cur, typeof(UnityEngine.Object));
        var getter = Expression.Lambda<Func<UnityEngine.Object, UnityEngine.Object>>
            (getterBody, rootObj).Compile();

        var valParam = Expression.Parameter(typeof(UnityEngine.Object), "val");
        var assign = Expression.Assign
            (cur, Expression.Convert(valParam, curType));
        var setter = Expression.Lambda<Action<UnityEngine.Object, UnityEngine.Object>>
            (assign, rootObj, valParam).Compile();

        return new ObjectReferenceAccessor(getter, setter);
    }

    private const BindingFlags BF = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

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
                yield return new IndexToken(int.Parse(path.Substring(i + 1, close - i - 1)));
                i = close + 1;
            }
            else
            {
                int j = i;
                while (j < path.Length && path[j] != '.' && path[j] != '[') j++;
                yield return new MemberToken(path.Substring(i, j - i));
                i = j;
            }
        }
    }

    private abstract record Token;

    private record MemberToken : Token
    {
        public MemberToken(string name)
        {
            this.name = name;
        }

        public string name;
    }

    private record IndexToken : Token
    {
        public IndexToken(int index)
        {
            this.index = index;
        }

        public int index;
    }
}