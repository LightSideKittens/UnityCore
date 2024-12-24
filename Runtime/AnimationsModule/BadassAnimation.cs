#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using LSCore.Editor;
using UnityEngine;
using UnityEditor;

public class BadassAnimation : EditorWindow
{
    [SerializeField] [Range(0, 1)] private float time = 0.5f;
    public BezierPointList points;
    private static float constWidth = 0.01f;
    private Color curveColor = Color.yellow;
    private Color tangentLineColor = Color.blue;
    private Color keyPointColor = Color.red;
    private Color tangentPointColor = Color.green;
    private Color selectionColor = new Color(0.21f, 0.48f, 1f, 0.6f);
    private Matrix4x4 matrix = Matrix4x4.identity;
    
    public void TryInit()
    {
        points ??= new BezierPointList();
        
        if (points.Count < 6)
        {
            points.Clear();
            points.Add(new Vector2(-0.5f, 0));
            points.Add(new Vector2());
            points.Add(new Vector2(0.5f, 0));
            points.Add(new Vector2(0.5f, 1));
            points.Add(new Vector2(1, 1));
            points.Add(new Vector2(1.5f, 1));
        }
    }
    
    public LSHandles.CameraData camData = new();
    public LSHandles.GridData gridData = new();
    
    [MenuItem(LSPaths.Windows.BadassAnimation)]
    public static void ShowWindow()
    {
        GetWindow<BadassAnimation>();
    }

    private void OnGUI()
    {
        TryInit();
        var rect = position;
        rect.position = Vector2.zero;
        LSHandles.StartMatrix(matrix);
        LSHandles.Begin(rect, camData);

        if (points.Count >= 4)
        {
            var min = points[0];
            var max = points[0];
            
            for (int i = 0; i < points.Count; i++)
            {
                UpdateMinMax(points[i]);
            }

            if (points.loop)
            {
                
            }
            else
            {
                var minPoint = points[1];
                minPoint.x = -10000;
                LSHandles.DrawLine(constWidth, curveColor, points[1], minPoint);
                
                var maxPoint = points[^2];
                maxPoint.x = 10000;
                LSHandles.DrawLine(constWidth, curveColor, points[^2], maxPoint);
            }

            for (int i = 0; i < selectedPointIndexes.Count; i++)
            {
                LSHandles.DrawSolidCircle(points[selectedPointIndexes[i]].e, constWidth * 1.5f, selectionColor);
            }
            
            var st = points[0];
            var sp = points[1];
            LSHandles.DrawLine(constWidth, tangentLineColor, st.e, sp.e);
            LSHandles.DrawSolidCircle(st.e, constWidth, tangentPointColor);
            
            for (int i = 1; i < points.Count - 2; i += 3)
            {
                var startPosition = points[i];
                var startTangent = points[i+1];
                var endTangent = points[i+2];
                var endPosition = points[i+3];
                
                LSHandles.DrawBezier(startPosition.p, startTangent.p, endTangent.p, endPosition.p, curveColor, constWidth);
                
                LSHandles.DrawLine(constWidth, tangentLineColor, startTangent.e, startPosition.e);
                LSHandles.DrawLine(constWidth, tangentLineColor, endTangent.e, endPosition.e);
                
                LSHandles.DrawSolidCircle(startPosition.e, constWidth, keyPointColor);
                LSHandles.DrawSolidCircle(startTangent.e, constWidth, tangentPointColor);
                LSHandles.DrawSolidCircle(endTangent.e, constWidth, tangentPointColor);
            }
            
            var ep = points[^2];
            var et = points[^1];
            LSHandles.DrawLine(constWidth, tangentLineColor, et.e, ep.e);
            LSHandles.DrawSolidCircle(et.e, constWidth, tangentPointColor);
            
            LSHandles.DrawSolidCircle(ep.e, constWidth, keyPointColor);
            ProcessEvents(Event.current);

            var dp = points.EvaluateNormalized(time);
            LSHandles.DrawSolidCircle(dp, constWidth, Color.white);
            
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

        LSHandles.DrawGrid(gridData);
        LSHandles.End(); 
        matrix = LSHandles.EndMatrix();
        
        time = EditorGUILayout.Slider("Time", time, 0, 1);
    }
    
    private readonly int[] workedPointIndexesArr = new int[2];
    private int draggingPointIndex;
    private List<int> selectedPointIndexes = new();
    
    private void ProcessEvents(Event e)
    {
        switch (e.type)
        {
            case EventType.MouseDown:
                var _ = LSHandles.MouseDeltaInWorldPoint;
                if (e.button == 1)
                {
                    var wasClicked = TryGetPointIndex(out var i);
                    
                    if (e.control)
                    {
                        if (wasClicked)
                        {
                            if (IsRoot(i))
                            {
                                points.DeleteKey((i-1) / 3);
                                selectedPointIndexes.Remove(i);
                                selectedPointIndexes.Remove(i-1);
                                selectedPointIndexes.Remove(i+1);
                                GUI.changed = true;
                            }
                        }
                        else
                        {
                            var x = LSHandles.MouseInWorldPoint.x;
                            int leftKeyIndex = points.GetLeftKeyIndexByX(x) + 1;
                            points.InsertKeyByX(x);
                            selectedPointIndexes.Clear();
                            selectedPointIndexes.Add(leftKeyIndex * 3 + 1);
                            GUI.changed = true;
                        }
                    }
                    else if (wasClicked)
                    {
                        var popup = new Popup();
                        HashSet<AlignType> types = new();

                        for (int j = 0; j < selectedPointIndexes.Count; j++)
                        {
                            var point = points[selectedPointIndexes[j]];
                            types.Add(point.alignType);
                        }
                        
                        Action onGui = () =>
                        {
                            popup.DrawFoldout(string.Join(", ", types), () =>
                            {
                                if (popup.DrawButton(AlignType.Aligned.ToString()))
                                {
                                    for (int j = 0; j < selectedPointIndexes.Count; j++)
                                    {
                                        ChangeType(selectedPointIndexes[j], AlignType.Aligned);
                                    }
                                }
                                
                                if (popup.DrawButton(AlignType.Free.ToString()))
                                {
                                    for (int j = 0; j < selectedPointIndexes.Count; j++)
                                    {
                                        ChangeType(selectedPointIndexes[j], AlignType.Free);
                                    }
                                }
                            });
                        };

                        popup.onGui = onGui;
                        PopupWindow.Show(new Rect(e.mousePosition, new Vector2(10, 10)), popup);
                        GUI.changed = true;
                    }
                }
                else if (e.button == 0)
                {
                    GUI.changed = true;
                    
                    var wasClicked = TryGetPointIndex(out var i);
                    
                    if (!e.shift && !(wasClicked && selectedPointIndexes.Contains(i)))
                    {
                        selectedPointIndexes.Clear();
                    }
                        
                    if (wasClicked)
                    {
                        draggingPointIndex = i;
                        if (!selectedPointIndexes.Contains(i))
                        {
                            selectedPointIndexes.Add(i);
                        }
                    }
                }
                break;

            case EventType.MouseUp:
                draggingPointIndex = -1;
                break;

            case EventType.MouseDrag:
                if (draggingPointIndex != -1 && e.button == 0)
                {
                    var dt = LSHandles.MouseDeltaInWorldPoint;
                    var isRoot = IsRoot(draggingPointIndex);
                    
                    for (int i = 0; i < selectedPointIndexes.Count; i++)
                    {
                        var targetIndex = selectedPointIndexes[i];
                        
                        if (isRoot)
                        {
                            if (IsRoot(targetIndex))
                            {
                                var ind = targetIndex;
                                MoveAsAnimation(ref ind, dt, true);
                                selectedPointIndexes[i] = ind;
                                
                                for (int j = -1; j < 2; j+=2)
                                {
                                     ind = targetIndex + j;
                                    MoveAsAnimation(ref ind, dt, true);
                                }
                            }
                        }
                        else
                        {
                            if (IsTangent(targetIndex))
                            {
                                var ind = targetIndex;
                                MoveAsAnimation(ref ind, dt, false);
                                selectedPointIndexes[i] = ind;
                            }
                        }
                    }
        
                    GUI.changed = true;
                }
                break;
        }

        bool TryGetPointIndex(out int index)
        {
            for (int i = 0; i < points.Count; i++)
            {
                if (IsPointClicked(points[i], LSHandles.MouseInWorldPoint, 0.1f))
                {
                    index = i;
                    return true;
                }
            }

            index = -1;
            return false;
        }
    }
    
    private void MoveAsAnimation(ref int i, Vector2 delta, bool isRootMoving)
    {
        var point = points[i];
        points[i] = point.ePlus(delta);
        
        if (ClampTangent(i))
        {
            if(isRootMoving) return;
            
            int f1;
            int f2;
            
            if (IsForwardTangent(i))
            {
                f1 = -1;
                f2 = -2;
            }
            else
            {
                f1 = 1;
                f2 = 2;
            }
            
            var root = points[i + f1];
            var tangent = points[i + f2];
            var dir = (point.e - root.p).normalized;
                
            if (root.alignType == AlignType.Aligned)
            {
                var dis = (tangent.e - root.p).magnitude;
                tangent.eSet(root.p + -dir * dis);
                points[i + f2] = tangent;
                ClampTangent(i + f2);
            }
        }
        else
        {
            TrySwapNext(ref i);
            TrySwapPrev(ref i);
            ClampTangent(i + 1);
            ClampTangent(i - 1);

            if (i == 1)
            {
                ClampTangent(i + 2);
            }
            else if(i == points.Count - 2)
            {
                ClampTangent(i - 2);
            }
            else
            {
                ClampTangent(i + 2);
                ClampTangent(i - 2);
            }

            return;
            
            void TrySwapNext(ref int ii)
            {
                if(ii + 3 > points.Count - 1) return;
                
                var next = points[ii + 3];
                if (point.x > next.x)
                {
                    SwapKeys(ref ii, ii + 3);
                }
            }

            void TrySwapPrev(ref int ii)
            {
                if(ii - 3 < 0) return;
                
                var prev = points[ii - 3];
                if (point.x < prev.x)
                {
                    SwapKeys(ref ii, ii - 3);
                }
            }
        }
    }

    private bool ClampTangent(int i)
    {
        if (IsTangent(i))
        {
            if (IsForwardTangent(i))
            {
                var inf = Vector2.positiveInfinity;
                var root = points[i - 1];
                var nextRoot = i < (points.Count-1) ? points[i + 2] : (BezierPoint)inf;
                var pos = points[i];
                pos.x = Mathf.Clamp(pos.ex, root.x, nextRoot.x);
                pos.ex = Mathf.Clamp(pos.ex, root.x, inf.x);
                points[i] = pos;
            }
            else
            {
                var inf = Vector2.negativeInfinity;
                var root = points[i + 1];
                var prevRoot = i > 0 ? points[i - 2] : (BezierPoint)inf;
                var pos = points[i];
                pos.x = Mathf.Clamp(pos.ex, prevRoot.x, root.x);
                pos.ex = Mathf.Clamp(pos.ex, inf.x, root.x);
                points[i] = pos;
            }

            return true;
        }

        return false;
    }

    private void ChangeType(int i, AlignType alignType)
    {
        int f1;
        int f2;
        
        if (IsRoot(i))
        {
            f1 = -1;
            f2 = 1;
        }
        else if(IsForwardTangent(i))
        {
            f1 = -1;
            f2 = -2;
        }
        else
        {
            f1 = 1;
            f2 = 2;
        }

        points.SetAlign(i, alignType);
        points.SetAlign(i + f1, alignType);
        points.SetAlign(i + f2, alignType);

        if (alignType == AlignType.Aligned)
        {
            var a = i;
            var zero = Vector2.zero;
            MoveAsAnimation(ref a, zero, true);
            a = i + f1;
            MoveAsAnimation(ref a, zero, false);
            a = i + f2;
            MoveAsAnimation(ref a, zero, false);
        }
    }
    
    private int[] GetTangentIndexes(int i)
    {
        if (IsTangent(i)) return Array.Empty<int>();
        
        workedPointIndexesArr[0] = i - 1;
        workedPointIndexesArr[1] = i + 1;
        return workedPointIndexesArr[..];
    }
    
    private bool IsRoot(int i)
    {
        i--;
        return i % 3 == 0;
    }
    
    private bool IsTangent(int i)
    {
        var f = i % 3;
        return f is 0 or 2;
    }
    
    private bool IsForwardTangent(int i)
    {
        return i % 3 == 2;
    }
    
    private bool IsBackwardTangent(int i)
    {
        return i % 3 == 0;
    }
    
    private bool IsPointClicked(BezierPoint point, Vector2 worldMousePos, float distance, bool dependsOnCamera = true)
    {
        if (dependsOnCamera)
        {
            distance *= LSHandles.CamSize / 3;
        }
        return Vector2.Distance(point.e, worldMousePos) <= distance;
    }

    public void SwapKeys(ref int root1, int root2)
    {
        if(IsTangent(root1) || IsTangent(root2)) return;

        var root1Tangents = GetTangentIndexes(root1);
        var root2Tangents = GetTangentIndexes(root2);
        
        (points[root1], points[root2]) = (points[root2], points[root1]);
        (root1, root2) = (root2, root1);
        
        for (int i = 0; i < 2; i++)
        {
            (points[root1Tangents[i]], points[root2Tangents[i]]) = (points[root2Tangents[i]], points[root1Tangents[i]]);
        }
    }

    
    
    public class Popup : PopupWindowContent
    {
        public Action onGui;
        
        public override void OnGUI(Rect rect)
        {
            onGui();
        }
        
        public void DrawFoldout(string name, Action gui, bool show = false)
        {
            EditorUtils.DrawInBoxFoldout(new GUIContent(name), gui, "BadassAnimation", show);
        }

        public bool DrawButton(string name)
        {
            if (GUILayout.Button(name, GUILayout.MaxWidth(200)))
            {
                editorWindow.Close();
                return true;
            }

            return false;
        }
    }

}


#endif