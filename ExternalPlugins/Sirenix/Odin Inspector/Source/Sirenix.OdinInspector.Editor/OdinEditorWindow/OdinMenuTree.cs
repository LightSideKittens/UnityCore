//-----------------------------------------------------------------------
// <copyright file="OdinMenuTree.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#if UNITY_EDITOR
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    using Sirenix.Utilities;
    using Sirenix.Utilities.Editor;
    using Sirenix.OdinInspector.Editor.Internal;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// OdinMenuTree provides a tree of <see cref="OdinMenuItem"/>s, and helps with selection, inserting menu items into the tree, and can handle keyboard navigation for you.
    /// </summary>
    /// <example>
    /// <code>
    /// OdinMenuTree tree = new OdinMenuTree(supportsMultiSelect: true)
    /// {
    ///     { "Home",                           this,                           EditorIcons.House       },
    ///     { "Odin Settings",                  null,                           SdfIconType.GearFill    },
    ///     { "Odin Settings/Color Palettes",   ColorPaletteManager.Instance,   EditorIcons.EyeDropper  },
    ///     { "Odin Settings/AOT Generation",   AOTGenerationConfig.Instance,   EditorIcons.SmartPhone  },
    ///     { "Camera current",                 Camera.current                                          },
    ///     { "Some Class",                     this.someData                                           }
    /// };
    /// 
    /// tree.AddAllAssetsAtPath("Some Menu Item", "Some Asset Path", typeof(ScriptableObject), true)
    ///     .AddThumbnailIcons();
    /// 
    /// tree.AddAssetAtPath("Some Second Menu Item", "SomeAssetPath/SomeAssetFile.asset");
    /// 
    /// var customMenuItem = new OdinMenuItem(tree, "Menu Style", tree.DefaultMenuStyle);
    /// tree.MenuItems.Insert(2, customMenuItem);
    /// 
    /// tree.Add("Menu/Items/Are/Created/As/Needed", new GUIContent());
    /// tree.Add("Menu/Items/Are/Created", new GUIContent("And can be overridden"));
    /// </code>
    /// OdinMenuTrees are typically used with <see cref="OdinMenuEditorWindow"/>s but is made to work perfectly fine on its own for other use cases.
    /// OdinMenuItems can be inherited and and customized to fit your needs.
    /// <code>
    /// // Draw stuff
    /// someTree.DrawMenuTree();
    /// // Draw stuff
    /// someTree.HandleKeybaordMenuNavigation();
    /// </code>
    /// </example>
    /// <seealso cref="OdinMenuItem" />
    /// <seealso cref="OdinMenuStyle" />
    /// <seealso cref="OdinMenuTreeSelection" />
    /// <seealso cref="OdinMenuTreeExtensions" />
    /// <seealso cref="OdinMenuEditorWindow" />
    public class OdinMenuTree : IEnumerable
    {
        private static bool preventAutoFocus;

        /// <summary>
        /// Gets the currently active menu tree.
        /// </summary>
        public static OdinMenuTree ActiveMenuTree;
        private static HashSet<OdinMenuItem> cachedHashList = new HashSet<OdinMenuItem>();

        private readonly OdinMenuItem root;
        private readonly OdinMenuTreeSelection selection;
        private OdinMenuTreeDrawingConfig defaultConfig;
        private bool regainSearchFieldFocus;
        private bool hadSearchFieldFocus;
        private Rect outerScrollViewRect;
        private int hideScrollbarsWhileContentIsExpanding;
        private Rect innerScrollViewRect;
        private bool isFirstFrame = true;
        private int forceRegainFocusCounter = 0;
        private bool requestRepaint;
        private GUIFrameCounter frameCounter = new GUIFrameCounter();
        private bool hasRepaintedCurrentSearchResult = true;
        private bool scollToCenter;
        private OdinMenuItem scrollToWhenReady;
        private bool isDirty;
        private bool updateSearchResults;
        private bool regainFocusWhenWindowFocus;
        private bool currWindowHasFocus;
        private bool wasMouseOverMenuTree;
        private SearchField searchField = new SearchField();

        // ===== EXPERIMENTAL_INTERNAL_SparseFixedLayouting
        internal OdinGUIScrollView ScrollView;

        private bool layoutRequiresUpdate = true;

        private int visibleMenuItemCount = 0;
        // EXPERIMENTAL_INTERNAL_SparseFixedLayouting ===== 
        

        internal static Rect VisibleRect;
        internal static Event CurrentEvent;
        internal static EventType CurrentEventType;

        public List<OdinMenuItem> FlatMenuTree = new List<OdinMenuItem>();

        internal OdinMenuItem Root
        {
            get { return this.root; }
        }

        /// <summary>
        /// Gets the selection.
        /// </summary>
        public OdinMenuTreeSelection Selection
        {
            get { return this.selection; }
        }

        /// <summary>
        /// Gets the root menu items.
        /// </summary>
        public List<OdinMenuItem> MenuItems
        {
            get { return this.root.ChildMenuItems; }
        }

        /// <summary>
        /// Gets the root menu item.
        /// </summary>
        public OdinMenuItem RootMenuItem
        {
            get { return this.root; }
        }

        /// <summary>
        /// If true, all indent levels will be ignored, and all menu items with IsVisible == true will be drawn.
        /// </summary>
        public bool DrawInSearchMode { get; private set; }

        /// <summary>
        /// Adds a menu item with the specified object instance at the the specified path.
        /// </summary>
        /// <returns>Returns all menu items created in order to add the menu item at the specified path.</returns>
        public IEnumerable<OdinMenuItem> Add(string path, object instance)
        {
            return this.AddObjectAtPath(path, instance);
        }

        /// <summary>
        /// Adds a menu item with the specified object instance and icon at the the specified path.
        /// </summary>
        /// <returns>Returns all menu items created in order to add the menu item at the specified path.</returns>
        public IEnumerable<OdinMenuItem> Add(string path, object instance, Texture icon)
        {
            return this.AddObjectAtPath(path, instance).AddIcon(icon);
        }

        /// <summary>
        /// Adds a menu item with the specified object instance and icon at the the specified path.
        /// </summary>
        /// <returns>Returns all menu items created in order to add the menu item at the specified path.</returns>
        public IEnumerable<OdinMenuItem> Add(string path, object instance, SdfIconType icon)
        {
            var addedMenuItems = this.AddObjectAtPath(path, instance);
            addedMenuItems.LastOrDefault()?.AddIcon(icon);
            return addedMenuItems;
        }

        /// <summary>
        /// Adds a menu item with the specified object instance and icon at the the specified path.
        /// </summary>
        /// <returns>Returns all menu items created in order to add the menu item at the specified path.</returns>
        public IEnumerable<OdinMenuItem> Add(string path, object instance, Sprite sprite)
        {
            return this.AddObjectAtPath(path, instance).AddIcon(AssetPreview.GetAssetPreview(sprite));
        }

        /// <summary>
        /// Adds a menu item with the specified object instance and icon at the the specified path.
        /// </summary>
        /// <returns>Returns all menu items created in order to add the menu item at the specified path.</returns>
        public IEnumerable<OdinMenuItem> Add(string path, object instance, EditorIcon icon)
        {
            return this.AddObjectAtPath(path, instance).AddIcon(icon);
        }

        /// <summary>
        /// Adds a collection of objects to the menu tree and returns all menu items created in random order.
        /// </summary>
        public IEnumerable<OdinMenuItem> AddRange<T>(IEnumerable<T> collection, Func<T, string> getPath)
        {
            if (collection == null)
            {
                return Enumerable.Empty<OdinMenuItem>();
            }

            cachedHashList.Clear();

            foreach (var item in collection)
            {
                cachedHashList.AddRange(this.AddObjectAtPath(getPath(item), item));
            }

            return cachedHashList;
        }

        /// <summary>
        /// Adds a collection of objects to the menu tree and returns all menu items created in random order.
        /// </summary>
        public IEnumerable<OdinMenuItem> AddRange<T>(IEnumerable<T> collection, Func<T, string> getPath, Func<T, Texture> getIcon)
        {
            if (collection == null)
            {
                return Enumerable.Empty<OdinMenuItem>();
            }

            cachedHashList.Clear();

            foreach (var item in collection)
            {
                if (getIcon != null)
                {
                    cachedHashList.AddRange(this.AddObjectAtPath(getPath(item), item).AddIcon(getIcon(item)));
                }
                else
                {
                    cachedHashList.AddRange(this.AddObjectAtPath(getPath(item), item));
                }
            }

            return cachedHashList;
        }

        /// <summary>
        /// Gets or sets the default menu item style from Config.DefaultStyle.
        /// </summary>
        public OdinMenuStyle DefaultMenuStyle
        {
            get { return this.Config.DefaultMenuStyle; }
            set { this.Config.DefaultMenuStyle = value; }
        }

        /// <summary>
        /// Gets or sets the default drawing configuration.
        /// </summary>
        public OdinMenuTreeDrawingConfig Config
        {
            get
            {
                this.defaultConfig = this.defaultConfig ?? new OdinMenuTreeDrawingConfig()
                {
                    DrawScrollView = true,
                    DrawSearchToolbar = false,
                    AutoHandleKeyboardNavigation = false
                };

                return this.defaultConfig;
            }
            set
            {
                this.defaultConfig = value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OdinMenuTree"/> class.
        /// </summary>
        public OdinMenuTree()
            : this(false, new OdinMenuStyle())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OdinMenuTree"/> class.
        /// </summary>
        /// <param name="supportsMultiSelect">if set to <c>true</c> [supports multi select].</param>
        public OdinMenuTree(bool supportsMultiSelect)
            : this(supportsMultiSelect, new OdinMenuStyle())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OdinMenuTree"/> class.
        /// </summary>
        /// <param name="supportsMultiSelect">if set to <c>true</c> [supports multi select].</param>
        /// <param name="defaultMenuStyle">The default menu item style.</param>
        public OdinMenuTree(bool supportsMultiSelect, OdinMenuStyle defaultMenuStyle)
        {
            this.DefaultMenuStyle = defaultMenuStyle;
            this.selection = new OdinMenuTreeSelection(supportsMultiSelect);
            this.root = new OdinMenuItem(this, "root", null);
            this.SetupAutoScroll();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OdinMenuTree"/> class.
        /// </summary>
        public OdinMenuTree(bool supportsMultiSelect, OdinMenuTreeDrawingConfig config)
        {
            this.Config = config;
            this.selection = new OdinMenuTreeSelection(supportsMultiSelect);
            this.root = new OdinMenuItem(this, "root", null);
            this.SetupAutoScroll();
        }

        /// <summary>
        /// Sets the focus to the <see cref="searchField"/>.
        /// </summary>
        public void FocusSearchField() => this.searchField?.Focus();

        private void SetupAutoScroll()
        {
            this.selection.SelectionChanged += (x) =>
            {
                if (this.Config.AutoScrollOnSelectionChanged && x == SelectionChangedType.ItemAdded)
                {
                    this.requestRepaint = true;
                    GUIHelper.RequestRepaint();

                    if (this.isFirstFrame)
                    {
                        this.ScrollToMenuItem(this.selection.LastOrDefault(), true);
                    }
                    else
                    {
                        this.ScrollToMenuItem(this.selection.LastOrDefault(), false);
                    }
                }
            };
        }

        /// <summary>
        /// Scrolls to the specified menu item.
        /// </summary>
        public void ScrollToMenuItem(OdinMenuItem menuItem, bool centerMenuItem = false)
        {
            if (menuItem != null)
            {
                this.scollToCenter = centerMenuItem;
                this.scrollToWhenReady = menuItem;

                if (!menuItem._IsVisible())
                {
                    foreach (var item in menuItem.GetParentMenuItemsRecursive(false))
                    {
                        item.Toggled = true;
                    }
                    return;
                }

                foreach (var item in menuItem.GetParentMenuItemsRecursive(false))
                {
                    item.Toggled = true;
                }

                if (this.outerScrollViewRect.height == 0 || menuItem.Rect.height <= 0.01f)
                {
                    return;
                }

                if (Event.current == null || Event.current.type != EventType.Repaint)
                {
                    return;
                }

                var config = this.Config;
                var rect = menuItem.Rect;

                float a, b;

                if (centerMenuItem)
                {
                    var r = this.outerScrollViewRect.AlignCenterY(rect.height);

                    a = rect.yMin - (this.innerScrollViewRect.y + config.ScrollPos.y - r.y);
                    b = (rect.yMax - r.height + this.innerScrollViewRect.y) - (config.ScrollPos.y + r.y);
                }
                else
                {
                    var viewRect = this.outerScrollViewRect;
                    viewRect.y = 0;

                    a = rect.yMin - (this.innerScrollViewRect.y + config.ScrollPos.y) - 1;
                    b = (rect.yMax - this.outerScrollViewRect.height + this.innerScrollViewRect.y) - config.ScrollPos.y;
                    a -= rect.height;
                    b += rect.height;
                }

                if (a < 0)
                {
                    config.ScrollPos.y += a;
                }

                if (b > 0)
                {
                    config.ScrollPos.y += b;
                }

                // Some windows takes a while to adjust themselves, where the inner and outer range are subject to change.
                if (this.frameCounter.FrameCount > 6)
                {
                    this.scrollToWhenReady = null;
                }
                else
                {
                    GUIHelper.RequestRepaint();
                }
            }
        }

        /// <summary>
        /// Enumerates the tree with a DFS.
        /// </summary>
        /// <param name="includeRootNode">if set to <c>true</c> then the invisible root menu item is included.</param>
        public IEnumerable<OdinMenuItem> EnumerateTree(bool includeRootNode = false)
        {
            return this.root.GetChildMenuItemsRecursive(includeRootNode);
        }

        /// <summary>
        /// Enumerates the tree with a DFS.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <param name="includeRootNode">if set to <c>true</c> then the invisible root menu item is included.</param>
        public IEnumerable<OdinMenuItem> EnumerateTree(Func<OdinMenuItem, bool> predicate, bool includeRootNode)
        {
            return this.root.GetChildMenuItemsRecursive(includeRootNode).Where(predicate);
        }

        /// <summary>
        /// Enumerates the tree with a DFS.
        /// </summary>
        public void EnumerateTree(Action<OdinMenuItem> action)
        {
            this.root.GetChildMenuItemsRecursive(false).ForEach(action);
        }

        /// <summary>
        /// Draws the menu tree recursively.
        /// </summary>
        public void DrawMenuTree()
        {
            OdinMenuTree.CurrentEditorTimeHelperDeltaTime = GUITimeHelper.LayoutDeltaTime;

            this.frameCounter.Update();
            var config = this.Config;

            if (this.requestRepaint)
            {
                GUIHelper.RequestRepaint();
                this.requestRepaint = false;
            }

            if (config.DrawSearchToolbar)
            {
                this.DrawSearchToolbar();
            }

            // Make window repaint when mouse is moving over a menu tree so we get mouse-over effects. 
            if (Event.current.delta != new Vector2())
            {
                var isMouseOverMenuTree = this.outerScrollViewRect.Contains(Event.current.mousePosition);

                if (this.wasMouseOverMenuTree || isMouseOverMenuTree)
                {
                    GUIHelper.RequestRepaint();
                }

                this.wasMouseOverMenuTree = isMouseOverMenuTree;
            }

            var outerRect = EditorGUILayout.BeginVertical();

            this.HandleActiveMenuTreeState(outerRect);

            if (config.DrawScrollView)
            {
                if (Event.current.type == EventType.Repaint)
                {
                    this.outerScrollViewRect = outerRect;
                }

                if (this.hideScrollbarsWhileContentIsExpanding > 0)
                {
                    config.ScrollPos = EditorGUILayout.BeginScrollView(config.ScrollPos, GUIStyle.none, GUIStyle.none, GUILayoutOptions.ExpandHeight(false));
                }
                else
                {
                    config.ScrollPos = EditorGUILayout.BeginScrollView(config.ScrollPos, GUILayoutOptions.ExpandHeight(false));
                }

                var size = EditorGUILayout.BeginVertical();

                // hideScrollbarsWhileContentIsExpanding:
                // When drawn in confined areas, the scrollbars on the scrollview will flicker in and out while expanding.
                // The code below ensures we hide the scorllbars remain invisible while the inner and outer scrollview heights are somewhat close to each other.
                if (this.innerScrollViewRect.height == 0 || Event.current.type == EventType.Repaint)
                {
                    var chancedSizeDiff = Mathf.Abs(this.innerScrollViewRect.height - size.height);
                    var boxDiff = Mathf.Abs(this.innerScrollViewRect.height - this.outerScrollViewRect.height);
                    var shouldHaveScrollViewRegardless = this.innerScrollViewRect.height - 40 > this.outerScrollViewRect.height;

                    if (!shouldHaveScrollViewRegardless && chancedSizeDiff > 0)
                    {
                        this.hideScrollbarsWhileContentIsExpanding = 5;
                        GUIHelper.RequestRepaint();
                    }
                    else if (Mathf.Abs(boxDiff) < 1)
                    {
                        this.hideScrollbarsWhileContentIsExpanding = 5;
                    }
                    else
                    {
                        this.hideScrollbarsWhileContentIsExpanding--;
                        if (this.hideScrollbarsWhileContentIsExpanding < 0)
                        {
                            this.hideScrollbarsWhileContentIsExpanding = 0;
                        }
                        else
                        {
                            GUIHelper.RequestRepaint();
                        }
                    }

                    this.innerScrollViewRect = size;
                }

                GUILayout.Space(-1);
            }

            if (this.isDirty && Event.current.type == EventType.Layout)
            {
                this.UpdateMenuTree();
                this.isDirty = false;
            }

            VisibleRect = GUIClipInfo.VisibleRect.Expand(300);
            CurrentEvent = Event.current;
            CurrentEventType = CurrentEvent.type;

            var tree = this.DrawInSearchMode ? this.FlatMenuTree : this.MenuItems;
            var count = tree.Count;

            if (config.EXPERIMENTAL_INTERNAL_SparseFixedLayouting)
            {
                if (this.ScrollView == null)
                {
                    this.ScrollView = new OdinGUIScrollView(this.MenuItems.Count > 0 ? this.MenuItems.Count : 16);
                }

                if (this.layoutRequiresUpdate)
                {
                    this.visibleMenuItemCount = 0;

                    if (this.DrawInSearchMode)
                    {
                        this.visibleMenuItemCount = tree.Count;
                    }
                    else
                    {
                        foreach (OdinMenuItem item in tree)
                        {
                            this.visibleMenuItemCount += item.CountVisibleRecursively();
                        }
                    }
                }

                var height = this.visibleMenuItemCount * this.Config.DefaultMenuStyle.Height;
                var rect = GUILayoutUtility.GetRect(0, height);

                if (this.layoutRequiresUpdate)
                {
                    this.ScrollView.SetBounds(rect);

                    this.ScrollView.BeginAllocations();
                    {
                        foreach (OdinMenuItem item in tree)
                        {
                            item.AllocateRectRecursivelyForScrollView();
                        }
                    }
                    this.ScrollView.EndAllocations();

                    this.layoutRequiresUpdate = false;
                }
                else
                {
                    this.ScrollView.SetBoundsForCurrentAllocations(rect);
                }

                this.ScrollView.Position = this.Config.ScrollPos;

                OdinGUIScrollView.VisibleItems visibleItems = this.ScrollView.GetVisibleItems();

                for (var i = 0; i < visibleItems.Length; i++)
                {
                    var item = visibleItems.GetAssociatedData<OdinMenuItem>(i);

                    item.EXPERIMENTAL_DontAllocateNewRect = true;

                    item.rect = visibleItems.GetRect(i);

                    item.DrawMenuItem((int) visibleItems.GetIndentation(i));
                }
            }
            else if (config.EXPERIMENTAL_INTERNAL_DrawFlatTreeFastNoLayout)
            {
                var itemHeight = this.DefaultMenuStyle.Height;
                var height = count * itemHeight;
                var rect = GUILayoutUtility.GetRect(0, height);

                rect.height = itemHeight;

                // TODO: calculate start and end index based on GUIClipInfo.VisibleRect.
                for (int i = 0; i < count; i++)
                {
                    var item = tree[i];

                    item.EXPERIMENTAL_DontAllocateNewRect = true;
                    item.rect = rect;
                    item.DrawMenuItem(0);

                    rect.y += itemHeight;
                }
            }
            else
            {
                if (this.DrawInSearchMode)
                {
                    for (int i = 0; i < count; i++)
                    {
                        tree[i].DrawMenuItem(0);
                    }
                }
                else
                {
                    for (int i = 0; i < count; i++)
                    {
                        tree[i].DrawMenuItems(0);
                    }
                }
            }

            if (config.DrawScrollView)
            {
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndScrollView();
            }

            EditorGUILayout.EndVertical();

            if (config.AutoHandleKeyboardNavigation)
            {
                this.HandleKeyboardMenuNavigation();
            }

            if (this.scrollToWhenReady != null)
            {
                this.ScrollToMenuItem(this.scrollToWhenReady, this.scollToCenter);
            }

            if (Event.current.type == EventType.Repaint)
            {
                this.isFirstFrame = false;
            }
        }

        private void HandleActiveMenuTreeState(Rect outerRect)
        {
            if (Event.current.type == EventType.Repaint)
            {
                if (this.currWindowHasFocus != GUIHelper.CurrentWindowHasFocus)
                {
                    this.currWindowHasFocus = GUIHelper.CurrentWindowHasFocus;

                    if (this.currWindowHasFocus && this.regainFocusWhenWindowFocus)
                    {
                        if (!OdinMenuTree.preventAutoFocus)
                        {
                            OdinMenuTree.ActiveMenuTree = this;
                        }

                        this.regainFocusWhenWindowFocus = false;
                    }
                }

                // Deactivate when another window is focused.
                if (!this.currWindowHasFocus && OdinMenuTree.ActiveMenuTree == this)
                {
                    OdinMenuTree.ActiveMenuTree = null;
                }

                // Whether we should activate the menu tree next time the window gets focus.
                if (this.currWindowHasFocus)
                {
                    this.regainFocusWhenWindowFocus = OdinMenuTree.ActiveMenuTree == this;
                }

                // Auto activate.
                if (this.currWindowHasFocus && OdinMenuTree.ActiveMenuTree == null)
                {
                    OdinMenuTree.ActiveMenuTree = this;
                }
            }

            this.MenuTreeActivationZone(outerRect);
        }

        internal void MenuTreeActivationZone(Rect rect)
        {
            if (OdinMenuTree.ActiveMenuTree == this)
            {
                return;
            }

            if (Event.current.rawType == EventType.MouseDown && rect.Contains(Event.current.mousePosition) && GUIHelper.CurrentWindowHasFocus)
            {
                this.regainSearchFieldFocus = true;
                OdinMenuTree.preventAutoFocus = true;
                OdinMenuTree.ActiveMenuTree = this;
                UnityEditorEventUtility.EditorApplication_delayCall += () => preventAutoFocus = false;
                GUIHelper.RequestRepaint();
            }
        }

        /// <summary>
        /// Marks the dirty. This will cause a tree.UpdateTree() in the beginning of the next Layout frame.
        /// </summary>
        public void MarkDirty()
        {
            this.isDirty = true;
            this.updateSearchResults = true;
        }

        /// <summary>
        /// Indicates that the layout has changed and needs to be recomputed.
        /// This is used when <see cref="OdinMenuTreeDrawingConfig.EXPERIMENTAL_INTERNAL_SparseFixedLayouting"/> is enabled.
        /// </summary>
        public void MarkLayoutChanged() => this.layoutRequiresUpdate = true;


        /// <summary>
        /// Draws the search toolbar.
        /// </summary>
        public void DrawSearchToolbar(GUIStyle toolbarStyle = null)
        {
            var config = this.Config;

            var searchFieldRect = GUILayoutUtility.GetRect(0, config.SearchToolbarHeight, GUILayoutOptions.ExpandWidth(true));

            if (Event.current.type == EventType.Repaint)
            {
                (toolbarStyle ?? SirenixGUIStyles.ToolbarBackground).Draw(searchFieldRect, GUIContent.none, 0);
            }

            searchFieldRect = searchFieldRect.Padding(4);
            searchFieldRect.yMax += 1;

            EditorGUI.BeginChangeCheck();
            config.SearchTerm = this.DrawSearchField(searchFieldRect, config.SearchTerm, config.AutoFocusSearchBar);
            var changed = EditorGUI.EndChangeCheck();

            if ((changed || this.updateSearchResults) && this.hasRepaintedCurrentSearchResult)
            {
                this.layoutRequiresUpdate = true;
                
                this.updateSearchResults = false;

                // We want fast visual search feedback. If the user is typing faster than the window can repaint,
                // then no results will be visible while he's typing. this.hasRepaintedCurrentSearchResult fixes that.

                this.hasRepaintedCurrentSearchResult = false;
                bool doSearch = !string.IsNullOrEmpty(config.SearchTerm);
                if (doSearch)
                {
                    if (!this.DrawInSearchMode)
                    {
                        config.ScrollPos = new Vector2();
                    }

                    this.DrawInSearchMode = true;

                    if (config.SearchFunction != null)
                    {
                        // Custom search
                        this.FlatMenuTree.Clear();
                        foreach (var item in this.EnumerateTree())
                        {
                            if (config.SearchFunction(item))
                            {
                                this.FlatMenuTree.Add(item);
                            }
                        }
                    }
                    else
                    {
                        // Fuzzy search with sorting.
                        this.FlatMenuTree.Clear();
                        this.FlatMenuTree.AddRange(
                            this.EnumerateTree()
                            .Where(x => x.Value != null)
                            .Select(x =>
                            {
                                int score;
                                bool include = FuzzySearch.Contains(this.Config.SearchTerm, x.SearchString, out score);
                                return new { score = score, item = x, include = include };
                            })
                            .Where(x => x.include)
                            .OrderByDescending(x => x.score)
                            .Select(x => x.item));
                    }

                    this.root.UpdateFlatMenuItemNavigation();
                }
                else
                {
                    this.DrawInSearchMode = false;
                    // Ensure all selected elements are visible, and scroll to the last one.
                    this.FlatMenuTree.Clear();
                    var last = this.selection.LastOrDefault();
                    this.UpdateMenuTree();
                    this.Selection.SelectMany(x => x.GetParentMenuItemsRecursive(false)).ForEach(x => x.Toggled = true);
                    if (last != null)
                    {
                        this.ScrollToMenuItem(last);
                    }

                    this.root.UpdateFlatMenuItemNavigation();
                }
            }

            if (Event.current.type == EventType.Repaint)
            {
                this.hasRepaintedCurrentSearchResult = true;
            }
        }

        private bool isFirstGuiFrame = false;
        internal static float CurrentEditorTimeHelperDeltaTime;

        private string DrawSearchField(Rect rect, string searchTerm, bool autoFocus)
        {
            // We're hacking a bit here to ignore certain KeyCodes used for navigating the tree.
            // If anyone knows a good way of doing that, do tell. Keep in mind that keyboard navigation is handled after the entire tree is done drawing.

            var hasFocus = searchField.HasFocus();
            if (this.hadSearchFieldFocus != hasFocus)
            {
                OdinMenuTree.ActiveMenuTree = this;
                this.hadSearchFieldFocus = hasFocus;
            }

            bool ignore = hasFocus &&
               (Event.current.keyCode == KeyCode.DownArrow ||
                Event.current.keyCode == KeyCode.UpArrow ||
                Event.current.keyCode == KeyCode.LeftArrow ||
                Event.current.keyCode == KeyCode.RightArrow ||
                Event.current.keyCode == KeyCode.Return);

            if (ignore)
            {
                GUIHelper.PushEventType(Event.current.type);
            }

            if ((this.isFirstFrame || this.regainSearchFieldFocus) && autoFocus && ActiveMenuTree == this)
            {
                this.searchField.Focus();
            }

            this.isFirstFrame = false;
            rect.y -= 1;

            searchTerm = this.searchField.Draw(rect, searchTerm);

            if (this.regainSearchFieldFocus && Event.current.type == EventType.Layout)
            {
                this.regainSearchFieldFocus = false;
            }

            if (ignore)
            {
                GUIHelper.PopEventType();
                if (ActiveMenuTree == this)
                {
                    this.regainSearchFieldFocus = true;
                }
            }

            if (this.forceRegainFocusCounter < 20)
            {
                if (autoFocus && this.forceRegainFocusCounter < 4)
                {
                    if (ActiveMenuTree == this)
                    {
                        this.regainSearchFieldFocus = true;
                    }
                }

                GUIHelper.RequestRepaint();
                GUIHelper.SafeHandleUtilityRepaint();
                if (Event.current.type == EventType.Repaint)
                {
                    this.forceRegainFocusCounter++;
                }
            }

            return searchTerm;
        }

        /// <summary>
        /// Updates the menu tree. This method is usually called automatically when needed.
        /// </summary>
        public void UpdateMenuTree()
        {
            this.root.UpdateMenuTreeRecursive(true);
            this.root.UpdateFlatMenuItemNavigation();
        }

        /// <summary>
        /// Handles the keyboard menu navigation. Call this at the end of your GUI scope, to prevent the menu tree from stealing input events from other text fields.
        /// </summary>
        /// <returns>Returns true, if anything was changed via the keyboard.</returns>
        [Obsolete("Use HandleKeyboardMenuNavigation instead.", false)]
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public bool HandleKeybaordMenuNavigation()
        {
            return this.HandleKeyboardMenuNavigation();
        }

        /// <summary>
        /// Handles the keyboard menu navigation. Call this at the end of your GUI scope, to prevent the menu tree from stealing input events from other text fields.
        /// </summary>
        /// <returns>Returns true, if anything was changed via the keyboard.</returns>
        public bool HandleKeyboardMenuNavigation()
        {
            if (Event.current.type != EventType.KeyDown)
            {
                return false;
            }

            if (OdinMenuTree.ActiveMenuTree != this)
            {
                return false;
            }

            GUIHelper.RequestRepaint();

            var keycode = Event.current.keyCode;

            // Select first or last if no visible items is selected.
            if (this.Selection.Count == 0 || !this.Selection.Any(x => x._IsVisible() && x.IsEnabled && x.IsSelectable))
            {
                var query = this.DrawInSearchMode ? this.FlatMenuTree : this.EnumerateTree().Where(x => x._IsVisible() && x.IsEnabled && x.IsSelectable);

                OdinMenuItem next = null;
                if (keycode == KeyCode.DownArrow)
                {
                    next = query.FirstOrDefault();
                }
                else if (keycode == KeyCode.UpArrow)
                {
                    next = query.LastOrDefault();
                }
                else if (keycode == KeyCode.LeftAlt)
                {
                    next = query.FirstOrDefault();
                }
                else if (keycode == KeyCode.RightAlt)
                {
                    next = query.FirstOrDefault();
                }

                if (next != null)
                {
                    next.Select();
                    Event.current.Use();
                    return true;
                }
            }
            else
            {
                if (keycode == KeyCode.LeftArrow && !this.DrawInSearchMode)
                {
                    bool goUp = true;
                    foreach (var curr in this.Selection.ToList())
                    {
                        if (curr.Toggled == true && curr.ChildMenuItems.Any())
                        {
                            goUp = false;
                            curr.Toggled = false;
                        }

                        if ((Event.current.modifiers & EventModifiers.Alt) != 0)
                        {
                            goUp = false;
                            foreach (var item in curr.GetChildMenuItemsRecursive(false))
                            {
                                item.Toggled = curr.Toggled;
                            }
                        }
                    }

                    if (goUp)
                    {
                        keycode = KeyCode.UpArrow;
                    }

                    Event.current.Use();
                }

                if (keycode == KeyCode.RightArrow && !this.DrawInSearchMode)
                {
                    bool goDown = true;
                    foreach (var curr in this.Selection.ToList())
                    {
                        if (curr.Toggled == false && curr.ChildMenuItems.Any())
                        {
                            curr.Toggled = true;
                            goDown = false;
                        }

                        if ((Event.current.modifiers & EventModifiers.Alt) != 0)
                        {
                            goDown = false;

                            foreach (var item in curr.GetChildMenuItemsRecursive(false))
                            {
                                item.Toggled = curr.Toggled;
                            }
                        }
                    }

                    if (goDown)
                    {
                        keycode = KeyCode.DownArrow;
                    }

                    Event.current.Use();
                }

                if (keycode == KeyCode.UpArrow)
                {
                    if ((Event.current.modifiers & EventModifiers.Shift) != 0)
                    {
                        var last = this.Selection.Last();
                        var prev = last.PrevSelectableMenuItem;

                        if (prev != null)
                        {
                            if (prev.IsSelected)
                            {
                                last.Deselect();
                            }
                            else
                            {
                                prev.Select(true);
                            }

                            Event.current.Use();
                            return true;
                        }
                    }
                    else
                    {
                        var prev = this.Selection.Last().PrevSelectableMenuItem;
                        if (prev != null)
                        {
                            prev.Select();
                            Event.current.Use();
                            return true;
                        }
                    }
                }

                if (keycode == KeyCode.DownArrow)
                {
                    if ((Event.current.modifiers & EventModifiers.Shift) != 0)
                    {
                        var last = this.Selection.Last();
                        var next = last.NextSelectableMenuItem;

                        if (next != null)
                        {
                            if (next.IsSelected)
                            {
                                last.Deselect();
                            }
                            else
                            {
                                next.Select(true);
                            }

                            Event.current.Use();
                            return true;
                        }
                    }
                    else
                    {
                        var next = this.Selection.Last().NextSelectableMenuItem;

                        if (next != null)
                        {
                            next.Select();
                            Event.current.Use();
                            return true;
                        }
                    }
                }

                if (keycode == KeyCode.Return)
                {
                    this.Selection.ConfirmSelection();
                    Event.current.Use();
                    return true;
                }
            }

            return false;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.MenuItems.GetEnumerator();
        }
    }
}
#endif