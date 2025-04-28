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
    static readonly Dictionary<(Type,string), ObjectReferenceAccessor> _cache = new();

    public static ObjectReferenceAccessor Get(Type rootType, string path)
        => _cache.TryGetValue((rootType,path), out var acc)
            ? acc
            : (_cache[(rootType,path)] = Build(rootType,path));

    // ----------------------------------------------------
    static ObjectReferenceAccessor Build(Type rootType, string path)
    {
        var tokens = Tokenize(path);             // IEnumerable<Token>
        var rootObj   = Expression.Parameter(typeof(UnityEngine.Object), "root");
        Expression cur = Expression.Convert(rootObj, rootType);
        Type curType   = rootType;

        foreach (var t in tokens)
        {
            switch (t)
            {
                case MemberToken m:
                    var fld = curType.GetField(m.Name, BF) as MemberInfo
                              ?? curType.GetProperty(m.Name, BF);
                    cur     = fld switch {
                        FieldInfo fi    => Expression.Field(cur, fi),
                        PropertyInfo pi => Expression.Property(cur, pi),
                        _               => throw new MissingMemberException(curType.Name, m.Name)
                    };
                    curType = fld switch {
                        FieldInfo fi    => fi.FieldType,
                        PropertyInfo pi => pi.PropertyType,
                        _               => curType
                    };
                    break;

                case IndexToken idx:
                    var idxExpr = Expression.Constant(idx.Index);

                    if (curType.IsArray)
                    {
                        cur     = Expression.ArrayAccess(cur, idxExpr);   // ← writeable
                        curType = curType.GetElementType()!;
                    }
                    else          // IList / List<T>
                    {
                        var itemProp = curType.GetProperty("Item");        // имеет и get, и set
                        cur          = Expression.MakeIndex(cur, itemProp!, new[] { idxExpr });
                        curType      = itemProp!.PropertyType;
                    }
                    break;
            }
        }

        // -------------- getter --------------
        var getterBody = Expression.Convert(cur, typeof(UnityEngine.Object));
        var getter = Expression.Lambda<Func<UnityEngine.Object,UnityEngine.Object>>
            (getterBody, rootObj).Compile();

        // -------------- setter --------------
        var valParam = Expression.Parameter(typeof(UnityEngine.Object), "val");
        var assign   = Expression.Assign
            (cur, Expression.Convert(valParam, curType));
        var setter   = Expression.Lambda<Action<UnityEngine.Object,UnityEngine.Object>>
            (assign, rootObj, valParam).Compile();

        return new ObjectReferenceAccessor(getter, setter);
    }

    // helpers ---------------------------------------------------------------
    const BindingFlags BF = BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic;
    static IEnumerable<Token> Tokenize(string path)
    {
        // очень простой парсер: слово или [число]
        int i=0;
        while (i < path.Length)
        {
            if (path[i]=='.') { i++; continue; }
            if (path[i]=='[')
            {
                int close = path.IndexOf(']', i);
                yield return new IndexToken(int.Parse(path.Substring(i+1, close-i-1)));
                i = close+1;
            }
            else
            {
                int j=i;
                while (j<path.Length && path[j]!='.' && path[j]!='[') j++;
                yield return new MemberToken(path.Substring(i, j-i));
                i=j;
            }
        }
    }
    abstract record Token;
    record MemberToken : Token
    {
        public MemberToken(string Name)
        {
            this.Name = Name;
        }

        public string Name;
    }

    record IndexToken : Token
    {
        public IndexToken(int Index)
        {
            this.Index = Index;
        }

        public int Index;
    }
}