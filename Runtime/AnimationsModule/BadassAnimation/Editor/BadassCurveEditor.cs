#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using LSCore.Editor;
using UnityEngine;
using UnityEditor;
using UnityEngine.Animations;
using static BadassCurve;
using Object = UnityEngine.Object;

[Serializable]
public partial class BadassCurveEditor
{
    public Object context;
    public BadassCurve curve;
    public const float constWidth = 0.05f / 3;
    private const float BezierWidth = constWidth / 4;
    private const float tangentWidth = constWidth / 10;
    
    public RefAction<BezierPoint> OverridePointPosition;
    public Color curveColor = new (1f, 0.32f, 0.36f);
    public Color tangentLineColor = new (1f, 0.32f, 0.36f);
    private Color keyPointColor = Color.black;
    private Color tangentPointColor = Color.black;
    private Color SelectionColor => LSHandles.Styles.selectionColor;
    private Color lastSelectionColor = Color.white;
    
    public float SnappingStep { get; set; }
    
    public BadassCurveEditor(BadassCurve curve, Object context)
    {
        this.curve = curve;
        this.context = context;
    }
    
    public void OnGUI(bool draw, bool processEvents)
    {
        TryInitPointsTransformer();
        if (draw)
        {
            if (curve.Count > 2)
            {
                var minPoint = curve[1];
                minPoint.x = -100000;

                var bezierWidth = BezierWidth;
                var curveColor = this.curveColor;
                var tangentLineColor = this.tangentLineColor;
                var keyPointColor = this.keyPointColor;
                var selectionColor = this.SelectionColor;

                if (!IsFocused)
                {
                    bezierWidth /= 2;
                    curveColor.a /= 3;
                    tangentLineColor.a /= 3;
                    
                    if (!IsSelected)
                    {
                        curveColor.a /= 4;
                        tangentLineColor.a /= 4;
                    }
                }
                
                if (IsLocked)
                {
                    keyPointColor = Color.gray;
                    selectionColor = Color.gray;
                }

                LSHandles.DrawLine(bezierWidth, curveColor, curve[1], minPoint);

                var maxPoint = curve[^2];
                maxPoint.x = 100000;
                LSHandles.DrawLine(bezierWidth, curveColor, curve[^2], maxPoint);

                var st = curve[0];
                var sp = curve[1];
                DrawTangentLine(tangentWidth, tangentLineColor, st.e, sp.e);
                DrawTangentPoint(st.e, constWidth, tangentPointColor);

                for (int i = 1; i < curve.Count - 2; i += 3)
                {
                    var startPosition = curve[i];
                    var startTangent = curve[i + 1];
                    var endTangent = curve[i + 2];
                    var endPosition = curve[i + 3];

                    LSHandles.DrawBezier(startPosition.p, startTangent.p, endTangent.p, endPosition.p, curveColor,
                        bezierWidth);

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
                    else if (!IsLocked)
                    {
                        LSHandles.DrawRing(curve[target].e, constWidth, selectionColor);
                    }
                }
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
    
    public void DrawKey(Vector2 pos, float size, Color color)
    {
        var keyPointColor = color;
        
        if (IsLocked)
        {
            keyPointColor = Color.gray;
        }

        LSHandles.DrawCircle(pos, size, keyPointColor);
    }
    
    public void DrawSelectedKey(Vector2 pos, float size)
    {
        var selectionColor = this.SelectionColor;

        if (IsLocked)
        {
            selectionColor = Color.gray;
        }

        LSHandles.DrawCircle(pos, size, selectionColor);
    }
    
    public bool IsPointSelected(int index) => selectedPointIndexes.Contains(index);
    
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
            for (int i = 1; i < points.Length; i+=3)
            {
                yield return points[i];
            }
        }
    }
    
    public IEnumerable<int> KeysIndexes
    {
        get
        {
            var count = curve.Points.Length;
            for (int i = 1; i < count; i+=3)
            {
                yield return i;
            }
        }
    }
    
    public Func<Rect> pointsBoundsGetter;
    private LSHandles.PointsTransformer pointsTransformer;
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
    [SerializeField] private bool isSelected;
    [NonSerialized] public bool isYBlocked;
    
    public bool IsFocusOrAnyPointSelected => IsFocused || selectedPointIndexes.Count > 0;
    
    public bool IsSelected
    {
        get => isSelected || IsFocusOrAnyPointSelected;
        set => isSelected = value;
    }
    
    private bool oldIsFocused;
    private BadassCurve copyPoints;
    private List<int> copySelectedPointIndexes;
    
    private void ProcessEvents(Event e)
    {
        WasClicked = false;
        WasClickedOnSelected = false;
        WantOpenPopup = false;
        
        if (pointsTransformer.TryHandleEvent(e))
        {
            return;
        }
        
        switch (e.type)
        {
            case EventType.KeyDown:
                if (e.modifiers != EventModifiers.None && e.keyCode != KeyCode.Delete) break;
                pointsTransformer.TrySetupEventHandler(e);
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
            var pos = curve[i];
            OverridePointPosition?.Invoke(this, i, ref pos, Check);
            if (Check())
            {
                index = i;
                return true;
            }

            continue;

            bool Check() => IsInDistance(pos, mp, constWidth);
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
            var point = curve[i];
            OverridePointPosition?.Invoke(this, i, ref point, Check);
            if (Check())
            {
                TrySelectPoint(i);
            }

            continue;

            bool Check() => rect.Contains(point.e);
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
        SetKeyPos(i, mousePos);
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


    private void SetKeyPos(int i, Vector2 pos)
    {
        if (IsRoot(i))
        {
            var delta = pos - curve[i].e;
            for (int j = -1; j < 2; j++)
            {
                var ind = i + j;
                SetPos(ref ind, curve[ind].e + delta, true);
            }
        }
    }
    
    public void UpdateKeyPos(int i)
    {
        if (IsRoot(i))
        {
            for (int j = -1; j < 2; j++)
            {
                var ind = i + j;
                UpdatePos(ref ind, true);
            }
        }
    }

    private void UpdatePos(ref int i, bool isRootMoving)
    {
        SetPos(ref i, curve[i], isRootMoving);
    }
    
    private void SetPos(ref int i, Vector2 pos, bool isRootMoving)
    {
        if(IsLocked) return;
        
        RecordMove();
        var point = curve[i];
        
        if (isYBlocked)
        {
            pos.y = point.e.y;
        }

        pos.x = LSHandles.SnapX(pos.x, SnappingStep);
        
        curve[i] = point.epSet(pos);
        
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
                tangent.epSet(root.p + -dir * dis);
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
                if (point.x > next.x)
                {
                    SwapKeys(ref ii, ii + 3);
                }
            }

            void TrySwapPrev(ref int ii)
            {
                if(ii - 3 < 0) return;
                
                var prev = curve[ii - 3];
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
                pos.x = Mathf.Clamp(pos.e.x, root.x, nextRoot.x);
                pos.e.x = Mathf.Clamp(pos.e.x, root.x, inf.x);
                curve[i] = pos;
            }
            else
            {
                var inf = Vector2.negativeInfinity;
                var root = curve[i + 1];
                var prevRoot = i > 0 ? curve[i - 2] : (BezierPoint)inf;
                var pos = curve[i];
                pos.x = Mathf.Clamp(pos.e.x, prevRoot.x, root.x);
                pos.e.x = Mathf.Clamp(pos.e.x, inf.x, root.x);
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
            UpdatePos(ref a, true);
            a = i + f1;
            UpdatePos(ref a, false);
            a = i + f2;
            UpdatePos(ref a, false);
        }
    }
    
    private int[] GetTangentIndexes(int i)
    {
        if (IsTangent(i)) return Array.Empty<int>();
        
        workedPointIndexesArr[0] = i - 1;
        workedPointIndexesArr[1] = i + 1;
        return workedPointIndexesArr[..];
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
                        SetPosByMouseDrag(ref ind, copyind, true);
                    }
                                
                    ind = targetIndex;
                    copyind = copyTargetIndex;
                    SetPosByMouseDrag(ref ind, copyind, true);
                    selectedPointIndexes[i] = ind;
                }
            }
            else
            {
                if (IsTangent(targetIndex))
                {
                    var ind = targetIndex;
                    var copyind = copyTargetIndex;
                    SetPosByMouseDrag(ref ind, copyind, false);
                    selectedPointIndexes[i] = ind;
                }
            }
        }
        
        GUI.changed = true;
        
        void SetPosByMouseDrag(ref int ind, int copyi, bool isRoot)
        {
            var delta = LSHandles.MouseInWorldPoint - startMousePosition;
            var point = copyPoints[copyi].e + delta;
            SetPos(ref ind, point, isRoot);
        }
    }

    public void TryInitPointsTransformer()
    {
        if (pointsTransformer == null)
        {
            pointsTransformer = new();

            bool yWasBlocked = false;
            
            pointsTransformer.canSetupHandler = () => selectedPointIndexes.Count > 0;
            pointsTransformer.HandlingStopped += () =>
            {
                isYBlocked = yWasBlocked;
                WantResetIsRecorded = true;
            };
            
            pointsTransformer.NeedSetupHandler += isResetup =>
            {
                if (!isResetup)
                {
                    yWasBlocked = isYBlocked;
                    isYBlocked = false;
                    if (yWasBlocked) pointsTransformer.currentAxis = Axis.X;
                }
                
                var copy = new BadassCurve(curve);
                
                var lastSelectedPointIndexesCopy = selectedPointIndexes;
                var selectedPointIndexesCopy = new List<int>(selectedPointIndexes);
                selectedPointIndexes = selectedPointIndexesCopy;

                var bounds = pointsBoundsGetter();
                var startPos = LSHandles.MouseInWorldPoint;

                pointsTransformer.applyEventHandler = _ => pointsTransformer.applyEventHandler = null;
                pointsTransformer.eventHandler = _ =>
                {
                    var curPos = LSHandles.MouseInWorldPoint;
                    var transformation = pointsTransformer.GetMatrix(bounds, startPos, curPos);

                    for (int i = 0; i < selectedPointIndexes.Count; i++)
                    {
                        var targetIndex = selectedPointIndexes[i];
                        var lastTargetIndex = lastSelectedPointIndexesCopy[i];

                        if (IsRoot(targetIndex))
                        {
                            int ind;

                            Vector3 point;

                            for (int j = -1; j < 2; j += 2)
                            {
                                ind = targetIndex + j;
                                point = transformation.MultiplyPoint(copy[lastTargetIndex + j].e);
                                SetPos(ref ind, point, true);
                            }

                            ind = targetIndex;
                            point = transformation.MultiplyPoint(copy[lastTargetIndex].e);
                            SetPos(ref ind, point, true);
                            selectedPointIndexes[i] = ind;
                        }
                        else
                        {
                            var ind = targetIndex;
                            var point = transformation.MultiplyPoint(copy[lastTargetIndex].e);
                            SetPos(ref ind, point, false);
                        }
                    }
                };
                pointsTransformer.discardEventHandler = _ =>
                {
                    curve.Apply(copy);
                    selectedPointIndexes = lastSelectedPointIndexesCopy;
                };
            };
        }
    }

    public void SetKeyY(float x, float y)
    {
        var lastYBlocked = isYBlocked;
        isYBlocked = false;
        var leftIndex = curve.GetLeftKeyIndexByX(x);
        if (leftIndex == -1)
        {
            leftIndex = 1;
        }

        if (curve.Count > leftIndex)
        {
            var leftPoint = curve[leftIndex];
            var mp = leftPoint.e;
            mp.x = x;
            
            if (IsInDistance(leftPoint, mp, constWidth))
            {
                leftPoint.e.y = y;
                SetKeyPos(leftIndex, leftPoint.e);
                goto ret;
            }
        }
        
        var rightIndex = leftIndex + 3;
        if(curve.Count > rightIndex)
        {
            var rightPoint = curve[rightIndex];
            var mpr = rightPoint.e;
            mpr.x = x;
            
            if (IsInDistance(rightPoint, mpr, constWidth))
            {
                rightPoint.e.y = y;
                SetKeyPos(rightIndex, rightPoint.e);
                goto ret;
            }
        }

        var index = curve.InsertKeyByX(x);
        var point = curve[index].e;
        point.y = y;
        SetKeyPos(index, point);
        
        ret:
        isYBlocked = lastYBlocked;
    }
}

public class Popup : PopupWindowContent
{
    public Action onGui;
    public Action onClose;
    public Vector2 position;
    public Vector2 size;

    public Popup()
    {
        size = new Vector2(200f, 200f);
    }

    public Popup(Vector2 position, Vector2 size)
    {
        this.position = position;
        this.size = size;
    }

    public void OnGUIInArea()
    {
        GUILayout.BeginArea(new Rect(position, size));
        onGui();
        GUILayout.EndArea();
    }
    
    public void OnGUI()
    {
        onGui();
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

    public void Show(Vector2 mousePos)
    {
        PopupWindow.Show(new Rect(mousePos, new Vector2(10, 10)), this);
    }
}

#endif