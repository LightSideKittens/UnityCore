using System;
using System.Collections.Generic;
using System.Linq;
using LSCore.Editor;
using LSCore.Extensions;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;
using static BadassAnimation.Handler;

public class BadassAnimationWindow : OdinMenuEditorWindow
{
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
    
    [Serializable]
    public class CurveEditor
    {
        public LSHandles.TimePointer pointer;
        public BadassMultiCurveEditor curvesEditor;

        public CurveEditor(LSHandles.TimePointer pointer, BadassMultiCurveEditor curvesEditor)
        {
            this.pointer = pointer;
            this.curvesEditor = curvesEditor;
        }

        public void OnGUI(Rect position)
        {
            curvesEditor.AfterDraw += pointer.OnGUI;
            curvesEditor.OnGUI(position);
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
    }
    
    public class Toolbar : OdinMenuItem
    {
        public event Action<Rect> NeedAddHandler;
        public event Action SelectionConfirmed;
        
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

        public BadassAnimationClip CurrentClip
        {
            get => window.animation.Clip;
            set => window.animation.Clip = value;
        }
        
        public Toolbar(OdinMenuTree tree, BadassAnimationWindow window) : base(tree, string.Empty, null)
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
            Style = Style.Clone();
            Style.Height = 50;
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
        
        protected override void OnDrawMenuItem(Rect rect, Rect labelRect)
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
            
            if (SirenixEditorGUI.SDFIconButton(buttons.TakeFromLeft(20), left, EnabledStyle))
            {
                if (IsReversed)
                {
                    IsPlaying = !IsPlaying;
                }
                IsReversed = true;
            }
            
            if (SirenixEditorGUI.SDFIconButton(buttons.TakeFromLeft(20), right, EnabledStyle))
            {
                if (!IsReversed)
                {
                    IsPlaying = !IsPlaying;
                }
                IsReversed = false;
            }
            
            if (SirenixEditorGUI.SDFIconButton(buttons, SdfIconType.ArrowRepeat, IsLoop ? EnabledStyle : DisabledStyle))
            {
                IsLoop = !IsLoop;
            }
            
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
            CurrentClip = clips.FirstOrDefault();
            SelectionConfirmed?.Invoke();
        }
    }

    public class CurveItem : MenuItem
    {
        public BadassCurve curve;
        private BadassAnimation.Handler handler;
        private BadassAnimationClip clip;
        
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

        public CurveItem(OdinMenuTree tree, string name, object value, BadassAnimationClip clip, BadassAnimation.Handler handler) : base(tree, name, value)
        {
            this.clip = clip;
            this.handler = handler;
            TryGetCurve(out _);
        }
    }
    
    public class HandlerItem : MenuItem
    {
        private BadassAnimation animation;
        private BadassAnimation.Handler handler;
        private PropertyTree propertyTree;
        
        public HandlerItem(OdinMenuTree tree, string name, object value, BadassAnimation animation, BadassAnimation.Handler handler) : base(tree, name, value)
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

    public class MenuItem : OdinMenuItem
    {
        private bool isLockedSelf;
        private bool isVisibleSelf = true;

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
        }

        protected override void OnDrawMenuItem(Rect rect, Rect labelRect)
        {
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
    
    [HideInInspector] public CurveEditor editor;
    [HideInInspector] public LSHandles.TimePointer timePointer;
    [HideInInspector] public bool isPlaying;
    [HideInInspector] public bool isReversed;
    
    private BadassAnimation animation;
    private Toolbar toolbar;
    
    private BadassAnimationClip CurrentClip
    {
        get => toolbar?.CurrentClip;
        set
        {
            if (toolbar != null) toolbar.CurrentClip = value;
        }
    }

    private static readonly Dictionary<Type, Action<string, OdinMenuTree, BadassAnimationClip, BadassAnimation.Handler>> addItemActionsByType = new()
    {
        { typeof(Color), (path, tree, clip, handler) =>
            {
                tree.AddMenuItemAtPath(path, new CurveItem(tree, PropNames.r, null, clip, handler));
                tree.AddMenuItemAtPath(path, new CurveItem(tree, PropNames.g, null, clip, handler));
                tree.AddMenuItemAtPath(path, new CurveItem(tree, PropNames.b, null, clip, handler));
                tree.AddMenuItemAtPath(path, new CurveItem(tree, PropNames.a, null, clip, handler));
            } 
        },
        { typeof(Vector3), (path, tree, clip, handler) =>
            {
                tree.AddMenuItemAtPath(path, new CurveItem(tree, PropNames.x, null, clip, handler));
                tree.AddMenuItemAtPath(path, new CurveItem(tree, PropNames.y, null, clip, handler));
                tree.AddMenuItemAtPath(path, new CurveItem(tree, PropNames.z, null, clip, handler));
            } 
        },
        { typeof(Vector2), (path, tree, clip, handler) =>
            {
                tree.AddMenuItemAtPath(path, new CurveItem(tree, PropNames.x, null, clip, handler));
                tree.AddMenuItemAtPath(path, new CurveItem(tree, PropNames.y, null, clip, handler));
            } 
        },
        { typeof(float), (path, tree, clip, handler) =>
            {
                tree.AddMenuItemAtPath(path, new CurveItem(tree, PropNames.value, null, clip, handler));
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
        foreach (var odinMenuItem in MenuTree.Selection)
        {
            if (odinMenuItem is CurveItem curveItem)
            {
                editor.SetFocusByCurve(curveItem.curve);

            }
        }
    }

    protected override OdinMenuTree BuildMenuTree()
    {
        lastTime = EditorApplication.timeSinceStartup;
        animation.TryInit();
        var tree = new OdinMenuTree();
        tree.Selection.SelectionChanged += OnSelectionChanged;
        
        var data = animation.data.FirstOrDefault(x => x.clip);

        if (data.clip == null)
        {
            return tree;
        }
        
        timePointer ??= new LSHandles.TimePointer();
        
        if (toolbar == null)
        {
            MenuWidth = 365;
            toolbar = new Toolbar(tree, this);
            
            toolbar.NeedAddHandler += rect =>
            {
                NeedAddHandler(rect, toolbar.CurrentClip.GetInstanceID());
            };

            var lastClip = CurrentClip;
        
            toolbar.SelectionConfirmed += () =>
            {
                if(lastClip == CurrentClip) return;
            
                lastClip = CurrentClip;
                if (CurrentClip.name == CreateNewClipLabel)
                {
                    CreateNewClip();
                }
            
                ForceMenuTreeRebuild();
            };
        }
        
        tree.AddMenuItemAtPath(string.Empty, toolbar);
        var currentClip = CurrentClip;
        handlerNamesCounter.Clear();
        editor ??= new(timePointer, new());
        editor.pointer = timePointer;
        editor.Clear();
        var toRemove = new HashSet<string>(currentClip.namesToCurvesByHandlerGuids.Keys);
        
        if (animation.handlersByClip.TryGetValue(currentClip.name, out var handlers))
        {
            foreach (var handler in handlers)
            {
                AddHandler(tree, currentClip, handler);
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
                TryCreateCurveEditor(curveItem);
            }
        }
        
        tree.Selection.SupportsMultiSelect = true;
        
        return tree;
    }

    private void TryCreateCurveEditor(CurveItem curveItem)
    {
        if (curveItem.TryGetCurve(out var curve))
        {
            var curveEditor = new BadassCurveEditor(curve, this);
            curveEditor.IsLocked = curveItem.IsLockedSelf;
            curveEditor.IsVisible = curveItem.IsVisibleSelf;
                    
            curveItem.LockedSelfChanged += state => curveEditor.IsLocked = state;
            curveItem.VisibleSelfChanged += state => curveEditor.IsVisible = state;
                    
            editor.Add(curveEditor);
        }
    }

    private Dictionary<string, int> handlerNamesCounter = new();
    
    private void AddHandler(OdinMenuTree tree, BadassAnimationClip clip, BadassAnimation.Handler handler)
    {
        if (string.IsNullOrEmpty(handler.guid))
        {
            handler.guid = Guid.NewGuid().ToString("N");
            EditorUtility.SetDirty(animation);
        }
        
        if (addItemActionsByType.TryGetValue(handler.ValueType, out var action))
        {
            handlerNamesCounter.TryGetValue(handler.HandlerName, out var counter);
            handlerNamesCounter[handler.HandlerName] = ++counter;
            
            var handlerName = $"{handler.HandlerName}({counter})";
            tree.AddMenuItemAtPath(animation.name, new MenuItem(tree, handlerName, null));
            
            var path = $"{animation.name}/{handlerName}";
            tree.AddMenuItemAtPath(path, new HandlerItem(tree, handler.HandlerName, null, animation, handler));
            action(path, tree, clip, handler);
        }
    }

    protected override void DrawMenu()
    {
        if(CurrentClip == null) return;
        base.DrawMenu();
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
    }

    private double lastTime;

    protected override void OnImGUI()
    {
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
            var handler = (BadassAnimation.Handler)OdinObjectSelector.Claim();
            if (animation.handlersByClip.TryGetValue(CurrentClip.name, out var handlers))
            {
                handlers.Add(handler);
                EditorUtility.SetDirty(animation);
                AddHandler(MenuTree, CurrentClip, handler);
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
                    curveItem.CreateCurve();
                    TryCreateCurveEditor(curveItem);
                    editor.SetFocusByCurve(curveItem.curve);
                    toolbar!.UpdateAnimationData();
                }
            }
            
            return;
        }
        
        editor.OnGUI(rect);
        var keyPointsBounds = editor.curvesEditor.GetKeyPointsBounds();
        timePointer.clampRange = new Vector2(0, keyPointsBounds.max.x);
        
        var e = Event.current;

        if (e.type == EventType.KeyDown)
        {
            if (e.keyCode == KeyCode.Space)
            {
                lastTime = EditorApplication.timeSinceStartup;
                isPlaying = !isPlaying;
            }   
        }
        
        if (isPlaying)
        {
            timePointer.Time -= (float)(EditorApplication.timeSinceStartup - lastTime) * isReversed.ToPosNeg();
            Repaint();
        }

        lastTime = EditorApplication.timeSinceStartup;
        animation.Editor_Evaluate(timePointer.Time);
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
                    
            animation.Add(newClip, new List<BadassAnimation.Handler>());
            toolbar?.OnClipAdded(newClip);
            EditorUtility.SetDirty(animation);
            CurrentClip = newClip;
        }
    }

    private void NeedAddHandler(Rect rect, int id)
    {
        OdinObjectSelector.Show(this, id, null, typeof(BadassAnimation.Handler), position: rect);
    }
    
    private void ShowTypeSelectionPopup(int id, object value)
    {
        OdinObjectSelector.Show(this, id, value, typeof(BadassAnimation.Handler));
    }
    
    public class CurveDrawer
    {
        [HideInInspector] public Func<Rect> rectGetter;
        [HideInInspector] public BadassAnimationClip clip;
        [HideInInspector] public BadassCurveEditor editor;

        [CustomValueDrawer("")]
        public void Create()
        {
            
        }
    }
}