using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Newtonsoft.Json.Linq;
using UnityEngine;

#if UNITY_EDITOR
[Serializable]
public enum AlignType
{
    Free,
    Aligned,
}
#endif

[Serializable]
public struct BezierPoint : IEquatable<BezierPoint>
{
    public Vector2 p
    {
        get => new(x, y);
        set
        {
            x = value.x;
            y = value.y;
        }
    }

    public float x;
    public float y;

    public bool Equals(BezierPoint other)
    {
        return p.Equals(other.p)
#if UNITY_EDITOR
               && alignType == other.alignType 
               && e.Equals(other.e)
#endif
               ;
    }

    public override bool Equals(object obj)
    {
        return obj is BezierPoint other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(p
#if UNITY_EDITOR
            , (int)alignType, e
#endif
            );
    }
    
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

    
#if UNITY_EDITOR
    public AlignType alignType;
    public Vector2 e;

    public BezierPoint epPlusX(float x)
    {
        e.x += x;
        this.x += x;
        return this;
    }

    public BezierPoint epSet(Vector2 b)
    {
        e = b;
        p = e;
        return this;
    }
    
    public JObject ToJObject()
    {
        var json = new JObject
        {
            { "p", VectorToJObject(p) },
            { "e", VectorToJObject(e) },
            { "alignType", (int)alignType},
        };
        
        return json;
    }
    
    public static BezierPoint FromJObject(JToken obj)
    {
        var bezierPoint = new BezierPoint();
        var p = obj["p"];
        var e = obj["e"];
        bezierPoint.alignType = (AlignType)obj["alignType"].ToObject<int>();
        bezierPoint.e = new Vector2(e["x"].ToObject<float>(), e["y"].ToObject<float>());
        bezierPoint.p = new Vector2(p["x"].ToObject<float>(), p["y"].ToObject<float>());
        return bezierPoint;
    }

    private static JObject VectorToJObject(Vector2 vector)
    {
        return new JObject()
        {
            { "x", vector.x },
            { "y", vector.y },
        };
    }
#endif
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

    public BadassCurve() { }

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

#if UNITY_EDITOR
    public void SetAlign(int index, AlignType alignType)
    {
        var p = points[index];
        p.alignType = alignType;
        points[index] = p;
    }
#endif
    
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
    
    public void InsertKey(int i, float t)
    {
        if (i + 3 >= Count)
            return;

        var pts = points;

        Vector2 p0 = pts[i];
        Vector2 p1 = pts[i + 1];
        Vector2 p2 = pts[i + 2];
        Vector2 p3 = pts[i + 3];

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

    public int LastKeyIndex => points.Length - 2;
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe int GetLeftKeyIndexByX(float xTarget)
    {
        int count = points.Length;

        if (count < 3)
            return -1;

        fixed (BezierPoint* pPoints = points)
        {
            float firstX = pPoints[1].x;
            if (xTarget <= firstX)
                return -1;

            int lastKeyIndex = count - 2;
            float lastX = pPoints[lastKeyIndex].x;

            if (xTarget >= lastX)
                return lastKeyIndex;

            int numKeys = (count - 1) / 3;
            int left = 0;
            int right = numKeys - 1;

            while (left <= right)
            {
                int mid = (left + right) >> 1;
                int midIndex = 1 + 3 * mid;
                float midX = pPoints[midIndex].x;

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

        float t = FindBezierTForX(pts[i].x, pts[i + 1].x, pts[i + 2].x, pts[i + 3].x, x);
        InsertKey(i, t);
        return i + 3;
    }

    private void InsertDefault(int i, float x, bool before)
    {
        Vector2 root = points.Length > 2 ? points[i] : Vector2.zero;
        root.x = x;
        Vector2 half = Vector2.right / 5;
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
    
    public int InsertKeys(int i,
        BezierPoint forwardTangent,
        BezierPoint root,
        BezierPoint backTangent)
    {
        if (i == -1)
        {
            Insert(1, true, forwardTangent, root, backTangent);
            return 1;
        }

        Insert(i, false, forwardTangent, root, backTangent);
        return i + 3;
    }
    
    private void Insert(int i, bool before, 
        BezierPoint forwardTangent,
        BezierPoint root,
        BezierPoint backTangent)
    {
        if (before)
            i--;
        else
            i += 2;

        Insert(i, forwardTangent);
        Insert(i, root);
        Insert(i, backTangent);
    }

    public Vector2 EvaluateNormalizedVector(float t)
    {
        float xTarget = GetXByNormalized(t);
        return new Vector2(xTarget, Evaluate(xTarget));
    }

    public float GetXByNormalized(float t)
    {
        float xMin = points[1].x;
        float xMax = points[points.Length - 2].x;
        float xTarget = Mathf.Lerp(xMin, xMax, t);
        return xTarget;
    }
    
    public float EvaluateNormalized(float t)
    {
        float xTarget = GetXByNormalized(t);
        return Evaluate(xTarget);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe float Evaluate(float x)
    {
        int count = points.Length;
        if(count < 3) return 0;

        fixed (BezierPoint* pPoints = points)
        {
            int i = GetLeftKeyIndexByX(x);
            if (i == -1) return pPoints[1].y;
            if (i == LastKeyIndex) return pPoints[count - 2].y;

            BezierPoint* p0 = &pPoints[i];
            BezierPoint* p1 = &pPoints[i + 1];
            BezierPoint* p2 = &pPoints[i + 2];
            BezierPoint* p3 = &pPoints[i + 3];

            float tForX = FindBezierTForX(p0->x, p1->x, p2->x, p3->x, x);
            float result = EvaluateCubicBezier(p0->y, p1->y, p2->y, p3->y, tForX);
            return result;
        }
    }

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