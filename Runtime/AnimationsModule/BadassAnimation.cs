#if UNITY_EDITOR
using System;
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
        
        if (points.Count < 4)
        {
            points.Clear();
            points.Add(new Vector2());
            points.Add(new Vector2(0.5f, 0));
            points.Add(new Vector2(0.5f, 1));
            points.Add(new Vector2(1, 1));
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
                var minPoint = points[0];
                minPoint.x = -10000;
                LSHandles.DrawLine(constWidth, curveColor, points[0], minPoint);
                
                var maxPoint = points[^1];
                maxPoint.x = 10000;
                LSHandles.DrawLine(constWidth, curveColor, points[^1], maxPoint);
            }
            
            for (int i = 0; i < points.Count - 1; i += 3)
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
            
            LSHandles.DrawSolidCircle(points[selectedPointIndex].e, constWidth * 1.5f, selectionColor);
            LSHandles.DrawSolidCircle(points[^1].e, constWidth, keyPointColor);
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
    private int[] tangentIndexes;
    private int draggingPointIndex;
    private int selectedPointIndex;
    
    private void ProcessEvents(Event e)
    {
        switch (e.type)
        {
            case EventType.MouseDown:
                var _ = LSHandles.MouseDeltaInWorldPoint;
                if (e.button == 1)
                {
                    for (int i = 0; i < points.Count; i++)
                    {
                        if (IsPointClicked(points[i], LSHandles.MouseInWorldPoint, 0.1f))
                        {
                            selectedPointIndex = i;
                            var popup = new Popup();
                            PopupWindow.Show(new Rect(e.mousePosition, new Vector2(200, 0)), popup);

                            Action onGui = () =>
                            {
                                if (popup.DrawButton("Codependents"))
                                {
                                    
                                }
                            };

                            popup.onGui = onGui;
                            GUI.changed = true;
                            break;
                        }
                    }
                }
                else if (e.button == 0)
                {
                    GUI.changed = true;
                    for (int i = 0; i < points.Count; i++)
                    {
                        if (IsPointClicked(points[i], LSHandles.MouseInWorldPoint, 0.1f))
                        {
                            if ((e.modifiers & EventModifiers.Alt) != 0)
                            {
                                if (i % 3 == 0)
                                {
                                    points.DeleteKey(i / 3);
                                    return;
                                }
                            }
                            
                            draggingPointIndex = i;
                            selectedPointIndex = i;

                            tangentIndexes = GetTangentIndexes(i);
                            break;
                        }
                    }
                    
                    if ((e.modifiers & EventModifiers.Alt) != 0)
                    {
                        points.InsertKeyByX(LSHandles.MouseInWorldPoint.x);
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
                    
                    MoveAsAnimation(ref draggingPointIndex, dt);
                    
                    for (int i = 0; i < tangentIndexes.Length; i++)
                    {
                        MoveAsAnimation(ref tangentIndexes[i], dt);
                    }
        
                    GUI.changed = true;
                }
                break;
        }
    }
    
    private void MoveAsAnimation(ref int i, Vector2 delta)
    {
        points[i] = points[i].ePlus(delta);
        if (!ClampTangent(i))
        {
            var root = points[i];
            if (i == 0)
            {
                TrySwapNext(ref i);
                ClampTangent(2);
            }
            else if(i == points.Count - 1)
            {
                TrySwapPrev(ref i);
                ClampTangent(i - 2);
            }
            else
            {
                TrySwapNext(ref i);
                TrySwapPrev(ref i);
                ClampTangent(i + 2);
                ClampTangent(i - 2);
            }

            return;
            
            void TrySwapNext(ref int ii)
            {
                var next = points[ii + 3];
                if (root.x > next.x)
                {
                    SwapKeys(ref ii, ii + 3);
                }
            }

            void TrySwapPrev(ref int ii)
            {
                var prev = points[ii - 3];
                if (root.x < prev.x)
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
                var root = points[i - 1];
                var nextRoot = points[i + 2];
                var pos = points[i];
                pos.x = Mathf.Clamp(pos.x, root.x, nextRoot.x);
                points[i] = pos;
            }
            else
            {
                var root = points[i + 1];
                var prevRoot = points[i - 2];
                var pos = points[i];
                pos.x = Mathf.Clamp(pos.x, prevRoot.x, root.x);
                points[i] = pos;
            }

            return true;
        }

        return false;
    }
    
    private int[] GetTangentIndexes(int i)
    {
        if (IsTangent(i)) return Array.Empty<int>();
        
        if (i == 0)
        {
            workedPointIndexesArr[0] = 1;
            return workedPointIndexesArr[..1];
        }
        
        if(i == points.Count - 1)
        {
            workedPointIndexesArr[0] = i - 1;
            return workedPointIndexesArr[..1];
        }
        
        workedPointIndexesArr[0] = i + 1;
        workedPointIndexesArr[1] = i - 1;
        return workedPointIndexesArr[..2];
    }
    
    private bool IsTangent(int i)
    {
        return !(i == 0 || i == points.Count - 1 || i % 3 == 0);
    }
    
    private bool IsForwardTangent(int i)
    {
        return i % 3 == 1;
    }
    
    private bool IsBackwardTangent(int i)
    {
        return i % 3 == 2;
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
        tangentIndexes = root2Tangents;
        (points[root1], points[root2]) = (points[root2], points[root1]);
        (root1, root2) = (root2, root1);
    }

    
    
    public class Popup : PopupWindowContent
    {
        public Action onGui;
        
        public override void OnGUI(Rect rect)
        {
            onGui();
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