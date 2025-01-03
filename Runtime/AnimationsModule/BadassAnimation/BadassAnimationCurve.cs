﻿using System;
using System.Collections;
using System.Collections.Generic;
using LSCore;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

[Serializable]
public enum AlignType
{
    Free,
    Aligned,
}

[Serializable]
public struct BezierPoint
{
    [JsonIgnore] public Vector2 p;
    
    public float x
    {
        get => p.x;
        set => p.x = value;
    }
    
    public float y
    {
        get => p.y;
        set => p.y = value;
    }
    
#if UNITY_EDITOR
    public AlignType alignType;
    [JsonIgnore] public Vector2 e;
    
    public float ex
    {
        get => e.x;
        set => e.x = value;
    }
    
    public float ey
    {
        get => e.y;
        set => e.y = value;
    }
    
    public BezierPoint ePlus(Vector2 b)
    {
        e += b;
        p = e;
        return this;
    }
    
    public BezierPoint eSet(Vector2 b)
    {
        e = b;
        p = e;
        return this;
    }

#endif
    
    public static BezierPoint operator +(BezierPoint a, Vector2 b)
    {
        a.p += b;
        return a;
    }
    
    public static implicit operator Vector2(BezierPoint point) => point.p;
    public static implicit operator Vector3(BezierPoint point) => point.p;

    public static explicit operator BezierPoint(Vector2 point)
    {
        return new BezierPoint
        {
            p = point,
#if UNITY_EDITOR
            e = point,
            alignType = AlignType.Aligned
#endif
        };
    }
}

[Serializable]
[JsonObject(MemberSerialization.OptIn)]
public class BadassAnimationCurve : IList<BezierPoint>, ISerializationCallbackReceiver
{
    [SerializeField] [JsonIgnore] private string json;
    
    [JsonProperty] public List<BezierPoint> Points { get; set; } = new();
    [JsonProperty] public bool Loop { get; set; }
    

    public BadassAnimationCurve() { }

    public BadassAnimationCurve(BadassAnimationCurve badassAnimationCurve)
    {
        Points = new List<BezierPoint>(badassAnimationCurve.Points);
        Loop = badassAnimationCurve.Loop;
    }

    public IEnumerator<BezierPoint> GetEnumerator() => Points.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public void Add(Vector2 item) => Add((BezierPoint)item);
    public void Add(BezierPoint item) => Points.Add(item);

    public void Clear() => Points.Clear();
    public bool Contains(BezierPoint item) => Points.Contains(item);
    public void CopyTo(BezierPoint[] array, int arrayIndex) => Points.CopyTo(array, arrayIndex);
    public bool Remove(BezierPoint item) => Points.Remove(item);
    public int Count => Points.Count;
    public bool IsReadOnly => false;
    
    public int IndexOf(BezierPoint item) => Points.IndexOf(item);
    public void Insert(int index, BezierPoint item) => Points.Insert(index, item);
    public void RemoveAt(int index) => Points.RemoveAt(index);

    public void SetAlign(int index, AlignType alignType)
    {
        var p = Points[index];
        p.alignType = alignType;
        Points[index] = p;
    }
    
    public BezierPoint this[int index]
    {
        get => Points[index];
        set => Points[index] = value;
    }

    /// <summary>
    /// Удаляем ключ (root) по его порядковому номеру (0,1,2,...),
    /// где фактический индекс root-а = keyIndex * 3 + 1.
    /// </summary>
    public void DeleteKey(int i)
    {
        if (i < 0 || i >= Count) return;
        
        int leftTangent = i - 1;
        int rightTangent = i + 1;
        
        if (rightTangent < Count) RemoveAt(rightTangent);
        
        RemoveAt(i);
        
        if (leftTangent >= 0) RemoveAt(leftTangent);
    }

    /// <summary>
    /// Вставить "промежуточный" ключ внутри кривой, деля сегмент пополам (или по t).
    /// keyIndex – порядковый индекс ключа, в который "врезаемся", 
    /// t – доля (0..1) вдоль соответствующего участка Безье.
    /// </summary>
    public void InsertKey(int i, float t)
    {
        if (i + 3 >= Count)
            return;

        BezierPoint p0 = Points[i];
        BezierPoint p1 = Points[i + 1];
        BezierPoint p2 = Points[i + 2];
        BezierPoint p3 = Points[i + 3];

        Vector2 q0 = Vector2.Lerp(p0, p1, t);
        Vector2 q1 = Vector2.Lerp(p1, p2, t);
        Vector2 q2 = Vector2.Lerp(p2, p3, t);

        Vector2 r0 = Vector2.Lerp(q0, q1, t);
        Vector2 r1 = Vector2.Lerp(q1, q2, t);
        Vector2 s  = Vector2.Lerp(r0, r1, t);

        Points[i + 1] = (BezierPoint)q0;
        Points[i + 2] = (BezierPoint)r0;
        Points[i + 3] = (BezierPoint)s;

        Insert(i + 4, (BezierPoint)r1);
        Insert(i + 5, (BezierPoint)q2);
        Insert(i + 6, p3);
    }

    private int LastKeyIndex => Count - 2;
    
    /// <summary>
    /// Найти индекс "левого" ключа (root), у которого x <= xTarget,
    /// но у следующего root x уже > xTarget
    /// </summary>
    public int GetLeftKeyIndexByX(float xTarget)
    {
        if (Points.Count < 3 || xTarget <= Points[1].x)
        {
            return -1;
        }

        int lastKeyIndex = LastKeyIndex;
        
        if (xTarget >= Points[lastKeyIndex].x)
        {
            return lastKeyIndex;
        }

        int keyIndex = 0;
        for (int i = 1; i < Count; i += 3)
        {
            if (Points[i].x >= xTarget)
            {
                return Mathf.Max(0, keyIndex - 1) * 3 + 1;
            }
            keyIndex++;
        }

        return lastKeyIndex;
    }

    /// <summary>
    /// Вставить ключ (root) по нужному x (автоматически найдём, в какой сегмент попадает)
    /// </summary>
    public int InsertKeyByX(float x)
    {
        int i = GetLeftKeyIndexByX(x);

        if (i == -1)
        {
            InsertDefault(1, x, true);
            return 1;
        }

        if (i == LastKeyIndex)
        {
            InsertDefault(i, x, false);
            return i + 3;
        }

        BezierPoint p0 = Points[i];
        BezierPoint p1 = Points[i + 1];
        BezierPoint p2 = Points[i + 2];
        BezierPoint p3 = Points[i + 3];

        float t = FindBezierTForX(p0.x, p1.x, p2.x, p3.x, x);

        InsertKey(i, t);
        return i + 3;
    }

    private void InsertDefault(int i, float x, bool before)
    {
        Vector2 root = Points.Count > 2 ? Points[i] : Vector2.zero;
        root.x = x;
        var half = Vector2.right / 2;
        Vector2 backTangent = root - half;
        Vector2 forwardTangent = root + half;

        if (before) i--;
        else i += 2;

        Insert(i, (BezierPoint)forwardTangent);
        Insert(i, (BezierPoint)root);
        Insert(i, (BezierPoint)backTangent);
    }

    /// <summary>
    /// Посчитать значение кривой "по оси X" (т.е. мы хотим y при заданном x), 
    /// считая что t = 0..1 распределяется между самой левой и самой правой root-точкой.
    /// </summary>
    public Vector2 EvaluateNormalized(float t)
    {
        float xMin = Points[1].x;
        float xMax = Points[^2].x; 
        float xTarget = Mathf.Lerp(xMin, xMax, t);
        return Evaluate(xTarget);
    }
    
    /// <summary>
    /// Возвращает точку (x,y) на кривой при заданном x.
    /// </summary>
    public Vector2 Evaluate(float x)
    {
        int i = GetLeftKeyIndexByX(x);
        
        if (i == -1) return Points[1];
        if (i == LastKeyIndex) return Points[^2];
        
        Vector2 p0 = Points[i];
        Vector2 p1 = Points[i + 1];
        Vector2 p2 = Points[i + 2];
        Vector2 p3 = Points[i + 3];
        
        float tForX = FindBezierTForX(p0.x, p1.x, p2.x, p3.x, x);
        
        return EvaluateCubicBezier(p0, p1, p2, p3, tForX);
    }

    /// <summary>
    /// Стандартная формула кубического Безье
    /// </summary>
    private Vector2 EvaluateCubicBezier(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;
        float uuu = uu * u;
        float ttt = tt * t;

        Vector2 p = uuu * p0;
        p += 3 * uu * t * p1;
        p += 3 * u * tt * p2;
        p += ttt * p3;

        return p;
    }

    /// <summary>
    /// Численно находим параметр t, при котором x(t) ≈ xTarget
    /// </summary>
    private float FindBezierTForX(float x0, float x1, float x2, float x3, float xTarget, float tolerance = 0.0001f)
    {
        float t0 = 0f;
        float t1 = 1f;

        while (t1 - t0 > tolerance)
        {
            float t = (t0 + t1) * 0.5f;
            float tm1 = 1 - t;
            float xMid = tm1*tm1*tm1 * x0
                         + 3 * tm1*tm1 * t * x1
                         + 3 * tm1 * t*t * x2
                         + t*t*t * x3;

            if (xMid < xTarget)
            {
                t0 = t;
            }
            else
            {
                t1 = t;
            }
        }

        return (t0 + t1) * 0.5f;
    }

    void ISerializationCallbackReceiver.OnBeforeSerialize()
    {
        json = GetJson();
    }

    void ISerializationCallbackReceiver.OnAfterDeserialize()
    {
        Points.Clear();
        JsonConvert.PopulateObject(json, this, SerializationSettings.Default.settings);
    }
    
    internal string GetJson() => JsonConvert.SerializeObject(this, SerializationSettings.Default.settings);
}