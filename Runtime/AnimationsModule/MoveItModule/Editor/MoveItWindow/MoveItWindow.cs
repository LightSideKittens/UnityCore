#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using LSCore.AnimationsModule;
using LSCore.Editor;
using LSCore.Extensions;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public partial class MoveItWindow : OdinMenuEditorWindow
{
    static MoveItWindow()
    {
        MoveIt.NeedShowWindow += ShowWindow;
    }
    
    private static void ShowWindow(MoveIt animation)
    {
        var window = GetWindow<MoveItWindow>();
        window.animation = animation;
        window.Show();
    }
    
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

    private const string CreateNewClipLabel = "Create New Clip...";
    private static MoveItWindow window;
    
    [HideInInspector] public CurvesEditor curvesEditor;
    [HideInInspector] public GUIScene.TimePointer timePointer;
    [HideInInspector] public bool isReversed;
    
    private bool isPlaying;

    [HideInInspector] public MoveIt animation;
    private Toolbar toolbar;
    
    public Rect Rect
    {
        get
        {
            var rect = position;
            rect.position = Vector2.zero;
            return rect;
        }
    }

    public bool isDopesheet = true;
    public bool IsDopesheet
    {
        get => isDopesheet;
        set
        {
            if (value != isDopesheet)
            {
                curvesEditor.curvesEditor.IsYBlocked = value;
            }
            isDopesheet = value;
        }
    }

    private MoveItClip CurrentClip
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
    private static Color[] colors =
    {
        red, green, blue, alpha
    };

    private static Func<string, OdinMenuTree, MoveIt.Handler, string> GetTransformHandlerMenuItemAction()
    {
        return (path, tree, handler) =>
        {
            var itemName = GetTransformMenuItemName(handler); 
            tree.AddMenuItemAtPath(path, new MenuItem(tree, itemName, null));
            return itemName;
        };
    }

    private static string GetTransformMenuItemName(MoveIt.Handler handler)
    {
        return $"Transform ({handler.Target.name})";
    }

    private static readonly Dictionary<Type, Func<string, OdinMenuTree, MoveIt.Handler, string>> addGroupItemActionsByType = new()
    {
        /*{ typeof(TransformPosition), GetTransformHandlerMenuItemAction() },
        { typeof(TransformRotation), GetTransformHandlerMenuItemAction() },
        { typeof(TransformScale), GetTransformHandlerMenuItemAction() },*/
    };

    protected override void OnEnable()
    {
        base.OnEnable();
        curvesEditor?.OnEnable();
        Undo.undoRedoEvent += OnUndoRedoPerformed;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        AnimationMode.StopAnimationMode();
        curvesEditor?.OnDisable();
        animation.Editor_SetClip(null, IsPreview);
        Undo.undoRedoEvent -= OnUndoRedoPerformed;
        Undo.postprocessModifications -= OnPostProcessModifications;
    }
    
    private bool isUndoPerforming;

    protected override OdinMenuTree BuildMenuTree()
    {
        if (!isUndoPerforming)
        {
            IsRecording = false;
            IsPreview = false;
            lastTime = EditorApplication.timeSinceStartup;
        }
        
        objectByHandlerType.Clear();
        animation.TryInit();
        UpdateAnimationComponent();
        var tree = new OdinMenuTree();
        tree.Selection.SelectionChanged += OnSelectionChanged;
        
        var data = animation.data.FirstOrDefault(x => x.clip);

        if (data == null)
        {
            return tree;
        }

        timePointer ??= new GUIScene.TimePointer();
        timePointer.Looped -= OnLooped;
        timePointer.Looped += OnLooped;
        
        if (toolbar == null)
        {
            MenuWidth = 365;
            toolbar = new Toolbar(this);
            
            toolbar.NeedAddHandler += NeedAddHandler;
            toolbar.ClipSelectionConfirmed += SelectClip;
            toolbar.ClipDeleteConfirmed += DeleteClip;
        }
        
        tree.AddMenuItemAtPath(string.Empty, new ToolbarDummy(tree));
        tree.AddMenuItemAtPath(string.Empty, new RootMenuItem(tree, animation.name));
        var currentClip = CurrentClip;
        handlerNamesCounter.Clear();
        curvesEditor ??= new(this, timePointer, new());
        curvesEditor.pointer = timePointer;
        curvesEditor.Clear();
        var toRemove = new HashSet<string>(currentClip.evaluatorsByHandlerGuids.Keys);
        
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
        
        curvesEditor.curvesEditor.IsYBlocked = IsDopesheet;
        tree.Selection.SupportsMultiSelect = true;
        
        return tree;
    }

    private void OnLooped()
    {
        animation.OnLooped();
    }

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
    
    private double lastTime;

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
            var buttonRect = rect.AlignCenter(150, 100);
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

                if (claimed is MoveIt.Handler handler)
                {
                    AddHandler(handler);
                }
                else if (claimed is MoveIt.Event eevent)
                {
                    eevent.x = curvesEditor.lastXForEvent;
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
                    curvesEditor.SetFocusByCurve(curveItem.Curve);
                }
                
                UpdateAnimationComponent();
            }
            
            return;
        }

        var lastTimePointer = timePointer.Time;
        curvesEditor.OnGUI(rect);
        
        var keyPointsBounds = curvesEditor.curvesEditor.GetKeyPointsBounds();
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
                SelectEvents(data.events, timePointer, isReversed, selectedEvents);
                foreach (var selectedEventToCall in selectedEvents)
                {
                    selectedEventToCall.Invoke();
                }
            }

            Repaint();
        }

        lastTime = EditorApplication.timeSinceStartup;

        EvaluateAnimation(IsPreview && !Mathf.Approximately(lastTimePointer, timePointer.Time));
        
        if (treePopup != null)
        {
            GUI.BeginClip(rect);
            e.type = lastType;
            treePopup.position -= rect.position;
            treePopup.OnGUIInArea();
            GUI.EndClip();
        }

        IsRecorded = false;
    }
    
    private void OnSelectionChanged(SelectionChangedType selectionChangedType)
    {
        if(curvesEditor.IsSelectionInProgress) return;
        
        curvesEditor.NeedUpdateItemSelection = true;
        foreach (var curveItem in curvesEditor.CurveItems)
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
                    curvesEditor.SetFocusByCurve(curveItem.Curve);
                }
            }
        }
    }

    private Dictionary<string, int> handlerNamesCounter = new();

    private void AddHandler(MoveIt.Handler handler)
    {
        if (animation.TryGetData(CurrentClip, out var data))
        {
            RecordAddHandler();
            data.handlers.Add(handler);
            EditorUtility.SetDirty(animation);
            AddExistHandler(MenuTree, CurrentClip, handler);
            UpdateAnimationComponent();
        }
    }
    
    private void AddExistHandler(OdinMenuTree tree, MoveItClip clip, MoveIt.Handler handler)
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
            var handlerItem = new HandlerItem(tree, handlerName, animation, handler);
            tree.AddMenuItemAtPath(startPath, handlerItem);
            
            var path = $"{startPath}/{handlerName}";
            
            if (handler is IFloatPropertyHandler _)
            {
                for (int i = 0; i < handler.evaluators.Count; i++)
                {
                    var dt = handler.evaluators[i];
                    var curveItem = new CurveItem(tree, dt.rawProperty, colors[i % colors.Length], CurrentClip, handler, curvesEditor);
                    
                    var split = curveItem.property.Split('.');
                    OdinMenuItem item = handlerItem;
            
                    for (var j = 0; j < split.Length - 1; j++)
                    {
                        var part = split[j];
                        var newItem = new MenuItem(tree, part, null);
                        tree.AddMenuItemAtPath(item.GetFullPath(), newItem);
                        item = newItem;
                    }
                    
                    tree.AddMenuItemAtPath(item.GetFullPath(), curveItem);
                }
            }
            else
            {
                action(path, tree, clip, handler, curvesEditor);
            }
        }
    }

    private void CreateCurve(CurveItem curveItem)
    {
        curveItem.CreateCurve(out _);
    }

    private void UpdateAnimationComponent()
    {
        bool needUpdate = false;
        
        if (window.IsAnimationMode)
        {
            window.IsAnimationMode = false;
            needUpdate = true;
        }
        
        var clip = animation.Clip;
        animation.Editor_SetClip(null, IsPreview);
        animation.Editor_SetClip(clip, IsPreview);

        if (needUpdate)
        {
            window.IsAnimationMode = true;
        }
    }
    
    private void EvaluateAnimation(bool needApply = true) => animation.Editor_Evaluate(timePointer.Time, needApply);

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
            var newClip = CreateInstance<MoveItClip>();
                    
            AssetDatabase.CreateAsset(newClip, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
                    
            newClip.CreateGuid();
            animation.Add(newClip, new(), new());
            toolbar?.OnClipAdded(newClip);
            EditorUtility.SetDirty(animation);
            CurrentClip = newClip;
        }
    }
    
    private void SelectClip(MoveItClip clip)
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
                
        curvesEditor = null;
        timePointer = null;
        ForceMenuTreeRebuild();
    }
    
    
    private void DeleteClip(MoveItClip clip)
    {
        if (clip == CurrentClip)
        {
            curvesEditor = null;
            timePointer = null;
            ForceMenuTreeRebuild();
        }
        
        animation.Remove(clip);
    }
    
    private void NeedAddHandler(Rect rect)
    {
        ShowTypeSelectionPopup<MoveIt.Handler>(rect);
    }
    
    private void NeedAddEvent(Rect rect)
    {
        ShowTypeSelectionPopup<MoveIt.Event>(rect);
    }
    
    private void ShowTypeSelectionPopup<T>(Rect rect)
    {
        OdinObjectSelector.Show(this, toolbar.CurrentClip.GetInstanceID(), null, typeof(T), position: rect);
    }

    
    private static List<MoveIt.Event> selectedEvents = new();
    public static void SelectEvents(List<MoveIt.Event> events, GUIScene.TimePointer pointer, bool reverse, List<MoveIt.Event> result)
    {
        var oldRealTime = pointer.OldRealTime;
        var realTime = pointer.RealTime;
        pointer.SyncRealTime();

        MoveIt.SelectEvents(events, oldRealTime, realTime, pointer.ClampRange, reverse, result);
    }
}
#endif