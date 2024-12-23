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
#if UNITY_EDITOR
            e = point,
#endif
        };
    }
}

[Serializable]
public class BezierPointList : IList<BezierPoint>
{
    public List<BezierPoint> points = new();
    public bool loop;
    
    public IEnumerator<BezierPoint> GetEnumerator() => points.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public void Add(Vector2 item) => Add((BezierPoint)item);
    public void Add(BezierPoint item) => points.Add(item);

    public void Clear() => points.Clear();
    public bool Contains(BezierPoint item) => points.Contains(item);
    public void CopyTo(BezierPoint[] array, int arrayIndex) => points.CopyTo(array, arrayIndex);
    public bool Remove(BezierPoint item) => points.Remove(item);
    public int Count => points.Count;
    public bool IsReadOnly => false;
    
    public int IndexOf(BezierPoint item) => points.IndexOf(item);
    public void Insert(int index, BezierPoint item) => points.Insert(index, item);
    public void RemoveAt(int index) => points.RemoveAt(index);

    public BezierPoint this[int index]
    {
        get => points[index];
        set => points[index] = value;
    }

    /// <summary>
    /// Удаляем ключ (root) по его порядковому номеру (0,1,2,...),
    /// где фактический индекс root-а = keyIndex * 3 + 1.
    /// </summary>
    public void DeleteKey(int keyIndex)
    {
        // Индекс самой root-точки в списке
        int rootIndex = keyIndex * 3 + 1;
        
        // Для удобства проверим границы
        if (rootIndex < 0 || rootIndex >= Count)
            return;

        // У нас сегмент выглядит (rootIndex - 1): tangent, (rootIndex): root, (rootIndex + 1): tangent
        // и до следующего root ещё (rootIndex + 2): tangent, (rootIndex + 3): следующий root.
        // Обычно достаточно убрать root + два соседних тангента.
        // Однако если keyIndex - это первая или последняя точка, смотрим на границы.

        // Примерно такая логика:
        int leftTangent = rootIndex - 1;
        int rightTangent = rootIndex + 1;

        // Если это "первый" ключ, то leftTangent может быть 0 (что валидно)
        // Если это "последний" ключ, то rightTangent может оказаться за границей.
        // Поэтому аккуратно удаляем в порядке убывания индексов (чтобы не сместить оставшиеся раньше времени).

        // Удаляем правую точку, если она в пределах
        if (rightTangent < Count)
            RemoveAt(rightTangent);

        // Удаляем сам root
        RemoveAt(rootIndex);

        // Удаляем левую точку, если она в пределах
        if (leftTangent >= 0)
            RemoveAt(leftTangent);
    }

    /// <summary>
    /// Вставить "промежуточный" ключ внутри кривой, деля сегмент пополам (или по t).
    /// keyIndex – порядковый индекс ключа, в который "врезаемся", 
    /// t – доля (0..1) вдоль соответствующего участка Безье.
    /// </summary>
    public void InsertKey(int keyIndex, float t)
    {
        // Индекс root'а, от которого пойдёт сегмент
        int i = keyIndex * 3 + 1;

        // Проверка, чтобы не вылететь за границы
        // Нам нужны точки i, i+1, i+2, i+3 (потому что до следующего root идёт +3)
        if (i + 3 >= Count)
            return;

        // Вот наш "кубический" сегмент: root, tangent, tangent, root
        BezierPoint p0 = points[i];     // текущий root
        BezierPoint p1 = points[i + 1]; // tangent
        BezierPoint p2 = points[i + 2]; // tangent
        BezierPoint p3 = points[i + 3]; // следующий root

        // Интерполяции (стандартная формула деления Безье на две части)
        Vector2 q0 = Vector2.Lerp(p0, p1, t);
        Vector2 q1 = Vector2.Lerp(p1, p2, t);
        Vector2 q2 = Vector2.Lerp(p2, p3, t);

        Vector2 r0 = Vector2.Lerp(q0, q1, t);
        Vector2 r1 = Vector2.Lerp(q1, q2, t);
        Vector2 s  = Vector2.Lerp(r0, r1, t);

        // Перезаписываем часть точек в "левом" сегменте
        points[i + 1] = (BezierPoint)q0;
        points[i + 2] = (BezierPoint)r0;
        points[i + 3] = (BezierPoint)s;

        // Во "вторую половину"Bezier добавим 3 новые точки 
        // (т.к. у нас исходный root (p3) мы сохраним, но он сместится правее)
        // Инсерты делать удобнее в порядке возрастания индексов:

        Insert(i + 4, (BezierPoint)r1);    // новый tangent
        Insert(i + 5, (BezierPoint)q2);    // ещё один tangent
        Insert(i + 6, p3);                // старый root (смещён)
    }
    
    /// <summary>
    /// Найти индекс "левого" ключа (root), у которого x <= xTarget,
    /// но у следующего root x уже > xTarget
    /// </summary>
    private int GetLeftKeyIndexByX(float xTarget)
    {
        // Если xTarget вообще левее первой root-точки => возвращаем 0
        if (xTarget <= points[1].x)
            return 0;

        // Если xTarget правее последней root-точки => возвращаем индекс последнего ключа
        // Последний root лежит по индексу (Count - 2), если структура ровная.
        // Но на всякий случай возьмём формулу (points.Count-1 - 1) / 3:
        // Т.е. (последний rootIndex - 1)/3
        int lastKeyIndex = (Count - 2) / 3;
        if (xTarget >= points[Count - 2].x)
            return lastKeyIndex;

        // Теперь пробегаем по root-индексам 1,4,7,... (шаг 3)
        // Ищем тот root, у которого x >= xTarget
        int keyIndex = 0;
        for (int i = 1; i < Count; i += 3)
        {
            if (points[i].x >= xTarget)
                return Mathf.Max(0, keyIndex - 1);
            keyIndex++;
        }

        return lastKeyIndex; 
    }

    /// <summary>
    /// Вставить ключ (root) по нужному x (автоматически найдём, в какой сегмент попадает)
    /// </summary>
    public void InsertKeyByX(float x)
    {
        int leftKeyIndex = GetLeftKeyIndexByX(x);
        // Индекс root'a для сегмента
        int i = leftKeyIndex * 3 + 1;

        // Границы
        if (i + 3 >= Count)
            return;

        // Точки сегмента
        BezierPoint p0 = points[i];
        BezierPoint p1 = points[i + 1];
        BezierPoint p2 = points[i + 2];
        BezierPoint p3 = points[i + 3];

        // Ищем t, при котором на кривой будет нужный x
        float t = FindBezierTForX(p0.x, p1.x, p2.x, p3.x, x);

        // Далее просто вставляем
        InsertKey(leftKeyIndex, t);
    }

    /// <summary>
    /// Посчитать значение кривой "по оси X" (т.е. мы хотим y при заданном x), 
    /// считая что t = 0..1 распределяется между самой левой и самой правой root-точкой.
    /// </summary>
    public Vector2 EvaluateNormalized(float t)
    {
        // Линейно берём x от левого (points[1]) к правому (points[^2]).
        float xMin = points[1].x;
        float xMax = points[^2].x; 
        float xTarget = Mathf.Lerp(xMin, xMax, t);
        return Evaluate(xTarget);
    }
    
    /// <summary>
    /// Возвращает точку (x,y) на кривой при заданном x.
    /// </summary>
    public Vector2 Evaluate(float x)
    {
        int curveIndex = GetLeftKeyIndexByX(x);
        int i = curveIndex * 3 + 1;

        // Берём 4 точки сегмента
        Vector2 p0 = points[i];     // root
        Vector2 p1 = points[i + 1]; // tangent
        Vector2 p2 = points[i + 2]; // tangent
        Vector2 p3 = points[i + 3]; // следующий root

        // Ищем t, при котором x(t) ≈ нужному x
        float tForX = FindBezierTForX(p0.x, p1.x, p2.x, p3.x, x);

        // Считаем кубический Безье
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

        Vector2 p = uuu * p0;            // (1 - t)^3 * p0
        p += 3 * uu * t * p1;            // 3 * (1 - t)^2 * t * p1
        p += 3 * u * tt * p2;            // 3 * (1 - t) * t^2 * p2
        p += ttt * p3;                   // t^3 * p3

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
}
