#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using LightSideCore.Editor;
using LSCore.ConfigModule;
using LSCore.Editor;
using LSCore.Editor.BackupSystem;
using LSCore.Extensions;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

public delegate void RefAction<T>(BadassCurveEditor editor, int index, ref T value, Func<bool> check);

[Serializable]
public class BadassMultiCurveEditor
{
    public Action BeforeDraw;
    public RefAction<BezierPoint> OverridePointPosForSelection;

    private static JToken Config => Manager.Config.data;
    private static EditorConfigManager Manager => EditorConfig.GetManager(nameof(BadassMultiCurveEditor));
    public BadassCurveEditor First => visibleEditors[0];
    public List<BadassCurveEditor> editors = new();
    private List<BadassCurveEditor> visibleEditors = new();
    public Matrix4x4 matrix = Matrix4x4.identity;
    public LSHandles.CameraData camData = new();
    public LSHandles.GridData gridData = new();
    public bool snapping = true;
    private int currentSelectionClickCount = 0;
    private bool wasDragging;
    private bool wasShift;
    private int draggingIndex;
    private Vector2 startMousePosition;

    private void UpdateSnappingStep()
    {
        foreach (var editor in visibleEditors)
        {
            editor.SnappingStep = snapping ? gridData.SnappingStep : 0;
        }
    }
    
    public BadassMultiCurveEditor(IEnumerable<BadassCurveEditor> editors)
    {
        this.editors.AddRange(editors);
        foreach (var editor in this.editors)
        {
            SetupPointsBoundsGetter(editor);
        }
        OnEnable();
    }
    
    public BadassMultiCurveEditor(BadassCurveEditor editor)
    {
        SetupPointsBoundsGetter(editor);
        editors.Add(editor);
        OnEnable();
    }
    
    public BadassMultiCurveEditor() { }
    
    public void Add(BadassCurveEditor editor)
    {
        SetupPointsBoundsGetter(editor);
        editors.Add(editor);
        editor.OnEnable();
    }

    public void Clear()
    {
        editors.Clear();
    }
    
    private void SetupPointsBoundsGetter(BadassCurveEditor editor)
    {
        editor.pointsBoundsGetter = GetSelectedPointsBounds;
    }

    public bool IsYBlocked
    {
        set
        {
            foreach (var editor in editors)
            {
                editor.isYBlocked = value;
            }
        }
    }
    
    private IEnumerable<Vector2> eSelectedPoints
    {
        get
        {
            foreach (var point in visibleEditors.SelectMany(x => x.eSelectedPoints))
            {
                yield return point;
            }
        }
    }
    
    public IEnumerable<Vector2> KeyPoints
    {
        get
        {
            int index = 0;
            foreach (var point in editors.SelectMany(x => x.Keys))
            {
                yield return point.p;
            }
        }
    }

    public float SnappingStep => gridData.SnappingStep;

    public Rect GetKeyPointsBounds()
    {
        return LSHandles.CalculateBounds(KeyPoints);
    }
    
    private Rect GetSelectedPointsBounds()
    {
        return LSHandles.CalculateBounds(eSelectedPoints);
    }
    
    public void OnGUI(Rect rect, bool draw)
    {
        visibleEditors.Clear();
        
        for (int i = 0; i < editors.Count; i++)
        {
            var editor = editors[i];
            if (editor.IsVisible)
            {
                visibleEditors.Add(editor);
            }
        }
        
        LSHandles.StartMatrix(matrix);
        LSHandles.Begin(rect, camData);
        LSHandles.DrawGrid(gridData);
        BeforeDraw?.Invoke();
        LSHandles.SelectRect.Draw();

        if (visibleEditors.Count > 0)
        {
            DrawEditors(rect, draw);
            TryCopyPointsToClipboard();
        }
        
        LSHandles.End();
        matrix = LSHandles.EndMatrix();
        
        BeforeDraw = null;
        OverridePointPosForSelection = null;
    }

    public void PastePointsFromClipboard(float x)
    {
        var list = (JArray)Config["copiedPoints"];
        if(list == null) return;
        var selectedEditors = visibleEditors.Where(e => e.IsSelected).ToList();
        var min = Math.Min(list.Count, selectedEditors.Count);
        
        for (int i = 0; i < min; i++)
        {
            var points = (JArray)list[i];
            var editor = selectedEditors[i];
            var leftIndex = editor.curve.GetLeftKeyIndexByX(x);
            var back = BezierPoint.FromJObject(points[0]);
            var root = BezierPoint.FromJObject(points[1]);
            var forward = BezierPoint.FromJObject(points[2]);
            var xOffset = x - root.e.x;
            back.epPlusX(xOffset);
            root.epPlusX(xOffset);
            forward.epPlusX(xOffset);
            editor.curve.InsertKeys(leftIndex, forward, root, back);
        }
    }
    
    private void TryCopyPointsToClipboard()
    {
        if (!Event.current.IsCopy()) return;
        
        var list = new JArray();
        
        for (int i = 0; i < visibleEditors.Count; i++)
        {
            var curve = visibleEditors[i].curve;
            
            foreach (var index in visibleEditors[i].SelectedPointsIndexes.OrderBy(x => x))
            {
                var points = new JArray();
                
                int a;
                int b;
                int c;
                
                if (BadassCurve.IsRoot(index))
                {
                    a = index-1;
                    b = index;
                    c = index+1;
                }
                else
                {
                    if(BadassCurve.IsBackwardTangent(index))
                    {
                        a = index;
                        b = index+1;
                        c = index+2;
                    }
                    else
                    {
                        a = index-2;
                        b = index-1;
                        c = index;
                    }
                }
                
                points.Add(curve[a].ToJObject());
                points.Add(curve[b].ToJObject());
                points.Add(curve[c].ToJObject());
                list.Add(points);
            }
        }
        
        Config["copiedPoints"] = list;
        Manager.Save();
    }
    
    private void DrawEditors(Rect rect, bool draw)
    {
        bool rectContainsMouse = rect.Contains(Event.current.mousePosition);
        bool wasClicked = false;
        bool wasClickedOnSelected = false;
        List<BadassCurveEditor> selectedEditors = new();
        List<BadassCurveEditor> clickedEditors = new();

        for (int i = 0; i < visibleEditors.Count; i++)
        {
            var editor = visibleEditors[i];
            editor.OverridePointPosition = OverridePointPosForSelection;
            editor.OnGUI(draw, rectContainsMouse);

            if (editor.SelectedPointIndexesCount > 0)
            {
                selectedEditors.Add(editor);
            }

            if (editor.WasClicked)
            {
                clickedEditors.Add(editor);
                wasClicked = true;
            }
        }

        ProcessEvents();

        void ProcessEvents()
        {
            if (!rectContainsMouse) return;
            UpdateSnappingStep();
            var e = Event.current;

            if (e.type == EventType.MouseDown)
            {
                if (e.button == 0)
                {
                    BadassCurveEditor focusedSelectedEditor = null;

                    if (clickedEditors.Count > 1)
                    {
                        for (int i = 0; i < clickedEditors.Count; i++)
                        {
                            var editor = visibleEditors[i];
                            if (editor.IsFocused)
                            {
                                focusedSelectedEditor = editor;
                                break;
                            }
                        }
                    }
                    else if (clickedEditors.Count == 1)
                    {
                        focusedSelectedEditor = clickedEditors[0];
                    }

                    if (focusedSelectedEditor == null)
                    {
                        for (int i = 0; i < selectedEditors.Count; i++)
                        {
                            var editor = visibleEditors[i];
                            if (editor.WasClicked)
                            {
                                focusedSelectedEditor = editor;
                                break;
                            }
                        }
                    }

                    if (focusedSelectedEditor != null)
                    {
                        if (focusedSelectedEditor.WasClickedOnSelected)
                        {
                            wasClickedOnSelected = true;
                        }

                        if (focusedSelectedEditor.WasClicked)
                        {
                            draggingIndex = focusedSelectedEditor.ClickedPointIndex;
                            startMousePosition = LSHandles.MouseInWorldPoint;
                        }
                    }

                    wasDragging = false;
                    wasShift = e.shift;

                    int totalSelectedPoints = 0;

                    if (!wasClickedOnSelected)
                    {
                        for (int i = 0; i < visibleEditors.Count; i++)
                        {
                            var editor = visibleEditors[i];
                            editor.TryDeselectAll();
                        }
                    }

                    for (int i = 0; i < selectedEditors.Count; i++)
                    {
                        var editor = selectedEditors[i];
                        editor.SetupForDragging();
                        totalSelectedPoints += editor.SelectedPointIndexesCount;
                    }

                    if (!wasClicked)
                    {
                        DeselectAll();
                        LSHandles.SelectRect.Start();
                    }

                    if (clickedEditors.Count > 0)
                    {
                        DeselectAll();
                        var clickedEditor = clickedEditors.GetCyclic(currentSelectionClickCount);
                        clickedEditor.HandleMultiSelection();
                        currentSelectionClickCount++;
                    }

                    void DeselectAll()
                    {
                        for (int i = 0; i < visibleEditors.Count; i++)
                        {
                            var editor = visibleEditors[i];
                            if (editor.IsFocused && !e.shift && totalSelectedPoints < 2)
                            {
                                editor.ForceDeselectAll();
                            }

                            editor.IsFocused = false;
                        }
                    }
                }
            }
            else if (e.type == EventType.MouseDrag)
            {
                if (e.button == 0 && !e.shift)
                {
                    wasDragging = true;

                    for (int i = 0; i < selectedEditors.Count; i++)
                    {
                        var editor = selectedEditors[i];
                        editor.DragSelectedPoints(draggingIndex, startMousePosition);
                    }
                }
            }
            else if (e.type == EventType.MouseUp)
            {
                if (e.button == 0)
                {
                    if (!wasDragging && !wasShift)
                    {
                        clickedEditors.Clear();

                        for (int i = 0; i < visibleEditors.Count; i++)
                        {
                            var editor = visibleEditors[i];
                            if (editor.TryGetPointIndex(out int index))
                            {
                                editor.WasClicked = true;
                                clickedEditors.Add(editor);
                            }

                            editor.ForceDeselectAll();
                        }

                        if (clickedEditors.Count > 0)
                        {
                            currentSelectionClickCount--;
                            var clickedEditor = clickedEditors.GetCyclic(currentSelectionClickCount);
                            clickedEditor.HandleMultiSelection();
                            currentSelectionClickCount++;
                        }
                    }

                    var selectionRect = LSHandles.SelectRect.End();

                    for (int i = 0; i < visibleEditors.Count; i++)
                    {
                        var editor = visibleEditors[i];
                        editor.SelectPointsByRect(selectionRect);
                    }
                }
            }

            bool wantResetIsRecorded = false;
            bool wantOpenPopup = false;

            for (int i = 0; i < visibleEditors.Count; i++)
            {
                var editor = visibleEditors[i];
                editor.PrepareRecordSelectIfChanged();
                wantResetIsRecorded |= editor.WantResetIsRecorded;
                wantOpenPopup |= editor.WantOpenPopup;
            }

            for (int i = 0; i < visibleEditors.Count; i++)
            {
                var editor = visibleEditors[i];
                editor.TryRecordSelectIfChanged();
            }

            if (wantResetIsRecorded)
            {
                for (int i = 0; i < visibleEditors.Count; i++)
                {
                    var editor = visibleEditors[i];
                    editor.IsRecorded = false;
                }
            }

            if (wantOpenPopup)
            {
                var popup = new Popup();
                HashSet<AlignType> types = new();

                for (int i = 0; i < visibleEditors.Count; i++)
                {
                    var editor = visibleEditors[i];
                    foreach (var point in editor.SelectedPoints)
                    {
                        types.Add(point.alignType);
                    }
                }

                Action onGui = () =>
                {
                    popup.DrawFoldout(string.Join(", ", types), () =>
                    {
                        DrawChangeTypeButton(popup, AlignType.Aligned);
                        DrawChangeTypeButton(popup, AlignType.Free);
                    });
                };

                popup.onGui = onGui;
                PopupWindow.Show(new Rect(e.mousePosition, new Vector2(10, 10)), popup);
                GUI.changed = true;

            }
        }
    }

    private void DrawChangeTypeButton(Popup popup, AlignType alignType)
    {
        if (popup.DrawButton(alignType.ToString()))
        {
            ChangeType(alignType);
        }
    }

    private void ChangeType(AlignType alignType)
    {
        for (int i = 0; i < visibleEditors.Count; i++)
        {
            var editor = visibleEditors[i];
            foreach (var index in editor.SelectedPointsIndexes)
            {
                editor.ChangeType(index, alignType);
            }
        }
    }

    public void OnEnable()
    {
        for (int i = 0; i < visibleEditors.Count; i++)
        {
            var editor = visibleEditors[i];
            editor.OnEnable();
        }
    }

    public void OnDisable()
    {
        for (int i = 0; i < visibleEditors.Count; i++)
        {
            var editor = visibleEditors[i];
            editor.OnDisable();
        }
    }

    public void SetFocusByCurve(BadassCurve curve)
    {
        for (int i = 0; i < visibleEditors.Count; i++)
        {
            var editor = visibleEditors[i];
            editor.IsFocused = false;
        }
        
        var e = visibleEditors.FirstOrDefault(x => x.curve == curve);
        
        if (e?.curve != null)
        {
            e.IsFocused = true;
        }
    }

    public void SelectByCurve(BadassCurve curve)
    {
        var e = visibleEditors.FirstOrDefault(x => x.curve == curve);
        
        if (e?.curve != null)
        {
            e.IsSelected = true;
        }
    }
}
#endif