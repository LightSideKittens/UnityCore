//-----------------------------------------------------------------------
// <copyright file="OdinEditorWindow.cs" company="Sirenix ApS">
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

    using UnityEditor;
    using UnityEngine;
    using Sirenix.Serialization;
    using Sirenix.Reflection.Editor;
    using Sirenix.Utilities.Editor;
    using System;
    using System.Collections;
    using Sirenix.Utilities;
    using System.Linq;
    using System.Collections.Generic;
    using UnityEngine.UIElements;

    /// <summary>
    /// Base class for creating editor windows using Odin.
    /// </summary>
    /// <example>
    /// <code>
    /// public class SomeWindow : OdinEditorWindow
    /// {
    ///     [MenuItem("My Game/Some Window")]
    ///     private static void OpenWindow()
    ///     {
    ///         GetWindow&lt;SomeWindow&gt;().Show();
    ///     }
    ///
    ///     [Button(ButtonSizes.Large)]
    ///     public void SomeButton() { }
    ///
    ///     [TableList]
    ///     public SomeType[] SomeTableData;
    /// }
    /// </code>
    /// </example>
    /// <example>
    /// <code>
    /// public class DrawSomeSingletonInAnEditorWindow : OdinEditorWindow
    /// {
    ///     [MenuItem("My Game/Some Window")]
    ///     private static void OpenWindow()
    ///     {
    ///         GetWindow&lt;DrawSomeSingletonInAnEditorWindow&gt;().Show();
    ///     }
    ///
    ///     protected override object GetTarget()
    ///     {
    ///         return MySingleton.Instance;
    ///     }
    /// }
    /// </code>
    /// </example>
    /// <example>
    /// <code>
    /// private void InspectObjectInWindow()
    /// {
    ///     OdinEditorWindow.InspectObject(someObject);
    /// }
    ///
    /// private void InspectObjectInDropDownWithAutoHeight()
    /// {
    ///     var btnRect = GUIHelper.GetCurrentLayoutRect();
    ///     OdinEditorWindow.InspectObjectInDropDown(someObject, btnRect, btnRect.width);
    /// }
    ///
    /// private void InspectObjectInDropDown()
    /// {
    ///     var btnRect = GUIHelper.GetCurrentLayoutRect();
    ///     OdinEditorWindow.InspectObjectInDropDown(someObject, btnRect, new Vector2(btnRect.width, 100));
    /// }
    ///
    /// private void InspectObjectInACenteredWindow()
    /// {
    ///     var window = OdinEditorWindow.InspectObject(someObject);
    ///     window.position = GUIHelper.GetEditorWindowRect().AlignCenter(270, 200);
    /// }
    ///
    /// private void OtherStuffYouCanDo()
    /// {
    ///     var window = OdinEditorWindow.InspectObject(this.someObject);
    ///
    ///     window.position = GUIHelper.GetEditorWindowRect().AlignCenter(270, 200);
    ///     window.titleContent = new GUIContent("Custom title", EditorIcons.RulerRect.Active);
    ///     window.OnClose += () => Debug.Log("Window Closed");
    ///     window.OnBeginGUI += () => GUILayout.Label("-----------");
    ///     window.OnEndGUI += () => GUILayout.Label("-----------");
    /// }
    /// </code>
    /// </example>
    /// <seealso cref="OdinMenuEditorWindow"/>
    [ShowOdinSerializedPropertiesInInspector]
    public class OdinEditorWindow : EditorWindow, ISerializationCallbackReceiver
    {
        /// <summary>
        /// Occurs when the window is closed.
        /// </summary>
        public event Action OnClose;

        /// <summary>
        /// Occurs at the beginning the OnGUI method.
        /// </summary>
        public event Action OnBeginGUI;

        /// <summary>
        /// Occurs at the end the OnGUI method.
        /// </summary>
        public event Action OnEndGUI;

        private Action _onBeginGUI;
        private Action _onEndGUI;

        private static System.Reflection.PropertyInfo materialForceVisibleProperty = typeof(MaterialEditor).GetProperty("forceVisible", Flags.AllMembers);

        private static bool hasUpdatedOdinEditors = false;

        private static int inspectObjectWindowCount = 3;

        private static readonly object[] EmptyObjectArray = new object[0];

        [SerializeField, HideInInspector]
        private SerializationData serializationData;

        [SerializeField, HideInInspector]
        private object inspectorTargetSerialized;

        [SerializeField, HideInInspector]
        private float labelWidth = 0.33f;

        [NonSerialized]
        private object inspectTargetObject;

        [SerializeField, HideInInspector]
        private Vector4 windowPadding = new Vector4(4, 4, 4, 4);

        [SerializeField, HideInInspector]
        private bool useScrollView = true;

        [SerializeField, HideInInspector]
        private bool drawUnityEditorPreview;

        [SerializeField, HideInInspector]
        private int wrappedAreaMaxHeight = 1000;

        [NonSerialized]
        private int warmupRepaintCount = 0;

        [NonSerialized]
        private bool isInitialized;
        private GUIStyle marginStyle;
        private object[] currentTargets = new object[0];
        private ImmutableList<object> currentTargetsImm;
        private Editor[] editors = new Editor[0];
        private PropertyTree[] propertyTrees = new PropertyTree[0];
        private Vector2 scrollPos;
        private int mouseDownId;
        private EditorWindow mouseDownWindow;
        private int mouseDownKeyboardControl;
        private Vector2 contenSize;
        private float defaultEditorPreviewHeight = 170;
        private bool preventContentFromExpanding;
        private bool isAutoHeightAdjustmentReady = false;

        private bool isInsideOnGUI;
        private bool isInOurImGUIContainer;

        private List<Toast> toasts = new List<Toast>();

        /// <summary>
        /// Gets the label width to be used. Values between 0 and 1 are treated as percentages, and values above as pixels.
        /// </summary>
        public virtual float DefaultLabelWidth
        {
            get { return this.labelWidth; }
            set { this.labelWidth = value; }
        }

        /// <summary>
        /// Gets or sets the window padding. x = left, y = right, z = top, w = bottom.
        /// </summary>
        public virtual Vector4 WindowPadding
        {
            get { return this.windowPadding; }
            set { this.windowPadding = value; }
        }

        /// <summary>
        /// Gets a value indicating whether the window should draw a scroll view.
        /// </summary>
        public virtual bool UseScrollView
        {
            get { return this.useScrollView; }
            set { this.useScrollView = value; }
        }

        /// <summary>
        /// Gets a value indicating whether the window should draw a Unity editor preview, if possible.
        /// </summary>
        public virtual bool DrawUnityEditorPreview
        {
            get { return this.drawUnityEditorPreview; }
            set { this.drawUnityEditorPreview = value; }
        }

        /// <summary>
        /// Gets the default preview height for Unity editors.
        /// </summary>
        public virtual float DefaultEditorPreviewHeight
        {
            get { return this.defaultEditorPreviewHeight; }
            set { this.defaultEditorPreviewHeight = value; }
        }

        /// <summary>
        /// Gets the target which which the window is supposed to draw. By default it simply returns the editor window instance itself. By default, this method is called by <see cref="GetTargets"/>().
        /// </summary>
        protected virtual object GetTarget()
        {
            if (this.inspectTargetObject != null)
            {
                return this.inspectTargetObject;
            }

            if (this.inspectorTargetSerialized != null)
            {
                if (this.inspectorTargetSerialized is UnityEngine.Object uObj && !uObj)
                {
                    return this;
                }

                return this.inspectorTargetSerialized;
            }

            return this;
        }

        /// <summary>
        /// Gets the targets to be drawn by the editor window. By default this simply yield returns the <see cref="GetTarget"/> method.
        /// </summary>
        protected virtual IEnumerable<object> GetTargets()
        {
            yield return this.GetTarget();
        }

        /// <summary>
        /// At the start of each OnGUI event when in the Layout event, the GetTargets() method is called and cached into a list which you can access from here.
        /// </summary>
        protected ImmutableList<object> CurrentDrawingTargets
        {
            get { return this.currentTargetsImm; }
        }

        /// <summary>
        /// <para>
        /// Pops up an editor window for the given object in a drop-down window which closes when it loses its focus.
        /// This particular overload uses a few frames to calculate the height of the content before showing the window with a height that matches its content.
        /// </para>
        /// <para>Protip: You can subscribe to OnClose if you want to know when that occurs.</para>
        /// </summary>
        public static OdinEditorWindow InspectObjectInDropDown(object obj, Rect btnRect, float windowWidth)
        {
            return InspectObjectInDropDown(obj, btnRect, new Vector2(windowWidth, 0));
        }

        /// <summary>
        /// Measures the GUILayout content height and adjusts the window height accordingly.
        /// Note that this feature becomes pointless if any layout group expands vertically.
        /// </summary>
        /// <param name="maxHeight">The max height of the window.</param>
        /// <param name="retainInitialWindowPosition">When the window height expands below the screen bounds, it will move the window
        /// upwards when needed, enabling this will move it back down when the window height is decreased. </param>
        protected void EnableAutomaticHeightAdjustment(int maxHeight, bool retainInitialWindowPosition)
        {
            this.preventContentFromExpanding = true;
            this.wrappedAreaMaxHeight = maxHeight;

            OdinEditorWindow wnd = this;
            var initialPos = new Vector2(float.NaN, float.NaN);
            float adjustedHeight = float.NaN;
            
            EditorApplication.CallbackFunction callback = null;
            
            callback = () =>
            {
                EditorApplication.update -= callback;
                EditorApplication.update -= callback;

                if (wnd == null)
                {
                    return;
                }

                if (wnd.isAutoHeightAdjustmentReady)
                {
                    if (wnd.contenSize != Vector2.zero && !Mathf.Approximately(adjustedHeight, wnd.contenSize.y))
                    {
                        bool isInitialPosValid = !float.IsNaN(initialPos.x) && !float.IsNaN(initialPos.y);

                        Rect pos = wnd.position;

                        if (retainInitialWindowPosition && isInitialPosValid)
                        {
                            pos.position = initialPos;
                        }

                        float height = Math.Min(wnd.contenSize.y, maxHeight);

                        wnd.minSize = new Vector2(wnd.minSize.x, height);
                        wnd.maxSize = new Vector2(wnd.maxSize.x, height);
                        pos.height = height;

                        //pos = EditorWindow_Internal.FitPositionInWorkingArea(wnd, pos, false);

                        if (!isInitialPosValid)
                        {
                            initialPos = pos.position;
                        }

                        wnd.position = pos;

                        adjustedHeight = wnd.contenSize.y;
                    }
                }

                EditorApplication.update += callback;
            };
            
            EditorApplication.update += callback;
        }

        public const int MIN_DROPDOWN_HEIGHT = 200;
        public const int MAX_DROPDOWN_HEIGHT = 600;

        /// <summary>
        /// <para>
        /// Pops up an editor window for the given object in a drop-down window which closes when it loses its focus.
        /// </para>
        /// <para>Protip: You can subscribe to OnClose if you want to know when that occurs.</para>
        /// </summary>
        public static OdinEditorWindow InspectObjectInDropDown(object obj, Rect btnRect, Vector2 windowSize)
        {
            var window = CreateOdinEditorWindowInstanceForObject(obj);

            if (windowSize.x <= 1) windowSize.x = btnRect.width;
            if (windowSize.x <= 1) windowSize.x = 400;

            // Having floating point values, can cause Unity's editor window to be transparent.
            btnRect.x = (int) btnRect.x;
            btnRect.width = (int) btnRect.width;
            btnRect.height = (int) btnRect.height;
            btnRect.y = (int) btnRect.y;
            windowSize.x = (int) windowSize.x;
            windowSize.y = (int) windowSize.y;

            try
            {
                // Also repaint parent window, when the drop down repaints.
                var curr = GUIHelper.CurrentWindow;
                if (curr != null)
                {
                    window.OnBeginGUI += () => curr.Repaint();
                }
            }
            catch { }

            // Draw lighter bg.
            if (!EditorGUIUtility.isProSkin)
            {
                window.OnBeginGUI += () =>
                    SirenixEditorGUI.DrawSolidRect(new Rect(0, 0, window.position.width, window.position.height), SirenixGUIStyles.MenuBackgroundColor);
            }

            // Draw borders.
            window.OnEndGUI += () => SirenixEditorGUI.DrawBorders(new Rect(0, 0, window.position.width, window.position.height), 1);
            window.labelWidth = 0.33f;
            window.DrawUnityEditorPreview = true;
            btnRect.position = GUIUtility.GUIToScreenPoint(btnRect.position);

            bool isWindowSizeYSet = (int) windowSize.y != 0;

            if (!isWindowSizeYSet)
            {
                // NOTE: We set it to 1 because EnableAutomaticHeightAdjustment will be set.
                // If this value were larger than the automatically adjusted value,
                // there would be 1 frame where the Selector has this large form, which looks weird.
                windowSize.y = 1;
            }

            Rect windowPosition = btnRect;
            windowPosition.y += btnRect.height;
            windowPosition.width = windowSize.x;
            windowPosition.height = windowSize.y;
            
            window.position = windowPosition;
            
            if (!isWindowSizeYSet)
            {
                window.EnableAutomaticHeightAdjustment(MAX_DROPDOWN_HEIGHT, true);
            }

            EditorWindow_Internal.ShowPopupAux(window);

            return window;
        }

        /// <summary>
        /// <para>
        /// Pops up an editor window for the given object in a drop-down window which closes when it loses its focus.
        /// </para>
        /// <para>Protip: You can subscribe to OnClose if you want to know when that occurs.</para>
        /// </summary>
        public static OdinEditorWindow InspectObjectInDropDown(object obj, Vector2 position)
        {
            var btnRect = new Rect(position.x, position.y, 1, 1);
            return InspectObjectInDropDown(obj, btnRect, 350);
        }

        /// <summary>
        /// <para>
        /// Pops up an editor window for the given object in a drop-down window which closes when it loses its focus.
        /// </para>
        /// <para>Protip: You can subscribe to OnClose if you want to know when that occurs.</para>
        /// </summary>
        public static OdinEditorWindow InspectObjectInDropDown(object obj, float windowWidth)
        {
            var position = Event.current.mousePosition;
            var btnRect = new Rect(position.x, position.y, 1, 1);
            return InspectObjectInDropDown(obj, btnRect, windowWidth);
        }

        /// <summary>
        /// <para>
        /// Pops up an editor window for the given object in a drop-down window which closes when it loses its focus.
        /// </para>
        /// <para>Protip: You can subscribe to OnClose if you want to know when that occurs.</para>
        /// </summary>
        public static OdinEditorWindow InspectObjectInDropDown(object obj, Vector2 position, float windowWidth)
        {
            var btnRect = new Rect(position.x, position.y, 1, 1);
            return InspectObjectInDropDown(obj, btnRect, windowWidth);
        }

        /// <summary>
        /// <para>
        /// Pops up an editor window for the given object in a drop-down window which closes when it loses its focus.
        /// </para>
        /// <para>Protip: You can subscribe to OnClose if you want to know when that occurs.</para>
        /// </summary>
        public static OdinEditorWindow InspectObjectInDropDown(object obj, float width, float height)
        {
            var r = new Rect(Event.current.mousePosition, Vector2.one);
            return InspectObjectInDropDown(obj, r, new Vector2(width, height));
        }

        /// <summary>
        /// <para>
        /// Pops up an editor window for the given object in a drop-down window which closes when it loses its focus.
        /// </para>
        /// <para>Protip: You can subscribe to OnClose if you want to know when that occurs.</para>
        /// </summary>
        public static OdinEditorWindow InspectObjectInDropDown(object obj)
        {
            return InspectObjectInDropDown(obj, Event.current.mousePosition);
        }

        /// <summary>
        /// Pops up an editor window for the given object.
        /// </summary>
        public static OdinEditorWindow InspectObject(object obj)
        {
            return InspectObject(obj, false);
        }

        internal static OdinEditorWindow InspectObject(object obj, bool forceSerializeInspectedObject)
        {
            var window = CreateOdinEditorWindowInstanceForObject(obj, forceSerializeInspectedObject);
            window.Show();

            var offset = new Vector2(30, 30) * ((inspectObjectWindowCount++ % 6) - 3);
            window.position = GUIHelper.GetEditorWindowRect()
                .AlignCenter(400, 300)
                .AddPosition(offset);

            return window;
        }

        /// <summary>
        /// Inspects the object using an existing OdinEditorWindow.
        /// </summary>
        public static OdinEditorWindow InspectObject(OdinEditorWindow window, object obj)
        {
            var uObj = obj as UnityEngine.Object;
            if (uObj)
            {
                // If it's a Unity object, then it's likely the reference can survive a recompile.
                window.inspectTargetObject = null;
                window.inspectorTargetSerialized = uObj;
            }
            else
            {
                // Otherwise, it can't. In which case we don't want want to serialize it - hence inspectorTargetObject and not inspectorTargetSerialized.
                // If we did the user would be inspecting a different reference than provided.
                window.inspectorTargetSerialized = null;
                window.inspectTargetObject = obj;
            }

            if (uObj as Component)
            {
                window.titleContent = new GUIContent((uObj as Component).gameObject.name);
            }
            else if (uObj)
            {
                window.titleContent = new GUIContent(uObj.name);
            }
            else
            {
                window.titleContent = new GUIContent(obj.ToString());
            }

            EditorUtility.SetDirty(window);
            return window;
        }

        /// <summary>
        /// Creates an editor window instance for the specified object, without opening the window.
        /// </summary>
        public static OdinEditorWindow CreateOdinEditorWindowInstanceForObject(object obj)
        {
            return CreateOdinEditorWindowInstanceForObject(obj, false);
        }

        /// <summary>
        /// Creates an editor window instance for the specified object, without opening the window.
        /// </summary>
        internal static OdinEditorWindow CreateOdinEditorWindowInstanceForObject(object obj, bool forceSerializeInspectedObject)
        {
            var window = CreateInstance<OdinEditorWindow>();

            // In Unity version 2017.3+ the new window doesn't recive focus on the first click if something from another window has focus.
            GUIUtility.hotControl = 0;
            GUIUtility.keyboardControl = 0;

            if (obj as UnityEngine.Object || forceSerializeInspectedObject)
            {
                // If it's a Unity object, then it's likely the reference can survive a recompile.
                window.inspectorTargetSerialized = obj;
            }
            else
            {
                // Otherwise, it can't. In which case we don't want want to serialize it - inspectorTargetObject and not inspectorTargetSerialized.
                // If we did the user would be inspecting a different reference than provided.
                window.inspectTargetObject = obj;
            }

            if (obj is Component com && com)
            {
                window.titleContent = new GUIContent(com.gameObject.name);
            }
            else if (obj is UnityEngine.Object uObj && uObj)
            {
                window.titleContent = new GUIContent(uObj.name);
            }
            else
            {
                window.titleContent = new GUIContent(obj.ToString());
            }

            window.position = GUIHelper.GetEditorWindowRect().AlignCenter(600, 600);

            EditorUtility.SetDirty(window);
            return window;
        }

        /// <summary>
        /// The Odin property tree drawn.
        /// </summary>
        [Obsolete("Support for non Odin drawn editors and drawing of multiple editors has been added, so there is no longer any guarantee that there will be a PropertyTree.")]
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public PropertyTree PropertyTree
        {
            get { return this.propertyTrees == null ? null : this.propertyTrees.FirstOrDefault(); }
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            UnitySerializationUtility.DeserializeUnityObject(this, ref this.serializationData);
            this.OnAfterDeserialize();
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            UnitySerializationUtility.SerializeUnityObject(this, ref this.serializationData);
            this.OnBeforeSerialize();
        }

        private void CreateGUI()
        {
            var container = new IMGUIContainer(this.OnImGUI);
            container.name = "Odin ImGUIContainer";
            container.style.display = DisplayStyle.Flex;
            container.style.height = Length.Percent(100);
            container.style.width = Length.Percent(100);
            this.rootVisualElement.Add(container);
        }

        protected virtual void OnImGUI()
        {
            this.isInOurImGUIContainer = true;

            if (this.warmupRepaintCount <= 10 && Event.current.type == EventType.Layout)
            {
                this.warmupRepaintCount++;
                this.Repaint();
            }

            this.InitializeIfNeeded();

            if (Event.current.type == EventType.Layout)
            {
                _onBeginGUI = this.OnBeginGUI;
                _onEndGUI = this.OnEndGUI;
            }

            try
            {
                this.isInsideOnGUI = true;

                bool measureArea = this.preventContentFromExpanding;
                if (measureArea)
                {
                    GUILayout.BeginArea(new Rect(0, 0, this.position.width, this.wrappedAreaMaxHeight));
                }

                if (this._onBeginGUI != null)
                {
                    this._onBeginGUI();
                }

                // Editor windows, can be created before Odin assigns OdinEditors to all relevent types via reflection.
                // This ensures that that happens before we render anything.
                if (!hasUpdatedOdinEditors)
                {
                    hasUpdatedOdinEditors = true;
                }

                this.marginStyle = this.marginStyle ?? new GUIStyle() { padding = new RectOffset() };

                if (Event.current.type == EventType.Layout)
                {
                    this.marginStyle.padding.left = (int)this.WindowPadding.x;
                    this.marginStyle.padding.right = (int)this.WindowPadding.y;
                    this.marginStyle.padding.top = (int)this.WindowPadding.z;
                    this.marginStyle.padding.bottom = (int)this.WindowPadding.w;

                    // Creates the editors.
                    UpdateEditors();
                }

                // Removes focus from text-fields when clicking on an empty area.
                var prevType = Event.current.type;
                if (Event.current.type == EventType.MouseDown)
                {
                    this.mouseDownId = GUIUtility.hotControl;
                    this.mouseDownKeyboardControl = GUIUtility.keyboardControl;
                    this.mouseDownWindow = focusedWindow;
                }

                // Draws the editors.
                bool useScrollWheel = this.UseScrollView;
                if (useScrollWheel)
                {
                    this.scrollPos = EditorGUILayout.BeginScrollView(this.scrollPos);
                }

                // Draw the GUI
                //var r = EditorGUILayout.BeginVertical();
                //{
                // Update the content rect
                Vector2 size;
                if (this.preventContentFromExpanding)
                {
                    size = EditorGUILayout.BeginVertical(GUILayoutOptions.ExpandHeight(false)).size;
                }
                else
                {
                    size = EditorGUILayout.BeginVertical().size;
                }

                if (this.contenSize == Vector2.zero || Event.current.type == EventType.Repaint)
                {
                    this.contenSize = size;
                }

                GUIHelper.PushHierarchyMode(false);
                float labelWidth;
                if (this.DefaultLabelWidth < 1)
                {
                    labelWidth = this.contenSize.x * this.DefaultLabelWidth;
                }
                else
                {
                    labelWidth = this.DefaultLabelWidth;
                }

                GUIHelper.PushLabelWidth(labelWidth);
                this.OnBeginDrawEditors();
                GUILayout.BeginVertical(this.marginStyle);

                DrawEditors();

                GUILayout.EndVertical();
                this.OnEndDrawEditors();
                GUIHelper.PopLabelWidth();
                GUIHelper.PopHierarchyMode();

                EditorGUILayout.EndVertical();

                if (useScrollWheel)
                {
                    EditorGUILayout.EndScrollView();
                }

                if (Event.current.type == EventType.Repaint)
                {
                    this.isAutoHeightAdjustmentReady = true;
                }

                if (this._onEndGUI != null)
                {
                    this._onEndGUI();
                }

                // Draw Toasts
                const int toastMargin = 10;
                const int iconSize = 16;
                const int horizontalPadding = 8;
                const int verticalPadding = 8;
                const int iconAreaWidth = iconSize + 2 * horizontalPadding;
                const int borderRadius = 3;
                const float fadeInDuration = 0.5f;
                const float fadeOutDuration = 1f;
                var toastOffsetTopLeft = toastMargin;
                var toastOffsetTopRight = toastMargin;
                var toastOffsetBottomLeft = toastMargin;
                var toastOffsetBottomRight = toastMargin;
                
                for (var i = 0; i < this.toasts.Count; i++)
                {
                    var toast = this.toasts[i];

                    Toast.Style.padding = new RectOffset(
                        left: toast.Icon != SdfIconType.None ? iconAreaWidth : horizontalPadding, 
                        right: horizontalPadding, 
                        top: verticalPadding, 
                        bottom: verticalPadding);
                    
                    // Calculate Toast Size
                    var toastWidth = Toast.Style.CalcSize(GUIHelper.TempContent(toast.Text)).x;
                    var toastHeight = (int)Toast.Style.CalcHeight(GUIHelper.TempContent(toast.Text), toastWidth);

                    var startX = 0f;
                    var startY = 0f;
                    var targetX = 0f;
                    var targetY = 0f;

                    switch (toast.ToastPosition)
                    {
                        case ToastPosition.TopLeft:
                            startX = toastMargin;
                            startY = 0f - toastHeight;
                            targetX = toastMargin;
                            targetY = toastOffsetTopLeft;
                            toastOffsetTopLeft += toastHeight + toastMargin;
                            break;
                        case ToastPosition.TopRight:
                            startX = this.position.width - toastWidth - toastMargin;
                            startY = 0f - toastHeight;
                            targetX = this.position.width - toastWidth - toastMargin;
                            targetY = toastOffsetTopRight;
                            toastOffsetTopRight += toastHeight + toastMargin;
                            break;
                        case ToastPosition.BottomLeft:
                            startX = toastMargin;
                            startY = this.position.height;
                            targetX = toastMargin;
                            targetY = this.position.height - toastOffsetBottomLeft - toastHeight;
                            toastOffsetBottomLeft += toastHeight + toastMargin;
                            break;
                        case ToastPosition.BottomRight:
                            startX = this.position.width - toastWidth - toastMargin;
                            startY = this.position.height;
                            targetX = this.position.width - toastWidth - toastMargin;
                            targetY = this.position.height - toastOffsetBottomRight - toastHeight;
                            toastOffsetBottomRight += toastHeight + toastMargin;
                            break;
                    }
                    
                    // Calculate Toast Target Rect
                    var targetRect = new Rect(
                        x: targetX,
                        y: targetY,
                        width: toastWidth,
                        height: toastHeight);
                    
                    // Set Toast Starting Position 
                    if (toast.CurrentRect == null)
                    {
                        toast.CurrentRect = new Rect(startX, startY, toastWidth, toastHeight);
                    }
                    
                    toast.TimePassed += GUITimeHelper.LayoutDeltaTime;
                    var alpha = Mathf.Clamp(toast.TimePassed / fadeInDuration, 0.0f, 1.0f);
                    if (toast.TimePassed > toast.Duration)
                    {
                        var timePassedSinceGoingOverDuration = toast.TimePassed - toast.Duration;
                        var fadeOutAlpha = Mathf.Clamp(1.0f - timePassedSinceGoingOverDuration / fadeOutDuration, 0.0f, 1.0f);
                        alpha = Mathf.Min(alpha, fadeOutAlpha);
                    }

                    // Move the Toast (animate)
                    toast.CurrentRect = new Rect(
                        targetRect.x,
                        Mathf.MoveTowards(toast.CurrentRect.Value.y, targetRect.y, GUITimeHelper.LayoutDeltaTime * 175),
                        targetRect.width, 
                        targetRect.height);
                    
                    // Draw Background
                    GUI.DrawTexture(
                        position: toast.CurrentRect.Value, 
                        image: Texture2D.whiteTexture, 
                        scaleMode: ScaleMode.StretchToFill, 
                        alphaBlend: true, 
                        imageAspect: 1, 
                        // Lerping produces a better looking transition than fading out the alpha alone.
                        color: Color.Lerp(toast.Color, new Color(toast.Color.r, toast.Color.g, toast.Color.b, 0f), 1f - alpha),
                        borderWidth: 0, 
                        borderRadius: borderRadius);

                    var toastColorLuminance = 0.299f * toast.Color.r + 0.587f * toast.Color.g + 0.114f * toast.Color.b;
                    var progressbarColor = toastColorLuminance > 0.3f ? new Color(0f, 0f, 0f, 0.08f) : new Color(1f, 1f, 1f, 0.04f);
                    var progressbarWidth = toast.CurrentRect.Value.width * toast.TimePassed / toast.Duration;
                    progressbarWidth = Mathf.Clamp(progressbarWidth, 0, toast.CurrentRect.Value.width);
                    var progressbarRect = toast.CurrentRect.Value.SetWidth(progressbarWidth);

                    // Draw Progressbar
                    GUI.DrawTexture(
                        position: progressbarRect,
                        image: Texture2D.whiteTexture, 
                        scaleMode: ScaleMode.StretchToFill, 
                        alphaBlend: true, 
                        imageAspect: 1, 
                        // Lerping produces a better looking transition than fading out the alpha alone.
                        color: Color.Lerp(progressbarColor, new Color(progressbarColor.r, progressbarColor.g, progressbarColor.b, 0f), 1f - alpha),
                        borderWidth: 0,
                        borderRadius: borderRadius);
                    
                    GUIHelper.PushColor(new Color(1f, 1f, 1f, alpha));
                    {
                        if (toast.Color.PerceivedLuminosity() > 0.65f)
                        {
                            GUIHelper.PushContentColor(new Color(0, 0, 0, 0.90f));
                            GUI.Label(toast.CurrentRect.Value, toast.Text, Toast.Style);
                            GUIHelper.PopContentColor();

                            var iconRect = toast.CurrentRect.Value.AlignLeft(iconAreaWidth);
                            SdfIcons.DrawIcon(iconRect.AlignCenter(iconSize), toast.Icon, new Color(0, 0, 0, 0.9f));
                        }
                        else
                        {
                            GUI.Label(toast.CurrentRect.Value, toast.Text, Toast.Style);
                            var iconRect = toast.CurrentRect.Value.AlignLeft(iconAreaWidth);
                            SdfIcons.DrawIcon(iconRect.AlignCenter(iconSize), toast.Icon, Color.white);
                        }
                    } 
                    GUIHelper.PopColor();

                    // Disappear immediately 
                    if (Event.current.OnMouseDown(toast.CurrentRect.Value, 0))
                    {
                        toast.TimePassed = toast.Duration + 0.7f;
                    }
                    
                    if (alpha == 0f)
                    {
                        this.toasts.RemoveAt(i--);
                    }
                    
                    GUIHelper.RequestRepaint();
                }

                this.RepaintIfRequested();

                if (measureArea)
                {
                    GUILayout.EndArea();
                }
            }
            finally
            {
                isInsideOnGUI = false;
            }
        }

        /// <summary>
        /// Draws the Odin Editor Window.
        /// </summary>
        [Obsolete("Rename this to OnImGUI()", true)]
        protected virtual void OnGUI()
        {
            if (this.isInOurImGUIContainer)
                return;

            this.OnImGUI();
        }

        /// <summary>
        /// Calls DrawEditor(index) for each of the currently drawing targets.
        /// </summary>
        protected virtual void DrawEditors()
        {
            for (int i = 0; i < this.currentTargets.Length; i++)
            {
                this.DrawEditor(i);
            }
        }

        protected void EnsureEditorsAreReady()
        {
            this.InitializeIfNeeded();
            this.UpdateEditors();
        }

        protected void UpdateEditors()
        {
            this.currentTargets = this.currentTargets ?? new object[] { };
            this.editors = this.editors ?? new Editor[] { };
            this.propertyTrees = this.propertyTrees ?? new PropertyTree[] { };

            var enumerable = this.GetTargets();
            IList<object> newTargets;

            if (enumerable is IList<object>)
            {
                newTargets = (IList<object>)enumerable;
            }
            else if (enumerable == null)
            {
                newTargets = EmptyObjectArray;
            }
            else
            {
                newTargets = enumerable.ToList();
            }

            if (this.currentTargets.Length != newTargets.Count)
            {
                if (this.editors.Length > newTargets.Count)
                {
                    var toDestroy = this.editors.Length - newTargets.Count;
                    for (int i = 0; i < toDestroy; i++)
                    {
                        var e = this.editors[this.editors.Length - i - 1];
                        if (e)
                        {
                            DestroyImmediate(e);
                        }
                    }
                }

                if (this.propertyTrees.Length > newTargets.Count)
                {
                    var toDestroy = this.propertyTrees.Length - newTargets.Count;
                    for (int i = 0; i < toDestroy; i++)
                    {
                        var e = this.propertyTrees[this.propertyTrees.Length - i - 1];
                        if (e != null)
                        {
                            e.Dispose();
                        }
                    }
                }

                Array.Resize(ref this.currentTargets, newTargets.Count);
                Array.Resize(ref this.editors, newTargets.Count);
                Array.Resize(ref this.propertyTrees, newTargets.Count);
                this.Repaint();

                this.currentTargetsImm = new ImmutableList<object>(this.currentTargets);
                this.warmupRepaintCount = 0;
            }

            for (int i = 0; i < newTargets.Count; i++)
            {
                var newTarget = newTargets[i];
                var curTarget = this.currentTargets[i];

                bool hasTargetChanged;

                if (curTarget is IEnumerable curEnumerable && newTarget is IEnumerable targetEnumerable)
                {
                    IEnumerator curEnumerator = curEnumerable.GetEnumerator();
                    IEnumerator targetEnumerator = targetEnumerable.GetEnumerator();

                    while (true)
                    {
                        bool hasCurNext = curEnumerator.MoveNext();
                        bool hasTargetNext = targetEnumerator.MoveNext();

                        if (!hasCurNext && !hasTargetNext)
                        {
                            hasTargetChanged = false;
                            break;
                        }

                        // Different collection sizes
                        if (!hasCurNext || !hasTargetNext)
                        {
                            hasTargetChanged = true;
                            break;
                        }

                        if (!object.ReferenceEquals(curEnumerator.Current, targetEnumerator.Current))
                        {
                            hasTargetChanged = true;
                            break;
                        }
                    }
                }
                else
                {
                    hasTargetChanged = !object.ReferenceEquals(newTarget, curTarget);
                }

                if (hasTargetChanged)
                {
                    this.warmupRepaintCount = 0;

                    GUIHelper.RequestRepaint();
                    this.currentTargets[i] = newTarget;

                    if (newTarget == null)
                    {
                        if (this.propertyTrees[i] != null) this.propertyTrees[i].Dispose();
                        this.propertyTrees[i] = null;
                        if (this.editors[i]) DestroyImmediate(this.editors[i]);
                        this.editors[i] = null;
                    }
                    else
                    {
                        var editorWindow = newTarget as EditorWindow;
                        if (newTarget.GetType().InheritsFrom<UnityEngine.Object>() && !editorWindow)
                        {
                            var unityObject = newTarget as UnityEngine.Object;
                            if (unityObject)
                            {
                                if (this.propertyTrees[i] != null) this.propertyTrees[i].Dispose();
                                this.propertyTrees[i] = null;
                                if (this.editors[i]) DestroyImmediate(this.editors[i]);
                                this.editors[i] = Editor.CreateEditor(unityObject);
                                var materialEditor = this.editors[i] as MaterialEditor;
                                if (materialEditor != null && materialForceVisibleProperty != null)
                                {
                                    materialForceVisibleProperty.SetValue(materialEditor, true, null);
                                }
                            }
                            else
                            {
                                if (this.propertyTrees[i] != null) this.propertyTrees[i].Dispose();
                                this.propertyTrees[i] = null;
                                if (this.editors[i]) DestroyImmediate(this.editors[i]);
                                this.editors[i] = null;
                            }
                        }
                        else
                        {
                            if (this.propertyTrees[i] != null) this.propertyTrees[i].Dispose();
                            if (this.editors[i]) DestroyImmediate(this.editors[i]);
                            this.editors[i] = null;

                            if (newTarget is System.Collections.IList)
                            {
                                this.propertyTrees[i] = PropertyTree.Create(newTarget as System.Collections.IList);
                            }
                            else
                            {
                                this.propertyTrees[i] = PropertyTree.Create(newTarget);
                            }
                        }
                    }
                }
            }
        }

        private void InitializeIfNeeded()
        {
            if (!isInitialized)
            {
                this.isInitialized = true;

                // Lets give it a better default name.
                if (this.titleContent != null && this.titleContent.text == this.GetType().FullName)
                {
                    this.titleContent.text = this.GetType().GetNiceName().SplitPascalCase();
                }

                // Mouse move please
                this.wantsMouseMove = true;
                Selection.selectionChanged -= this.SelectionChanged;
                Selection.selectionChanged += this.SelectionChanged;
                this.Initialize();
            }
        }

        /// <summary>
        /// Initialize get called by OnEnable and by OnGUI after assembly reloads
        /// which often happens when you recompile or enter and exit play mode.
        /// </summary>
        protected virtual void Initialize()
        {

        }

        private void SelectionChanged()
        {
            this.Repaint();
        }

        /// <summary>
        /// Called when the window is enabled. Remember to call base.OnEnable();
        /// </summary>
        protected virtual void OnEnable()
        {
            this.InitializeIfNeeded();
        }

        /// <summary>
        /// Draws the editor for the this.CurrentDrawingTargets[index].
        /// </summary>
        protected virtual void DrawEditor(int index)
        {
            if (!this.isInsideOnGUI)
            {
                this.EnsureEditorsAreReady();
            }

            var tmpPropertyTree = this.propertyTrees[index];
            var tmpEditor = this.editors[index];

            if (tmpPropertyTree != null || (tmpEditor != null && tmpEditor.target != null))
            {
                if (tmpPropertyTree != null)
                {
                    bool withUndo = tmpPropertyTree.WeakTargets.FirstOrDefault() as UnityEngine.Object;
                    tmpPropertyTree.Draw(withUndo);
                }
                else
                {
                    OdinEditor.ForceHideMonoScriptInEditor = true;
                    try
                    {
                        tmpEditor.OnInspectorGUI();
                    }
                    finally
                    {
                        OdinEditor.ForceHideMonoScriptInEditor = false;
                    }
                }
            }

            if (this.DrawUnityEditorPreview)
            {
                this.DrawEditorPreview(index, this.defaultEditorPreviewHeight);
            }
        }

        /// <summary>
        /// Uses the <see cref="UnityEditor.Editor.DrawPreview(Rect)"/> method to draw a preview for the this.CurrentDrawingTargets[index].
        /// </summary>
        protected virtual void DrawEditorPreview(int index, float height)
        {
            if (!this.isInsideOnGUI)
            {
                this.EnsureEditorsAreReady();
            }

            Editor editor = this.editors[index];

            if (editor != null && editor.HasPreviewGUI())
            {
                Rect rect = EditorGUILayout.GetControlRect(false, height);
                editor.DrawPreview(rect);
            }
        }

        protected virtual void OnDisable()
        {
            this.Cleanup();
        }

        /// <summary>
        /// Called when the window is destroyed. Remember to call base.OnDestroy();
        /// </summary>
        protected virtual void OnDestroy()
        {
            this.Cleanup();

            if (this.OnClose != null)
            {
                this.OnClose();
            }
        }

        private void Cleanup()
        {
            if (this.editors != null)
            {
                for (int i = 0; i < this.editors.Length; i++)
                {
                    if (this.editors[i])
                    {
                        DestroyImmediate(this.editors[i]);
                        this.editors[i] = null;
                    }
                }
                this.editors = null;
            }

            if (this.propertyTrees != null)
            {
                for (int i = 0; i < this.propertyTrees.Length; i++)
                {
                    if (this.propertyTrees[i] != null)
                    {
                        this.propertyTrees[i].Dispose();
                        this.propertyTrees[i] = null;
                    }
                }

                this.propertyTrees = null;
            }

            Selection.selectionChanged -= this.SelectionChanged;
            Selection.selectionChanged -= this.SelectionChanged;
        }

        /// <summary>
        /// Called before starting to draw all editors for the <see cref="CurrentDrawingTargets"/>.
        /// </summary>
        protected virtual void OnEndDrawEditors()
        {
        }

        /// <summary>
        /// Called after all editors for the <see cref="CurrentDrawingTargets"/> has been drawn.
        /// </summary>
        protected virtual void OnBeginDrawEditors()
        {
        }

        /// <summary>
        /// See ISerializationCallbackReceiver.OnBeforeSerialize for documentation on how to use this method.
        /// </summary>
        protected virtual void OnAfterDeserialize()
        {
        }

        /// <summary>
        /// Implement this method to receive a callback after unity serialized your object.
        /// </summary>
        protected virtual void OnBeforeSerialize()
        {
        }

        public void ShowToast(ToastPosition toastPosition, SdfIconType icon, string text, Color color, float duration)
        {
            this.toasts.Insert(0, new Toast(toastPosition, icon, text, color, duration));
        }
    }
}
#endif