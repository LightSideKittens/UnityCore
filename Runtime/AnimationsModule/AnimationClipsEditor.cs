using System;
using System.Collections.Generic;
using DG.DemiEditor;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

public class AnimationClipsEditor : OdinEditorWindow
{
    [SerializeField] private string propertyName;
    [SerializeField] private AnimationClip[] clips;
    [SerializeField] [Range(0, 1)] private float time = 0.5f;
    [SerializeField] private Vector2 scale = new Vector2();
    [SerializeField] private float gridSize = 10.0f;
    [SerializeField] private float gridOpacity = 0.2f;
    [SerializeField] private Vector2 xRange = new Vector2(-1000, 1000);
    [SerializeField] private Vector2 yRange = new Vector2(-1000, 1000);
    private Vector2 scrollPosition;
    private Vector2 anchorPoint;
    private Rect bounds;

    [MenuItem(LSPaths.Windows.AnimationClipsEditor)]
    private static void OpenWindow()
    {
        GetWindow<AnimationClipsEditor>().Show();
    }
    
    protected override void DrawEditor(int index)
    {
        base.DrawEditor(index);
        showFoldout = EditorGUILayout.Foldout(showFoldout, "Foldout Title", true);
        if(!showFoldout) return;
        TryInit();
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        
        Handles.BeginGUI();
        Color curveColor = Color.red;
        Handles.color = Color.white;
        
        if (points.Count >= 4)
        {
            var min = points[0];
            var max = points[0];
            
            for (int i = 0; i < points.Count; i++)
            {
                UpdateMinMax(points[i]);
            }

            bounds.min = min * scale;
            bounds.max = max * scale;
            DrawGrid();
            wh = bounds.height;
            for (int i = 0; i < points.Count - 1; i += 3)
            {
                var startPosition = GetPoint(i);
                var startTangent = GetPoint(i+1);
                var endTangent = GetPoint(i+2);
                var endPosition = GetPoint(i+3);
                
                Handles.DrawBezier(startPosition, endPosition, startTangent, endTangent, curveColor, null, 4);
                Handles.DrawAAPolyLine(2, startTangent, startPosition);
                Handles.DrawAAPolyLine(2, endTangent, endPosition);

                DrawKey(startPosition);
                DrawTangent(startTangent);
                DrawTangent(endTangent);
            }
            
            DrawKey(GetPoint(^1));
            ProcessEvents(Event.current); 
            ProcessEventsScale(Event.current);

            var dp = Evaluate1(time) * scale + anchorPoint;
            dp.y = wh - dp.y;
            Handles.DrawSolidDisc(dp, Vector3.forward, 6);
            Handles.EndGUI();
            
            if (GUI.changed)
            {
                Repaint();
            }
            
            void UpdateMinMax(in Vector2 vector)
            {
                if (vector.x < min.x) min.x = vector.x;
                if (vector.y < min.y) min.y = vector.y;
                if (vector.x > max.x) max.x = vector.x;
                if (vector.y > max.y) max.y = vector.y;
            }
        }
        
        
        EditorGUILayout.EndScrollView();
    }
    
    private void DrawGrid()
    {
        var box = bounds;
        var offset = scale / 2;
        box.size += offset * 2;
        GUILayout.Box("", GUILayout.Width(box.width), GUILayout.Height(box.height));
        boxRect = GUILayoutUtility.GetLastRect();
        anchorPoint = boxRect.position;
        anchorPoint.y -= offset.y + 8;
        anchorPoint.x += offset.x;
        int widthDivs = Mathf.CeilToInt(boxRect.width / scale.x);
        int heightDivs = Mathf.CeilToInt(boxRect.height / scale.y);
        var startPoint = new Vector2(boxRect.xMin, boxRect.yMax) + offset;
        
        Handles.color = new Color(0.5f, 0.5f, 0.5f, gridOpacity);

        for (int i = 0; i < widthDivs; i++)
        {
            Handles.DrawLine(new Vector3(startPoint.x, boxRect.yMin, 0), new Vector3(startPoint.x, boxRect.yMax, 0));
            startPoint.x += scale.x;
        }

        for (int j = 0; j < heightDivs; j++)
        {
            startPoint.y -= scale.y;
            Handles.DrawLine(new Vector3(boxRect.xMin, startPoint.y, 0), new Vector3(boxRect.xMax, startPoint.y, 0));
        }
        
        Handles.color = Color.white;
    }

    private void DrawKey(Vector2 pos)
    {
        Handles.color = Color.yellow;
        Handles.DrawSolidDisc(pos, Vector3.forward, 5);
        Handles.color = Color.white;
    }
    
    private void DrawTangent(Vector2 pos)
    {
        Handles.color = Color.blue;
        Handles.DrawSolidDisc(pos, Vector3.forward, 5);
        Handles.color = Color.white;
    }
    
    private int draggingPointIndex = -1;
    private List<int> pointIndexes = new();
    private List<List<int>> interdependentIndexes = new();

    private Vector2 GetPoint(Index index)
    {
        var p = points[index] * scale + anchorPoint;
        p.y = wh - p.y;
        return p;
    }
    
    private Vector2 GetPoint(int index)
    {
        var p = points[index] * scale + anchorPoint;
        p.y = wh - p.y;
        return p;
    }
    
    private void ProcessEvents(Event e)
    {
        switch (e.type)
        {
            case EventType.MouseDown:
                if (e.button == 0) // Левая кнопка мыши
                {
                    pointIndexes.Clear();
                    for (int i = 0; i < points.Count; i++)
                    {
                        if (IsPointClicked(GetPoint(i), e.mousePosition, 10f))
                        {
                            if ((e.modifiers & EventModifiers.Alt) != 0)
                            {
                                if (i % 3 == 0)
                                {
                                    DeleteKey(i / 3);
                                    return;
                                }
                            }
                            
                            draggingPointIndex = i; // Начинаем перетаскивание точки
                            if (i == 0)
                            {
                                pointIndexes.Add(1);
                            }
                            else if(i == points.Count - 1)
                            {
                                pointIndexes.Add(i - 1);
                            }
                            else
                            {
                                if (i % 3 == 0)
                                {
                                    pointIndexes.Add(i + 1);
                                    pointIndexes.Add(i - 1);
                                }
                            }
                            
                            GUI.changed = true;
                            break;
                        }
                    }
                    
                    if ((e.modifiers & EventModifiers.Alt) != 0)
                    {
                        InsertKeyByX(((e.mousePosition - anchorPoint) / scale).x);
                    }
                }
                break;

            case EventType.MouseUp:
                draggingPointIndex = -1; // Окончание перетаскивания
                break;

            case EventType.MouseDrag:
                if (draggingPointIndex != -1)
                {
                    var dt = e.delta / scale;
                    dt.y *= -1;
                    points[draggingPointIndex] += dt; // Перемещаем выбранную точку
                    for (int i = 0; i < pointIndexes.Count; i++)
                    {
                        points[pointIndexes[i]] += dt;
                    }
        
                    GUI.changed = true;
                }
                break;
        }
    }
    private void ProcessEventsScale(Event e)
    {
        if (boxRect.Contains(e.mousePosition))
        {
            switch (e.type)
            {
                case EventType.ScrollWheel:
                    var wdt = e.delta * -Mathf.InverseLerp(0, 100000, scale.sqrMagnitude) * 10;
                    if (e.control)
                    {
                        scale.x += wdt.y; 
                    }
                    else if(e.alt)
                    {
                        scale.y += wdt.y;
                    }
                    else
                    {
                        scale.x += wdt.y; 
                        scale.y += wdt.y;
                    }
                   
                    break;
            }

            GUI.changed = true;
        }
    }

    // Проверка, кликнул ли пользователь в пределах точки
    private bool IsPointClicked(Vector2 point, Vector2 mousePosition, float distance)
    {
        return Vector2.Distance(point, mousePosition) <= distance;
    }
    
    public List<Vector2> points;
    private bool showFoldout;
    private float wh;
    private Rect boxRect;

    public void TryInit()
    {
        if (points.Count < 4)
        {
            points.Clear();
            points.Add(new Vector2());
            points.Add(new Vector2(0.5f, 0));
            points.Add(new Vector2(0.5f, 1));
            points.Add(new Vector2(1, 1));
        }
    }

    // Вычисление точки на объединённой кривой
    public Vector2 Evaluate1(float t)
    {
        float xTarget = Mathf.Lerp(points[0].x, points[^1].x, t);
        int curveIndex = GetLeftKeyIndexByX(xTarget);
        int index = curveIndex * 3;
        Vector2 p0 = points[index];
        Vector2 p1 = points[index + 1];
        Vector2 p2 = points[index + 2];
        Vector2 p3 = points[index + 3];
        
        float tForX = FindBezierTForX(p0.x, p1.x, p2.x, p3.x, xTarget);
        return EvaluateCubicBezier(p0, p1, p2, p3, tForX); // Вычисление точки на текущей кривой
    }

    private void InsertKeyByX(float x)
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

    [Button]
    public void DeleteKey(int index)
    {
        index *= 3;
        
        if (index == 0)
        {
            points.RemoveAt(index);
            points.RemoveAt(index);
            points.RemoveAt(index);
        }
        else if(index == points.Count - 1)
        {
            points.RemoveAt(points.Count - 1);
            points.RemoveAt(points.Count - 1);
            points.RemoveAt(points.Count - 1);
        }
        else
        {
            index--;
            points.RemoveAt(index);
            points.RemoveAt(index);
            points.RemoveAt(index);
        }
    }

    [Button]
    public void InsertKey(int index, float t)
    {
        // Вычисляем промежуточные точки на основе t
        var i = index * 3;
        if (i >= points.Count - 1)
        {
            return;
        }
        Vector2 p0 = points[i];
        Vector2 p1 = points[i + 1];
        Vector2 p2 = points[i + 2];
        Vector2 p3 = points[i + 3];

        Vector2 q0 = Vector2.Lerp(p0, p1, t);
        Vector2 q1 = Vector2.Lerp(p1, p2, t);
        Vector2 q2 = Vector2.Lerp(p2, p3, t);

        Vector2 r0 = Vector2.Lerp(q0, q1, t);
        Vector2 r1 = Vector2.Lerp(q1, q2, t);
        Vector2 s = Vector2.Lerp(r0, r1, t);

        // Создаем две новые кривые
        points[i] = p0;
        points[i + 1] = q0;
        points[i + 2] = r0;
        points[i + 3] = s;
        points.Insert(i + 4, r1);
        points.Insert(i + 5, q2);
        points.Insert(i + 6, p3);
    }
    
    private Vector2 EvaluateCubicBezier(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;
        float uuu = uu * u;
        float ttt = tt * t;

        Vector2 p = uuu * p0; // Начальная точка умножается на u^3
        p += 3 * uu * t * p1; // Первая контрольная точка умножается на 3 * u^2 * t
        p += 3 * u * tt * p2; // Вторая контрольная точка умножается на 3 * u * t^2
        p += ttt * p3; // Конечная точка умножается на t^3

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
    
    
    
    
    
    [Button]
    private void Apply()
    {
        foreach (var clip in clips)
        {
            foreach (var binding in AnimationUtility.GetCurveBindings(clip))
            {
                Debug.Log(binding.path);
                Debug.Log(binding.propertyName);
                /*if (binding.propertyName.StartsWith("m_LocalPosition."))
                {
                    AnimationCurve curve = AnimationUtility.GetEditorCurve(clip, binding);
                    for (int i = 0; i < curve.keys.Length; i++)
                    {
                        Keyframe key = curve.keys[i];
                        key.value /= 32f;
                        curve.keys[i] = key;
                    }
                    AnimationUtility.SetEditorCurve(clip, binding, curve);
                }*/
            }
        }
    }
}
