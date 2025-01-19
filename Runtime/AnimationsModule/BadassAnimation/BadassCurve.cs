using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

[Serializable]
public enum AlignType
{
    Free,
    Aligned,
}

[Serializable]
public struct BezierPoint : IEquatable<BezierPoint>
{
    public Vector2 p;

#if UNITY_EDITOR
    public AlignType alignType;
    public Vector2 e;

    public BezierPoint epPlus(Vector2 b)
    {
        e += b;
        p = e;
        return this;
    }

    public BezierPoint epSet(Vector2 b)
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

    public bool Equals(BezierPoint other)
    {
        return p.Equals(other.p) && alignType == other.alignType && e.Equals(other.e);
    }

    public override bool Equals(object obj)
    {
        return obj is BezierPoint other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(p, (int)alignType, e);
    }
}

[Serializable]
public class BadassCurve : IList<BezierPoint>
{
    [SerializeField]
    private BezierPoint[] points = Array.Empty<BezierPoint>();
    
    public BezierPoint[] Points
    {
        get => points;
        set => points = value;
    }

    public BadassCurve()
    {
    }

    public BadassCurve(BadassCurve badassCurve)
    {
        Points = new BezierPoint[badassCurve.Points.Length];
        Array.Copy(badassCurve.Points, Points, Points.Length);
    }

    public IEnumerator<BezierPoint> GetEnumerator()
    {
        foreach (var point in Points)
        {
            yield return point;
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public void Add(BezierPoint item)
    {
        int length = Points.Length;
        Array.Resize(ref points, length + 1);
        points[length] = item;
    }

    public void Clear()
    {
        points = Array.Empty<BezierPoint>();
    }

    public bool Contains(BezierPoint item)
    {
        return Array.IndexOf(points, item) >= 0;
    }

    public void CopyTo(BezierPoint[] array, int arrayIndex)
    {
        Array.Copy(points, 0, array, arrayIndex, points.Length);
    }

    public bool Remove(BezierPoint item)
    {
        int index = IndexOf(item);
        if (index >= 0)
        {
            RemoveAt(index);
            return true;
        }
        return false;
    }

    public int Count => points.Length;

    public bool IsReadOnly => false;

    public int IndexOf(BezierPoint item)
    {
        return Array.IndexOf(points, item);
    }

    public void Insert(int index, BezierPoint item)
    {
        int length = points.Length;
        Array.Resize(ref points, length + 1);
        if (index < length)
        {
            Array.Copy(points, index, points, index + 1, length - index);
        }
        points[index] = item;
    }

    public void RemoveAt(int index)
    {
        int length = points.Length;
        if (index < length - 1)
        {
            Array.Copy(points, index + 1, points, index, length - index - 1);
        }
        Array.Resize(ref points, length - 1);
    }

    public BezierPoint this[int index]
    {
        get => points[index];
        set => points[index] = value;
    }

    public void SetAlign(int index, AlignType alignType)
    {
        var p = points[index];
        p.alignType = alignType;
        points[index] = p;
    }

    /// <summary>
    /// Удаляем ключ (root) по его порядковому номеру (0,1,2,...),
    /// где фактический индекс root-а = keyIndex * 3 + 1.
    /// </summary>
    public void DeleteKey(int i)
    {
        if (i < 0 || i >= Count)
            return;

        int leftTangent = i - 1;
        int rightTangent = i + 1;

        if (rightTangent < Count)
            RemoveAt(rightTangent);
        RemoveAt(i);
        if (leftTangent >= 0)
            RemoveAt(leftTangent);
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

        var pts = points;

        Vector2 p0 = pts[i].p;
        Vector2 p1 = pts[i + 1].p;
        Vector2 p2 = pts[i + 2].p;
        Vector2 p3 = pts[i + 3].p;

        Vector2 q0 = Vector2.Lerp(p0, p1, t);
        Vector2 q1 = Vector2.Lerp(p1, p2, t);
        Vector2 q2 = Vector2.Lerp(p2, p3, t);

        Vector2 r0 = Vector2.Lerp(q0, q1, t);
        Vector2 r1 = Vector2.Lerp(q1, q2, t);

        Vector2 s = Vector2.Lerp(r0, r1, t);

        pts[i + 1] = (BezierPoint)q0;
        pts[i + 2] = (BezierPoint)r0;
        pts[i + 3] = (BezierPoint)s;

        Insert(i + 4, (BezierPoint)r1);
        Insert(i + 5, (BezierPoint)q2);
        Insert(i + 6, (BezierPoint)p3);
    }

    private int LastKeyIndex => points.Length - 2;

    /// <summary>
    /// Найти индекс "левого" ключа (root), у которого x <= xTarget,
    /// но у следующего root x уже > xTarget
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private unsafe int GetLeftKeyIndexByX(float xTarget)
    {
        int count = points.Length;

        if (count < 3)
            return -1;

        fixed (BezierPoint* pPoints = points)
        {
            float firstX = pPoints[1].p.x;
            if (xTarget <= firstX)
                return -1;

            int lastKeyIndex = count - 2;
            float lastX = pPoints[lastKeyIndex].p.x;

            if (xTarget >= lastX)
                return lastKeyIndex;

            int numKeys = (count - 1) / 3;
            int left = 0;
            int right = numKeys - 1;

            while (left <= right)
            {
                int mid = (left + right) >> 1;
                int midIndex = 1 + 3 * mid;
                float midX = pPoints[midIndex].p.x;

                if (Math.Abs(midX - xTarget) < 1e-6f)
                {
                    return midIndex;
                }
                if (midX < xTarget)
                {
                    left = mid + 1;
                }
                else
                {
                    right = mid - 1;
                }
            }
            int keyIndex = left - 1;
            if (keyIndex < 0) keyIndex = 0;
            return 1 + 3 * keyIndex;
        }
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

        var pts = points;

        Vector2 p0 = pts[i].p;
        Vector2 p1 = pts[i + 1].p;
        Vector2 p2 = pts[i + 2].p;
        Vector2 p3 = pts[i + 3].p;

        float t = FindBezierTForX(p0.x, p1.x, p2.x, p3.x, x);
        InsertKey(i, t);
        return i + 3;
    }

    private void InsertDefault(int i, float x, bool before)
    {
        Vector2 root = points.Length > 2 ? points[i].p : Vector2.zero;
        root.x = x;
        Vector2 half = Vector2.right / 2;
        Vector2 backTangent = root - half;
        Vector2 forwardTangent = root + half;

        if (before)
            i--;
        else
            i += 2;

        Insert(i, (BezierPoint)forwardTangent);
        Insert(i, (BezierPoint)root);
        Insert(i, (BezierPoint)backTangent);
    }

    public Vector2 EvaluateNormalizedVector(float t)
    {
        float xTarget = GetXByNormalized(t);
        return new Vector2(xTarget, Evaluate(xTarget));
    }

    public float GetXByNormalized(float t)
    {
        float xMin = points[1].p.x;
        float xMax = points[points.Length - 2].p.x;
        float xTarget = Mathf.Lerp(xMin, xMax, t);
        return xTarget;
    }

    /// <summary>
    /// Посчитать значение кривой "по оси X" (т.е. мы хотим y при заданном x),
    /// считая что t = 0..1 распределяется между самой левой и самой правой root-точкой.
    /// </summary>
    public float EvaluateNormalized(float t)
    {
        float xTarget = GetXByNormalized(t);
        return Evaluate(xTarget);
    }

    /// <summary>
    /// Возвращает значение y на кривой при заданном x.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe float Evaluate(float x)
    {
        int count = points.Length;
        if(count < 3) return 0;

        fixed (BezierPoint* pPoints = points)
        {
            int i = GetLeftKeyIndexByX(x);
            if (i == -1) return pPoints[1].p.y;
            if (i == LastKeyIndex) return pPoints[count - 2].p.y;

            Vector2* p0 = &pPoints[i].p;
            Vector2* p1 = &pPoints[i + 1].p;
            Vector2* p2 = &pPoints[i + 2].p;
            Vector2* p3 = &pPoints[i + 3].p;

            float tForX = FindBezierTForX(p0->x, p1->x, p2->x, p3->x, x);
            float result = EvaluateCubicBezier(p0->y, p1->y, p2->y, p3->y, tForX);
            return result;
        }
    }

    /// <summary>
    /// Стандартная формула кубического Безье
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private float EvaluateCubicBezier(float p0, float p1, float p2, float p3, float t)
    {
        float t2 = t * t;
        float t3 = t2 * t;
        float a = -p0 + 3f * p1 - 3f * p2 + p3;
        float b = 3f * p0 - 6f * p1 + 3f * p2;
        float c = -3f * p0 + 3f * p1;
        float d = p0;
        return a * t3 + b * t2 + c * t + d;
    }

    /// <summary>
    /// Численно находим параметр t, при котором x(t) ≈ xTarget
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private float FindBezierTForX(float x0, float x1, float x2, float x3, float xTarget, float tolerance = 0.0001f)
    {
        float a = -x0 + 3f * x1 - 3f * x2 + x3;
        float b = 3f * x0 - 6f * x1 + 3f * x2;
        float c = -3f * x0 + 3f * x1;
        float d = x0 - xTarget;

        const int MaxIterations = 5;
        float t = 0.5f;
        float tMin = 0f;
        float tMax = 1f;

        for (int i = 0; i < MaxIterations; i++)
        {
            float f = ((a * t + b) * t + c) * t + d;
            float df = (3f * a * t + 2f * b) * t + c;

            if (Math.Abs(df) < 1e-6f) break;

            float tNew = t - f / df;

            if (tNew < tMin || tNew > tMax)
            {
                break;
            }

            if (Math.Abs(tNew - t) < tolerance)
            {
                t = tNew;
                return t;
            }

            t = tNew;
        }

        while (tMax - tMin > tolerance)
        {
            t = 0.5f * (tMin + tMax);
            float f = ((a * t + b) * t + c) * t + d;

            if (f > 0f)
            {
                tMax = t;
            }
            else
            {
                tMin = t;
            }
        }

        t = 0.5f * (tMin + tMax);
        return t;
    }
    
    public static bool IsRoot(int i)
    {
        i--;
        return i % 3 == 0;
    }
    
    public static bool IsTangent(int i)
    {
        var f = i % 3;
        return f is 0 or 2;
    }
    
    public static bool IsForwardTangent(int i)
    {
        return i % 3 == 2;
    }
    
    public static bool IsBackwardTangent(int i)
    {
        return i % 3 == 0;
    }

    public void Apply(BadassCurve copy)
    {
        Points = copy.Points;
    }
}