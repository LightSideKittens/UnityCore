#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using LSCore.Editor;
using Sirenix.Utilities;
using UnityEngine;
using UnityEditor;
using UnityEngine.Animations;

public partial class BadassAnimation : EditorWindow
{
    public BadassAnimationCurve points = new();
    [SerializeField] [Range(0, 1)] private float time = 0.5f;
    private const float constWidth = 0.05f / 3;
    private const float bezierWidth = constWidth / 5;
    private const float tangentWidth = constWidth / 10;
    
    private Color curveColor = new (1f, 0.32f, 0.36f);
    private Color tangentLineColor = new (1f, 0.32f, 0.36f);
    private Color keyPointColor = Color.black;
    private Color tangentPointColor = Color.black;
    private Color selectionColor = new Color(1f, 0.54f, 0.16f);
    private Color lastSelectionColor = Color.white;
    private Matrix4x4 matrix = Matrix4x4.identity;
    
    public LSHandles.CameraData camData = new();
    public LSHandles.GridData gridData = new();
    
    [MenuItem(LSPaths.Windows.BadassAnimation)]
    public static void ShowWindow()
    {
        var window = GetWindow<BadassAnimation>();
        window.gridData.displayScale = true;
    }

    private void OnGUI()
    {
        GUI.SetNextControlName("InvisibleFocus");
        GUI.FocusControl("InvisibleFocus");
        
        var rect = position;
        rect.position = Vector2.zero;
        rect.TakeFromTop(50);
        rect.TakeFromLeft(20);
        rect.TakeFromRight(35);
        rect.TakeFromBottom(73);
        
        LSHandles.StartMatrix(matrix);
        LSHandles.Begin(rect, camData);
        LSHandles.DrawGrid(gridData);
        
        
        if (points.Count > 2)
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
                minPoint.x = -100000;
                LSHandles.DrawLine(bezierWidth, curveColor, points[1], minPoint);
                
                var maxPoint = points[^2];
                maxPoint.x = 100000;
                LSHandles.DrawLine(bezierWidth, curveColor, points[^2], maxPoint);
            }
            
            var st = points[0];
            var sp = points[1];
            LSHandles.DrawLine(tangentWidth, tangentLineColor, st.e, sp.e);
            LSHandles.DrawCircle(st.e, constWidth, tangentPointColor);
            
            for (int i = 1; i < points.Count - 2; i += 3)
            {
                var startPosition = points[i];
                var startTangent = points[i+1];
                var endTangent = points[i+2];
                var endPosition = points[i+3];
                
                LSHandles.DrawBezier(startPosition.p, startTangent.p, endTangent.p, endPosition.p, curveColor, bezierWidth);
                
                LSHandles.DrawLine(tangentWidth, tangentLineColor, startTangent.e, startPosition.e);
                LSHandles.DrawLine(tangentWidth, tangentLineColor, endTangent.e, endPosition.e);
                
                LSHandles.DrawCircle(startPosition.e, constWidth, keyPointColor);
                LSHandles.DrawRing(startTangent.e, constWidth, tangentPointColor);
                LSHandles.DrawRing(endTangent.e, constWidth, tangentPointColor);
            }
            
            var ep = points[^2];
            var et = points[^1];
            LSHandles.DrawLine(tangentWidth, tangentLineColor, et.e, ep.e);
            LSHandles.DrawCircle(et.e, constWidth, tangentPointColor);
            
            LSHandles.DrawCircle(ep.e, constWidth, keyPointColor);
            
            for (int i = 0; i < selectedPointIndexes.Count; i++)
            {
                var target = selectedPointIndexes[i];
                if (IsRoot(target))
                {
                    LSHandles.DrawCircle(points[target].e, constWidth, selectionColor);
                }
                else
                {
                    LSHandles.DrawRing(points[target].e, constWidth, selectionColor);
                }
            }

            var dp = points.EvaluateNormalized(time);
            LSHandles.DrawCircle(dp, constWidth, Color.white);
            LSHandles.SelectRect.Draw();
            
            void UpdateMinMax(in Vector2 vector)
            {
                if (vector.x < min.x) min.x = vector.x;
                if (vector.y < min.y) min.y = vector.y;
                if (vector.x > max.x) max.x = vector.x;
                if (vector.y > max.y) max.y = vector.y;
            }
        }
        
        LSHandles.End(); 
        matrix = LSHandles.EndMatrix();
        
        ProcessEvents(Event.current);
        
        if (GUI.changed)
        {
            Repaint();
        }
    }
    
    private readonly int[] workedPointIndexesArr = new int[2];
    private int draggingPointIndex;
    [SerializeField] private List<int> selectedPointIndexes = new();

    public bool TrySelectPoint(int pointIndex)
    {
        RecordSelect();
        if (!selectedPointIndexes.Contains(pointIndex))
        {
            selectedPointIndexes.Add(pointIndex);
            return true;
        }
        
        return false;
    }
    
    public bool SelectPointOrRemove(int pointIndex)
    {
        if (!TrySelectPoint(pointIndex))
        {
            selectedPointIndexes.Remove(pointIndex);
            return false;
        }

        return true;
    }

    private IEnumerable<BezierPoint> SelectedPoints
    {
        get
        {
            for (int i = 0; i < selectedPointIndexes.Count; i++)
            {
                yield return points[selectedPointIndexes[i]];
            }
        }
    }
    
    private IEnumerable<Vector2> eSelectedPoints
    {
        get
        {
            foreach (var point in SelectedPoints)
            {
                yield return point.e;
            }
        }
    }
    
    private IEnumerable<Vector2> pSelectedPoints
    {
        get
        {
            foreach (var point in SelectedPoints)
            {
                yield return point.p;
            }
        }
    }
    
    private Action<Event> eventHandler;
    private Action<Event> applyEventHandler;
    private Action<Event> discardEventHandler;
    private LSHandles.TransformationMode currentTransformationMode;
    private Axis currentAxis = Axis.None;
    private Vector2 lastMousePosition;
    private Vector2 startMousePosition;
    private bool isRecorded;
    private BadassAnimationCurve copyPoints;
    private List<int> copySelectedPointIndexes;
    
    private void ProcessEvents(Event e)
    {
        bool isMouseMove = lastMousePosition != e.mousePosition;
        
        if (isMouseMove)
        {
            lastMousePosition = e.mousePosition;
        }
        
        if (applyEventHandler != null)
        {
            GUI.changed = true;
            if (isMouseMove)
            {
                eventHandler(e);
            }
            
            switch (e.type)
            {
                case EventType.MouseDown:
                    applyEventHandler(e);
                    StopEventHandling();
                    break;
                case EventType.KeyDown:
                    TrySetupEventHandler();
                    switch (e.keyCode)
                    {
                        case KeyCode.Escape:
                            discardEventHandler(e);
                            StopEventHandling();
                            break;
                        case KeyCode.X:
                            SetAxis(Axis.X);
                            SetupEventHandler();
                            break;
                        case KeyCode.Y:
                            SetAxis(Axis.Y);
                            SetupEventHandler();
                            break;
                    }
                    break;
            }

            return;
        }
        
        switch (e.type)
        {
            case EventType.KeyDown:
                TrySetupEventHandler();
                if (e.keyCode == KeyCode.A)
                {
                    RecordSelect();
                    selectedPointIndexes.Clear();
                    for (int i = 0; i < points.Count; i++)
                    {
                        selectedPointIndexes.Add(i);
                    }
                    GUI.changed = true;
                }
                else if(e.keyCode == KeyCode.Delete)
                {
                    for (int i = 0; i < selectedPointIndexes.Count; i++)
                    {
                        var removed = TryDeleteRoot(ref i, selectedPointIndexes[i]);

                        for (int j = 0; j < selectedPointIndexes.Count; j++)
                        {
                            var targetPointIndex = selectedPointIndexes[j];
                            foreach (var pointIndex in removed)
                            {
                                if (targetPointIndex > pointIndex)
                                {
                                    if (IsRoot(pointIndex))
                                    {
                                        selectedPointIndexes[j] -= 3;
                                    }
                                }
                            }
                        }
                        
                    }
                }
                break;
            case EventType.MouseDown:
                if (e.button == 1)
                {
                    var wasClicked = TryGetPointIndex(out var i);
                    
                    if (e.control)
                    {
                        if (wasClicked)
                        {
                            int a = 0;
                            TryDeleteRoot(ref a, i);
                        }
                        else
                        {
                            InsertKey();
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
                    
                    if (selectedPointIndexes.Count > 0 
                        && !e.shift 
                        && !(wasClicked && selectedPointIndexes.Contains(i)))
                    {
                        RecordSelect();
                        selectedPointIndexes.Clear();
                    }
                    
                    if (wasClicked)
                    {
                        startMousePosition = LSHandles.MouseInWorldPoint;
                        draggingPointIndex = i;
                        
                        if (e.shift)
                        {
                            SelectPointOrRemove(i);
                        }
                        else
                        {
                            TrySelectPoint(i);
                        }
                        
                        copyPoints = new BadassAnimationCurve(points);
                        copySelectedPointIndexes = new List<int>(selectedPointIndexes);
                    }
                    else
                    {
                        LSHandles.SelectRect.Start();
                    }
                }
                break;

            case EventType.MouseUp:
                var rect = LSHandles.SelectRect.End();
                SelectPointsByRect(rect);

                draggingPointIndex = -1;
                GUI.changed = true;
                isRecorded = false;
                break;

            case EventType.MouseDrag:
                if (draggingPointIndex != -1 && e.button == 0)
                {
                    var isRoot = IsRoot(draggingPointIndex);
                    
                    for (int i = 0; i < selectedPointIndexes.Count; i++)
                    {
                        var targetIndex = selectedPointIndexes[i];
                        var copyTargetIndex = copySelectedPointIndexes[i];
                        
                        if (isRoot)
                        {
                            if (IsRoot(targetIndex))
                            {
                                int ind;
                                int copyind;
                                
                                for (int j = -1; j < 2; j+=2)
                                {
                                    ind = targetIndex + j;
                                    copyind = copyTargetIndex + j;
                                    SetPosAsAnimationByMouseDrag(ref ind, copyind, true);
                                }
                                
                                ind = targetIndex;
                                copyind = copyTargetIndex;
                                SetPosAsAnimationByMouseDrag(ref ind, copyind, true);
                                selectedPointIndexes[i] = ind;
                            }
                        }
                        else
                        {
                            if (IsTangent(targetIndex))
                            {
                                var ind = targetIndex;
                                var copyind = copyTargetIndex;
                                SetPosAsAnimationByMouseDrag(ref ind, copyind, false);
                                selectedPointIndexes[i] = ind;
                            }
                        }
                    }
        
                    GUI.changed = true;
                }
                break;
        }

        void SetPosAsAnimationByMouseDrag(ref int ind, int copyi, bool isRoot)
        {
            var delta = LSHandles.MouseInWorldPoint - startMousePosition;
            var point = copyPoints[copyi].e + delta;
            SetPosAsAnimation(ref ind, point, isRoot);
        }
        
        bool TryGetPointIndex(out int index)
        {
            for (int i = 0; i < points.Count; i++)
            {
                if (IsInDistance(points[i], LSHandles.MouseInWorldPoint, constWidth))
                {
                    index = i;
                    return true;
                }
            }

            index = -1;
            return false;
        }

        void SelectPointsByRect(Rect rect)
        {
            for (int i = 0; i < points.Count; i++)
            {
                if (rect.Contains(points[i].e))
                {
                    TrySelectPoint(i);
                }
            }
        }

        void TrySetupEventHandler()
        {
            if (selectedPointIndexes.Count > 0)
            {
                if (e.keyCode == KeyCode.G)
                {
                    SetupEventHandlerWithMode(LSHandles.TransformationMode.Translate);
                }
                else if (e.keyCode == KeyCode.S)
                {
                    SetupEventHandlerWithMode(LSHandles.TransformationMode.Scale);
                }
                else if (e.keyCode == KeyCode.R)
                {
                    SetupEventHandlerWithMode(LSHandles.TransformationMode.Rotate);
                }
            }
        }

        void SetupEventHandlerWithMode(LSHandles.TransformationMode mode)
        {
            currentTransformationMode = mode;
            SetupEventHandler();
        }

        void SetupEventHandler()
        {
            discardEventHandler?.Invoke(e);
            var last = points;
            var copy = new BadassAnimationCurve(points);
            points = copy;
            
            var lastSelectedPointIndexesCopy = selectedPointIndexes;
            var selectedPointIndexesCopy = new List<int>(selectedPointIndexes);
            selectedPointIndexes = selectedPointIndexesCopy;

            var bounds = LSHandles.CalculateBounds(eSelectedPoints);
            var startPos = LSHandles.MouseInWorldPoint;
                    
            applyEventHandler = _ => applyEventHandler = null;
            eventHandler = ev =>
            {
                var curPos = LSHandles.MouseInWorldPoint;
                var transformation = LSHandles.GetMatrix(bounds, startPos, curPos, currentTransformationMode, currentAxis);
                
                for (int i = 0; i < selectedPointIndexes.Count; i++)
                {
                    var targetIndex = selectedPointIndexes[i];
                    var lastTargetIndex = lastSelectedPointIndexesCopy[i];
                        
                    if (IsRoot(targetIndex))
                    {
                        int ind;

                        Vector3 point;
                        
                        for (int j = -1; j < 2; j+=2)
                        {
                            ind = targetIndex + j;
                            point = transformation.MultiplyPoint(last[lastTargetIndex + j].e);
                            SetPosAsAnimation(ref ind, point, true);
                        }
                                
                        ind = targetIndex;
                        point = transformation.MultiplyPoint(last[lastTargetIndex].e);
                        SetPosAsAnimation(ref ind, point, true);
                        selectedPointIndexes[i] = ind;
                    }
                    else
                    {
                        var ind = targetIndex;
                        var point = transformation.MultiplyPoint(last[lastTargetIndex].e);
                        SetPosAsAnimation(ref ind, point, false);
                    }
                }
            };
            discardEventHandler = _ =>
            {
                points = last;
                selectedPointIndexes = lastSelectedPointIndexesCopy;
            };
        }

        void SetAxis(Axis axis)
        {
            currentAxis = currentAxis == axis ? Axis.None : axis;
        }

        void StopEventHandling()
        {
            isRecorded = false;
            currentAxis = Axis.None;
            eventHandler = null;
            discardEventHandler = null;
            applyEventHandler = null;
        }
    }

    private void InsertKey()
    {
        RecordInsertKey();
        var mousePos = LSHandles.MouseInWorldPoint;
        var x = mousePos.x;
        var i = points.InsertKeyByX(x);
        selectedPointIndexes.Clear();
        selectedPointIndexes.Add(i);
        SetKeyPosAsAnimation(i, mousePos);
        GUI.changed = true;
    }

    private int[] deleteKeyList = new int[3];
    
    private int[] TryDeleteRoot(ref int ii, int i)
    {
        int deletedCount = 0;
        
        if (IsRoot(i))
        {
            RecordDeleteKey();
            points.DeleteKey(i);
            ClampTangentsByKey(i);
            
            for (int j = -1; j < 2; j++)
            {
                var iii = selectedPointIndexes.IndexOf(i + j);
                if (iii >= 0)
                {
                    if (ii >= iii)
                    {
                        ii--;
                    }
                    
                    selectedPointIndexes.RemoveAt(iii);
                    deleteKeyList[deletedCount] = i + j;
                    deletedCount++;
                }
            }
            
            GUI.changed = true;
        }

        return deleteKeyList[..deletedCount];
    }

    private void UpdatePosAsAnimation(ref int i, bool isRootMoving)
    {
        SetPosAsAnimation(ref i, points[i], isRootMoving);
    }


    private void SetKeyPosAsAnimation(int i, Vector2 pos)
    {
        if (IsRoot(i))
        {
            var delta = pos - points[i].e;
            for (int j = -1; j < 2; j++)
            {
                var ind = i + j;
                SetPosAsAnimation(ref ind, points[ind].e + delta, true);
            }
        }
    }

    private void SetPosAsAnimation(ref int i, Vector2 pos, bool isRootMoving)
    {
        RecordMove();
        var point = points[i];
        points[i] = point.eSet(pos);
        
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
            var dir = (points[i].e - root.p).normalized;
                
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
            var li = i;
            TrySwapNext(ref i);
            TrySwapPrev(ref i);
            
            if (li == i)
            {
                ClampTangentsByKey(i);
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
    
    private void ClampTangentsByKey(int keyIndex)
    {
        ClampTangent(keyIndex - 1);
        ClampTangent(keyIndex + 1);
        ClampTangent(keyIndex + 2);
        ClampTangent(keyIndex - 2);
    }

    private bool ClampTangent(int i)
    {
        if(i < 0 || i > points.Count - 1) return false;
        
        RecordMove();
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
        RecordChangeType();
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
            UpdatePosAsAnimation(ref a, true);
            a = i + f1;
            UpdatePosAsAnimation(ref a, false);
            a = i + f2;
            UpdatePosAsAnimation(ref a, false);
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
    
    private static bool IsInDistance(BezierPoint point, Vector2 worldMousePos, float distance) => LSHandles.IsInDistance(point.e, worldMousePos, distance);

    public void SwapKeys(ref int root1, int root2)
    {
        RecordMove();
        if(IsTangent(root1) || IsTangent(root2)) return;

        var root1Tangents = GetTangentIndexes(root1);
        var root2Tangents = GetTangentIndexes(root2);
        
        (points[root1], points[root2]) = (points[root2], points[root1]);
        (root1, root2) = (root2, root1);
        
        for (int i = 0; i < 2; i++)
        {
            (points[root1Tangents[i]], points[root2Tangents[i]]) = (points[root2Tangents[i]], points[root1Tangents[i]]);
        }

        ClampTangentsByKey(root1);
        ClampTangentsByKey(root2);
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