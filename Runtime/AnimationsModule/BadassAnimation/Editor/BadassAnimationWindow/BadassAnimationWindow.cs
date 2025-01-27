using System;
using System.Collections.Generic;
using System.Linq;
using DG.DemiEditor;
using LSCore.AnimationsModule;
using LSCore.Editor;
using LSCore.Extensions;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;
using static BadassAnimation.Handler;

[InitializeOnLoad]
public partial class BadassAnimationWindow : OdinMenuEditorWindow
{
    static BadassAnimationWindow()
    {
        BadassAnimation.NeedShowWindow += ShowWindow;
    }
    
    private static void ShowWindow(BadassAnimation animation)
    {
        var window = GetWindow<BadassAnimationWindow>();
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
    private static BadassAnimationWindow window;
    
    [HideInInspector] public CurvesEditor editor;
    [HideInInspector] public LSHandles.TimePointer timePointer;
    [HideInInspector] public bool isReversed;
    
    private bool isPlaying;

    private BadassAnimation animation;
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
                editor.curvesEditor.IsYBlocked = value;
            }
            isDopesheet = value;
        }
    }
    
    private bool isPreview;
    private bool IsPreview
    {
        get => isPreview;
        set
        {
            if (!value && IsRecording)
            {
                IsRecording = false;
            }
            
            isPreview = value;
            UpdateAnimationComponent();
            if (value)
            {
                EvaluateAnimation();
            }
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

    protected override void OnEnable()
    {
        base.OnEnable();
        editor?.OnEnable();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        editor?.OnDisable();
        animation.Editor_SetClip(null, IsPreview);
    }

    protected override OdinMenuTree BuildMenuTree()
    {
        objectByHandlerType.Clear();
        IsRecording = false;
        IsPreview = false;
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
                    UpdateAnimationComponent();
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
            EvaluateAnimation();
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
        animation.Editor_SetClip(null, IsPreview);
        animation.Editor_SetClip(clip, IsPreview);
    }
    
    private void EvaluateAnimation() => animation.Editor_Evaluate(timePointer.Time);

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