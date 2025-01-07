using System;
using System.Collections.Generic;
using System.Linq;
using LSCore.AnimationsModule;
using LSCore.Extensions;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

public class BadassAnimationWindow : OdinMenuEditorWindow
{
    public class Toolbar : OdinMenuItem
    {
        public event Action<Rect> NeedAddHandler;
        public event Action SelectionConfirmed;
        private OdinSelector<BadassAnimationClip> clipSelector;
        public BadassAnimationClip currentClip;
        private readonly List<BadassAnimationClip> badassAnimationClips;

        private static GUIStyle enabledStyle = new()
        {
            normal = new GUIStyleState { textColor = new Color(0.82f, 0.82f, 0.82f), },
            hover = new GUIStyleState { textColor = Color.white },
        };
        
        public Toolbar(OdinMenuTree tree, IEnumerable<BadassAnimationClip> clips) : base(tree, string.Empty, null)
        {
            badassAnimationClips = clips.ToList();
            var createNewClip = CreateInstance<BadassAnimationClip>();
            createNewClip.name = CreateNewClipLabel;
            badassAnimationClips.Add(createNewClip);
            currentClip = badassAnimationClips.FirstOrDefault();
            CreateClipSelector(badassAnimationClips);
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
            var savedRect = rect;
            var plusButtonRect = rect.TakeFromLeft(20).AlignCenter(20, 20);
            if (SirenixEditorGUI.SDFIconButton(plusButtonRect, SdfIconType.Plus, enabledStyle))
            {
                NeedAddHandler?.Invoke(savedRect);
            }
            
            if (GUI.Button(rect, currentClip == null ? "Select Clip..." : currentClip.name))
            {
                var w = clipSelector.ShowInPopup();
            }
        }

        private void OnSelectionChanged(IEnumerable<BadassAnimationClip> clips)
        {
            currentClip = clips.FirstOrDefault();
            SelectionConfirmed?.Invoke();
        }
    }

    public class CurveItem : MenuItem
    {
        private BadassAnimation.Handler handler;
        private BadassAnimationClip clip;
        
        public bool TryGetCurve(out BadassAnimationCurve curve)
        {
            if (clip.namesToCurvesByHandlerGuids.TryGetValue(handler.guid, out var curves))
            {
                if (curves.TryGetValue(Name, out curve))
                {
                    return true;
                }
            }
             
            curve = null;
            return false;
        }

        public void CreateCurve()
        {
            clip.Add(handler, Name, new BadassAnimationCurve());
            EditorUtility.SetDirty(clip);
        }

        public CurveItem(OdinMenuTree tree, string name, object value, BadassAnimationClip clip, BadassAnimation.Handler handler) : base(tree, name, value)
        {
            this.clip = clip;
            this.handler = handler;
        }
    }
    
    public class HandlerItem : OdinMenuItem
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
            EditorUtils.DrawInBoxFoldout("Handler", Draw, this, false);
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
        
        public bool IsLocked
        {
            set
            {
                isLockedSelf = value;
                
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
                            menuItem.isLockedSelf = false;
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
                isVisibleSelf = value;
                
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
                            menuItem.isVisibleSelf = true;
                        }
                    
                        cur = cur.Parent;    
                    }
                }
            }
        }

        private static GUIStyle enabledStyle = new()
        {
            normal = new GUIStyleState { textColor = new Color(0.82f, 0.82f, 0.82f), },
            hover = new GUIStyleState { textColor = Color.white },
        };
        
        private static GUIStyle disabledStyle = new()
        {
            normal = new GUIStyleState { textColor = new Color(1f, 1f, 1f, 0.47f), },
            hover = new GUIStyleState { textColor = Color.white },
        };
        
        public MenuItem(OdinMenuTree tree, string name, object value) : base(tree, name, value)
        {
            Style.Height = 20;
        }

        protected override void OnDrawMenuItem(Rect rect, Rect labelRect)
        {
            if (SirenixEditorGUI.SDFIconButton(rect.TakeFromLeft(15), isVisibleSelf ? SdfIconType.EyeFill : SdfIconType.EyeSlashFill, isVisibleSelf ? enabledStyle : disabledStyle))
            {
                IsVisible = !isVisibleSelf;
            }
            
            rect.TakeFromRight(25);
            if (SirenixEditorGUI.SDFIconButton(rect.TakeFromRight(15), isLockedSelf ? SdfIconType.LockFill : SdfIconType.UnlockFill, isLockedSelf ? enabledStyle : disabledStyle))
            {
                IsLocked = !isLockedSelf;
            }
        }
    }

    private const string CreateNewClipLabel = "Create New Clip...";
    
    private BadassAnimationMultiCurveEditor editor;
    private BadassAnimation animation;
    private Toolbar toolbar;
    
    private BadassAnimationClip CurrentClip
    {
        get => toolbar?.currentClip;
        set
        {
            if (toolbar != null) toolbar.currentClip = value;
        }
    }

    private static readonly Dictionary<Type, Action<string, OdinMenuTree, BadassAnimationClip, BadassAnimation.Handler>> addItemActionsByType = new()
    {
        { typeof(Color), (path, tree, clip, handler) =>
            {
                tree.AddMenuItemAtPath(path, new CurveItem(tree, "r", null, clip, handler));
                tree.AddMenuItemAtPath(path, new CurveItem(tree, "g", null, clip, handler));
                tree.AddMenuItemAtPath(path, new CurveItem(tree, "b", null, clip, handler));
                tree.AddMenuItemAtPath(path, new CurveItem(tree, "a", null, clip, handler));
            } 
        },
        { typeof(Vector3), (path, tree, clip, handler) =>
            {
                tree.AddMenuItemAtPath(path, new CurveItem(tree, "x", null, clip, handler));
                tree.AddMenuItemAtPath(path, new CurveItem(tree, "y", null, clip, handler));
                tree.AddMenuItemAtPath(path, new CurveItem(tree, "z", null, clip, handler));
            } 
        },
        { typeof(Vector2), (path, tree, clip, handler) =>
            {
                tree.AddMenuItemAtPath(path, new CurveItem(tree, "x", null, clip, handler));
                tree.AddMenuItemAtPath(path, new CurveItem(tree, "y", null, clip, handler));
            } 
        },
        { typeof(float), (path, tree, clip, handler) =>
            {
                tree.AddMenuItemAtPath(path, new CurveItem(tree, "value", null, clip, handler));
            } 
        },
    };
    
    
    public static void ShowWindow(BadassAnimation animation)
    {
        var window = GetWindow<BadassAnimationWindow>();
        window.animation = animation;
        window.Show();
    }
    
    protected override OdinMenuTree BuildMenuTree()
    {
        animation.TryInit();
        var tree = new OdinMenuTree();
        var data = animation.data.FirstOrDefault(x => x.clip);

        if (data.clip == null)
        {
            return tree;
        }

        if (toolbar == null)
        {
            toolbar = new Toolbar(tree, animation.data.Select(x => x.clip));
            
            toolbar.NeedAddHandler += rect =>
            {
                NeedAddHandler(rect, toolbar.currentClip.GetInstanceID());
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

            var listCurves = new List<BadassAnimationCurveEditor>();

            foreach (var clip in lastClip.data.SelectMany(d => d.namesToCurves.Select(nc => nc.curve)))
            {
                listCurves.Add(new BadassAnimationCurveEditor(clip, animation));
            }

            if (listCurves.Count > 0)
            {
                editor = new BadassAnimationMultiCurveEditor(listCurves);
            }
        }
        
        tree.AddMenuItemAtPath(string.Empty, toolbar);
        var currentClip = CurrentClip;
        handlerNamesCounter.Clear();
        editor = new();
        
        if (animation.handlersByClip.TryGetValue(currentClip.name, out var handlers))
        {
            foreach (var handler in handlers)
            {
                AddHandler(tree, currentClip, handler);
            }
        }

        foreach (var item in tree.EnumerateTree())
        {
            if (item is CurveItem curveItem)
            {
                if (curveItem.TryGetCurve(out var curve))
                {
                    editor.Add(new BadassAnimationCurveEditor(curve, currentClip));
                }
            }
        }
        
        tree.Selection.SupportsMultiSelect = true;
        
        return tree;
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
            
            var path = $"{animation.name}/{handler.HandlerName}({counter})";
            
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
                }
            }
            
            return;
        }
        
        editor.OnGUI(rect);
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
        [HideInInspector] public BadassAnimationCurveEditor editor;

        [CustomValueDrawer("")]
        public void Create()
        {
            
        }
    }
}