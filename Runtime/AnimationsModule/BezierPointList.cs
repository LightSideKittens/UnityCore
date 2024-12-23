using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct BezierPoint
{
    public Vector2 p;
    
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
    public Vector2 e;
    
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
            e = point,
        };
    }
}

[Serializable]
public class BezierPointList : IList<BezierPoint>
{
    public List<BezierPoint> points = new();
    public bool loop;
    
    public IEnumerator<BezierPoint> GetEnumerator()
    {
        return points.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Add(Vector2 item) => Add((BezierPoint)item);

    public void Add(BezierPoint item)
    {
        points.Add(item);
    }

    public void Clear()
    {
        points.Clear();
    }

    public bool Contains(BezierPoint item)
    {
        return points.Contains(item);
    }

    public void CopyTo(BezierPoint[] array, int arrayIndex)
    {
        points.CopyTo(array, arrayIndex);
    }

    public bool Remove(BezierPoint item)
    {
        return points.Remove(item);
    }

    public int Count => points.Count;
    public bool IsReadOnly => false;
    
    public int IndexOf(BezierPoint item)
    {
        return points.IndexOf(item);
    }

    public void Insert(int index, BezierPoint item)
    {
        points.Insert(index, item);
    }

    public void RemoveAt(int index)
    {
        points.RemoveAt(index);
    }

    public BezierPoint this[int index]
    {
        get => points[index];
        set => points[index] = value;
    }
    
    public void DeleteKey(int index)
    {
        index *= 3;
        
        if (index == 0)
        {
            RemoveAt(index);
            RemoveAt(index);
            RemoveAt(index);
        }
        else if(index == points.Count - 1)
        {
            RemoveAt(Count - 1);
            RemoveAt(Count - 1);
            RemoveAt(Count - 1);
        }
        else
        {
            index--;
            RemoveAt(index);
            RemoveAt(index);
            RemoveAt(index);
        }
    }
    
    public void InsertKey(int index, float t)
    {
        var i = index * 3;
        if (i >= Count - 1)
        {
            return;
        }
        
        BezierPoint p0 = points[i];
        BezierPoint p1 = points[i + 1];
        BezierPoint p2 = points[i + 2];
        BezierPoint p3 = points[i + 3];

        Vector2 q0 = Vector2.Lerp(p0, p1, t);
        Vector2 q1 = Vector2.Lerp(p1, p2, t);
        Vector2 q2 = Vector2.Lerp(p2, p3, t);

        Vector2 r0 = Vector2.Lerp(q0, q1, t);
        Vector2 r1 = Vector2.Lerp(q1, q2, t);
        Vector2 s = Vector2.Lerp(r0, r1, t);
        
        points[i + 1] = (BezierPoint)q0;
        points[i + 2] = (BezierPoint)r0;
        points[i + 3] = (BezierPoint)s;
        Insert(i + 4, (BezierPoint)r1);
        Insert(i + 5, (BezierPoint)q2);
        Insert(i + 6, p3);
    }
    
    public Vector2 EvaluateNormalized(float t)
    {
        float xTarget = Mathf.Lerp(points[0].x, points[^1].x, t);
        return Evaluate(xTarget);
    }
    
    public Vector2 Evaluate(float x)
    {
        int curveIndex = GetLeftKeyIndexByX(x);
        int index = curveIndex * 3;
        Vector2 p0 = points[index];
        Vector2 p1 = points[index + 1];
        Vector2 p2 = points[index + 2];
        Vector2 p3 = points[index + 3];
        
        float tForX = FindBezierTForX(p0.x, p1.x, p2.x, p3.x, x);
        return EvaluateCubicBezier(p0, p1, p2, p3, tForX);
    }

    public void InsertKeyByX(float x)
    {
        var keyIndex = GetLeftKeyIndexByX(x);
        var index = keyIndex * 3;
        Vector2 p0 = points[index];
        Vector2 p1 = points[index + 1];
        Vector2 p2 = points[index + 2];
        Vector2 p3 = points[index + 3];

        var targetX = FindBezierTForX(p0.x, p1.x, p2.x, p3.x, x);
        InsertKey(keyIndex, targetX);
    }

    private int GetLeftKeyIndexByX(float x)
    {
        if (x <= points[0].x) return 0;
        if (x >= points[^1].x) return (points.Count - 4) /  3;
        var index = 0;

        for (int i = 3; i < points.Count; i += 3)
        {
            if (x <= points[i].x)
            {
                return index;
            }
                
            index++;
        }

        return index;
    }
    
    
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

    private float FindBezierTForX(float x0, float x1, float x2, float x3, float xTarget, float tolerance = 0.0001f)
    {
        float t0 = 0f;
        float t1 = 1f;

        while (t1 - t0 > tolerance)
        {
            float t = (t0 + t1) / 2;
            float tm1 = 1 - t;
            float xMid = Mathf.Pow(tm1, 3) * x0 + 3 * Mathf.Pow(tm1, 2) * t * x1 + 3 * tm1 * Mathf.Pow(t, 2) * x2 +
                         Mathf.Pow(t, 3) * x3;

            if (xMid < xTarget)
            {
                t0 = t;
            }
            else
            {
                t1 = t;
            }
        }

        return (t0 + t1) / 2;
    }
}