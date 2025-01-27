using System;
using System.Collections.Generic;
using System.Linq;
using LightSideCore.Editor;
using LSCore.AnimationsModule;
using LSCore.Editor;
using LSCore.Extensions;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;
using static BadassAnimation.Handler;
using Object = UnityEngine.Object;

public class BadassAnimationWindow : OdinMenuEditorWindow
{
    private static Popup treePopup;
    private static GUIStyle enabledStyle;
    private static GUIStyle EnabledStyle => enabledStyle ??= new()
    {
        normal = new GUIStyleState { textColor = new Color(0.82f, 0.82f, 0.82f), },
        hover = new GUIStyleState { textColor = Color.white },
    };

    private static GUIStyle disabledStyle;
    private static GUIStyle DisabledStyle => disabledStyle ??= new()
    {
        normal = new GUIStyleState { textColor = new Color(1f, 1f, 1f, 0.47f), },
        hover = new GUIStyleState { textColor = Color.white },
    };

    public bool isDopesheet = true;
    public bool IsDopesheet
    {
        get => isDopesheet;
        set
        {
            if (value != isDopesheet)
            {
                editor.curvesEditor.IsYBlocked = value;
            }
            isDopesheet = value;
        }
    }
    
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
        private Color dopesheetKeyColor = new (0.75f, 0.75f, 0.75f, 1);

        public bool Snapping
        {
            get => curvesEditor.snapping;
            set => curvesEditor.snapping = value;
        }
        
        public float SnappingStep => Snapping ? curvesEditor.SnappingStep : 0;
        public List<CurveItem> CurveItems => curveItems ??= new List<CurveItem>();
        public bool NeedUpdateItemSelection { get; set; }
        public bool IsSelectionInProgress { get; private set; }

        public CurvesEditor(BadassAnimationWindow window, LSHandles.TimePointer pointer, BadassMultiCurveEditor curvesEditor)
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
                        if(!item.IsSelected) action += () =>
                        {
                            item.Select(true);
                            item.editor.IsSelected = true;
                        };
                    }
                    else
                    {
                        if(item.IsSelected) action += () =>
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

        private void OverridePointPosition(BadassCurveEditor badassCurveEditor, int index, ref BezierPoint point, Func<bool> check)
        {
            if (BadassCurve.IsTangent(index)) goto moveAway;
            
            MenuItem curveItem = CurveItems.FirstOrDefault(x => x.editor == badassCurveEditor);

            if(curveItem == null) goto moveAway;
            
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
                if(!curveItem.IsVisibleSelf) continue;
                
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
                        else if(Math.Abs(lastY - p.y) < 0.001f)
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
                        var c = wasSelected ? LSHandles.Styles.selectionColor.SetAlpha(0.5f) : Color.gray.SetAlpha(0.5f);
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
    
    public class ToolbarDummy : OdinMenuItem
    {
        public ToolbarDummy(OdinMenuTree tree) : base(tree, string.Empty, null)
        {
            Style = Style.Clone();
            Style.Height = 50;
        }
        
        protected override void OnDrawMenuItem(Rect rect, Rect labelRect)
        {
            GUI.DrawTexture(rect, EditorUtils.GetTextureByColor(new Color(0.15f, 0.14f, 0.23f)));
        }
    }
    
    public class Toolbar
    {
        public event Action<Rect> NeedAddHandler;
        public event Action<BadassAnimationClip> SelectionConfirmed;
        private Color selectionColor = new(1f, 0.54f, 0.16f);
        private OdinSelector<BadassAnimationClip> clipSelector;
        private readonly List<BadassAnimationClip> badassAnimationClips;
        private BadassAnimationWindow window;

        public bool IsPlaying
        {
            get => window.isPlaying;
            set => window.isPlaying = value;
        }
        
        public bool IsReversed
        {
            get => window.isReversed;
            set => window.isReversed = value;
        }
        
        public bool IsLoop
        {
            get => window.timePointer.loop;
            set => window.timePointer.loop = value;
        }
        
        public bool Snapping
        {
            get => window.editor.Snapping;
            set => window.editor.Snapping = value;
        }
        
        public bool IsRecording
        {
            get => window.IsRecording;
            set => window.IsRecording = value;
        }

        public BadassAnimationClip CurrentClip
        {
            get => window.animation.Clip;
            set => window.animation.Clip = value;
        }
        
        public Toolbar(OdinMenuTree tree, BadassAnimationWindow window)
        {
            this.window = window;
            badassAnimationClips = window.animation.data.Select(x => x.clip).ToList();
            var createNewClip = CreateInstance<BadassAnimationClip>();
            createNewClip.name = CreateNewClipLabel;
            badassAnimationClips.Add(createNewClip);
            if (CurrentClip == null)
            {
                CurrentClip = badassAnimationClips.FirstOrDefault();
            }
            else
            {
                UpdateAnimationData();
            }
            
            CreateClipSelector(badassAnimationClips);
        }

        public void UpdateAnimationData()
        {
            var last = CurrentClip;
            CurrentClip = null;
            CurrentClip = last;
        }
        
        public void OnClipAdded(BadassAnimationClip clip)
        {
            badassAnimationClips.Insert(badassAnimationClips.Count - 1, clip);
            CreateClipSelector(badassAnimationClips);
        }

        private void CreateClipSelector(IEnumerable<BadassAnimationClip> clips)
        {
            clipSelector = new GenericSelector<BadassAnimationClip>("Select Clip", false, x => x.name, clips);
            clipSelector.SelectionConfirmed += OnSelectionChanged;
        }
        
        public void OnGUI(Rect rect)
        {
            GUI.DrawTexture(rect, EditorUtils.GetTextureByColor(new Color(0.15f, 0.14f, 0.23f)));
            var playbarRect = rect.SplitVertical(0, 2);
            var buttons = playbarRect.AlignCenter(60);

            var left = SdfIconType.CaretLeft;
            var right = SdfIconType.CaretRight;

            if (IsPlaying)
            {
                if(IsReversed) left = SdfIconType.CaretLeftFill;
                else           right = SdfIconType.CaretRightFill;
            }
            
            
            var enableStyle = EnabledStyle;

            if (IsReversed)
            {
                enableStyle = new GUIStyle(enableStyle);
                enableStyle.normal.textColor = selectionColor;
                enableStyle.hover.textColor = selectionColor;
            }

            if (SirenixEditorGUI.SDFIconButton(buttons.TakeFromLeft(20), left, enableStyle))
            {
                if (IsReversed)
                {
                    IsPlaying = !IsPlaying;
                }
                IsReversed = true;
            }

            enableStyle = EnabledStyle;
            
            if (!IsReversed)
            {
                enableStyle = new GUIStyle(enableStyle);
                enableStyle.normal.textColor = selectionColor;
                enableStyle.hover.textColor = selectionColor;
            }
            
            if (SirenixEditorGUI.SDFIconButton(buttons.TakeFromLeft(20), right, enableStyle))
            {
                if (!IsReversed)
                {
                    IsPlaying = !IsPlaying;
                }
                IsReversed = false;
            }
            
            var disable = new GUIStyle(DisabledStyle);
            disable.hover.textColor = disable.normal.textColor;
            
            if (SirenixEditorGUI.SDFIconButton(buttons, SdfIconType.ArrowRepeat, IsLoop ? EnabledStyle : disable))
            {
                IsLoop = !IsLoop;
            }

            playbarRect.TakeFromRight(5);
            window.timePointer.SnappingStep = Snapping && !IsPlaying ? window.editor.curvesEditor.SnappingStep : 0;
            if (SirenixEditorGUI.SDFIconButton(playbarRect.TakeFromRight(20), Snapping ? SdfIconType.BandaidFill : SdfIconType.Bandaid, Snapping ? EnabledStyle : disable))
            {
                Snapping = !Snapping;
            }
            
            playbarRect.TakeFromLeft(5);
            var c = GUI.color;
            if(IsRecording) GUI.color = red;
            if (SirenixEditorGUI.SDFIconButton(playbarRect.TakeFromLeft(20), IsRecording ? SdfIconType.RecordCircleFill : SdfIconType.RecordCircle, IsRecording ? EnabledStyle : disable))
            {
                IsRecording = !IsRecording;
            }

            GUI.color = c;
            
            var toolbarRect = rect.SplitVertical(1, 2);
            var savedRect = toolbarRect;
            
            var plusButtonRect = toolbarRect.TakeFromLeft(25).AlignCenter(25, 25);
            if (SirenixEditorGUI.SDFIconButton(plusButtonRect, SdfIconType.Plus, EnabledStyle))
            {
                NeedAddHandler?.Invoke(savedRect);
            }
            
            if (GUI.Button(toolbarRect, CurrentClip == null ? "Select Clip..." : CurrentClip.name))
            {
                var w = clipSelector.ShowInPopup();
            }
        }

        private void OnSelectionChanged(IEnumerable<BadassAnimationClip> clips)
        {
            var clip = clips.FirstOrDefault();
            SelectionConfirmed?.Invoke(clip);
        }
    }

    public class CurveItem : MenuItem
    {
        public CurvesEditor curvesEditor;
        public BadassCurveEditor editor;
        public BadassCurve curve;
        private BadassAnimation.Handler handler;
        private BadassAnimationClip clip;
        public Color color;

        public bool TryGetCurve(out BadassCurve curve)
        {
            if (clip.namesToCurvesByHandlerGuids.TryGetValue(handler.guid, out var curves))
            {
                if (curves.TryGetValue(Name, out curve))
                {
                    this.curve = curve;
                    return true;
                }
            }
             
            curve = null;
            this.curve = curve;
            return false;
        }

        public void CreateCurve()
        {
            curve = new BadassCurve();
            clip.Add(handler, Name, curve);
            EditorUtility.SetDirty(clip);
        }

        public CurveItem(OdinMenuTree tree, string name, Color color, BadassAnimationClip clip, BadassAnimation.Handler handler, CurvesEditor editor) : base(tree, name, null)
        {
            curvesEditor = editor;
            this.color = color;
            this.clip = clip;
            this.handler = handler;
            Color = color.SetAlpha(0.5f);
            Style = Style.Clone();
            Style.SelectedColorDarkSkin = Color;
            Style.SelectedColorLightSkin = Color;
            TryGetCurve(out _);
        }

        protected override void OnDrawMenuItem(Rect rect, Rect labelRect)
        {
            base.OnDrawMenuItem(rect, labelRect);

            var e = Event.current;
            if (e.OnContextClick(rect))
            {
                var popup = new Popup();
                popup.onGui = () =>
                {
                    if (popup.DrawButton("Delete Curve"))
                    {
                        DeleteCurve();
                    }
                };
                popup.Show(e.mousePosition);
            }
            
            if (handler.TryGetEvaluator(Name, out var evaluator))
            {
                rect.TakeFromRight(5);
                rect.TakeFromRight(40);
                EditorGUI.FloatField(rect.TakeFromRight(40), evaluator.y);
            }
        }

        public void DeleteCurve()
        {
            curvesEditor.curvesEditor.Remove(editor);
            curvesEditor.CurveItems.Remove(this);
            clip.Remove(handler, Name);
            handler.RemoveEvaluator(Name);
            editor = null;
            curve = null;
        }

        public void TryCreateCurveEditor(Object context)
        {
            if(editor != null) return;
            
            if (TryGetCurve(out var c))
            {
                var bce = new BadassCurveEditor(c, context);
                bce.curveColor = color;
                bce.tangentLineColor = color;
                bce.IsLocked = IsLockedSelf;
                bce.IsVisible = IsVisibleSelf;
                    
                LockedSelfChanged += state => bce.IsLocked = state;
                VisibleSelfChanged += state => bce.IsVisible = state;
                
                editor = bce;
                curvesEditor.Add(bce);
                curvesEditor.CurveItems.Add(this);
            }
        }
    }

    public class HandlerItem : MenuItem
    {
        public BadassAnimation.Handler handler;
        private BadassAnimation animation;

        public HandlerItem(OdinMenuTree tree, string name, BadassAnimation animation, BadassAnimation.Handler handler) : base(tree, name, null)
        {
            this.animation = animation;
            this.handler = handler;
        }

        protected override void OnDrawMenuItem(Rect rect, Rect labelRect)
        {
            base.OnDrawMenuItem(rect, labelRect);
            
            var e = Event.current;
            if (e.OnContextClick(rect))
            {
                var popup = new Popup();
                popup.onGui = () =>
                {
                    if (popup.DrawButton("Delete Handler"))
                    {
                        foreach (var curveItem in ChildMenuItems.OfType<CurveItem>())
                        {
                            curveItem.DeleteCurve();
                        }
                        Remove();
                        animation.Remove(handler);
                        handler.Stop();
                    }
                };
                popup.Show(e.mousePosition);
            }
        }
    }

    public class HandlerNoItem : MenuItem
    {
        public BadassAnimation.Handler handler;
        private BadassAnimation animation;
        private PropertyTree propertyTree;
        
        public HandlerNoItem(OdinMenuTree tree, string name, object value, BadassAnimation animation, BadassAnimation.Handler handler) : base(tree, name, value)
        {
            this.animation = animation;
            this.handler = handler;
            propertyTree = PropertyTree.Create(handler);
        }

        public override void DrawMenuItem(int indentLevel)
        {
            var last = EditorGUI.indentLevel;
            EditorGUI.indentLevel = indentLevel;
            EditorUtils.DrawInBoxFoldout("Handler", Draw, this, false);
            EditorGUI.indentLevel = last;
        }

        private void Draw()
        {
            EditorGUI.BeginChangeCheck();
            propertyTree.Draw(false);
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(animation);
            }
        }
    }
    
    public class RootMenuItem : MenuItem
    {
        public RootMenuItem(OdinMenuTree tree, string name) : base(tree, name, null) { }

        protected override void OnDrawMenuItem(Rect rect, Rect labelRect)
        {
            Rect = ConvertRect(rect);
            BaseOnDrawMenuItem(rect, labelRect);
        }
    }

    public class MenuItem : OdinMenuItem
    {
        private bool isLockedSelf;
        private bool isVisibleSelf = true;

        public new Rect Rect { get; protected set; }
        public Color Color { get; protected set; }
        
        public bool IsLockedSelf
        {
            get { return isLockedSelf; }
            set
            {
                if (value != isLockedSelf) LockedSelfChanged?.Invoke(value);
                isLockedSelf = value;
            }
        }

        public bool IsVisibleSelf
        {
            get { return isVisibleSelf; }
            set
            {
                if (value != isVisibleSelf) VisibleSelfChanged?.Invoke(value);
                isVisibleSelf = value;
            }
        }

        public event Action<bool> VisibleSelfChanged;
        public event Action<bool> LockedSelfChanged;

        public bool IsLocked
        {
            set
            {
                IsLockedSelf = value;
                
                foreach (var item in ChildMenuItems)
                {
                    if (item is MenuItem menuItem)
                    {
                        menuItem.IsLocked = value;
                    }
                }
                
                if (!value)
                {
                    var cur = Parent;
                
                    while (cur != null)
                    {
                        if (cur is MenuItem menuItem)
                        {
                            menuItem.IsLockedSelf = false;
                        }
                    
                        cur = cur.Parent;    
                    }
                }
            }
        }
        
        public new bool IsVisible
        {
            set
            {
                IsVisibleSelf = value;
                
                foreach (var item in ChildMenuItems)
                {
                    if (item is MenuItem menuItem)
                    {
                        menuItem.IsVisible = value;
                    }
                }

                if (value)
                {
                    var cur = Parent;
                
                    while (cur != null)
                    {
                        if (cur is MenuItem menuItem)
                        {
                            menuItem.IsVisibleSelf = true;
                        }
                    
                        cur = cur.Parent;
                    }
                }
            }
        }
        
        public MenuItem(OdinMenuTree tree, string name, object value) : base(tree, name, value)
        {
            Style.IndentAmount = 10;
            Style.Height = 20;
            Style.AlignTriangleLeft = true;
            Style.TrianglePadding = 0;
            Color = LSHandles.Styles.selectionColor.SetAlpha(0.5f);
            Style.SelectedColorLightSkin = Color;
            Style.SelectedColorDarkSkin = Color;
        }

        protected void BaseOnDrawMenuItem(Rect rect, Rect labelRect)
        {
            base.OnDrawMenuItem(rect, labelRect);
        }

        public bool IsShowing
        {
            get
            {
                var parent = Parent;

                while (parent != null)
                {
                    if (!parent.Toggled)
                    {
                        return false;
                    }
                    
                    parent = parent.Parent;
                }
                
                return MenuItemIsBeingRendered;
            }
        }

        protected Rect ConvertRect(Rect rect)
        {
            Vector2 screenPosTL = GUIUtility.GUIToScreenPoint(rect.position);
            
            Vector2 screenPosBR = GUIUtility.GUIToScreenPoint(
                new Vector2(rect.xMax, rect.yMax)
            );
            
            Rect screenRect = new Rect(
                screenPosTL,
                screenPosBR - screenPosTL
            );
            
            screenRect.x -= window.position.x;
            screenRect.y -= window.position.y;
            screenRect.y -= 25;
            return screenRect;
        }
        
        protected override void OnDrawMenuItem(Rect rect, Rect labelRect)
        {
            Rect = ConvertRect(rect);
            rect.TakeFromRight(10);
            
            if (SirenixEditorGUI.SDFIconButton(rect.TakeFromRight(15), isVisibleSelf ? SdfIconType.EyeFill : SdfIconType.EyeSlashFill, isVisibleSelf ? EnabledStyle : DisabledStyle))
            {
                IsVisible = !isVisibleSelf;
            }
            
            if (SirenixEditorGUI.SDFIconButton(rect.TakeFromRight(15), isLockedSelf ? SdfIconType.LockFill : SdfIconType.UnlockFill, isLockedSelf ? EnabledStyle : DisabledStyle))
            {
                IsLocked = !isLockedSelf;
            }
        }
    }

    private const string CreateNewClipLabel = "Create New Clip...";
    
    [HideInInspector] public CurvesEditor editor;
    [HideInInspector] public LSHandles.TimePointer timePointer;
    [HideInInspector] public bool isReversed;
    private bool isRecording;
    private bool isPlaying;

    private BadassAnimation animation;
    private Toolbar toolbar;

    private bool IsRecording
    {
        get
        {
            return isRecording;
        }
        set
        {
            if (isRecording != value)
            {
                isRecording = value;
                if (value)
                {
                    AnimationMode.StartAnimationMode();
                    Patchers._Transform.TransformTool.Calling += OnCallingTransformTool;
                    Patchers._Transform.TransformTool.Called += OnCalledTransformTool;
                    Patchers._EditorGUI.BeginPropertyInternal.Calling += OnSetDirty;
                    Patchers._EditorGUI.EndProperty.Called += OnSetDirty;
                }
                else
                {
                    AnimationMode.StopAnimationMode();
                    Patchers._Transform.TransformTool.Calling -= OnCallingTransformTool;
                    Patchers._Transform.TransformTool.Called -= OnCalledTransformTool;
                    Patchers._EditorGUI.BeginPropertyInternal.Calling -= OnSetDirty;
                    Patchers._EditorGUI.EndProperty.Called -= OnSetDirty;
                }
            }
        }
    }

    private Queue<(Vector3 lastPosition, Vector3 lastEulerAngles, Vector3 lastScale)> transformsData = new();
    
    private void OnCallingTransformTool(Transform transform)
    {
        transformsData.Enqueue((transform.localPosition, transform.localEulerAngles, transform.localScale));
    }
    
    private void OnCalledTransformTool(Transform transform, bool isChanged)
    {
        var (lastPosition, lastEulerAngles, lastScale) = transformsData.Dequeue();
        if (!isChanged) return;
        var data = GetChangedAxis(lastPosition, transform.localPosition);
        
        if (data.Count > 0)
        {
            Modify<TransformPosition>();
            return;
        }
        
        data = GetChangedAxis(lastEulerAngles, transform.localEulerAngles);
        
        if (data.Count > 0)
        {
            Modify<TransformRotation>();
            return;
        }
        
        data = GetChangedAxis(lastScale, transform.localScale);
        
        if (data.Count > 0)
        {
            Modify<TransformScale>();
            return;
        }

        void Modify<T>() where T : BaseTransformHandler, new()
        {
            if (!objectByHandlerType.TryGetValue(transform, out Dictionary<Type, BadassAnimation.Handler> handlers))
            {
                objectByHandlerType[transform] = handlers = new Dictionary<Type, BadassAnimation.Handler>();
            }
        
            var handler = TryAddTransformHandler<T>(handlers, transform);
            OdinMenuItem handlerItem = FindHandlerItem(handler);

            foreach (var (axis, value) in data)
            {
                ModifyCurve(handlerItem, axis, value);
            }
        }

        List<(string axis, float value)> GetChangedAxis(Vector3 last, Vector3 cur)
        {
            (string axis, float value) data = default;
            var result = new List<(string axis, float value)>();
            
            if (!Mathf.Approximately(last.x, cur.x))
            {
                data.axis = "x";
                data.value = cur.x;
                result.Add(data);
            }
            
            if(!Mathf.Approximately(last.y, cur.y))
            {
                data.axis = "y";
                data.value = cur.y;
                result.Add(data);
            }
            
            if(!Mathf.Approximately(last.z, cur.z))
            {
                data.axis = "z";
                data.value = cur.z;
                result.Add(data);
            }
            
            return result;
        }
    }

    private Dictionary<Object, Dictionary<Type, BadassAnimation.Handler>> objectByHandlerType = new();
    private static readonly Stack<SerializedProperty> propertyStack = new ();
    private static BadassAnimationWindow window;
    
    private void OnSetDirty(Rect arg1, GUIContent arg2, SerializedProperty arg3)
    {
        propertyStack.Push(arg3);
        EditorGUI.BeginChangeCheck();
    }

    private int axisIsDirty;
    
    private void OnSetDirty()
    {
        var prop = propertyStack.Pop();
        if(prop == null) return;
        if (EditorGUI.EndChangeCheck())
        {
            var target = prop.serializedObject.targetObject;
            
            if (target is Transform transform)
            {
                HandleTransformPropertyChanges(transform, prop);
            }
        }
    }

    private OdinMenuItem FindHandlerItem(BadassAnimation.Handler handler)
    {
        OdinMenuItem handlerItem = MenuTree.RootMenuItem.GetChildMenuItemsRecursive(true)
            .OfType<HandlerItem>().FirstOrDefault(x => x.handler == handler);
        return handlerItem;
    }
    
    private void HandleTransformPropertyChanges(Transform transform, SerializedProperty prop)
    {
        if (!objectByHandlerType.TryGetValue(transform, out Dictionary<Type, BadassAnimation.Handler> handlers))
        {
            objectByHandlerType[transform] = handlers = new Dictionary<Type, BadassAnimation.Handler>();
        }

        TryAddHandler<TransformPosition>("m_LocalPosition");
        TryAddHandler<TransformRotation>("m_LocalRotation");
        TryAddHandler<TransformScale>("m_LocalScale");

        void TryAddHandler<T>(string propName) where T : BaseTransformHandler, new()
        {
            var split = prop.propertyPath.Split('.');
            if (split.Length < 2) return;

            if (split[0] == propName)
            {
                var handler = TryAddTransformHandler<T>(handlers, transform);
                OdinMenuItem handlerItem = FindHandlerItem(handler);

                if (handlerItem != null)
                {
                    if (split[0] == "m_LocalRotation")
                    {
                        var axis = split[1];
                        Patchers._Transform.SetLocalEulerAngles.Called += TryModifyRotationCurve;

                        void TryModifyRotationCurve(Vector3 eulerAngles)
                        {
                            Patchers._Transform.SetLocalEulerAngles.Called -= TryModifyRotationCurve;
                            
                            TryModify("x", eulerAngles.x);
                            TryModify("y", eulerAngles.y);
                            TryModify("z", eulerAngles.z);
                            void TryModify(string curveName, float value)
                            {
                                if (axis == curveName)
                                {
                                    ModifyCurve(handlerItem, curveName, value);
                                }
                            }
                        }
                        return;
                    }
                    
                    TryModifyCurve("x");
                    TryModifyCurve("y");
                    TryModifyCurve("z");

                    void TryModifyCurve(string curveName)
                    {
                        if (split[1] == curveName)
                        {
                            ModifyCurve(handlerItem, curveName, prop.floatValue);
                        }
                    }
                }
            }
        }
    }

    private BadassAnimation.Handler TryAddTransformHandler<T>(Dictionary<Type, BadassAnimation.Handler> handlers, Transform transform) where T : BaseTransformHandler, new()
    {
        if (!handlers.TryGetValue(typeof(T), out var handler))
        {
            handlers[typeof(T)] = handler = new T();
            var transformHandler = (T)handler;
            transformHandler.transform = transform;
            AddHandler(handler);
        }

        return handler;
    }
    
    private void ModifyCurve(OdinMenuItem handlerItem, string curveName, float value)
    {
        var curveItem = handlerItem.ChildMenuItems.OfType<CurveItem>()
            .FirstOrDefault(x => x.Name == curveName);
        if (curveItem != null)
        {
            if (!curveItem.TryGetCurve(out _))
            {
                CreateCurve(curveItem);
            }

            curveItem.TryCreateCurveEditor(this);
            LSHandles.StartMatrix(editor.curvesEditor.matrix);
            curveItem.editor.SetKeyY(timePointer.Time, value);
            LSHandles.EndMatrix();
        }
    }

    private BadassAnimationClip CurrentClip
    {
        get => toolbar?.CurrentClip;
        set
        {
            if (toolbar != null) toolbar.CurrentClip = value;
        }
    }

    private static Color red = new(1f, 0.32f, 0.36f);
    private static Color green = new(0.55f, 0.86f, 0f);
    private static Color blue = new(0.16f, 0.56f, 1f);
    private static Color alpha = new(0.87f, 0.87f, 0.87f);

    private static Func<string, OdinMenuTree, BadassAnimation.Handler, string> GetTransformHandlerMenuItemAction()
    {
        return (path, tree, handler) =>
        {
            var itemName = GetTransformMenuItemName(handler); 
            tree.AddMenuItemAtPath(path, new MenuItem(tree, itemName, null));
            return itemName;
        };
    }

    private static string GetTransformMenuItemName(BadassAnimation.Handler handler)
    {
        return $"Transform ({handler.Target.name})";
    }

    private static readonly Dictionary<Type, Func<string, OdinMenuTree, BadassAnimation.Handler, string>> addGroupItemActionsByType = new()
    {
        { typeof(TransformPosition), GetTransformHandlerMenuItemAction() },
        { typeof(TransformRotation), GetTransformHandlerMenuItemAction() },
        { typeof(TransformScale), GetTransformHandlerMenuItemAction() },
    };
    
    private static readonly Dictionary<Type, Action<string, OdinMenuTree, BadassAnimationClip, BadassAnimation.Handler, CurvesEditor>> addItemActionsByType = new()
    {
        { typeof(Color), (path, tree, clip, handler, editor) =>
            {
                tree.AddMenuItemAtPath(path, new CurveItem(tree, PropNames.r, red, clip, handler, editor));
                tree.AddMenuItemAtPath(path, new CurveItem(tree, PropNames.g, green, clip, handler, editor));
                tree.AddMenuItemAtPath(path, new CurveItem(tree, PropNames.b, blue, clip, handler, editor));
                tree.AddMenuItemAtPath(path, new CurveItem(tree, PropNames.a, alpha, clip, handler, editor));
            } 
        },
        { typeof(Vector3), (path, tree, clip, handler, editor) =>
            {
                tree.AddMenuItemAtPath(path, new CurveItem(tree, PropNames.x, red, clip, handler, editor));
                tree.AddMenuItemAtPath(path, new CurveItem(tree, PropNames.y, green, clip, handler, editor));
                tree.AddMenuItemAtPath(path, new CurveItem(tree, PropNames.z, blue, clip, handler, editor));
            } 
        },
        { typeof(Vector2), (path, tree, clip, handler, editor) =>
            {
                tree.AddMenuItemAtPath(path, new CurveItem(tree, PropNames.x, red, clip, handler, editor));
                tree.AddMenuItemAtPath(path, new CurveItem(tree, PropNames.y, green, clip, handler, editor));
            } 
        },
        { typeof(float), (path, tree, clip, handler, editor) =>
            {
                tree.AddMenuItemAtPath(path, new CurveItem(tree, PropNames.value, red, clip, handler, editor));
            } 
        },
    };
    
    
    public static void ShowWindow(BadassAnimation animation)
    {
        var window = GetWindow<BadassAnimationWindow>();
        window.animation = animation;
        window.Show();
    }

    private void OnSelectionChanged(SelectionChangedType selectionChangedType)
    {
        if(editor.IsSelectionInProgress) return;
        
        editor.NeedUpdateItemSelection = true;
        foreach (var curveItem in editor.CurveItems)
        {
            curveItem.editor.IsSelected = false;
        }
        
        foreach (var odinMenuItem in MenuTree.Selection)
        {
            if (odinMenuItem is CurveItem curveItem)
            {
                if (curveItem.editor != null)
                {
                    curveItem.editor.IsSelected = true;
                    editor.SetFocusByCurve(curveItem.curve);
                }
            }
        }
    }

    protected override OdinMenuTree BuildMenuTree()
    {
        objectByHandlerType.Clear();
        IsRecording = false;
        lastTime = EditorApplication.timeSinceStartup;
        animation.TryInit();
        var tree = new OdinMenuTree();
        tree.Selection.SelectionChanged += OnSelectionChanged;
        
        var data = animation.data.FirstOrDefault(x => x.clip);

        if (data == null)
        {
            return tree;
        }
        
        timePointer ??= new LSHandles.TimePointer();
        
        if (toolbar == null)
        {
            MenuWidth = 365;
            toolbar = new Toolbar(tree, this);
            
            toolbar.NeedAddHandler += NeedAddHandler;
            
            toolbar.SelectionConfirmed += clip =>
            {
                if(clip == CurrentClip) return;
                
                if (clip.name == CreateNewClipLabel)
                {
                    CreateNewClip();
                }
                else
                {
                    CurrentClip = clip;
                }
                
                editor = null;
                timePointer = null;
                ForceMenuTreeRebuild();
            };
        }
        
        tree.AddMenuItemAtPath(string.Empty, new ToolbarDummy(tree));
        tree.AddMenuItemAtPath(string.Empty, new RootMenuItem(tree, animation.name));
        var currentClip = CurrentClip;
        handlerNamesCounter.Clear();
        editor ??= new(this, timePointer, new());
        editor.pointer = timePointer;
        editor.Clear();
        var toRemove = new HashSet<string>(currentClip.namesToCurvesByHandlerGuids.Keys);
        
        if (animation.TryGetData(currentClip, out var d))
        {
            foreach (var handler in d.handlers)
            {
                var target = handler.Target;
                
                if (target != null)
                {
                    if (!objectByHandlerType.TryGetValue(target, out var types))
                    {
                        objectByHandlerType[target] = types = new();
                    }
                    
                    types.Add(handler.GetType(), handler);
                }
                
                AddExistHandler(tree, currentClip, handler);
                toRemove.Remove(handler.guid);
            }
        }

        foreach (var handlerGuid in toRemove)
        {
            currentClip.Remove(handlerGuid);
        }

        foreach (var item in tree.EnumerateTree())
        {
            if (item is CurveItem curveItem)
            {
                curveItem.TryCreateCurveEditor(this);
            }
        }
        
        editor.curvesEditor.IsYBlocked = IsDopesheet;
        tree.Selection.SupportsMultiSelect = true;
        
        return tree;
    }

    private Dictionary<string, int> handlerNamesCounter = new();

    private void AddHandler(BadassAnimation.Handler handler)
    {
        if (animation.TryGetData(CurrentClip, out var data))
        {
            data.handlers.Add(handler);
            EditorUtility.SetDirty(animation);
            AddExistHandler(MenuTree, CurrentClip, handler);
            UpdateAnimationComponent();
        }
    }
    
    private void AddExistHandler(OdinMenuTree tree, BadassAnimationClip clip, BadassAnimation.Handler handler)
    {
        if (string.IsNullOrEmpty(handler.guid))
        {
            handler.guid = Guid.NewGuid().ToString("N");
            EditorUtility.SetDirty(animation);
        }

        var startPath = animation.name;

        if (addGroupItemActionsByType.TryGetValue(handler.GetType(), out var func))
        {
            var itemName = func(startPath, tree, handler);
            startPath += $"/{itemName}";
        }
        
        if (addItemActionsByType.TryGetValue(handler.ValueType, out var action))
        {
            handlerNamesCounter.TryGetValue(handler.HandlerName, out var counter);
            handlerNamesCounter[handler.HandlerName] = ++counter;
            
            var handlerName = $"{handler.HandlerName}({counter})";
            tree.AddMenuItemAtPath(startPath, new HandlerItem(tree, handlerName, animation, handler));
            
            var path = $"{startPath}/{handlerName}";
            tree.AddMenuItemAtPath(path, new HandlerNoItem(tree, handler.HandlerName, null, animation, handler));
            action(path, tree, clip, handler, editor);
        }
    }

    private void CreateCurve(CurveItem curveItem)
    {
        curveItem.CreateCurve();
        UpdateAnimationComponent();
    }

    private void UpdateAnimationComponent()
    {
        var clip = animation.Clip;
        animation.Clip = null;
        animation.Clip = clip;
    }

    public Rect Rect
    {
        get
        {
            var rect = position;
            rect.position = Vector2.zero;
            return rect;
        }
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        editor?.OnEnable();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        editor?.OnDisable();
        animation.Clip = null;
    }

    private double lastTime;

    protected override void DrawMenu()
    {
        if(CurrentClip == null) return;
        
        var rect = Rect;
        var menuRect = rect.TakeFromLeft(MenuWidth);
        var bottomButtonsRect = menuRect.TakeFromBottom(20);
        var toolbarRect = menuRect.TakeFromTop(50);
        var e = Event.current;
        var lastType = e.type;
        var lastMp = e.mousePosition;
        
        if (!e.OnRepaint() && !e.OnLayout() && (e.IsMouseOver(bottomButtonsRect) || e.IsMouseOver(toolbarRect)))
        {
            e.type = EventType.Used;
            e.mousePosition = Vector2.positiveInfinity;
        }
        
        base.DrawMenu();
        
        e.type = lastType;
        e.mousePosition = lastMp;
        toolbar?.OnGUI(toolbarRect);

        GUI.DrawTexture(bottomButtonsRect, EditorUtils.GetTextureByColor(new Color(0.15f, 0.14f, 0.23f)));
        
        var c = GUI.color;
        
        if(IsDopesheet) GUI.color = c.SetAlpha(0.5f);
        if (GUI.Button(bottomButtonsRect.TakeFromRight(55), "Curves"))
        {
            IsDopesheet = false;
        }
        GUI.color = c;
        
        if(!IsDopesheet) GUI.color = c.SetAlpha(0.5f);
        if (GUI.Button(bottomButtonsRect.TakeFromRight(75), "Dopesheet"))
        {
            IsDopesheet = true;
        }
        GUI.color = c;
    }

    protected override void OnImGUI()
    {
        if (IsRecording)
        {
            Repaint();
        }
        
        window = this;
        var e = Event.current;
        var lastType = Event.current.type;
        
        if (treePopup != null)
        {
            if (e.IsMouseOver(new Rect(treePopup.position, treePopup.size)))
            {
                if (!e.OnRepaint() && !e.OnLayout())
                {
                    e.Use();
                }
            }
        }

        if (toolbar == null) ForceMenuTreeRebuild();
        
        var rect = Rect;
        
        if (CurrentClip == null)
        { 
            var buttonRect = rect.AlignCenter(150, 50);
            if (GUI.Button(buttonRect, "Create New Clip"))
            {
                CreateNewClip();
            }
            return;
        }
        
        if (OdinObjectSelector.IsReadyToClaim(this, CurrentClip.GetInstanceID()))
        {
            if (animation.TryGetData(CurrentClip, out var data))
            {
                var claimed = OdinObjectSelector.Claim();

                if (claimed is BadassAnimation.Handler handler)
                {
                    AddHandler(handler);
                }
                else if (claimed is BadassAnimation.Event eevent)
                {
                    eevent.x = editor.lastXForEvent;
                    data.Add(eevent);
                    EditorUtility.SetDirty(animation);
                }
            }
        }

        rect.TakeFromLeft(MenuWidth);
        
        base.OnImGUI();
        
        var curveItemList = GetSelectedCurvesWithoutCurves().ToList();

        if (curveItemList.Count > 0)
        {
            var buttonRect = rect.AlignCenter(150, 50);
            if (GUI.Button(buttonRect, "Create Curve"))
            {
                foreach (var curveItem in curveItemList)
                {
                    CreateCurve(curveItem);
                    curveItem.TryCreateCurveEditor(this);
                    editor.SetFocusByCurve(curveItem.curve);
                    toolbar!.UpdateAnimationData();
                }
            }
            
            return;
        }
        
        editor.OnGUI(rect);
        
        var keyPointsBounds = editor.curvesEditor.GetKeyPointsBounds();
        var length = keyPointsBounds.max.x;
        CurrentClip.length = length;
        timePointer.ClampRange = new Vector2(0, length);

        if (e.type == EventType.KeyDown)
        {
            if (e.keyCode == KeyCode.Space)
            {
                timePointer.SetTimeAtLastPosition();
                lastTime = EditorApplication.timeSinceStartup;
                isPlaying = !isPlaying;
            }   
        }
        
        if (isPlaying)
        {
            timePointer.Time -= (float)(EditorApplication.timeSinceStartup - lastTime) * isReversed.ToPosNeg();
            
            if (animation.TryGetData(CurrentClip, out var data))
            {
                foreach (var selectedEventToCall in SelectEvents(data.events, timePointer, isReversed))
                {
                    selectedEventToCall.Invoke();
                }
            }

            Repaint();
        }

        lastTime = EditorApplication.timeSinceStartup;

        if (e.OnRepaint())
        {
            animation.Editor_Evaluate(timePointer.Time);
        }
        
        if (treePopup != null)
        {
            GUI.BeginClip(rect);
            e.type = lastType;
            treePopup.position -= rect.position;
            treePopup.OnGUIInArea();
            GUI.EndClip();
        }
    }

    private IEnumerable<CurveItem> GetSelectedCurvesWithoutCurves()
    {
        foreach (var item in MenuTree.Selection)
        {
            if (item is CurveItem curveItem)
            {
                if (!curveItem.TryGetCurve(out _))
                {
                    yield return curveItem;
                }
            }
        }
    }

    private void CreateNewClip()
    {
        string path = EditorUtility.SaveFilePanelInProject(
            "Save Animation Clip",
            "NewAnimationClip",
            "asset",
            "Please enter a file name to save the Animation Clip to"
        );
                
        if (!string.IsNullOrEmpty(path))
        {
            var newClip = CreateInstance<BadassAnimationClip>();
                    
            AssetDatabase.CreateAsset(newClip, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
                    
            animation.Add(newClip, new(), new());
            toolbar?.OnClipAdded(newClip);
            EditorUtility.SetDirty(animation);
            CurrentClip = newClip;
        }
    }
    
    private void NeedAddHandler(Rect rect)
    {
        ShowTypeSelectionPopup<BadassAnimation.Handler>(rect);
    }
    
    private void NeedAddEvent(Rect rect)
    {
        ShowTypeSelectionPopup<BadassAnimation.Event>(rect);
    }
    
    private void ShowTypeSelectionPopup<T>(Rect rect)
    {
        OdinObjectSelector.Show(this, toolbar.CurrentClip.GetInstanceID(), null, typeof(T), position: rect);
    }

    public static IEnumerable<BadassAnimation.Event> SelectEvents(List<BadassAnimation.Event> events, LSHandles.TimePointer pointer, bool reverse)
    {
        var oldRealTime = pointer.OldRealTime;
        var realTime = pointer.RealTime;
        pointer.SyncRealTime();
        
        foreach (var selectedEventToCall in BadassAnimation.SelectEvents(events, oldRealTime, realTime, pointer.ClampRange, reverse))
        {
            yield return selectedEventToCall;
        }
    }
}