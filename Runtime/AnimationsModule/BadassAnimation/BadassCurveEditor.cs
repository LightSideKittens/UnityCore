#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using LSCore.Editor;
using UnityEngine;
using UnityEditor;
using UnityEngine.Animations;
using Object = UnityEngine.Object;

[Serializable]
public partial class BadassCurveEditor
{
    public Object context;
    public BadassCurve curve;
    public float time;
    private const float constWidth = 0.05f / 3;
    private const float BezierWidth = constWidth / 4;
    private const float tangentWidth = constWidth / 10;
    
    public Color curveColor = new (1f, 0.32f, 0.36f);
    public Color tangentLineColor = new (1f, 0.32f, 0.36f);
    private Color keyPointColor = Color.black;
    private Color tangentPointColor = Color.black;
    private Color selectionColor = new Color(1f, 0.54f, 0.16f);
    private Color lastSelectionColor = Color.white;
    
    public BadassCurveEditor(BadassCurve curve, Object context)
    {
        this.curve = curve;
        this.context = context;
    }
    
    public void OnGUI(bool processEvents)
    {
        if (curve.Count > 2)
        {
            var min = curve[0];
            var max = curve[0];
            
            for (int i = 0; i < curve.Count; i++)
            {
                UpdateMinMax(curve[i]);
            }
            
            var minPoint = curve[1];
            minPoint.p.x = -100000;
            
            var bezierWidth = BezierWidth;
            var curveColor = this.curveColor;
            var tangentLineColor = this.tangentLineColor;
            var keyPointColor = this.keyPointColor;
            var selectionColor = this.selectionColor;
            
            if (!IsFocused)
            {
                bezierWidth /= 3;
                curveColor.a /= 3;
                tangentLineColor.a /= 3;
            }

            if (IsLocked)
            {
                keyPointColor = Color.gray;
                selectionColor = Color.gray;
            }
            
            LSHandles.DrawLine(bezierWidth, curveColor, curve[1], minPoint);
                
            var maxPoint = curve[^2];
            maxPoint.p.x = 100000;
            LSHandles.DrawLine(bezierWidth, curveColor, curve[^2], maxPoint);
            
            var st = curve[0];
            var sp = curve[1];
            DrawTangentLine(tangentWidth, tangentLineColor, st.e, sp.e);
            DrawTangentPoint(st.e, constWidth, tangentPointColor);
            
            for (int i = 1; i < curve.Count - 2; i += 3)
            {
                var startPosition = curve[i];
                var startTangent = curve[i+1];
                var endTangent = curve[i+2];
                var endPosition = curve[i+3];
                
                LSHandles.DrawBezier(startPosition.p, startTangent.p, endTangent.p, endPosition.p, curveColor, bezierWidth);
                
                DrawTangentLine(tangentWidth, tangentLineColor, startTangent.e, startPosition.e);
                DrawTangentLine(tangentWidth, tangentLineColor, endTangent.e, endPosition.e);
                
                LSHandles.DrawCircle(startPosition.e, constWidth, keyPointColor);
                DrawTangentPoint(startTangent.e, constWidth, tangentPointColor);
                DrawTangentPoint(endTangent.e, constWidth, tangentPointColor);
            }
            
            var ep = curve[^2];
            var et = curve[^1];
            DrawTangentLine(tangentWidth, tangentLineColor, et.e, ep.e);
            DrawTangentPoint(et.e, constWidth, tangentPointColor);
            
            LSHandles.DrawCircle(ep.e, constWidth, keyPointColor);
            
            for (int i = 0; i < selectedPointIndexes.Count; i++)
            {
                var target = selectedPointIndexes[i];
                if (IsRoot(target))
                {
                    LSHandles.DrawCircle(curve[target].e, constWidth, selectionColor);
                }
                else if(!IsLocked)
                {
                    LSHandles.DrawRing(curve[target].e, constWidth, selectionColor);
                }
            }
            
            var dp = curve.EvaluateNormalizedVector(time);
            LSHandles.DrawCircle(dp, constWidth, Color.white);
            
            void UpdateMinMax(in Vector2 vector)
            {
                if (vector.x < min.p.x) min.p.x = vector.x;
                if (vector.y < min.p.y) min.p.y = vector.y;
                if (vector.x > max.p.x) max.p.x = vector.x;
                if (vector.y > max.p.y) max.p.y = vector.y;
            }
        }

        if (processEvents)
        {
            ProcessEvents(Event.current);
        }

        void DrawTangentPoint(Vector2 pos, float size, Color color)
        {
            if(IsLocked) return;
            LSHandles.DrawRing(pos, size, color);
        }
        
        void DrawTangentLine(float width, Color color, params Vector3[] points)
        {
            if(IsLocked) return;
            LSHandles.DrawLine(width, color, points);
        }
    }
    
    private readonly int[] workedPointIndexesArr = new int[2];
    [SerializeField] private List<int> selectedPointIndexes = new();
    private List<int> oldsSelectedPointIndexes;

    public int SelectedPointIndexesCount => IsLocked ? 0 : selectedPointIndexes.Count;

    public IEnumerable<int> SelectedPointsIndexes => selectedPointIndexes;
    
    public IEnumerable<BezierPoint> SelectedPoints
    {
        get
        {
            for (int i = 0; i < selectedPointIndexes.Count; i++)
            {
                yield return curve[selectedPointIndexes[i]];
            }
        }
    }
    
    public IEnumerable<Vector2> eSelectedPoints
    {
        get
        {
            if (IsLocked) yield break;
            
            foreach (var point in SelectedPoints)
            {
                yield return point.e;
            }
        }
    }
    
    public IEnumerable<BezierPoint> Keys
    {
        get
        {
            var points = curve.Points;
            for (int i = 0; i < points.Length; i++)
            {
                if(IsRoot(i)) yield return points[i];
            }
        }
    }
    
    public Func<Rect> pointsBoundsGetter;
    private Action<Event> eventHandler;
    private Action<Event> applyEventHandler;
    private Action<Event> discardEventHandler;
    private LSHandles.TransformationMode currentTransformationMode;
    private Axis currentAxis = Axis.None;
    private Vector2 lastMousePosition;
    private bool lastIsRecorded;

    public bool IsRecorded
    {
        get => SessionState.GetBool($"CurveEditing{context.GetInstanceID()}", false);
        set
        {
            SessionState.SetBool($"CurveEditing{context.GetInstanceID()}", value);
            if (value == false && lastIsRecorded)
            {
                Edited?.Invoke();
            }
            lastIsRecorded = value;
        }
    }
    
    public bool WantResetIsRecorded { get; private set; }
    public bool WantOpenPopup { get; private set; }
    public bool IsVisible { get; set; } = true;
    public bool IsLocked { get; set; }
    public bool WasClicked { get; set; }
    public bool WasClickedOnSelected { get; set; }
    public int ClickedPointIndex { get; private set; }
    [field: SerializeField] public bool IsFocused { get; set; }
    
    private bool oldIsFocused;
    private BadassCurve copyPoints;
    private List<int> copySelectedPointIndexes;
    
    private void ProcessEvents(Event e)
    {
        WasClicked = false;
        WasClickedOnSelected = false;
        WantOpenPopup = false;
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
                    if (e.modifiers != EventModifiers.None) break;
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
                if (e.modifiers != EventModifiers.None && e.keyCode != KeyCode.Delete) break;
                TrySetupEventHandler();
                if (e.keyCode == KeyCode.A)
                {
                    oldsSelectedPointIndexes = new(selectedPointIndexes);
                    selectedPointIndexes.Clear();
                    for (int i = 0; i < curve.Count; i++)
                    {
                        selectedPointIndexes.Add(i);
                    }
                    GUI.changed = true;
                    RecordSelectIfChanged();
                    WantResetIsRecorded = false;
                }
                else if(e.keyCode == KeyCode.Delete)
                {
                    if (selectedPointIndexes.Count == 0 || IsLocked) break;
                    
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
                    
                    WantResetIsRecorded = true;
                }
                break;
            case EventType.MouseDown:
                if (e.button == 1)
                {
                    WasClicked = TryGetPointIndex(out var i);

                    if (IsFocused && !IsLocked)
                    {
                        if (e.control)
                        {
                            if (WasClicked)
                            {
                                int a = 0;
                                TryDeleteRoot(ref a, i);
                            }
                            else
                            {
                                InsertKey();
                            }
                        }
                        else
                        {
                            WantOpenPopup = true;
                        }
                    }
                }
                else if (e.button == 0)
                {
                    oldsSelectedPointIndexes = new(selectedPointIndexes);
                    oldIsFocused = IsFocused;
                    GUI.changed = true;
                    
                    WasClicked = TryGetPointIndex(out var i);
                    if (WasClicked && selectedPointIndexes.Contains(i))
                    {
                        WasClickedOnSelected = true;
                    }
                    ClickedPointIndex = i;
                }
                break;

            case EventType.MouseUp:
                GUI.changed = true;
                if (e.button == 0)
                {
                    RecordSelectIfChanged();
                }
                WantResetIsRecorded = true;
                break;
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
            var last = curve;
            var copy = new BadassCurve(curve);
            curve = copy;
            
            var lastSelectedPointIndexesCopy = selectedPointIndexes;
            var selectedPointIndexesCopy = new List<int>(selectedPointIndexes);
            selectedPointIndexes = selectedPointIndexesCopy;

            var bounds = pointsBoundsGetter();
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
                curve = last;
                selectedPointIndexes = lastSelectedPointIndexesCopy;
            };
        }

        void SetAxis(Axis axis)
        {
            currentAxis = currentAxis == axis ? Axis.None : axis;
        }

        void StopEventHandling()
        {
            WantResetIsRecorded = true;
            currentAxis = Axis.None;
            eventHandler = null;
            discardEventHandler = null;
            applyEventHandler = null;
        }
    }

    private Action beforeRecordSelectIfChanged;
    private Action afterRecordSelectIfChanged;

    public void PrepareRecordSelectIfChanged()
    {
        beforeRecordSelectIfChanged?.Invoke();
        beforeRecordSelectIfChanged = null;
    }
    
    public void TryRecordSelectIfChanged()
    {
        afterRecordSelectIfChanged?.Invoke();
        afterRecordSelectIfChanged = null;
    }
    
    private void RecordSelectIfChanged()
    {
        beforeRecordSelectIfChanged = () =>
        {
            if (oldsSelectedPointIndexes != null && !oldsSelectedPointIndexes.SequenceEqual(selectedPointIndexes))
            {
                WantResetIsRecorded = true;
                var l = IsFocused;
                var last = selectedPointIndexes;
                
                IsFocused = oldIsFocused;
                selectedPointIndexes = oldsSelectedPointIndexes;
                
                afterRecordSelectIfChanged = () =>
                {
                    RecordSelect();
                    selectedPointIndexes = last;
                    IsFocused = l;
                };
            } 
            else if (IsFocused != oldIsFocused)
            {
                var l = IsFocused;
                IsFocused = oldIsFocused;
                afterRecordSelectIfChanged = () =>
                {
                    RecordFocus();
                    IsFocused = l;
                };
            }
        };
    }
    
    public bool TryGetPointIndex(out int index)
    {
        var mp = LSHandles.MouseInWorldPoint;
        for (int i = 0; i < curve.Count; i++)
        {
            if (IsInDistance(curve[i], mp, constWidth))
            {
                index = i;
                return true;
            }
        }

        index = -1;
        return false;
    }
    
    public bool TrySelectPoint(int pointIndex)
    {
        if (!selectedPointIndexes.Contains(pointIndex))
        {
            selectedPointIndexes.Add(pointIndex);
            return true;
        }
        
        return false;
    }
    
    public bool SelectOrDeselectPoint(int pointIndex)
    {
        if (!TrySelectPoint(pointIndex))
        {
            selectedPointIndexes.Remove(pointIndex);
            return false;
        }

        return true;
    }
    
    public void SelectPointsByRect(Rect rect)
    {
        for (int i = 0; i < curve.Count; i++)
        {
            if (rect.Contains(curve[i].e))
            {
                TrySelectPoint(i);
            }
        }
    }

    private void InsertKey()
    {
        RecordInsertKey();
        var mousePos = LSHandles.MouseInWorldPoint;
        var x = mousePos.x;
        var i = curve.InsertKeyByX(x);
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
            curve.DeleteKey(i);
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
        SetPosAsAnimation(ref i, curve[i], isRootMoving);
    }


    private void SetKeyPosAsAnimation(int i, Vector2 pos)
    {
        if (IsRoot(i))
        {
            var delta = pos - curve[i].e;
            for (int j = -1; j < 2; j++)
            {
                var ind = i + j;
                SetPosAsAnimation(ref ind, curve[ind].e + delta, true);
            }
        }
    }

    private void SetPosAsAnimation(ref int i, Vector2 pos, bool isRootMoving)
    {
        if(IsLocked) return;
        
        RecordMove();
        var point = curve[i];
        curve[i] = point.eSet(pos);
        
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
            
            var root = curve[i + f1];
            var tangent = curve[i + f2];
            var dir = (curve[i].e - root.p).normalized;
                
            if (root.alignType == AlignType.Aligned)
            {
                var dis = (tangent.e - root.p).magnitude;
                tangent.eSet(root.p + -dir * dis);
                curve[i + f2] = tangent;
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
                if(ii + 3 > curve.Count - 1) return;
                
                var next = curve[ii + 3];
                if (point.p.x > next.p.x)
                {
                    SwapKeys(ref ii, ii + 3);
                }
            }

            void TrySwapPrev(ref int ii)
            {
                if(ii - 3 < 0) return;
                
                var prev = curve[ii - 3];
                if (point.p.x < prev.p.x)
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
        if(i < 0 || i > curve.Count - 1) return false;
        
        RecordMove();
        if (IsTangent(i))
        {
            if (IsForwardTangent(i))
            {
                var inf = Vector2.positiveInfinity;
                var root = curve[i - 1];
                var nextRoot = i < (curve.Count-1) ? curve[i + 2] : (BezierPoint)inf;
                var pos = curve[i];
                pos.p.x = Mathf.Clamp(pos.e.x, root.p.x, nextRoot.p.x);
                pos.e.x = Mathf.Clamp(pos.e.x, root.p.x, inf.x);
                curve[i] = pos;
            }
            else
            {
                var inf = Vector2.negativeInfinity;
                var root = curve[i + 1];
                var prevRoot = i > 0 ? curve[i - 2] : (BezierPoint)inf;
                var pos = curve[i];
                pos.p.x = Mathf.Clamp(pos.e.x, prevRoot.p.x, root.p.x);
                pos.e.x = Mathf.Clamp(pos.e.x, inf.x, root.p.x);
                curve[i] = pos;
            }

            return true;
        }

        return false;
    }

    public void ChangeType(int i, AlignType alignType)
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

        curve.SetAlign(i, alignType);
        curve.SetAlign(i + f1, alignType);
        curve.SetAlign(i + f2, alignType);

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
        
        (curve[root1], curve[root2]) = (curve[root2], curve[root1]);
        (root1, root2) = (root2, root1);
        
        for (int i = 0; i < 2; i++)
        {
            (curve[root1Tangents[i]], curve[root2Tangents[i]]) = (curve[root2Tangents[i]], curve[root1Tangents[i]]);
        }

        ClampTangentsByKey(root1);
        ClampTangentsByKey(root2);
    }

    public void HandleMultiSelection()
    {
        var e = Event.current;
        var i = ClickedPointIndex;
        TryDeselectAll();
        
        if (WasClicked)
        {
            IsFocused = true;
                        
            if (e.shift)
            {
                SelectOrDeselectPoint(i);
            }
            else
            {
                TrySelectPoint(i);
            }
            
            SetupForDragging();
        }
    }

    public void TryDeselectAll()
    {
        var e = Event.current;
        var i = ClickedPointIndex;
        
        if (selectedPointIndexes.Count > 0 
            && !e.shift 
            && !(WasClicked && selectedPointIndexes.Contains(i)))
        {
            ForceDeselectAll();
        }
    }

    public void ForceDeselectAll()
    {
        selectedPointIndexes.Clear();
    }

    public void SetupForDragging()
    {
        copyPoints = new BadassCurve(curve);
        copySelectedPointIndexes = new List<int>(selectedPointIndexes);
    }

    public void DragSelectedPoints(int draggingPointIndex, Vector2 startMousePosition)
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
        
        void SetPosAsAnimationByMouseDrag(ref int ind, int copyi, bool isRoot)
        {
            var delta = LSHandles.MouseInWorldPoint - startMousePosition;
            var point = copyPoints[copyi].e + delta;
            SetPosAsAnimation(ref ind, point, isRoot);
        }
    }
}

public class Popup : PopupWindowContent
{
    public Action onGui;
    public Action onClose;
    public Vector2 size;

    public Popup()
    {
        size = new Vector2(200f, 200f);
    }

    public Popup(Vector2 size)
    {
        this.size = size;
    }

    public override void OnGUI(Rect rect)
    {
        onGui();
    }
    
    public void DrawFoldout(string name, Action gui, bool show = false)
    {
        EditorUtils.DrawInBoxFoldout(new GUIContent(name), gui, name, show);
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

    public override void OnClose()
    {
        base.OnClose();
        onClose?.Invoke();
    }

    public override Vector2 GetWindowSize()
    {
        return size;
    }

    public void Close()
    {
        if (editorWindow != null) editorWindow.Close();
    }

    public void Repaint() => editorWindow.Repaint();
}

#endif