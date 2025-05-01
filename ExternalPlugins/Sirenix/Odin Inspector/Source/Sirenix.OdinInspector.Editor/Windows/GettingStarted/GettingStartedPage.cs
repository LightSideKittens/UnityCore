//-----------------------------------------------------------------------
// <copyright file="GettingStartedPage.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#if UNITY_EDITOR
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
using Sirenix.OdinInspector.Editor.Internal;
using Sirenix.OdinInspector.Editor.Windows;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Sirenix.OdinInspector.Editor.GettingStarted
{
#pragma warning disable

    public class GettingStartedPage
    {
        private Vector2 scrollPos;

        public float VerticalSlideT => 1 - Window.VerticalSlideT;
        public string Title;
        public SdfIconType TitleIcon;
        public float FooterSize = 80;

        [NonSerialized]
        public GettingStartedWindow Window;

        private static GUIStyle padding;

        public virtual void DrawFooter(Rect rect)
        {
        }

        public virtual void DrawPage(Rect rect)
        {
        }

        public virtual void GoBack()
        {
            if (this.Window.Pages.Count > 0)
            {
                this.Window.Pages.RemoveAt(this.Window.Pages.Count - 1);
            }
        }

        public virtual void EnterPage()
        {
            this.Window.Pages.Add(this);
        }

        protected void BeginScrollableLayoutPage(Rect rect, int paddingSize = 20)
        {
            padding = padding ?? new GUIStyle()
            {
                padding = new RectOffset() { bottom = 20, left = 20, top = 20, right = 20 }
            };
            padding.padding.left = paddingSize;
            padding.padding.right = paddingSize;
            padding.padding.top = paddingSize;
            padding.padding.bottom = paddingSize;

            GUILayout.BeginArea(rect);
            this.scrollPos = EditorGUILayout.BeginScrollView(this.scrollPos);
            GUILayout.BeginVertical(padding);
        }

        protected void EndScrollableLayoutPage()
        {
            GUILayout.EndVertical();
            EditorGUILayout.EndScrollView();
            GUILayout.EndArea();
        }
    }

    public class ButtonPage : GettingStartedPage
    {
        private Action action;

        public ButtonPage(Action action)
        {
            this.action = action;
        }

        public override void EnterPage()
        {
            this.action();
        }
    }

    public class TutorialPage : GettingStartedPage
    {
        static GUIStyle multiline;

        public List<Tutorial> Tutorials = new List<Tutorial>();

        private float prevWidth = 200;

        public override void DrawPage(Rect pageRect)
        {
            multiline = multiline ?? new GUIStyle(SirenixGUIStyles.MultiLineLabel)
            {
                alignment = TextAnchor.UpperLeft,
                clipping = TextClipping.Overflow,
            };

            if (Event.current.type == EventType.MouseMove)
            {
                GUIHelper.RequestRepaint();
            }

            this.BeginScrollableLayoutPage(pageRect);

            if (this.prevWidth != 0)
            {
                for (int i = 0; i < Tutorials.Count; i++)
                {
                    var item = this.Tutorials[i];

                    if (item.Visible != null && item.Visible() == false)
                        continue;

                    var enabled = true;
                    if (item.Enabled != null && item.Enabled() == false)
                    {
                        enabled = false;
                    }

                    var prevEnabled = GUI.enabled;
                    GUI.enabled = enabled;

                    var hasActionBtns = item.ActionButtons != null;
                    var actionBtnsHeight = hasActionBtns ? 25 : 0;
                    var textPadding = 10;
                    var iconWidth = 50;
                    var difficultyRectWidth = 30;
                    var predicted = prevWidth - textPadding - textPadding - iconWidth - difficultyRectWidth;
                    var descriptionSize = string.IsNullOrEmpty(item.Description) ? 0 : multiline.CalcHeight(GUIHelper.TempContent(item.Description), predicted);
                    var rect = GUILayoutUtility.GetRect(0, (50 + descriptionSize + actionBtnsHeight) * this.VerticalSlideT);
                    var bgRect = rect;
                    var iconRect = rect.TakeFromLeft(iconWidth).AlignCenterY(25 * this.VerticalSlideT);
                    var difficultyRect = rect.AlignBottom(EditorGUIUtility.singleLineHeight).SubX(textPadding * 0.5f).SubY(textPadding * 0.5f);
                    var contentRect = rect.Padding(textPadding);
                    var titleRect = contentRect.TakeFromTop(20);
                    var descriptionRect = contentRect;
                    var isMouseOver = enabled && !hasActionBtns && bgRect.Contains(Event.current.mousePosition);
                    if (string.IsNullOrEmpty(item.Description))
                    {
                        titleRect.y += 5;
                    }

                    SdfIcons.DrawIcon(iconRect.AddX(5), item.Icon, SirenixGUIStyles.Label.normal.textColor);
                    EditorGUI.DrawRect(bgRect, SirenixGUIStyles.HeaderBoxBackgroundColor);

                    if (isMouseOver)
                    {
                        EditorGUI.DrawRect(bgRect, SirenixGUIStyles.HeaderBoxBackgroundColor);
                    }

                    SirenixEditorGUI.DrawBorders(bgRect, 1);
                    GUI.Label(titleRect, item.Title, SirenixGUIStyles.BoldLabel);
                    if (!string.IsNullOrEmpty(item.Description))
                    {
                        GUI.Label(descriptionRect, item.Description, multiline);
                    }

                    if (item.Difficulty != Difficulty.None)
                    {
                        GUI.Label(difficultyRect, item.Difficulty.ToString(), SirenixGUIStyles.RightAlignedGreyMiniLabel);
                    }

                    if (hasActionBtns)
                    {
                        var btnsRect = descriptionRect.TakeFromBottom(actionBtnsHeight);
                        foreach (var btn in item.ActionButtons)
                        {
                            var w = GUI.skin.button.CalcSize(GUIHelper.TempContent(btn.btnName)).x + 20;
                            if (GUI.Button(btnsRect.TakeFromLeft(w), btn.btnName))
                            {
                                btn.action();
                            }

                            btnsRect.TakeFromLeft(10);
                        }
                    }
                    else
                    {
                        if (GUI.Button(rect, GUIContent.none, GUIStyle.none))
                        {
                            if (item.ChildPage != null)
                            {
                                item.ChildPage.Window = this.Window;
                                item.ChildPage.EnterPage();
                            }

                            item.OnClick?.Invoke();
                            GUIHelper.ExitGUI(true);
                        }
                    }

                    if (i != this.Tutorials.Count - 1)
                    {
                        GUILayoutUtility.GetRect(0, 10);
                    }

                    GUI.enabled = prevEnabled;
                }
            }

            var ww = GUILayoutUtility.GetRect(0, 1).width;

            if (Event.current.type == EventType.Repaint)
                this.prevWidth = ww;

            this.EndScrollableLayoutPage();
        }
    }

    public class WizardPage : GettingStartedPage
    {
        private static GUIStyle multiLineCenteredTopText;
        public List<WizardPageStep> Steps = new List<WizardPageStep>();
        public int CurrentWizardStage;
        public int NextWizardStage;
        private int nextNextWizardStage;

        public override void EnterPage()
        {
            this.CurrentWizardStage = 0;
            this.NextStageTransitionProgress = 0;
            this.NextWizardStage = 0;

            base.EnterPage();
        }

        public float NextStageTransitionProgress;

        public WizardPage(string title)
        {
            this.Title = title;
            this.TitleIcon = SdfIconType.Magic;
            this.FooterSize = 80;
        }

        public override void DrawPage(Rect rect)
        {
            if (Event.current.type == EventType.Layout)
            {
                this.nextNextWizardStage = this.NextWizardStage;
                this.nextNextWizardStage = Mathf.Clamp(this.nextNextWizardStage, 0, this.Steps.Count - 1);
            }

            if (this.nextNextWizardStage == this.CurrentWizardStage)
            {
                this.NextStageTransitionProgress = 0;
                this.Steps[CurrentWizardStage].OnGUI(rect);
            }
            else
            {
                if (Event.current.type == EventType.Layout)
                {
                    var targetT = Mathf.MoveTowards(this.NextStageTransitionProgress, 1, GUITimeHelper.LayoutDeltaTime * 2);
                    if (targetT != this.NextStageTransitionProgress)
                    {
                        this.NextStageTransitionProgress = targetT;
                        GUIHelper.RequestRepaint();
                    }
                    else
                    {
                        // Arrived at page.
                        this.CurrentWizardStage = this.nextNextWizardStage;
                        this.NextStageTransitionProgress = 1;
                    }
                }

                var r1 = rect;
                var r2 = rect;

                var left = rect;
                var right = rect;

                var direction = this.nextNextWizardStage > this.CurrentWizardStage;

                var t = direction ? this.NextStageTransitionProgress : (1 - this.NextStageTransitionProgress);
                t = t * t * (3 - 2 * t);
                t = t * t * (3 - 2 * t);

                left.x -= t * rect.width;
                right.x = left.xMax;

                var from = this.CurrentWizardStage;
                var to = this.nextNextWizardStage;

                if (!direction)
                {
                    from = this.nextNextWizardStage;
                    to = this.CurrentWizardStage;
                }

                var prevCol = GUI.color;
                GUI.color = prevCol * new Color(1, 1, 1, 1 - t);
                this.Steps[from].OnGUI(left);
                GUI.color = prevCol * new Color(1, 1, 1, t);
                this.Steps[to].OnGUI(right);
                GUI.color = prevCol;
            }
        }

        public override void DrawFooter(Rect rect)
        {
            var currentStep = this.CurrentWizardStage;

            GUIHelper.PushGUIEnabled(currentStep < (this.Steps.Count - 1) && this.NextWizardStage < (this.Steps.Count - 1));

            if (GettingStartedWindow.Button(ref rect, "Next", SdfIconType.ChevronRight, Direction.Right, Direction.Right))
            {
                this.GoToNextStage();
            }

            GUIHelper.PopGUIEnabled();

            var green = SirenixGUIStyles.ValidatorGreen;
            var black = SirenixGUIStyles.DarkEditorBackground;
            var grey = new Color(0.4352942f, 0.4352942f, 0.4352942f, 1);

            grey.a = GUI.color.a;
            black.a = GUI.color.a;
            green.a = GUI.color.a;

            var t = this.nextNextWizardStage <= this.CurrentWizardStage ? 0 : this.NextStageTransitionProgress;

            //rect.y -= 15;
            var stepSize = rect.width / this.Steps.Count;
            var bgLine = rect.AlignCenterY(5).HorizontalPadding(stepSize * 0.5f);
            EditorGUI.DrawRect(bgLine, grey);
            bgLine.width = stepSize * currentStep + t * stepSize;
            EditorGUI.DrawRect(bgLine, green);

            for (int i = 0; i < this.Steps.Count; i++)
            {
                var sectionRect = rect.Split(i, this.Steps.Count).AlignCenterY(20);
                var circleRect = sectionRect.AlignCenterX(sectionRect.height);

                var innerCircleRect = circleRect.Padding(6);

                if (i == currentStep)
                {
                    GUI.DrawTexture(circleRect, Texture2D.whiteTexture, ScaleMode.ScaleAndCrop, true, 1, green, 0, 100);
                    var white = Color.white;
                    var checkRect = innerCircleRect;

                    var s1 = innerCircleRect.width * (1 - t);
                    var s2 = innerCircleRect.width * t;

                    var b = black;
                    b.a *= (1 - t);

                    checkRect = checkRect.AlignCenter(s2, s2);
                    GUI.DrawTexture(innerCircleRect, Texture2D.whiteTexture, ScaleMode.ScaleAndCrop, true, 1, b, 0, 100);
                    SdfIcons.DrawIcon(checkRect, SdfIconType.Check, black);
                }
                else if (i < currentStep)
                {
                    GUI.DrawTexture(circleRect, Texture2D.whiteTexture, ScaleMode.ScaleAndCrop, true, 1, green, 0, 100);
                    SdfIcons.DrawIcon(innerCircleRect, SdfIconType.Check, black);
                }
                else if (i - 1 == currentStep)
                {
                    var b = black;
                    b.a *= t;

                    var g1 = grey;
                    var g2 = green;
                    g1.a *= 1 - t;
                    g2.a *= t;

                    GUI.DrawTexture(circleRect, Texture2D.whiteTexture, ScaleMode.ScaleAndCrop, true, 1, g1, 0, 100);
                    GUI.DrawTexture(circleRect, Texture2D.whiteTexture, ScaleMode.ScaleAndCrop, true, 1, g2, 0, 100);
                    GUI.DrawTexture(innerCircleRect, Texture2D.whiteTexture, ScaleMode.ScaleAndCrop, true, 1, b, 0, 100);
                }
                else
                {
                    GUI.DrawTexture(circleRect, Texture2D.whiteTexture, ScaleMode.ScaleAndCrop, true, 1, grey, 0, 100);
                }

                var titleRect = new Rect(
                    new Vector2(circleRect.center.x - stepSize * 0.5f + 5, circleRect.yMax + 5),
                    new Vector2(stepSize - 10, 60));

                multiLineCenteredTopText = multiLineCenteredTopText ?? new GUIStyle(SirenixGUIStyles.MultiLineCenteredLabel);
                multiLineCenteredTopText.alignment = TextAnchor.UpperCenter;

                GUI.Label(titleRect, Steps[i].Name, multiLineCenteredTopText);
            }
        }

        public virtual bool GoToNextStage()
        {
            if (this.NextWizardStage < this.Steps.Count)
            {
                this.NextWizardStage++;
                return true;
            }

            return false;
        }

        public override void GoBack()
        {
            if (this.CurrentWizardStage <= 0)
            {
                base.GoBack();
            }
            else
            {
                this.NextWizardStage--;
            }
        }
    }

    public class WizardPageStep
    {
        public string Name;
        public Action<Rect> OnGUI;

        public WizardPageStep(string name, Action<Rect> onGUI)
        {
            Name = name;
            OnGUI = onGUI;
        }
    }

    public struct Tutorial
    {
        public string Title;
        public string Description;
        public SdfIconType Icon;
        public Difficulty Difficulty;
        public Action OnClick;
        public GettingStartedPage ChildPage;
        public Func<bool> Enabled;
        public Func<bool> Visible;
        public List<(string btnName, Action action)> ActionButtons;
    }

    public enum Difficulty
    {
        None,
        Beginner,
        Intermediate,
        Advanced,
    }

    public class GettingStartedWindow : OdinEditorWindow
    {
        private static float speed = 2;
        private static Texture2D odinBg;

        /// <summary>
        /// Gets a texture of an odin bg symbol.
        /// </summary>
        public static Texture2D OdinBg
        {
            get
            {
                if (odinBg == null)
                {
                    var bytes = Convert.FromBase64String(GettingStartedBg.PngString);
                    odinBg = TextureUtilities.LoadImage(1024, 1024, bytes);
                    CleanupUtility.DestroyObjectOnAssemblyReload(odinBg);
                }

                return odinBg;
            }
        }

        [NonSerialized] private GettingStartedPage slideToPage;
        [NonSerialized] private GettingStartedPage slideFromPage;
        private int HorizontalSlideDirection;
        [NonSerialized] private int prevPageCount;

        [SerializeField] private float verticalSlideTRaw;
        [SerializeField] private float horizontalSlideTRaw;
        [SerializeField] private int selectedProduct;

        [SerializeField] public List<GettingStartedPage> Pages = new List<GettingStartedPage>();
        [SerializeField] public float VerticalSlideT;
        [SerializeField] public float HorizontalSlideT;
        public static bool firstOpen;

        private void CreateGUI()
        {
            var container = new IMGUIContainer(this.OnImGUI);
            container.name = "Odin ImGUIContainer";
            container.style.display = DisplayStyle.Flex;
            container.style.height = Length.Percent(100);
            container.style.width = Length.Percent(100);

            var eulaContainer = new IMGUIContainer(this.DrawEULA);
            eulaContainer.name = "Odin EULA ImGUIContainer";
            eulaContainer.style.display = DisplayStyle.Flex;
            eulaContainer.style.height = Length.Percent(100);
            eulaContainer.style.width = Length.Percent(100);

            this.rootVisualElement.Add(container);
            container.Add(eulaContainer);
        }

        protected override void OnEnable()
        {
            this.wantsMouseMove = true;
        }

        protected override void OnImGUI()
        {
            UpdateThings();

            var rect = this.position.ResetPosition();
            this.DrawToolbar(rect.TakeFromTop(EditorStyles.toolbarButton.fixedHeight + 4));
            this.DrawFancyOdinSelector(ref rect);

            if ((this.slideToPage ?? this.slideFromPage) != null)
            {
                if (this.slideToPage == null || this.slideFromPage == null)
                {
                    var page = this.slideToPage ?? this.slideFromPage;
                    this.DrawPage(ref rect, page, page.FooterSize);
                }
                else if (this.slideToPage != this.slideFromPage)
                {
                    var left = rect;
                    var right = rect;

                    var t = this.HorizontalSlideDirection == 1 ? this.HorizontalSlideT : (1 - this.HorizontalSlideT);
                    left.x -= t * rect.width;
                    right.x = left.xMax;

                    var from = this.slideFromPage;
                    var to = this.slideToPage;

                    if (this.HorizontalSlideDirection < 0)
                    {
                        to = this.slideFromPage;
                        from = this.slideToPage;
                    }

                    var footerSize = from.FooterSize * (1 - t) + to.FooterSize * t;
                    var prevCol = GUI.color;
                    GUI.color = prevCol * new Color(1, 1, 1, 1 - t);
                    this.DrawPage(ref left, from, footerSize);

                    GUI.color = prevCol * new Color(1, 1, 1, t);
                    this.DrawPage(ref right, to, footerSize);

                    GUI.color = prevCol;

                    rect.TakeFromTop(Mathf.Max(left.height, right.height)); // Left & right height should be the same.
                }
                else
                {
                    this.DrawPage(ref rect, this.slideToPage, this.slideToPage.FooterSize);
                }
            }

            this.RepaintIfRequested();
        }

        [InitializeOnLoadMethod]
        private static void AutoOpen()
        {
            if (EditorPrefs.GetBool("ODIN_INSPECTOR_SHOW_GETTING_STARTED", false))
            {
                EditorPrefs.SetBool("ODIN_INSPECTOR_SHOW_GETTING_STARTED", false);
                firstOpen = true;
                EditorApplication.delayCall += () => ShowWindow(true, false);
            }
        }

        private void UpdateThings()
        {
            if (this.prevPageCount != this.Pages.Count)
            {
                this.HorizontalSlideDirection = this.prevPageCount > this.Pages.Count ? -1 : 1;
                this.prevPageCount = this.Pages.Count;
            }

            if (Event.current.type == EventType.Layout)
            {
                // Set current page.
                this.slideToPage = this.Pages.Count == 0 ? null : this.Pages[this.Pages.Count - 1];

                var slideVertically = false;
                var slideHorizontally = false;

                if (this.slideFromPage != this.slideToPage)
                {
                    if (this.slideToPage == null)
                    {
                        // Slide up!
                        slideVertically = true;
                    }
                    else if (this.slideFromPage == null)
                    {
                        // Slide down!
                        slideVertically = true;
                    }
                    else
                    {
                        // Slide side!
                        slideHorizontally = true;
                    }
                }

                if (slideVertically)
                {
                    // Set current page.
                    var targetT = Mathf.MoveTowards(this.verticalSlideTRaw, this.slideToPage == null ? 1 : 0, GUITimeHelper.LayoutDeltaTime * speed);
                    if (targetT != this.verticalSlideTRaw)
                    {
                        this.verticalSlideTRaw = targetT;
                        GUIHelper.RequestRepaint();
                    }
                    else
                    {
                        // Arrived at page.
                        this.slideFromPage = this.slideToPage;
                    }

                    this.VerticalSlideT = this.verticalSlideTRaw * this.verticalSlideTRaw * (3 - 2 * this.verticalSlideTRaw);
                    this.VerticalSlideT = this.VerticalSlideT * this.VerticalSlideT * (3 - 2 * this.VerticalSlideT);
                }

                if (slideHorizontally)
                {
                    // Set current page.
                    var targetT = Mathf.MoveTowards(this.horizontalSlideTRaw, 1, GUITimeHelper.LayoutDeltaTime * speed);
                    if (targetT != this.horizontalSlideTRaw)
                    {
                        this.horizontalSlideTRaw = targetT;
                        GUIHelper.RequestRepaint();
                    }
                    else
                    {
                        // Arrived at page.
                        this.slideFromPage = this.slideToPage;
                        this.horizontalSlideTRaw = 1;
                    }

                    this.HorizontalSlideT = this.horizontalSlideTRaw * this.horizontalSlideTRaw * (3 - 2 * this.horizontalSlideTRaw);
                    this.HorizontalSlideT = this.HorizontalSlideT * this.HorizontalSlideT * (3 - 2 * this.HorizontalSlideT);
                }
                else
                {
                    this.horizontalSlideTRaw = 0;
                    this.HorizontalSlideT = 0;
                }

                if (this.slideFromPage != null) this.slideFromPage.Window = this;
                if (this.slideToPage != null) this.slideToPage.Window = this;
            }
        }

        private void DrawPage(ref Rect rect, GettingStartedPage page, float footerSize)
        {
            var vertical_t = page.VerticalSlideT;

            var prevCol = GUI.color;
            GUI.color *= new Color(1, 1, 1, vertical_t);
            if (rect.height > 1)
            {
                // Top
                {
                    var topRect = rect.TakeFromTop(50 * vertical_t);
                    var headerText = page.Title;
                    var style = SirenixGUIStyles.SectionHeaderCentered;
                    var textWidth = style.CalcSize(GUIHelper.TempContent(headerText)).x;
                    var iconSize = 25;
                    var spacing = 5;
                    var icon = page.TitleIcon;
                    var r = topRect.AlignCenterX(iconSize + spacing + textWidth);

                    var c = GUI.color;
                    GUI.color = Color.white;
                    EditorGUI.DrawRect(topRect, SirenixGUIStyles.HeaderBoxBackgroundColor);
                    GUI.color = c;

                    GUI.Label(r.AlignRight(textWidth), headerText, style);
                    SdfIcons.DrawIcon(r.AlignLeft(iconSize), icon, style.normal.textColor);
                }

                // Bottom 
                {
                    var bottomRect = rect.TakeFromBottom(footerSize * vertical_t);

                    // Don't fade slide background sections.
                    var c = GUI.color;
                    GUI.color = Color.white;
                    EditorGUI.DrawRect(bottomRect, SirenixGUIStyles.DarkEditorBackground);
                    EditorGUI.DrawRect(bottomRect.AlignTop(1), SirenixGUIStyles.BorderColor);
                    GUI.color = c;

                    bottomRect = bottomRect.HorizontalPadding(20);
                    bottomRect = bottomRect.AlignCenterY(25);

                    if (Button(ref bottomRect, "Back", SdfIconType.ChevronLeft, Direction.Left, Direction.Left))
                    {
                        page.GoBack();
                    }

                    page.DrawFooter(bottomRect);
                }

                // Body
                {
                    var bodyRect = rect;
                    EditorGUI.DrawRect(bodyRect.AlignTop(1), SirenixGUIStyles.BorderColor);
                    page.DrawPage(bodyRect);
                }
            }

            GUI.color = prevCol;
        }

        private void DrawToolbar(Rect rect)
        {
            EditorGUI.DrawRect(rect.TakeFromBottom(2), new Color(0, 0, 0, 1));

            rect = rect.AlignCenterY(EditorGUIUtility.singleLineHeight);

            if (ToolbarButton(ref rect, SdfIconType.InfoCircleFill, "Support", false, null))
                Application.OpenURL("https://odininspector.com/support");

            if (ToolbarButton(ref rect, SdfIconType.Discord, "Discord", false, null))
                Application.OpenURL("https://discord.gg/WTYJEra");

            if (ToolbarButton(ref rect, SdfIconType.BookFill, "Tutorials", false, null))
                Application.OpenURL("https://odininspector.com/tutorials");

            var versionName = "Version " + OdinInspectorVersion.Version + " " + OdinInspectorVersion.BuildName;

            if (ToolbarButton(ref rect, SdfIconType.None, versionName, false, SirenixGUIStyles.CenteredGreyMiniLabel))
                Application.OpenURL("https://odininspector.com/patch-notes");

            if (ToolbarButtonFromLeft(ref rect, "Overview", this.Pages.Count == 0, EditorStyles.toolbarButton))
            {
                this.Pages.Clear();
            }

            for (int i = 0; i < Pages.Count; i++)
            {
                var p = this.Pages[i];

                if (ToolbarButtonFromLeft(ref rect, p.Title, i == (this.Pages.Count - 1), EditorStyles.toolbarButton))
                {
                    this.Pages.SetLength(i + 1);
                    break;
                }
            }
        }

        public static void ShowWindow()
        {
            var wnd = GetWindow<GettingStartedWindow>("Getting Started");
            wnd.position = GUIHelper.GetEditorWindowRect().AlignCenter(900f, 900f);
            wnd.verticalSlideTRaw = 1;
            wnd.VerticalSlideT = 1;
            wnd.slideToPage = null;
            wnd.slideFromPage = null;
            wnd.ShowUtility();
        }

        internal static void ShowWindow(bool showOdinNotification, bool showValidatorNotification)
        {
            var wnd = GetWindow<GettingStartedWindow>();
            wnd.position = GUIHelper.GetEditorWindowRect().AlignCenter(900f, 900f);
            wnd.verticalSlideTRaw = 1;
            wnd.VerticalSlideT = 1;
            wnd.slideToPage = null;
            wnd.slideFromPage = null;
            wnd.ShowUtility();

            if (showOdinNotification)
            {
                GettingStartedWindowData.Products[0].NotificationT = 1;
                GettingStartedWindowData.Products[0].NotificationTargetT = 1;
            }

            if (showValidatorNotification)
            {
                GettingStartedWindowData.Products[1].NotificationT = 1;
                GettingStartedWindowData.Products[1].NotificationTargetT = 1;
            }
        }

        private void DrawFancyOdinSelector(ref Rect totalRect)
        {
            var selectorRect = totalRect.TakeFromTop(Mathf.Lerp(totalRect.height, 120, 1 - this.VerticalSlideT));

            descStyle = descStyle ?? new GUIStyle(SirenixGUIStyles.MultiLineCenteredLabel);
            descStyle.wordWrap = false;
            descStyle.alignment = TextAnchor.UpperLeft;

            titleStyle = titleStyle ?? new GUIStyle(SirenixGUIStyles.BoldTitle);
            titleStyle.alignment = TextAnchor.UpperLeft;
            titleStyle.normal.textColor = Color.white;
            titleStyle.fontSize = 14;

            EditorGUI.DrawRect(selectorRect.TakeFromBottom(2 * (1 - this.VerticalSlideT)), SirenixGUIStyles.BorderColor);
            EditorGUI.DrawRect(selectorRect.Split(0, 3), SirenixGUIStyles.ListItemColorOdd);
            EditorGUI.DrawRect(selectorRect.Split(2, 3), SirenixGUIStyles.ListItemColorOdd);
            EditorGUI.DrawRect(selectorRect.Split(0, 3).AlignRight(1), SirenixGUIStyles.BorderColor);
            EditorGUI.DrawRect(selectorRect.Split(1, 3).AlignRight(1), SirenixGUIStyles.BorderColor);

            for (int i = 0; i < GettingStartedWindowData.Products.Length; i++)
            {
                ref var p = ref GettingStartedWindowData.Products[i];
                var rect = selectorRect.Split(i, GettingStartedWindowData.Products.Length);

                if (this.VerticalSlideT > 0.001)
                {
                    GUI.color = new Color(1, 1, 1, this.VerticalSlideT);

                    // Draw buttons
                    {
                        rect.TakeFromBottom(20 * this.VerticalSlideT);
                        var btnsRect = rect.TakeFromBottom(40 * this.VerticalSlideT).Padding(10, 5 * this.VerticalSlideT);

                        for (int j = 0; j < p.Pages.Length; j++)
                        {
                            if (GUI.Button(btnsRect.Split(j, p.Pages.Length), p.Pages[j].btnName))
                            {
                                this.selectedProduct = i;
                                p.Pages[j].page.Window = this;
                                p.Pages[j].page.EnterPage();
                            }
                        }

                        rect.TakeFromBottom(20 * this.VerticalSlideT);
                    }

                    // Draw status
                    {
                        var r = rect.TakeFromBottom(40 * this.VerticalSlideT);
                        EditorGUI.DrawRect(r.AlignBottom(1), SirenixGUIStyles.BorderColor);
                        r = r.Padding(10, 8 * this.VerticalSlideT);
                        var iconRect = r.TakeFromLeft(r.height);
                        r.TakeFromLeft(10);
                        SdfIcons.DrawIcon(iconRect, p.StatusIcon, p.StatusIconColor);
                        GUI.Label(r, p.Status, SirenixGUIStyles.BoldTitle);
                    }

                    GUI.color = Color.white;
                }

                // Draw image
                {
                    var greyScale = p.Enabled ? 0f : 1f;
                    var color = p.Enabled ? Color.white : new Color(1, 1, 1, 0.5f);

                    {
                        if (this.selectedProduct != i)
                            color = Color.Lerp(color, new Color(1, 1, 1, 0.4f), 1 - this.VerticalSlideT);
                        else
                            color = Color.Lerp(color, new Color(1, 1, 1, 1), 1 - this.VerticalSlideT);

                        greyScale = Mathf.Lerp(greyScale, this.selectedProduct == i ? 0 : 1, 1 - this.VerticalSlideT);
                    }

                    GUITextureDrawingUtil.DrawTexture(rect.Expand(1), OdinBg, ScaleMode.ScaleAndCrop, color, p.HueColor, greyScale);
                    GUITextureDrawingUtil.DrawTexture(rect.Padding(40, 0), p.Logo, ScaleMode.ScaleToFit, color, default, greyScale);

                    if (i > 0) EditorGUI.DrawRect(rect.AlignLeft(2).AddXMin(-1), Color.black);

                    if (i < 2 && (OdinDefineSymbols.ODIN_ASSET_STORE || OdinDefineSymbols.SIRENIX_INTERNAL) && p.Enabled)
                    {
                        var t1 = this.VerticalSlideT;
                        if (t1 > 0)
                        {
                            var bgColor = new Color(0.25f, 0.25f, 0.25f, t1);
                            var nRect = rect.AddY(40 * t1);
                            var t2 = p.NotificationT * t1;
                            t2 = t2 * t2 * (3 - 2 * t2);
                            var starColor = new Color(1f, 0.9f, 0f, t1);// p.HueColor;
                            var a = nRect.AlignRight(t2 * (rect.width - 20 - 32) * t1 + 32 * t1).AlignTop(t2 * 85 + 32 * t1);
                            var mouseOver = a.Contains(Event.current.mousePosition) ? 1 : 0;
                            var starRect = a.Padding(7 * t1).SetSize(17 * t1);
                            var buttonsRect = a.Padding(10).AlignBottom(28);
                            var titleRect = starRect.AddX(starRect.width + 10).SetXMax(rect.xMax - 10).AddY(1);
                            var descRect = starRect.AddX(3).AddY(30).SetXMax(rect.xMax - 10).SetYMax(buttonsRect.y - 10);

                            GUI.DrawTexture(a, Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 0, bgColor, default, new Vector4(3, 0, 0, 3));
                            SdfIcons.DrawIcon(starRect.Expand(2 * mouseOver * (1 - t2)), SdfIconType.StarFill, starColor);
                            if (t2 <= 0.95f)
                            {
                                if (GUI.Button(a, GUIContent.none, GUIStyle.none))
                                    p.NotificationTargetT = 1;
                            }
                            if (t2 > 0.01)
                            {
                                var prev = GUI.color;
                                GUI.color = new Color(1, 1, 1, t2 * prev.a);
                                GUI.Label(titleRect.AddYMax(6), $"Enjoying {p.Name}?", titleStyle);

                                GUI.Label(descRect, "If you've been enjoying Odin, please consider \nleaving a review - it makes a big difference!", descStyle);

                                if (SirenixEditorGUI.SDFIconButton(titleRect.TakeFromRight(titleRect.height), GUIContent.none, SdfIconType.X, style: SirenixGUIStyles.IconButton))
                                    p.NotificationTargetT = 0;

                                if (SirenixEditorGUI.SDFIconButton(buttonsRect, "Leave a review on the Asset Store", SdfIconType.Link45deg, style: SirenixGUIStyles.ButtonLeft))
                                    Application.OpenURL(p.ReviewUrl);

                                GUI.color = prev;
                            }


                            if (Event.current.type == EventType.Repaint)
                                p.NotificationT = Mathf.MoveTowards(p.NotificationT, p.NotificationTargetT, 4 * GUITimeHelper.RepaintDeltaTime);

                            if (p.NotificationT != p.NotificationTargetT)
                                GUIHelper.RequestRepaint();
                        }
                    }

                }
            }

            //DrawRateOdinNotification(rateOdinNotificationRect);
        }

        public static float Smoothstep(float a, float b, float t)
        {
            t = Mathf.Clamp((t - a) / (b - a), 0.0f, 1.0f);
            return t * t * (3.0f - 2.0f * t);
        }

        private static GUIStyle padding;
        private static GUIStyle titleStyle;
        private static GUIStyle descStyle;

        //private void DrawRateOdinNotification(Rect area)
        //{
        //    padding = padding ?? new GUIStyle() { padding = new RectOffset(10, 10, 10, 10) };

        //    var bgColor = new Color(0.22f, 0.22f, 0.22f, 1);
        //    area.y += 32;

        //    GUILayout.BeginArea(area.AlignRight(400));

        //    var rect = EditorGUILayout.BeginVertical(padding);
        //    {
        //        var titleRect = GUILayoutUtility.GetRect(0, 20);
        //        GUI.DrawTexture(rect, Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 0, bgColor, Vector4.zero, new Vector4(10, 0, 0, 10));

        //        SdfIcons.DrawIcon(titleRect.TakeFromLeft(titleRect.height), SdfIconType.StarFill, new Color(0.95f, 0.65f, 0.14f, 1f), bgColor);
        //        if (SirenixEditorGUI.SDFIconButton(titleRect.TakeFromRight(titleRect.height), GUIContent.none, SdfIconType.X, style: SirenixGUIStyles.IconButton))
        //        {

        //        }
        //        titleRect.TakeFromLeft(10);
        //        GUI.Label(titleRect, "Lalalala", title);
        //    }
        //    GUILayout.Space(10);
        //    GUILayout.Label("Hiiii", SirenixGUIStyles.MultiLineLabel);
        //    GUILayout.Space(10);

        //    {
        //        var btnsRect = GUILayoutUtility.GetRect(0, 20);

        //        if (GUI.Button(btnsRect.Split(0, 2).HorizontalPadding(0, 5), "Rate on Unity Asset Store", SirenixGUIStyles.Button))
        //        {
        //        }
        //        if (GUI.Button(btnsRect.Split(1, 2).HorizontalPadding(5, 0), "Send Feedback", SirenixGUIStyles.Button))
        //        {
        //            OdinFeedbackWindow.Open("Odin Validator");
        //        }
        //    }

        //    EditorGUILayout.EndVertical();
        //    GUILayout.FlexibleSpace();
        //    GUILayout.EndArea();
        //}

        static bool ToolbarButtonFromLeft(ref Rect rect, string text, bool isOn, GUIStyle style)
        {
            var textStyle = style;
            var content = Sirenix.Utilities.Editor.GUIHelper.TempContent(text);
            var textWidth = textStyle.CalcSize(content).x;
            var btnPadding = 5;
            var btnWidth = textWidth + btnPadding * 2;
            var btnRect = rect.TakeFromLeft(btnWidth);
            GUIHelper.RequestRepaint();

            var hover = btnRect.Contains(Event.current.mousePosition);

            if (Event.current.type == EventType.Repaint)
            {
                style.Draw(btnRect, content, false, hover, isOn, false);
            }

            return GUI.Button(btnRect, GUIContent.none, GUIStyle.none);
        }

        static bool ToolbarButton(ref Rect rect, SdfIconType icon, string text, bool fromLeft, GUIStyle textLabelStyle)
        {
            var textStyle = textLabelStyle ?? SirenixGUIStyles.Label;
            var content = Sirenix.Utilities.Editor.GUIHelper.TempContent(text);
            var iconWidth = icon == SdfIconType.None ? 0 : rect.height;
            var iconPadding = icon == SdfIconType.None ? 0 : 5;
            var btnPadding = 5;
            var textWidth = textStyle.CalcSize(content).x;
            var btnWidth = textWidth + iconPadding + btnPadding * 2 + iconWidth;
            var r = fromLeft ? rect.TakeFromLeft(btnWidth) : rect.TakeFromRight(btnWidth);
            var clicked = GUI.Button(r, GUIContent.none, EditorStyles.toolbarButton);

            r.TakeFromLeft(btnPadding);
            var iconRect = r.TakeFromLeft(iconWidth);

            r.TakeFromLeft(iconPadding);
            var textRect = r.TakeFromLeft(textWidth);

            if (icon != SdfIconType.None)
            {
                SdfIcons.DrawIcon(iconRect.AlignCenterY(rect.height - 4), icon, textStyle.normal.textColor);
            }

            GUI.Label(textRect, content, textStyle);

            return clicked;
        }

        internal static float CalcButtonWidth(Rect rect, string text)
        {
            var textStyle = SirenixGUIStyles.WhiteLabel;
            var content = Sirenix.Utilities.Editor.GUIHelper.TempContent(text);
            var iconWidth = rect.height;
            var iconPadding = 0;
            var textWidth = textStyle.CalcSize(content).x;
            var btnWidth = textWidth + iconPadding + 5 + 10 * 2 + iconWidth;
            return btnWidth;
        }

        internal static bool Button(ref Rect rect, string text, SdfIconType icon, Direction takeDirection, Direction iconDirection)
        {
            if (Event.current.type == EventType.Layout)
                return false;

            var btnStyle = GUI.skin.button;
            var textStyle = SirenixGUIStyles.WhiteLabel;
            var content = Sirenix.Utilities.Editor.GUIHelper.TempContent(text);
            var iconWidth = rect.height;
            var iconPadding = 0;
            var textWidth = textStyle.CalcSize(content).x;
            var btnWidth = textWidth + iconPadding + 5 + 10 * 2 + iconWidth;
            var r = rect.TakeFromDir(btnWidth, takeDirection);
            var btnRect = r;

            int id = GUIUtility.GetControlID(21345155, FocusType.Passive, rect);

            Rect iconRect;
            r.TakeFromDir(5, iconDirection);
            iconRect = r.TakeFromDir(iconWidth, iconDirection).AlignCenterY(r.height * 0.4f);
            r.TakeFromDir(iconPadding, iconDirection);
            var textRect = r.TakeFromDir(textWidth, iconDirection);

            Event current = Event.current;
            var hover = btnRect.Contains(Event.current.mousePosition);
            switch (current.type)
            {
                case EventType.Repaint:
                btnStyle.Draw(btnRect, GUIContent.none, id, GUIUtility.hotControl == id, hover);
                SdfIcons.DrawIcon(iconRect, icon, textStyle.normal.textColor);
                GUI.Label(textRect, content, textStyle);
                break;
                case EventType.MouseDown:
                if (hover)
                {
                    GUIUtility.hotControl = id;
                    current.Use();
                }

                break;
                case EventType.KeyDown:
                {
                    bool flag = current.alt || current.shift || current.command || current.control;
                    if ((current.keyCode == KeyCode.Space || current.keyCode == KeyCode.Return || current.keyCode == KeyCode.KeypadEnter) && !flag && GUIUtility.keyboardControl == id)
                    {
                        current.Use();
                        GUI.changed = true;
                        return true;
                    }

                    break;
                }
                case EventType.MouseUp:
                if (GUIUtility.hotControl == id)
                {
                    GUIHelper.RemoveFocusControl();
                    current.Use();
                    if (hover)
                    {
                        GUI.changed = true;
                        return true;
                    }
                }

                break;
                case EventType.MouseDrag:
                if (GUIUtility.hotControl == id)
                {
                    current.Use();
                }

                break;
            }

            return false;
        }

        private void DrawEULA()
        {
            #if !ODIN_TRIAL && !ODIN_EDUCATIONAL && !ODIN_ENTERPRISE
            var rect = GUILayoutUtility.GetRect(0f, 0f, GUILayoutOptions.ExpandWidth().ExpandHeight());

            if (!AcceptEULAWindow.HasAcceptedEULA)
            {
                EditorGUI.DrawRect(rect, new Color(0f, 0f, 0f, 0.85f));
                AcceptEULAWindow.Draw(rect.AlignCenter(600f, 480f), !firstOpen, this);

                // Make UI elements in the background unusable while the EULA window is up.
                if (Event.current.type == EventType.MouseDown)
                {
                    Event.current.Use();
                }
            }
            #endif
        }
    }

    public static class GettingStartedWindowData
    {
        private static Color textColor => EditorGUIUtility.isProSkin ? new Color(0.769f, 0.769f, 0.769f, 1f) : new Color(0.035f, 0.035f, 0.035f, 1f);

        public static GettingStartedProduct[] Products;

        public static TutorialPage OdinInspectorPage;
        public static WizardPage OdinValidatorWizardPage;
        public static TutorialPage OdinValidatorGettingStartedPage;
        public static TutorialPage OdinSerializerPage;

        static GettingStartedWindowData()
        {
            var validatorPage = Sirenix.Serialization.TwoWaySerializationBinder.Default.BindToType("Sirenix.OdinValidator.Editor.SetupWizard");
            if (validatorPage != null)
            {
                OdinValidatorWizardPage = Activator.CreateInstance(validatorPage) as WizardPage;

                OdinValidatorGettingStartedPage = new TutorialPage()
                {
                    Title = "Get started with Odin Validator",
                    TitleIcon = SdfIconType.BookHalf,
                    Tutorials = new List<Tutorial>()
                    {
                        new Tutorial()
                        {
                            Title       = "Quick Start",
                            Description = "Get started quickly with an overview of the basics of Odin Validator.",
                            OnClick     = () => Application.OpenURL("https://odininspector.com/tutorials/odin-validator/getting-started-with-odin-validator"),
                            Icon        = SdfIconType.Link,
                            Difficulty  = Difficulty.Beginner,
                        },
                        new Tutorial()
                        {
                            Title       = "Validate Using Attributes",
                            Description = "Learn the basic usage of Odin Validator; how to customize and specify validation logic using simple attributes.",
                            OnClick     = () => Application.OpenURL("https://odininspector.com/tutorials/odin-validator/validate-using-attributes"),
                            Icon        = SdfIconType.Link,
                            Difficulty  = Difficulty.Beginner,
                        },
                        new Tutorial()
                        {
                            Title       = "Validator Types Overview",
                            Description = "Become familiar with how to customize Odin Validator beyond the basic attributes, by learning about the basic validator types and interfaces.",
                            OnClick     = () => Application.OpenURL("https://odininspector.com/tutorials/odin-validator/validator-types-overview"),
                            Icon        = SdfIconType.Link,
                            Difficulty  = Difficulty.Intermediate,
                        },
                        new Tutorial()
                        {
                            Title       = "Validators vs Validation Rules",
                            Description = "Learn about how to create validation rules, and the distinction between a normal validator and a validation rule.",
                            OnClick     = () => Application.OpenURL("https://odininspector.com/tutorials/odin-validator/validators-vs-validation-rules"),
                            Icon        = SdfIconType.Link,
                            Difficulty  = Difficulty.Intermediate,
                        },
                        new Tutorial()
                        {
                            Title       = "Creating Custom Fixes",
                            Description = "Learn all about how to add custom fixes to validation issues.",
                            OnClick     = () => Application.OpenURL("https://odininspector.com/tutorials/odin-validator/creating-custom-fixes"),
                            Icon        = SdfIconType.Link,
                            Difficulty  = Difficulty.Advanced,
                        },
                        new Tutorial()
                        {
                            Title       = "Custom Pipeline",
                            Description = "Learn about how to integrate the Odin Validator into your custom pipeline.",
                            OnClick     = () => Application.OpenURL("https://odininspector.com/tutorials/odin-validator/using-the-validator-in-your-custom-pipeline"),
                            Icon        = SdfIconType.Link,
                            Difficulty  = Difficulty.Intermediate,
                        },
                        new Tutorial()
                        {
                            Title       = "Migrating 2.1 Validators to 3.0",
                            Description = "Validators written prior to Odin 3.0 may be obsolete and relying on old APIs and functionality that will eventually be removed. " +
                                          "This tutorial covers which things changed, and how to upgrade your old validators to the new standards.",
                            OnClick     = () => Application.OpenURL("https://odininspector.com/tutorials/odin-validator/migrating-2.1-and-3.0-validators-to-3.1"),
                            Icon        = SdfIconType.Link,
                            Difficulty  = Difficulty.Intermediate,
                        },
                    }
                };
            }

            OdinInspectorPage = new TutorialPage()
            {
                Title = "Get started with Odin Inspector",
                TitleIcon = SdfIconType.Book,
                Tutorials = new List<Tutorial>()
                {
                    new Tutorial()
                    {
                        Title       = "Odin Attributes Overview",
                        Description = "The best way to get started using Odin is to open the Attributes Overview window found at Tools > Odin > Inspector > Attribute Overview.",
                        Icon        = SdfIconType.Window,
                        Difficulty  = Difficulty.Beginner,
                        OnClick     = () => AttributesExampleWindow.OpenWindow()
                    },
                    new Tutorial()
                    {
                        Title = "The Static Inspector",
                        Description = "If you're a programmer, then you're likely going find the static inspector helpful during debugging and testing. " +
                                      "Just open up the window, and start using it! You can find the utility under 'Tools > Odin > Inspector > Static Inspector'.",
                        Difficulty = Difficulty.Beginner,
                        Icon       = SdfIconType.Window,
                        OnClick    = () => StaticInspectorWindow.InspectType(typeof(UnityEngine.Time), StaticInspectorWindow.AccessModifierFlags.All, StaticInspectorWindow.MemberTypeFlags.AllButObsolete),
                    },
                    new Tutorial()
                    {
                        Title = "Odin Editor Windows",
                        Description = "Learn how you can use Odin to rapidly create custom Editor Windows to help organize your project data. " +
                                      "This is where Odin can really help boost your workflow.",
                        Difficulty = Difficulty.Beginner,
                        Icon       = SdfIconType.FolderFill,
                        ChildPage = new TutorialPage()
                        {
                            Title     = "Editor Windows",
                            TitleIcon = SdfIconType.Window,
                            Tutorials = new List<Tutorial>()
                            {
                                new Tutorial()
                                {
                                    Title      = EditorTutorialIsNotImported() ? "Import package" : "Package imported",
                                    Difficulty = Difficulty.None,
                                    Icon       = SdfIconType.FolderFill,
                                    Enabled    = EditorTutorialIsNotImported,
                                    OnClick = () =>
                                    {
                                        AssetDatabase.ImportPackage(SirenixAssetPaths.SirenixPluginPath + "Demos/Editor Windows.unitypackage", true);
                                    }
                                },
                                new Tutorial()
                                {
                                    Title = "Basic Odin Editor Window",
                                    Description = "Inherit from OdinEditorWindow instead of EditorWindow. This will enable you to render fields, properties and methods " +
                                                  "and make editor windows using attributes, without writing any custom editor code.",
                                    Difficulty = Difficulty.Beginner,
                                    Icon       = SdfIconType.FileEarmarkCodeFill,
                                    Enabled    = EditorTutorialIsImported,
                                    ActionButtons = new List<(string btnName, Action action)>()
                                    {
                                        ("Open window", () =>
                                        {
                                            AssemblyUtilities.GetTypeByCachedFullName("Sirenix.OdinInspector.Demos.BasicOdinEditorExampleWindow")
                                                .GetMethod("OpenWindow", Flags.AllMembers)
                                                .Invoke(null, null);
                                        }),
                                        ("Open script", () => { AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(SirenixAssetPaths.SirenixPluginPath + "Demos/Editor Windows/Scripts/Editor/BasicOdinEditorExampleWindow.cs")); }),
                                    },
                                },
                                new Tutorial()
                                {
                                    Title = "Odin Menu Editor Windows",
                                    Description = "Derive from OdinMenuEditorWindow to create windows that inspect a custom tree of target objects. " +
                                                  "These are great for organizing your project, and managing Scriptable Objects etc." +
                                                  " Odin itself uses this to draw its preferences window.",
                                    Difficulty = Difficulty.Beginner,
                                    Icon       = SdfIconType.FileEarmarkCodeFill,
                                    Enabled    = EditorTutorialIsImported,
                                    ActionButtons = new List<(string btnName, Action action)>()
                                    {
                                        ("Open window", () => { AssemblyUtilities.GetTypeByCachedFullName("Sirenix.OdinInspector.Demos.OdinMenuEditorWindowExample").GetMethod("OpenWindow", Flags.AllMembers).Invoke(null, null); }),
                                        ("Open script", () => { AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(SirenixAssetPaths.SirenixPluginPath + "Demos/Editor Windows/Scripts/Editor/OdinMenuEditorWindowExample.cs")); }),
                                    },
                                },
                                new Tutorial()
                                {
                                    Title = "Override GetTargets()",
                                    Description = "Odin Editor Windows are not limited to drawing themselves; you can override GetTarget() or GetTargets() to make them display " +
                                                  "scriptable objects, components or any arbitrary types (except value types like structs).",
                                    Difficulty = Difficulty.Advanced,
                                    Icon       = SdfIconType.FileEarmarkCodeFill,
                                    Enabled    = EditorTutorialIsImported,
                                    ActionButtons = new List<(string btnName, Action action)>()
                                    {
                                        ("Open window", () => { AssemblyUtilities.GetTypeByCachedFullName("Sirenix.OdinInspector.Demos.OverrideGetTargetsExampleWindow").GetMethod("OpenWindow", Flags.AllMembers).Invoke(null, null); }),
                                        ("Open script", () => { AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(SirenixAssetPaths.SirenixPluginPath + "Demos/Editor Windows/Scripts/Editor/OverrideGetTargetsExampleWindow.cs")); }),
                                    },
                                },
                                new Tutorial()
                                {
                                    Title       = "Quickly inspect objects",
                                    Description = "Call OdinEditorWindow.InspectObject(myObj) to quickly pop up an editor window for any given object. This is a great way to quickly debug objects or make custom editor windows on the spot!",
                                    Difficulty  = Difficulty.Intermediate,
                                    Icon        = SdfIconType.FileEarmarkCodeFill,
                                    Enabled     = EditorTutorialIsImported,
                                    ActionButtons = new List<(string btnName, Action action)>()
                                    {
                                        ("Open window", () => { OdinEditorWindow.InspectObject(Activator.CreateInstance(AssemblyUtilities.GetTypeByCachedFullName("Sirenix.OdinInspector.Demos.QuicklyInspectObjects"))); }),
                                        ("Open script", () => { AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(SirenixAssetPaths.SirenixPluginPath + "Demos/Editor Windows/Scripts/Editor/QuicklyInspectObjects.cs")); }),
                                    },
                                },
                            }
                        }
                    },
                    new Tutorial()
                    {
                        Title = "Custom Drawers",
                        Description = "Making custom drawers in Odin is 10x faster and 10x more powerful than in vanilla Unity. " +
                                      "Drawers are strongly typed, with generic resolution.",
                        Icon       = SdfIconType.FolderFill,
                        Difficulty = Difficulty.Intermediate,
                        ChildPage = new TutorialPage()
                        {
                            Title     = "Custom Drawers",
                            TitleIcon = SdfIconType.Code,
                            Tutorials = new List<Tutorial>()
                            {
                                new Tutorial()
                                {
                                    Title      = "Import example scene",
                                    Difficulty = Difficulty.None,
                                    Icon       = SdfIconType.FolderFill,
                                    Enabled    = CustomDrawersIsNotImported,
                                    OnClick = () =>
                                    {
                                        AssetDatabase.ImportPackage(SirenixAssetPaths.SirenixPluginPath + "Demos/Custom Drawers.unitypackage", true);
                                    }
                                },
                                new Tutorial()
                                {
                                    Title      = "Open example scene",
                                    Difficulty = Difficulty.Intermediate,
                                    Icon       = SdfIconType.SendCheckFill,
                                    Enabled    = CustomDrawersIsImported,
                                    OnClick = () =>
                                    {
                                        OpenScene(SirenixAssetPaths.SirenixPluginPath + "Demos/Custom Drawers/Custom Drawers.unity");
                                    }
                                },
                            }
                        }
                    },
                    new Tutorial()
                    {
                        Title = "Attribute Processors",
                        Description = "You can take complete control over how Odin finds its members to display and which attributes to put on those members. " +
                                      "This can be extremely useful for automation and providing support and editor customizations for third-party libraries " +
                                      "you don't own the code for.",
                        Icon       = SdfIconType.FolderFill,
                        Difficulty = Difficulty.Advanced,
                        ChildPage = new TutorialPage()
                        {
                            Title     = "Attribute Processors",
                            TitleIcon = SdfIconType.Code,
                            Tutorials = new List<Tutorial>()
                            {
                                new Tutorial()
                                {
                                    Title = "Attribute Processors",
                                    Description =
                                        "Need to add attributes to 3rd party code? Can't access source code for an asset and still want to modify the inspector? " +
                                        "With custom attribute processors and Odin Inspector, you can do that!",
                                    Difficulty = Difficulty.Advanced,
                                    Icon       = SdfIconType.Youtube,
                                    OnClick    = () => Application.OpenURL("https://odininspector.com/tutorials/using-property-resolvers-and-attribute-processors/custom-attribute-processors#odin-inspector"),
                                },
                                new Tutorial()
                                {
                                    Title = "Property Processors",
                                    Description =
                                        "Odin property processors are the code that tells Odin what data to display and what attributes are associated with that data. This means that creating a custom property processor can allow you to alter the properties that are shown in the inspector. " +
                                        "This is done without altering the original code in any way and works with 3rd party code where you may not have access to the source code. " +
                                        "Property processors let you define your own rules for how properties are made, both globally or just for select types.",
                                    Difficulty = Difficulty.Advanced,
                                    Icon       = SdfIconType.Youtube,
                                    OnClick    = () => Application.OpenURL("https://odininspector.com/tutorials/using-property-resolvers-and-attribute-processors/custom-property-processors#odin-inspector"),
                                },
                                new Tutorial()
                                {
                                    Title      = "Import example scene",
                                    Difficulty = Difficulty.None,
                                    Icon       = SdfIconType.FolderFill,
                                    Enabled    = AttributeProcessorsIsNotImported,
                                    OnClick = () =>
                                    {
                                        AssetDatabase.ImportPackage(SirenixAssetPaths.SirenixPluginPath + "Demos/Custom Attribute Processors.unitypackage", true);
                                    }
                                },
                                new Tutorial()
                                {
                                    Title      = "Open example scene",
                                    Difficulty = Difficulty.Advanced,
                                    Icon       = SdfIconType.SendCheckFill,
                                    Enabled    = AttributeProcessorsIsImported,
                                    OnClick    = () =>
                                    {
                                        OpenScene(SirenixAssetPaths.SirenixPluginPath + "Demos/Custom Attribute Processors/Custom Attribute Processors.unity");
                                    }
                                },
                            }
                        }
                    },
                    new Tutorial()
                    {
                        Title = "RPG Editor (sample project)",
                        Description = "This project showcases Odin Editor Windows, Odin Selectors, various attribute combinations, and custom " +
                                      "drawers to build a feature-rich editor window for managing scriptable objects.",
                        Icon       = SdfIconType.FolderFill,
                        Difficulty = Difficulty.Advanced,
                        ChildPage = new TutorialPage()
                        {
                            Title     = "RPG Editor (sample project)",
                            TitleIcon = SdfIconType.Code,
                            Tutorials = new List<Tutorial>()
                            {
                                new Tutorial()
                                {
                                    Title      = AttributeProcessorsIsNotImported() ? "Import package" : "Package imported",
                                    Difficulty = Difficulty.None,
                                    Icon       = SdfIconType.FolderFill,
                                    Enabled    = RPGSampleProjectIsNotImported,
                                    OnClick = () =>
                                    {

                                        AssetDatabase.ImportPackage(SirenixAssetPaths.SirenixPluginPath + "Demos/Sample - RPG Editor.unitypackage", true);
                                    }
                                },
                                new Tutorial()
                                {
                                    Title       = "Open RPG editor window",
                                    Description = "You can also find the window under Tools > Odin > Demos",
                                    Difficulty  = Difficulty.Advanced,
                                    Icon        = SdfIconType.Window,
                                    Enabled     = RPGSampleProjectIsImported,
                                    OnClick     = () => AssemblyUtilities.GetTypeByCachedFullName("Sirenix.OdinInspector.Demos.RPGEditor.RPGEditorWindow").GetMethod("Open", Flags.AllMembers).Invoke(null, null)
                                },
                                new Tutorial()
                                {
                                    Title       = "Open script",
                                    Description = "Go and explore how things are implemented, the script for the window itself is a great place to start",
                                    Difficulty  = Difficulty.Advanced,
                                    Icon        = SdfIconType.FileEarmarkCodeFill,
                                    Enabled     = RPGSampleProjectIsImported,
                                    OnClick     = () => AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(SirenixAssetPaths.SirenixPluginPath + "Demos/Sample - RPG Editor/Scripts/Editor/RPGEditorWindow.cs"))
                                },
                            }
                        }
                    },
                }
            };

            OdinSerializerPage = new TutorialPage()
            {
                Title = "Get started with Odin Serializer",
                TitleIcon = SdfIconType.Book,
                Tutorials = new List<Tutorial>()
                {
                    new Tutorial()
                    {
                        Title       = "Quick start",
                        Description = "Getting started with Odin Serializer is very easy; read this to get a quick start and hit the ground running.", // TODO: Tor. Text.,
                        OnClick     = () => Application.OpenURL("https://odininspector.com/tutorials/serialize-anything/odin-serializer-quick-start#odin-serializer"),
                        Icon        = SdfIconType.Link,
                        Difficulty  = Difficulty.Beginner,
                    },
                    new Tutorial()
                    {
                        Title = "The Serialization Debugger",
                        Description = "If you are utilizing Odin's serialization, the Serialization Debugger can show you which members of any given " +
                                      "type are being serialized, and whether they are serialized by Unity, Odin or both. " +
                                      "You can find the utility under 'Tools > Odin > Serializer > Serialization Debugger' or from the context menu in the inspector.",
                        OnClick    = SerializationDebuggerWindow.ShowWindow,
                        Icon       = SdfIconType.Window,
                        Difficulty = Difficulty.Beginner,
                    },
                    new Tutorial()
                    {
                        Title = "AOT Serialization",
                        Description = "On AOT (ahead-of-time) platforms like IL2CPP, special care is needed to ensure all necessary code for serialization is present at runtime. " +
                                      "Learn about Odin Serializer's solutions for easily and automatically generating AOT support.",
                        OnClick    = () => Application.OpenURL("https://odininspector.com/tutorials/serialize-anything/aot-serialization#odin-serializer"),
                        Icon       = SdfIconType.Link,
                        Difficulty = Difficulty.Beginner,
                    },
                    new Tutorial()
                    {
                        Title       = "Implementing Odin serialization without inheritance",
                        Description = "You don't have to inherit from one of our base classes to get Odin serialization, you can also make your own by implementing Odin's serialization manually. ",
                        OnClick     = () => Application.OpenURL("https://odininspector.com/tutorials/serialize-anything/implementing-the-odin-serializer#odin-serializer"),
                        Icon        = SdfIconType.Link,
                        Difficulty  = Difficulty.Intermediate,
                    },
                    new Tutorial()
                    {
                        Title       = "Serializing without Unity objects",
                        Description = " In this tutorial you'll learn to serialize non-Unity-object types to bytes using the SerializationUtility API.",
                        OnClick     = () => Application.OpenURL("https://odininspector.com/tutorials/serialize-anything/serializing-without-serialized-base-classes"),
                        Icon        = SdfIconType.Link,
                        Difficulty  = Difficulty.Intermediate,
                    }
                }
            };

            CreateProducts();
        }

        private static void CreateProducts()
        {
            Products = new GettingStartedProduct[3];

            ref var ins = ref Products[0];
            ref var val = ref Products[1];
            ref var ser = ref Products[2];

            ins = new GettingStartedProduct()
            {
                Name = "Odin Inspector",
                ReviewUrl = "https://assetstore.unity.com/packages/tools/utilities/odin-inspector-and-serializer-89041#reviews",
                Enabled = true,
                Logo = OdinEditorResources.OdinInspectorLogo,
                HueColor = SirenixGUIStyles.InspectorOrange,
                Status = "Installed",
                StatusIcon = SdfIconType.CheckCircleFill,
                StatusIconColor = SirenixGUIStyles.ValidatorGreen,
                Pages = new (GettingStartedPage page, string btnName)[]
                {
                    (OdinInspectorPage, "Get started"),
                }
            };

            // TODO: If downloaded through Unity, then 
            //validatorWizardPage = null;
            if (OdinValidatorWizardPage == null)
            {
                val = new GettingStartedProduct()
                {
                    Name = "Odin Validator",
                    ReviewUrl = "https://assetstore.unity.com/packages/tools/utilities/odin-validator-227861#reviews",
                    Enabled = false,
                    Logo = OdinEditorResources.OdinValidatorLogo,
                    HueColor = SirenixGUIStyles.ValidatorGreen,
                    Status = "Not installed",
                    StatusIcon = SdfIconType.XCircleFill,
                    StatusIconColor = textColor,
                    Pages = new (GettingStartedPage page, string btnName)[]
                    {
#if ODIN_ASSET_STORE
                        (new ButtonPage(OpenProjectValidatorURLAssetStore), "Learn more"),
#else
                        (new ButtonPage(OpenProjectValidatorURL), "Learn more"),
#endif
                        (new ButtonPage(OpenProjectValidatorDownloadURL), "Download"),
                    }
                };
            }
            else
            {
                val = new GettingStartedProduct()
                {
                    Name = "Odin Validator",
                    ReviewUrl = "https://assetstore.unity.com/packages/tools/utilities/odin-validator-227861#reviews",
                    Enabled = true,
                    Logo = OdinEditorResources.OdinValidatorLogo,
                    HueColor = SirenixGUIStyles.ValidatorGreen,
                    Status = "Installed",
                    StatusIcon = SdfIconType.CheckCircleFill,
                    StatusIconColor = SirenixGUIStyles.ValidatorGreen,
                    Pages = new (GettingStartedPage page, string btnName)[]
                    {
                        (OdinValidatorWizardPage, "Setup Wizard"),
                        (OdinValidatorGettingStartedPage, "Getting Started"),
                    }
                };
            }

            if (EditorOnlyModeConfig.Instance.IsEditorOnlyModeEnabled())
            {
                ser = new GettingStartedProduct()
                {
                    Name = "Odin Serializer",
                    Enabled = false,
                    Logo = OdinEditorResources.OdinSerializerLogo,
                    HueColor = SirenixGUIStyles.SerializerYellow,
                    Status = "Disabled",
                    StatusIcon = SdfIconType.XCircleFill,
                    StatusIconColor = textColor,
                    Pages = new (GettingStartedPage page, string btnName)[]
                    {
                        (new ButtonPage(OpenEditorOnlyModeToggle), "Configure"),
                        (OdinSerializerPage, "Learn more"),
                    }
                };
            }
            else
            {
                ser = new GettingStartedProduct()
                {
                    Name = "Odin Serializer",
                    Enabled = true,
                    Logo = OdinEditorResources.OdinSerializerLogo,
                    HueColor = SirenixGUIStyles.SerializerYellow,
                    Status = "Enabled",
                    StatusIcon = SdfIconType.CheckCircleFill,
                    StatusIconColor = SirenixGUIStyles.ValidatorGreen,
                    Pages = new (GettingStartedPage page, string btnName)[]
                    {
                        (new ButtonPage(OpenEditorOnlyModeToggle), "Configure"),
                        (OdinSerializerPage, "Getting Started"),
                    }
                };
            }
        }

        private static bool attributeProcessorsTutorialIsImported = Serialization.TwoWaySerializationBinder.Default.BindToType("Sirenix.OdinInspector.Demos.PutAttributesOnAnyType") != null;
        private static bool AttributeProcessorsIsImported() => attributeProcessorsTutorialIsImported;
        private static bool AttributeProcessorsIsNotImported() => !AttributeProcessorsIsImported();

        private static bool rpgSampleProjectIsIsImported = Serialization.TwoWaySerializationBinder.Default.BindToType("Sirenix.OdinInspector.Demos.RPGEditor.RPGEditorWindow") != null;
        private static bool RPGSampleProjectIsImported() => rpgSampleProjectIsIsImported;
        private static bool RPGSampleProjectIsNotImported() => !RPGSampleProjectIsImported();

        private static bool customDrawersTutorialIsImported = Serialization.TwoWaySerializationBinder.Default.BindToType("Sirenix.OdinInspector.Demos.CustomGroupExample") != null;
        private static bool CustomDrawersIsImported() => customDrawersTutorialIsImported;
        private static bool CustomDrawersIsNotImported() => !CustomDrawersIsImported();

        private static bool editorTutorialIsImported = Serialization.TwoWaySerializationBinder.Default.BindToType("Sirenix.OdinInspector.Demos.OdinMenuEditorWindowExample") != null;
        private static bool EditorTutorialIsImported() => editorTutorialIsImported;
        private static bool EditorTutorialIsNotImported() => !EditorTutorialIsImported();

        private static void OpenEditorOnlyModeToggle() => SirenixPreferencesWindow.OpenWindow(EditorOnlyModeConfig.Instance);
        private static void OpenProjectValidatorURL() => Application.OpenURL("https://odininspector.com/odin-project-validator");
        private static void OpenProjectValidatorURLAssetStore() => Application.OpenURL("https://odininspector.com/odin-validator-asset-store-redirect");
        private static void OpenProjectValidatorDownloadURL() => Application.OpenURL("https://odininspector.com/download");

        private static void OpenScene(string scenePath)
        {
            UnityEditorEventUtility.DelayAction(() =>
            {
                var scene = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);
                AssetDatabase.OpenAsset(scene);
                if (scene as SceneAsset)
                {
                    UnityEditorEventUtility.DelayAction(() =>
                    {
                        GameObject.FindObjectsOfType<Transform>()
                            .Where(x => x.parent == null && x.childCount > 0)
                            .OrderByDescending(x => x.GetSiblingIndex())
                            .Select(x => x.transform.GetChild(0).gameObject)
                            .ForEach(x => EditorGUIUtility.PingObject(x));
                    });
                }
            }, true);
        }

    }

    public struct GettingStartedProduct
    {
        public string Name;
        public string ReviewUrl;
        public bool Enabled;
        public Texture2D Logo;
        public SdfIconType StatusIcon;
        public Color StatusIconColor;
        public string Status;
        public Color HueColor;
        public float NotificationT;
        public float NotificationTargetT;
        public (GettingStartedPage page, string btnName)[] Pages;
    }
}


//var basicOdinEditor_cs = SirenixAssetPaths.SirenixPluginPath + "Demos/" + "Editor Windows" + "/Scripts/Editor/BasicOdinEditorExampleWindow.cs";
//var getTargets_cs = SirenixAssetPaths.SirenixPluginPath + "Demos/" + "Editor Windows" + "/Scripts/Editor/OverrideGetTargetsExampleWindow.cs";
//var inspectObjects_cs = SirenixAssetPaths.SirenixPluginPath + "Demos/" + "Editor Windows" + "/Scripts/Editor/QuicklyInspectObjects.cs";
//var menu_cs = SirenixAssetPaths.SirenixPluginPath + "Demos/" + "Editor Windows" + "/Scripts/Editor/OverrideGetTargetsExampleWindow.cs";


//// Editor Windows
//var ODIN_MENU_EDITOR_WINDOW_EXAMPLE_CS = "/Scripts/Editor/OdinMenuEditorWindowExample.cs";
//var QUICKLY_INSPECT_OBJECTS_TYPE = "Sirenix.OdinInspector.Demos.QuicklyInspectObjects";
//var BASIC_ODIN_EDITOR_EXAMPLE_WINDOWS_TYPE = "Sirenix.OdinInspector.Demos.BasicOdinEditorExampleWindow";
//var ODIN_MENU_EDITOR_WINDOW_EXAMPLE_TYPE = "Sirenix.OdinInspector.Demos.OdinMenuEditorWindowExample";
//var OVERRIDE_GET_TARGETS_EXAMPLE_WINDOW_TYPE = "Sirenix.OdinInspector.Demos.OverrideGetTargetsExampleWindow";

//// Sample Project
//var SAMPLE_RPG_EDITOR_FOLDER = "Sample - RPG Editor";
//var RPG_EDITOR_WINDOW_CS = "/Scripts/Editor/RPGEditorWindow.cs";
//var RPG_EDITOR_WINDOW_TYPE = "Sirenix.OdinInspector.Demos.RPGEditor.RPGEditorWindow";

//// Attribute Processors
//var CUSTOM_ATTRIBUTE_PROCESSORS_FOLDER = "Custom Attribute Processors";
//var CUSTOM_ATTRIBUTE_PROCESSORS_SCENE = "/Custom Attribute Processors.unity";

//// Custom Drawwers
//var CUSTOM_DRAWERS_FOLDER = "Custom Drawers";
//var CUSTOM_DRAWERS_SCENE = "/Custom Drawers.unity";
#endif