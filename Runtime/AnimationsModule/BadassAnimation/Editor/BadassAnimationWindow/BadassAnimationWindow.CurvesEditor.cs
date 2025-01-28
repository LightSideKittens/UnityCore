#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using DG.DemiEditor;
using LightSideCore.Editor;
using LSCore.Editor;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

public partial class BadassAnimationWindow
{
    [Serializable]
    public class CurvesEditor
    {
        [Serializable]
        public class Events
        {
            [SerializeReference] public List<BadassAnimation.Event> events;
        }

        public const float eventPointSize = 0.02f;
        public const float dopesheetKeySize = 0.02f;
        public BadassAnimationWindow window;
        public BadassAnimation animation;
        public LSHandles.TimePointer pointer;
        public BadassMultiCurveEditor curvesEditor;
        public float lastXForEvent;
        private List<CurveItem> curveItems;
        private List<BadassAnimation.Event> clickedEvents = new();
        private float lastMatrixYScale;
        private float lastMatrixY;
        private float lastCamY = float.NegativeInfinity;
        private Vector2 mp;
        private Rect position;
        private Color dopesheetKeyColor = new(0.75f, 0.75f, 0.75f, 1);

        public bool Snapping
        {
            get => curvesEditor.snapping;
            set => curvesEditor.snapping = value;
        }

        public float SnappingStep => Snapping ? curvesEditor.SnappingStep : 0;
        public List<CurveItem> CurveItems => curveItems ??= new List<CurveItem>();
        public bool NeedUpdateItemSelection { get; set; }
        public bool IsSelectionInProgress { get; private set; }

        public CurvesEditor(BadassAnimationWindow window, LSHandles.TimePointer pointer,
            BadassMultiCurveEditor curvesEditor)
        {
            this.window = window;
            animation = window.animation;
            this.pointer = pointer;
            this.curvesEditor = curvesEditor;
        }

        public void OnGUI(Rect position)
        {
            var e = Event.current;
            var mouseClicked = e.OnMouseDown(0, false);
            NeedUpdateItemSelection |= mouseClicked;

            if (float.IsNegativeInfinity(lastCamY))
            {
                InitLastData();
            }

            this.position = position;
            mp = e.mousePosition;
            curvesEditor.gridData.displayYGrid = !window.IsDopesheet;
            curvesEditor.gridData.displayYScale = !window.IsDopesheet;

            if (window.IsDopesheet)
            {
                curvesEditor.BeforeDraw += DopesheetGUI;
                curvesEditor.OverridePointPosForSelection += OverridePointPosition;
                var matrixPos = curvesEditor.matrix.GetPosition();
                var matrixScale = curvesEditor.matrix.lossyScale;
                matrixPos.y = lastMatrixY;
                matrixScale.y = lastMatrixYScale;
                curvesEditor.matrix = Matrix4x4.TRS(matrixPos, Quaternion.identity, matrixScale);
                curvesEditor.camData.position.y = lastCamY;
            }
            else
            {
                InitLastData();
            }

            curvesEditor.BeforeDraw += TimeGUI;
            curvesEditor.OnGUI(position, !window.IsDopesheet);

            if (e.IsPaste())
            {
                curvesEditor.PastePointsFromClipboard(pointer.Time);
            }

            if (NeedUpdateItemSelection)
            {
                Action action = null;
                foreach (var item in CurveItems)
                {
                    bool isSelected = mouseClicked ? item.editor.IsFocusOrAnyPointSelected : item.editor.IsSelected;
                    if (isSelected)
                    {
                        if (!item.IsSelected)
                            action += () =>
                            {
                                item.Select(true);
                                item.editor.IsSelected = true;
                            };
                    }
                    else
                    {
                        if (item.IsSelected)
                            action += () =>
                            {
                                item.Deselect();
                                item.editor.IsSelected = false;
                            };
                    }
                }

                IsSelectionInProgress = true;
                action?.Invoke();
                IsSelectionInProgress = false;
                NeedUpdateItemSelection = false;
            }

            Event.current.mousePosition = mp;
        }

        private void InitLastData()
        {
            lastMatrixYScale = curvesEditor.matrix.lossyScale.y;
            lastMatrixY = curvesEditor.matrix.GetPosition().y;
            lastCamY = curvesEditor.camData.position.y;
        }

        private bool isEventDragging;

        private void TimeGUI()
        {
            var e = Event.current;
            pointer.OnGUI();

            if (animation.TryGetData(window.CurrentClip, out var data))
            {
                if (!window.isPlaying)
                {
                    foreach (var selectedEventToCall in SelectEvents(data.events, pointer, window.isReversed))
                    {
                        selectedEventToCall.Invoke();
                    }
                }

                bool wasCleared = false;
                bool needSort = false;

                foreach (var eevent in data.events)
                {
                    var x = LSHandles.Matrix.MultiplyPoint3x4(new Vector3(eevent.x, 0, 0)).x;
                    var y = pointer.mouseClickArea.yMin;
                    var pos = new Vector2(x, y);
                    var mp = LSHandles.MouseInWorldPoint;

                    using (LSHandles.SetIdentityMatrix())
                    {
                        LSHandles.DrawCircle(pos, eventPointSize, green);
                        if (e.OnMouseDown(1, false))
                        {
                            if (!wasCleared)
                            {
                                wasCleared = true;
                                treePopup = null;
                                clickedEvents = new List<BadassAnimation.Event>();
                            }

                            if (LSHandles.IsInDistance(pos, LSHandles.MouseInWorldPoint, eventPointSize))
                            {
                                clickedEvents.Add(eevent);
                            }
                        }
                        else if (e.type == EventType.MouseDrag && e.button == 1)
                        {
                            isEventDragging = true;
                            treePopup?.OnClose();
                            treePopup = null;

                            foreach (var eventt in clickedEvents)
                            {
                                var mpx = LSHandles.SnapX(mp.x, SnappingStep);
                                eventt.x = mpx;
                                needSort = true;
                            }
                        }
                    }
                }

                if (e.OnMouseUp(1, false))
                {
                    isEventDragging = false;
                }

                if (needSort)
                {
                    data.events.Sort((a, b) => a.x.CompareTo(b.x));
                    EditorUtility.SetDirty(animation);
                }

                if (clickedEvents.Count > 0 && !isEventDragging)
                {
                    if (treePopup == null)
                    {
                        treePopup = new Popup(e.mousePosition, new Vector2(500, 300));
                        var firstEventX = clickedEvents[0].x;
                        e.Use();
                        var eventsObject = new Events { events = new List<BadassAnimation.Event>(clickedEvents) };
                        var tree = PropertyTree.Create(eventsObject);

                        treePopup.onClose = () => { tree.Dispose(); };

                        treePopup.onGui = () =>
                        {
                            EditorGUI.BeginChangeCheck();
                            tree.Draw(false);
                            if (EditorGUI.EndChangeCheck())
                            {
                                EditorUtility.SetDirty(animation);
                            }

                            if (!clickedEvents.SequenceEqual(eventsObject.events))
                            {
                                var removed = clickedEvents.Except(eventsObject.events).ToList();

                                foreach (var ev in removed)
                                {
                                    data.events.Remove(ev);
                                    clickedEvents.Remove(ev);
                                }

                                var added = eventsObject.events.Except(clickedEvents).ToList();

                                foreach (var ev in added)
                                {
                                    ev.x = firstEventX;
                                    data.Add(ev);
                                    clickedEvents.Add(ev);
                                }
                            }

                            if (Event.current.type == EventType.Repaint)
                            {
                                var rect = GUILayoutUtility.GetLastRect();
                                treePopup.size.y = rect.size.y + 30;
                            }

                            if (GUILayout.Button("Apply"))
                            {
                                clickedEvents.Clear();
                                treePopup.OnClose();
                                treePopup = null;
                            }
                        };
                    }
                    else
                    {
                        var x = LSHandles.Matrix.MultiplyPoint3x4(new Vector3(clickedEvents[0].x, 0, 0)).x;
                        var y = pointer.mouseClickArea.yMin;
                        var pos = new Vector2(x + eventPointSize * 2, y - eventPointSize * 2);

                        using (LSHandles.SetIdentityMatrix())
                        {
                            pos = LSHandles.WorldToScreen(pos);
                            treePopup.position.x = pos.x;
                            treePopup.position.y = pos.y;
                        }
                    }
                }
            }

            if (e.OnMouseDown(1, false))
            {
                var mp = LSHandles.MouseInWorldPoint;
                var mpx = LSHandles.SnapX(mp.x, SnappingStep);
                lastXForEvent = mpx;
                using (LSHandles.SetIdentityMatrix())
                {
                    if (pointer.ContainsClickArea(LSHandles.MouseInWorldPoint))
                    {
                        var popup = new Popup();

                        popup.onGui = () =>
                        {
                            if (popup.DrawButton("Create Event"))
                            {
                                var rect = GUILayoutUtility.GetLastRect();
                                window.NeedAddEvent(rect);
                            }
                        };

                        PopupWindow.Show(new Rect(e.mousePosition, new Vector2(10, 10)), popup);
                    }
                }
            }
        }

        private void OverridePointPosition(BadassCurveEditor badassCurveEditor, int index, ref BezierPoint point,
            Func<bool> check)
        {
            if (BadassCurve.IsTangent(index)) goto moveAway;

            MenuItem curveItem = CurveItems.FirstOrDefault(x => x.editor == badassCurveEditor);

            if (curveItem == null) goto moveAway;

            while (true)
            {
                if (curveItem != null)
                {
                    if (!curveItem.IsShowing) goto skip;

                    var p = point.e;
                    p.y = LSHandles.ScreenToWorld(curveItem.Rect.center).y;
                    point.e = p;

                    if (!check()) goto skip;

                    break;
                    skip:
                    curveItem = curveItem.Parent as MenuItem;
                    continue;
                }

                goto moveAway;
            }

            return;
            moveAway:
            point.e = Vector2.negativeInfinity;
        }

        private void DopesheetGUI()
        {
            Action drawSelected = null;

            foreach (var item in CurveItems)
            {
                MenuItem curveItem = item;
                var editor = item.editor;
                if (!curveItem.IsVisibleSelf) continue;

                while (curveItem != null)
                {
                    if (!curveItem.IsShowing)
                    {
                        goto skip;
                    }

                    var worldRect = curveItem.Rect;
                    worldRect.x = position.x;
                    worldRect.width = position.width;
                    worldRect = LSHandles.ScreenToWorld(worldRect);
                    var y = worldRect.center.y;
                    var c = curveItem.IsSelected ? curveItem.Color.SetAlpha(0.15f) : curveItem.Color.SetAlpha(0.05f);
                    LSHandles.currentDrawLayer -= 10000;
                    LSHandles.DrawRect(worldRect, c, false);
                    LSHandles.currentDrawLayer += 10000;
                    Vector2 range = Vector2.negativeInfinity;
                    float lastY = float.NegativeInfinity;
                    bool isCurveItem = curveItem is CurveItem;
                    bool wasSelected = false;

                    foreach (var index in editor.KeysIndexes)
                    {
                        var p = editor.curve.Points[index].e;
                        var isSelected = editor.IsPointSelected(index);

                        if (isCurveItem)
                        {
                            DrawSameRect(p, isSelected);
                        }

                        p.y = y;

                        if (!isSelected)
                        {
                            editor.DrawKey(p, dopesheetKeySize, dopesheetKeyColor);
                        }
                        else
                        {
                            drawSelected += () => editor.DrawSelectedKey(p, dopesheetKeySize);
                        }
                    }

                    Draw();
                    skip:
                    curveItem = curveItem.Parent as MenuItem;

                    void DrawSameRect(Vector2 p, bool isSelected)
                    {
                        if (float.IsNegativeInfinity(range.x))
                        {
                            range.x = p.x;
                            wasSelected |= isSelected;
                        }
                        else if (Math.Abs(lastY - p.y) < 0.001f)
                        {
                            range.y = p.x;
                            wasSelected |= isSelected;
                        }
                        else
                        {
                            Draw();
                            wasSelected |= isSelected;
                            range.x = p.x;
                            range.y = float.NegativeInfinity;
                        }

                        lastY = p.y;
                    }

                    void Draw()
                    {
                        if (float.IsNegativeInfinity(range.y)) return;
                        var w = range.y - range.x;
                        var r = new Rect(range.x, worldRect.y, w, worldRect.height);
                        r = r.AlignCenter(w, worldRect.height * 0.7f);
                        LSHandles.currentDrawLayer -= 10000;
                        var c = wasSelected
                            ? LSHandles.Styles.selectionColor.SetAlpha(0.5f)
                            : Color.gray.SetAlpha(0.5f);
                        LSHandles.DrawRect(r, c, false);
                        LSHandles.currentDrawLayer += 10000;
                    }
                }
            }

            drawSelected?.Invoke();
        }

        public void OnEnable()
        {
            curvesEditor.OnEnable();
        }

        public void OnDisable()
        {
            curvesEditor.OnDisable();
        }

        public void Add(BadassCurveEditor curveEditor)
        {
            curvesEditor.Add(curveEditor);
        }

        public void Clear()
        {
            curvesEditor.Clear();
        }

        public void SetFocusByCurve(BadassCurve curve)
        {
            curvesEditor.SetFocusByCurve(curve);
        }

        public void SelectByCurve(BadassCurve curve)
        {
            curvesEditor.SelectByCurve(curve);
        }
    }
}
#endif