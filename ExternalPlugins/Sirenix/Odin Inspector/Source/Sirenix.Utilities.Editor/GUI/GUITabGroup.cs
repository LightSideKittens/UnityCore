//-----------------------------------------------------------------------
// <copyright file="GUITabGroup.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#if UNITY_EDITOR
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
namespace Sirenix.Utilities.Editor
{
#pragma warning disable

    using Sirenix.OdinInspector;
    using Sirenix.Reflection.Editor;
    using Sirenix.Utilities;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// The GUITabGroup is a utility class to draw animated tab groups.
    /// </summary>
    /// <example>
    /// <code>
    /// var tabGroup = SirenixEditorGUI.CreateAnimatedTabGroup(someKey);
    /// // Register your tabs before starting BeginGroup.
    /// var tab1 = tabGroup.RegisterTab("tab 1");
    /// var tab2 = tabGroup.RegisterTab("tab 2");
    ///
    /// tabGroup.BeginGroup(drawToolbar: true);
    /// {
    ///     if (tab1.BeginPage())
    ///     {
    ///         // Draw GUI for the first tab page;
    ///     }
    ///     tab1.EndPage();
    ///
    ///     if (tab2.BeginPage())
    ///     {
    ///         // Draw GUI for the second tab page;
    ///     }
    ///     tab2.EndPage();
    /// }
    /// tabGroup.EndGroup();
    ///
    /// // Control the animation speed.
    /// tabGroup.AnimationSpeed = 0.2f;
    ///
    /// // If true, the tab group will have the height equal to the biggest page. Otherwise the tab group will animate in height as well when changing page.
    /// tabGroup.FixedHeight = true;
    ///
    /// // You can change page by calling:
    /// tabGroup.GoToNextPage();
    /// tabGroup.GoToPreviousPage();
    /// </code>
    /// </example>
    /// <seealso cref="SirenixEditorGUI"/>
    public class GUITabGroup
    {
        private GUILayoutOption[] options = GUILayoutOptions.ExpandWidth(true).ExpandHeight(false);
        private GUITabPage currentPage;
        private GUITabPage targetPage;
        private Vector2 scrollPosition;
        private float currentHeight;
        private Dictionary<string, GUITabPage> pages = new Dictionary<string, GUITabPage>();
        private float t = 1f;
        private bool isAnimating;
        private GUITabPage nextPage;
        private bool drawToolbar;
        private float toolbarHeight = 26;

        /// <summary>
        /// The animation speed
        /// </summary>
        public float AnimationSpeed = 4;
        public bool FixedHeight;
        public bool ExpandHeight;
        public TabLayouting TabLayouting;

        [Obsolete("This no longer does anything.")]
        public bool DrawNonSelectedTabsAsDisabled = false;

        private IEnumerable<GUITabPage> OrderedPages
        {
            get
            {
                return this.pages.Select(x => x.Value).OrderBy(x => x.Order);
            }
        }

        /// <summary>
        /// Gets the outer rect of the entire tab group.
        /// </summary>
        public Rect OuterRect { get; private set; }

        /// <summary>
        /// The inner rect of the current tab page.
        /// </summary>
        public Rect InnerRect { get; private set; }

        /// <summary>
        /// If true, the tab group will have the height equal to the biggest page. Otherwise the tab group will animate in height as well when changing page.
        /// </summary>

        /// <summary>
        /// Sets the current page.
        /// </summary>
        /// <param name="page">The page to switch to.</param>
        public void SetCurrentPage(GUITabPage page)
        {
            if (!this.pages.ContainsValue(page))
            {
                throw new InvalidOperationException("Page is not part of TabGroup");
            }

            this.currentPage = page;
            this.targetPage = null;
        }

        public GUITabPage NextPage { get { return this.nextPage; } }

        /// <summary>
        /// Gets the current page.
        /// </summary>
        public GUITabPage CurrentPage { get { return this.targetPage ?? this.currentPage; } }

        /// <summary>
        /// Gets the t.
        /// </summary>
        public float T { get { return this.t; } }

        internal bool IsAnimating { get { return this.isAnimating; } }

        internal float InnerContainerWidth { get; private set; }

        internal float LabelWidth { get; private set; }

        /// <summary>
        /// The height of the tab buttons.
        /// </summary>
        public float ToolbarHeight
        {
            get { return this.toolbarHeight; }
            set { this.toolbarHeight = value; }
        }

        /// <summary>
        /// Registers the tab.
        /// </summary>
        public GUITabPage RegisterTab(string title)
        {
            if (title == null)
            {
                throw new ArgumentNullException("title");
            }
            GUITabPage result;
            if (this.pages.TryGetValue(title, out result) == false)
            {
                result = this.pages[title] = new GUITabPage(this, title);
            }

            return result;
        }

        /// <summary>
        /// Begins the group.
        /// </summary>
        /// <param name="drawToolbar">if set to <c>true</c> a tool-bar for changing pages is drawn.</param>
        /// <param name="style">The style.</param>
        public void BeginGroup(bool drawToolbar = true, GUIStyle style = null)
        {
            this.LabelWidth = GUIHelper.BetterLabelWidth;

            if (Event.current.type == EventType.Layout)
            {
                this.drawToolbar = drawToolbar;
            }

            style = style ?? SirenixGUIStyles.MessageBox;

            this.InnerContainerWidth = this.OuterRect.width - (
                 style.padding.left +
                 style.padding.right +
                 style.margin.left +
                 style.margin.right
             );

            if (this.currentPage == null && this.pages.Count > 0)
                this.currentPage = this.pages.Select(x => x.Value).OrderBy(x => x.Order).First();

            if (this.currentPage != null && this.pages.ContainsKey(this.currentPage.TabName) == false)
            {
                if (this.pages.Count > 0)
                    this.currentPage = this.OrderedPages.First();
                else
                    this.currentPage = null;
            }

            float maxHeight = 0;

            foreach (var page in this.pages.GFValueIterator())
            {
                page.OnBeginGroup();
                maxHeight = Mathf.Max(page.Rect.height, maxHeight);
                if (Event.current.type == EventType.Layout)
                {
                    if (page.IsVisible != (page.IsVisible = page == this.targetPage || page == this.currentPage))
                    {
                        if (this.targetPage == null)
                        {
                            this.scrollPosition.x = 0f;
                            this.currentHeight = this.currentPage.Rect.height;
                        }
                        else
                        {
                            this.scrollPosition.x = this.targetPage.Order >= this.currentPage.Order ? 0 : this.scrollPosition.x = this.OuterRect.width;
                            this.currentHeight = this.currentPage.Rect.height;
                        }
                    }
                }
            }

            GUILayout.Space(1);
            var outerRect = EditorGUILayout.BeginVertical(style, GUILayoutOptions.ExpandWidth(true).ExpandHeight(this.ExpandHeight));
            if (this.drawToolbar)
            {
                this.DrawToolbar(style);
            }


            if (this.InnerRect.width > 0 && !this.ExpandHeight)
            {
                if (this.options.Length == 2)
                {
                    if (this.currentPage != null)
                    {
                        this.currentHeight = this.currentPage.Rect.height;
                    }

                    this.options = GUILayoutOptions.ExpandWidth(true).ExpandHeight(this.ExpandHeight).Height(this.currentHeight);
                }

                if (this.FixedHeight)
                {
                    this.options[2] = GUILayout.Height(maxHeight);
                }
                else
                {
                    this.options[2] = GUILayout.Height(this.currentHeight);
                }
            }

            GUIHelper.PushGUIEnabled(false);
            GUILayout.BeginScrollView(this.scrollPosition, false, false, GUIStyle.none, GUIStyle.none, this.options);
            GUIHelper.PopGUIEnabled();
            var innerRect = EditorGUILayout.BeginHorizontal(GUILayoutOptions.ExpandHeight(this.ExpandHeight));

            if (Event.current.type == EventType.Repaint)
            {
                this.OuterRect = outerRect;
                this.InnerRect = innerRect;
            }

            Animate();
        }

        /// <summary>
        /// Ends the group.
        /// </summary>
        public void EndGroup()
        {
            EditorGUILayout.EndHorizontal();
            GUIHelper.PushGUIEnabled(false);
            GUILayout.EndScrollView();
            GUIHelper.PopGUIEnabled();

            EditorGUILayout.EndVertical();

            if (this.targetPage != this.currentPage && this.targetPage != null)
            {
                GUIHelper.RequestRepaint();
            }

            //Animate();

            foreach (var page in this.pages.GFValueIterator())
            {
                page.OnEndGroup();
            }

            if (this.isAnimating == false && this.nextPage != null)
            {
                this.targetPage = this.nextPage;
                this.nextPage = null;
            }
        }

        private void Animate()
        {
            if (this.currentPage != null && Event.current.type == EventType.Layout)
            {
                if (this.isAnimating && this.targetPage != null && this.targetPage != this.currentPage)
                {
                    this.t = this.t + GUITimeHelper.LayoutDeltaTime * this.AnimationSpeed;
                    this.scrollPosition.x = Mathf.Lerp(this.currentPage.Rect.x, this.targetPage.Rect.x, Mathf.Min(1f, MathUtilities.Hermite01(this.t)));
                    this.currentHeight = Mathf.Lerp(this.currentPage.Rect.height, this.targetPage.Rect.height, Mathf.Min(1f, MathUtilities.Hermite01(this.t)));

                    if (this.t >= 1f)
                    {
                        this.currentPage.IsVisible = false;
                        this.currentPage = this.targetPage;
                        this.targetPage = null;
                        this.scrollPosition.x = 0f;
                        this.currentHeight = this.currentPage.Rect.height;
                        this.t = 1f;
                    }
                }
                else
                {
                    this.t = 0f;
                    this.isAnimating = false;
                    this.scrollPosition.x = this.currentPage.Rect.x;
                    this.currentHeight = this.currentPage.Rect.height;
                    if (this.targetPage != null && this.targetPage != this.currentPage && this.targetPage.IsVisible)
                    {
                        this.isAnimating = true;
                        this.scrollPosition.x = this.targetPage.Order > this.currentPage.Order ? 0 : this.scrollPosition.x = this.OuterRect.width;
                        this.t = 0;
                    }
                }
            }
        }

        private class ToolbarBtnWidth
        {
            public GUITabPage page;
            public int w1;
            public int w2;
            public int w3;
            internal int width;

            public ToolbarBtnWidth(GUITabPage page, int w1, int w2, int w3)
            {
                this.page = page;
                this.w1 = w1;
                this.w2 = w2;
                this.w3 = w3;
            }
        }

        private void DrawToolbar(GUIStyle style)
        {
            if (this.TabLayouting == TabLayouting.Shrink)
            {
                DrawSingleLineToolbar(style);
            }
            else if (this.TabLayouting == TabLayouting.MultiRow)
            {
                DrawMultilineToolbar(style);
            }
            else
            {
                throw new NotImplementedException(this.TabLayouting.ToString());
            }
        }

        MultilineWrapLayoutUtility util = new MultilineWrapLayoutUtility(20);
        List<GUITabPage> tabs = new List<GUITabPage>();

        private void DrawMultilineToolbar(GUIStyle style)
        {
            tabs.Clear();
            var selectedIndex = 0;
            GUITabPage selected = null;
            foreach (var tab in this.OrderedPages)
            {
                if (tab.IsActive)
                {
                    if (tab == (this.nextPage ?? this.CurrentPage))
                    {
                        selectedIndex = tabs.Count;
                        selected = tab;
                    }

                    tabs.Add(tab);
                }
            }

            if (util.Items.Length != tabs.Count)
            {
                util.Items = new MultilineWrapLayoutUtility.Item[tabs.Count];
                for (int i = 0; i < tabs.Count; i++)
                {
                    var tab = tabs[i];
                    var hasIcon = tab.Icon != SdfIconType.None;
                    SirenixEditorGUI.CalculateMinimumSDFIconButtonWidth(tab.Title, SirenixGUIStyles.LabelCentered, hasIcon, 20, out var labelWidth, out var iconWidth, out var singlePadding, out var btnWidth);
                    util.Items[i].Index = i;
                    util.Items[i].width = btnWidth + 2;
                }
            }

            util.SelectedIndex = selectedIndex;
            util.LineHeight = 20;
            util.marginTop = -style.padding.top + 1;
            util.marginLeft = -style.padding.left;
            util.marginRight = -style.padding.right;
            util.ComputeAndAllocateRect();


            var bottomBorder = style == GUIStyle.none ? 1 : 0;
            var isProSkin = EditorGUIUtility.isProSkin;
            var borderColor = isProSkin ? SirenixGUIStyles.BorderColor : new Color(0, 0, 0, 0.12f);

            foreach (var item in util.Items)
            {
                var p = tabs[item.Index];
                var r = item.Rect;

                var isActive = item.Index == selectedIndex;

                if (!isActive)
                {
                    var bgColor = isProSkin ? new Color(0.19f, 0.19f, 0.19f, 1f) : new Color(0, 0, 0, 0.1f);
                    GUI.DrawTexture(r, Texture2D.whiteTexture, ScaleMode.StretchToFill, false, 0, bgColor, Vector4.zero, new Vector4(0, 0, 0, 0));
                }


                var leftBorderWidth = item.isFirstInRow ? 1 : 0;
                var bottomBorderWidth = 1;
                var rightBorderWidth = isActive ? bottomBorder : 1;

                if (!isProSkin && item.isFirstInRow)
                    leftBorderWidth = 0;

                if (!isProSkin && item.isLastInRow)
                {
                    if (!isActive)
                    {
                        GUI.DrawTexture(r.AlignBottom(1), Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 0, borderColor, 0, 0);
                    }
                }
                else
                {
                    GUI.DrawTexture(r, Texture2D.whiteTexture, ScaleMode.StretchToFill, false, 0, borderColor,
                        new Vector4(leftBorderWidth, 0, bottomBorderWidth, rightBorderWidth),
                        new Vector4(0, 0, 0, 0));
                }

                Color prevTextColor = default;
                if (p.TextColor.HasValue)
                {
                    prevTextColor = SirenixGUIStyles.LabelCentered.normal.textColor;
                    SirenixGUIStyles.LabelCentered.normal.textColor = p.TextColor.Value;
                    SirenixGUIStyles.LabelCentered.hover.textColor = p.TextColor.Value;

                }

                if (SirenixEditorGUI.SDFIconButton(r, GUIHelper.TempContent(p.Title, p.Tooltip), p.Icon, style: SirenixGUIStyles.LabelCentered))
                {
                    if (this.currentPage != p)
                        this.nextPage = p;
                }

                if (p.TextColor.HasValue)
                {
                    SirenixGUIStyles.LabelCentered.normal.textColor = prevTextColor;
                    SirenixGUIStyles.LabelCentered.hover.textColor = prevTextColor;
                }

            }

        }

        private void DrawSingleLineToolbar(GUIStyle style)
        {
            var toolbarRect = GUILayoutUtility.GetRect(0, this.toolbarHeight - style.padding.vertical).Expand(style.padding.left, style.padding.right, style.padding.top, 0);

            if (Event.current.type != EventType.Layout)
            {
                var totalWidth = toolbarRect.width;
                var tabBtns = new List<ToolbarBtnWidth>();

                int totalW1 = 0;
                int totalW2 = 0;
                int totalW3 = 0;

                foreach (var page in this.OrderedPages.Where(x => x.IsActive))
                {
                    var hasIcon = page.Icon != SdfIconType.None;
                    SirenixEditorGUI.CalculateMinimumSDFIconButtonWidth(page.Title, SirenixGUIStyles.LabelCentered, hasIcon, toolbarRect.height, out var labelWidth, out var iconWidth, out var singlePadding, out var btnWidth);
                    btnWidth++;
                    iconWidth++;
                    var w1 = hasIcon ? (int)(iconWidth + singlePadding * 2) : 15;
                    var w2 = hasIcon ? (int)(iconWidth + singlePadding * 2) : (int)btnWidth;
                    var w3 = (int)btnWidth;

                    if (page == CurrentPage)
                        w1 = w2 = w3;

                    totalW1 += w1;
                    totalW2 += w2;
                    totalW3 += w3;
                    tabBtns.Add(new ToolbarBtnWidth(page, w1, w2, w3));
                }

                float remaining;

                if (totalWidth >= totalW3)
                {
                    remaining = totalWidth - totalW3;
                    foreach (var t in tabBtns)
                        t.width = t.w3;
                }
                else if (totalWidth >= totalW2)
                {
                    var f = 1 - (totalW3 - totalWidth) / (totalW3 - totalW2);
                    remaining = totalWidth;
                    foreach (var t in tabBtns)
                    {
                        if (t.w1 == 15)
                            t.width = (int)Mathf.Lerp(t.w2, t.w3, f);
                        else
                            t.width = t.w2;

                        remaining -= t.width;
                    }
                }
                else
                {
                    var f = 1 - (totalW2 - totalWidth) / (totalW2 - totalW1);
                    remaining = totalWidth - Mathf.Lerp(totalW1, totalW2, f);
                    foreach (var t in tabBtns)
                        t.width = (int)Mathf.Lerp(t.w1, t.w2, f);
                }

                remaining = Math.Max(0, remaining);

                var split = (int)(remaining / tabBtns.Count);

                var rect = toolbarRect;
                rect.yMin++;
                var bottomBorder = style == GUIStyle.none ? 1 : 0;
                var isProSkin = EditorGUIUtility.isProSkin;
                var borderColor = isProSkin ? SirenixGUIStyles.BorderColor : new Color(0, 0, 0, 0.12f);

                for (int i = 0; i < tabBtns.Count; i++)
                {
                    var p = tabBtns[i];
                    //var width = Math.Max(p.width + split, p.minWidth);
                    var width = p.width + split;
                    var r = (i == tabBtns.Count - 1) ? rect : rect.TakeFromLeft(width);

                    var isActive = p.page == (this.nextPage ?? this.CurrentPage);

                    if (!isActive)
                    {
                        var bgColor = isProSkin ? new Color(0.19f, 0.19f, 0.19f, 1f) : new Color(0, 0, 0, 0.1f);


                        GUI.DrawTexture(r, Texture2D.whiteTexture, ScaleMode.StretchToFill, false, 0, bgColor, Vector4.zero, new Vector4(0, 0, 0, 0));
                    }

                    var leftBorderWidth = i == 0 ? 1 : 0;
                    var bottomBorderWidth = 1;
                    var rightBorderWidth = isActive ? bottomBorder : 1;

                    if (!isProSkin && i == 0)
                        leftBorderWidth = 0;

                    if (!isProSkin && i == tabBtns.Count - 1)
                    {
                        if (!isActive)
                        {
                            GUI.DrawTexture(r.AlignBottom(1), Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 0, borderColor, 0, 0);
                        }
                    }
                    else
                    {
                        GUI.DrawTexture(r, Texture2D.whiteTexture, ScaleMode.StretchToFill, false, 0, borderColor,
                            new Vector4(leftBorderWidth, 0, bottomBorderWidth, rightBorderWidth),
                            new Vector4(0, 0, 0, 0));
                    }

                    Color prevTextColor = default;
                    if (p.page.TextColor.HasValue)
                    {
                        prevTextColor = SirenixGUIStyles.LabelCentered.normal.textColor;
                        SirenixGUIStyles.LabelCentered.normal.textColor = p.page.TextColor.Value;
                        SirenixGUIStyles.LabelCentered.hover.textColor = p.page.TextColor.Value;

                    }

                    if (SirenixEditorGUI.SDFIconButton(r, GUIHelper.TempContent(p.page.Title, p.page.Tooltip), p.page.Icon, style: SirenixGUIStyles.LabelCentered))
                    {
                        if (this.currentPage != p.page)
                            this.nextPage = p.page;
                    }

                    if (p.page.TextColor.HasValue)
                    {
                        SirenixGUIStyles.LabelCentered.normal.textColor = prevTextColor;
                        SirenixGUIStyles.LabelCentered.hover.textColor = prevTextColor;
                    }
                }
            }
        }

        /// <summary>
        /// Goes to page.
        /// </summary>
        public void GoToPage(GUITabPage page)
        {
            this.nextPage = page;
        }

        public void GoToPage(string pageName)
        {
            GUITabPage page;
            if (this.pages.TryGetValue(pageName, out page))
            {
                this.GoToPage(page);
            }
            else
            {
                throw new InvalidOperationException("No such tab page exists");
            }
        }

        /// <summary>
        /// Goes to next page.
        /// </summary>
        public void GoToNextPage()
        {
            if (this.currentPage != null)
            {
                bool takeNext = false;
                var ordered = this.OrderedPages.ToList();
                for (int i = 0; i < ordered.Count; i++)
                {
                    if (takeNext && ordered[i].IsActive)
                    {
                        this.nextPage = ordered[i];
                        break;
                    }
                    if (ordered[i] == (this.nextPage ?? this.CurrentPage))
                    {
                        takeNext = true;
                    }
                }
            }
        }

        /// <summary>
        /// Goes to previous page.
        /// </summary>
        public void GoToPreviousPage()
        {
            if (this.currentPage != null)
            {
                var ordered = this.OrderedPages.ToList();
                int prevIdx = -1;
                for (int i = 0; i < ordered.Count; i++)
                {
                    if (ordered[i] == (this.nextPage ?? this.CurrentPage))
                    {
                        if (prevIdx >= 0)
                        {
                            this.nextPage = ordered[prevIdx];
                        }
                        break;
                    }

                    if (ordered[i].IsActive)
                    {
                        prevIdx = i;
                    }
                }
            }
        }
    }

    public class MultilineWrapLayoutUtility
    {
        public Item[] Items;
        public int SelectedIndex;
        public float LineHeight;

        private Rect lastWidth;
        private bool isFirstFrame;
        private int selectedRowIndex = -1;
        private List<Row> rows = new List<Row>();
        private int[] rowOrder = new int[0];

        public int marginLeft;
        public int marginRight;
        public int marginTop;
        public int marginBottom;

        private struct Row
        {
            public int from;
            public int to;
            public int rowIndex;
            public float remainingWidth;

            public Row(int from, int to, int rowIndex, float remainingWidth)
            {
                this.from = from;
                this.to = to;
                this.rowIndex = rowIndex;
                this.remainingWidth = remainingWidth;
            }
        }

        public MultilineWrapLayoutUtility(int lineHeight)
        {
            this.Items = new Item[0];
            this.isFirstFrame = true;
            this.LineHeight = lineHeight;
        }

        public struct Item
        {
            public int Index;
            public Rect Rect;
            public float width;
            public float xOffset;
            public bool isFirstInRow;
            public bool isLastInRow;
        }

        public Rect ComputeAndAllocateRect()
        {
            var newRect = GUILayoutUtilityCalcHeightBasedOnWidthLayoutEntry.GetRect(this.CalcHeight);

            if (Event.current.type != EventType.Repaint)
                return default;

            newRect.x += marginLeft;
            newRect.y += marginTop;
            newRect.height -= marginBottom;

            var yOffset = 0f;

            if (newRect != this.lastWidth)
                this.isFirstFrame = true;

            this.lastWidth = newRect;

            if (isFirstFrame)
            {
                isFirstFrame = false;
                this.selectedRowIndex = -1;

                for (int j = 0; j < rows.Count; j++)
                {
                    for (int i = this.rows[j].from; i < this.rows[j].to; i++)
                    {
                        ref var item = ref this.Items[i];
                        item.Rect.height = this.LineHeight;
                        item.Rect.x = newRect.x + item.xOffset;
                        item.Rect.y = newRect.y + yOffset;
                    }

                    yOffset += this.LineHeight;
                }
            }
            else
            {
                var lastRowY = newRect.y + rows.Count * LineHeight;

                for (int j = 0; j < rows.Count; j++)
                {
                    for (int i = this.rows[j].from; i < this.rows[j].to; i++)
                    {
                        ref var item = ref this.Items[i];
                        item.Rect.height = this.LineHeight;
                        item.Rect.x = newRect.x + item.xOffset;
                        item.Rect.y = Mathf.Lerp(item.Rect.y, newRect.y + yOffset, GUITimeHelper.RepaintDeltaTime * 20);
                    }

                    yOffset += this.LineHeight;
                }
            }

            return newRect;
        }

        private float CalcHeight(float totalWidth)
        {
            totalWidth -= marginRight + marginLeft;

            this.rows.Clear();
            var prevSelectedRowIndex = this.selectedRowIndex;
            this.selectedRowIndex = 0;
            float offsetY = 0;

            {
                float offsetX = 0;
                var rowStart = 0;
                var rowIndex = 0;

                for (int i = 0; i < Items.Length; i++)
                {
                    ref var item = ref this.Items[i];
                    offsetX += item.width;

                    if (i == this.Items.Length - 1 || offsetX + this.Items[i + 1].width >= totalWidth)
                    {
                        float w = 0f;
                        for (int j = rowStart; j < i + 1; j++)
                            w += this.Items[j].width;

                        this.rows.Add(new Row(rowStart, i + 1, rowIndex, totalWidth - w));

                        offsetX = 0;
                        offsetY += this.LineHeight;
                        rowStart = i + 1;
                        rowIndex++;
                    }
                }
            }

            if (rows.Count > 1)
            {
                var totalNumberOfItems = rows.Sum(x => x.to - x.from);
                var bestNumberOfItemsInEachRow = totalNumberOfItems / this.rows.Count;
                if (bestNumberOfItemsInEachRow > 1)
                {

                    var k = 0;

                    while (k < 3)
                    {
                        bool movedItem = false;

                        for (int i = 0; i < rows.Count; i++)
                        {
                            var row = rows[i];
                            var length = row.to - row.from;
                            var delta = length - bestNumberOfItemsInEachRow;

                            if (delta > 0 && i < rows.Count - 1)
                            {
                                var nextRow = rows[i + 1];
                                var lastItemWidthInThisRow = Items[row.to - 1].width;
                                var spaceInNextRow = nextRow.remainingWidth;

                                if (lastItemWidthInThisRow <= spaceInNextRow)
                                {
                                    nextRow.remainingWidth -= lastItemWidthInThisRow;
                                    row.remainingWidth += lastItemWidthInThisRow;
                                    row.to--;
                                    nextRow.from--;
                                    movedItem = true;
                                    rows[i + 1] = nextRow;
                                    rows[i] = row;
                                }
                            }
                        }

                        if (!movedItem)
                        {
                            k++;
                            break;
                        }
                    }

                    //Debug.Log(k);
                }
            }


            foreach (var row in this.rows)
            {
                float remaining = totalWidth;

                if (this.SelectedIndex >= row.from && this.SelectedIndex < row.to)
                    this.selectedRowIndex = row.rowIndex;

                for (int i = row.from; i < row.to; i++)
                {
                    var item = this.Items[i];
                    remaining -= item.width;
                }

                var split = (int)(remaining / (row.to - row.from));
                var currX = 0f;
                for (int i = row.from; i < row.to; i++)
                {
                    ref var item = ref this.Items[i];

                    item.isLastInRow = i == row.to - 1;
                    item.isFirstInRow = i == row.from;

                    if (item.isLastInRow)
                    {
                        split = (int)(totalWidth - currX - item.width);
                    }

                    var width = item.width + split;
                    item.Rect.width = width;
                    item.xOffset = currX;
                    currX += width;
                }
            }

            if (this.rowOrder.Length != this.rows.Count)
            {
                this.rowOrder = new int[this.rows.Count];
                for (int i = 0; i < this.rowOrder.Length; i++)
                    this.rowOrder[i] = i;
            }

            if (prevSelectedRowIndex != this.selectedRowIndex || this.isFirstFrame)
            {
                int currentOrder = rowOrder[selectedRowIndex];
                for (int i = 0; i < rows.Count; i++)
                    if (rowOrder[i] > currentOrder)
                        rowOrder[i]--;
                rowOrder[selectedRowIndex] = rows.Count - 1;
                prevSelectedRowIndex = this.selectedRowIndex;
            }

            rows.Sort((a, b) => this.rowOrder[a.rowIndex].CompareTo(this.rowOrder[b.rowIndex]));


            return offsetY;
        }
    }


    public static class GUILayoutUtilityCalcHeightBasedOnWidthLayoutEntry
    {
        public static Rect GetRect(Func<float, float> calcHeight)
        {
            var e = Event.current.type;

            if (e == EventType.Layout)
            {
                GUILayoutUtility_Internals.TopLevel.Add(Create(calcHeight));
                return default;
            }
            else if (e == EventType.Used)
            {
                return GUILayoutUtility_Internals.TopLevel.GetNext().rect;
                return new Rect();
            }
            else
            {
                return GUILayoutUtility_Internals.TopLevel.GetNext().rect;
            }
        }

        private static GUILayoutEntry_Internal<Func<float, float>> Create(Func<float, float> element)
        {
            return GUILayoutEntry_Internal<Func<float, float>>.CreateCustom(element, SetVertical, SetHorizontal, CalcWidth, CalcHeight);
        }

        private static void CalcWidth(ref GUILayoutEntry_Internal<Func<float, float>> entry) { }

        private static void CalcHeight(ref GUILayoutEntry_Internal<Func<float, float>> entry) { }

        private static void SetVertical(ref GUILayoutEntry_Internal<Func<float, float>> entry, float y, float height)
        {
            entry.rect.y = y;
            Update(ref entry);
        }

        private static void SetHorizontal(ref GUILayoutEntry_Internal<Func<float, float>> entry, float x, float width)
        {
            entry.rect.x = x;
            entry.minWidth = width;
            entry.maxWidth = width;
            Update(ref entry);
        }

        private static void Update(ref GUILayoutEntry_Internal<Func<float, float>> entry)
        {
            var size = entry.Value(entry.minWidth);
            entry.maxHeight = size;
            entry.minHeight = size;
            entry.rect.width = entry.minWidth;
            entry.rect.height = entry.minHeight;
        }
    }
}
#endif