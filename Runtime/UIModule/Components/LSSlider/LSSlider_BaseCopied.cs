using System;
using System.Collections.Generic;
using LSCore.Extensions.Unity;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LSCore
{ 
    public static class LSMultipleDisplayUtilities
        {
            /// <summary>
            /// Converts the current drag position into a relative position for the display.
            /// </summary>
            /// <param name="eventData"></param>
            /// <param name="position"></param>
            /// <returns>Returns true except when the drag operation is not on the same display as it originated</returns>
            public static bool GetRelativeMousePositionForDrag(PointerEventData eventData, ref Vector2 position)
            {
#if UNITY_EDITOR
                position = eventData.position;
#else
            int pressDisplayIndex = eventData.pointerPressRaycast.displayIndex;
            var relativePosition = RelativeMouseAtScaled(eventData.position, eventData.displayIndex);
            int currentDisplayIndex = (int)relativePosition.z;

            // Discard events on a different display.
            if (currentDisplayIndex != pressDisplayIndex)
                return false;

            // If we are not on the main display then we must use the relative position.
            position = pressDisplayIndex != 0 ? (Vector2)relativePosition : eventData.position;
#endif
                return true;
            }

            internal static Vector3 GetRelativeMousePositionForRaycast(PointerEventData eventData)
            {
                // The multiple display system is not supported on all platforms, when it is not supported the returned position
                // will be all zeros so when the returned index is 0 we will default to the event data to be safe.
                Vector3 eventPosition = RelativeMouseAtScaled(eventData.position, eventData.displayIndex);
                if (eventPosition == Vector3.zero)
                {
                    eventPosition = eventData.position;
#if UNITY_EDITOR
                    eventPosition.z = Display.activeEditorGameViewTarget;
#endif
                    // We don't really know in which display the event occurred. We will process the event assuming it occurred in our display.
                }

                // We support multiple display on some platforms. When supported:
                //  - InputSystem will set eventData.displayIndex
                //  - Old Input System will set eventPosition.z
                //
                // If the event is on the main display, both displayIndex and eventPosition.z
                // will be 0 so in that case we can leave the eventPosition untouched (see UUM-47650).
#if ENABLE_INPUT_SYSTEM && PACKAGE_INPUTSYSTEM
            if (eventData.displayIndex > 0)
                eventPosition.z = eventData.displayIndex;
#endif

                return eventPosition;
            }

            /// <summary>
            /// A version of Display.RelativeMouseAt that scales the position when the main display has a different rendering resolution to the system resolution.
            /// By default, the mouse position is relative to the main render area, we need to adjust this so it is relative to the system resolution
            /// in order to correctly determine the position on other displays.
            /// </summary>
            /// <returns></returns>
            public static Vector3 RelativeMouseAtScaled(Vector2 position, int displayIndex)
            {
#if !UNITY_EDITOR && !UNITY_WSA
            // If the main display is now the same resolution as the system then we need to scale the mouse position. (case 1141732)
            if (Display.main.renderingWidth != Display.main.systemWidth || Display.main.renderingHeight != Display.main.systemHeight)
            {
                // The system will add padding when in full-screen and using a non-native aspect ratio. (case UUM-7893)
                // For example Rendering 1920x1080 with a systeem resolution of 3440x1440 would create black bars on each side that are 330 pixels wide.
                // we need to account for this or it will offset our coordinates when we are not on the main display.
                var systemAspectRatio = Display.main.systemWidth / (float)Display.main.systemHeight;

                var sizePlusPadding = new Vector2(Display.main.renderingWidth, Display.main.renderingHeight);
                var padding = Vector2.zero;
                if (Screen.fullScreen)
                {
                    var aspectRatio = Screen.width / (float)Screen.height;
                    if (Display.main.systemHeight * aspectRatio < Display.main.systemWidth)
                    {
                        // Horizontal padding
                        sizePlusPadding.x = Display.main.renderingHeight * systemAspectRatio;
                        padding.x = (sizePlusPadding.x - Display.main.renderingWidth) * 0.5f;
                    }
                    else
                    {
                        // Vertical padding
                        sizePlusPadding.y = Display.main.renderingWidth / systemAspectRatio;
                        padding.y = (sizePlusPadding.y - Display.main.renderingHeight) * 0.5f;
                    }
                }

                var sizePlusPositivePadding = sizePlusPadding - padding;

                // If we are not inside of the main display then we must adjust the mouse position so it is scaled by
                // the main display and adjusted for any padding that may have been added due to different aspect ratios.
                if (position.y < -padding.y || position.y > sizePlusPositivePadding.y ||
                     position.x < -padding.x || position.x > sizePlusPositivePadding.x)
                {
                    var adjustedPosition = position;

                    if (!Screen.fullScreen)
                    {
                        // When in windowed mode, the window will be centered with the 0,0 coordinate at the top left, we need to adjust so it is relative to the screen instead.
                        adjustedPosition.x -= (Display.main.renderingWidth - Display.main.systemWidth) * 0.5f;
                        adjustedPosition.y -= (Display.main.renderingHeight - Display.main.systemHeight) * 0.5f;
                    }
                    else
                    {
                        // Scale the mouse position to account for the black bars when in a non-native aspect ratio.
                        adjustedPosition += padding;
                        adjustedPosition.x *= Display.main.systemWidth / sizePlusPadding.x;
                        adjustedPosition.y *= Display.main.systemHeight / sizePlusPadding.y;
                    }

					// fix for UUM-63551: Use the display index provided to this method.  Display.RelativeMouseAt( ) no longer works starting with 2021 LTS and new input system
					// as the Pointer position is reported in Window coordinates rather than relative to the primary window as Display.RelativeMouseAt( ) expects.
#if ENABLE_INPUT_SYSTEM && PACKAGE_INPUTSYSTEM && (UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX)
                    var relativePos = new Vector3(adjustedPosition.x, adjustedPosition.y, displayIndex);
#else
                    var relativePos = Display.RelativeMouseAt(adjustedPosition);
#endif

                    // If we are not on the main display then return the adjusted position.
                    if (relativePos.z != 0)
                        return relativePos;
                }

                // We are using the main display.
                return new Vector3(position.x, position.y, 0);
            }
#endif

                // fix for UUM-63551: Use the display index provided to this method.  Display.RelativeMouseAt( ) no longer works starting with 2021 LTS and new input system
                // as the Pointer position is reported in Window coordinates rather than relative to the primary window as Display.RelativeMouseAt( ) expects.
#if ENABLE_INPUT_SYSTEM && PACKAGE_INPUTSYSTEM && (UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX)
            return new Vector3(position.x, position.y, displayIndex);
#else
                return Display.RelativeMouseAt(position);
#endif
            }
        }
            
    public partial class LSSlider
    {

        private static class SetPropertyUtility
        {
            public static bool SetColor(ref Color currentValue, Color newValue)
            {
                if (currentValue.r == newValue.r && currentValue.g == newValue.g && currentValue.b == newValue.b && currentValue.a == newValue.a)
                    return false;

                currentValue = newValue;
                return true;
            }

            public static bool SetStruct<T>(ref T currentValue, T newValue) where T : struct
            {
                if (EqualityComparer<T>.Default.Equals(currentValue, newValue))
                    return false;

                currentValue = newValue;
                return true;
            }

            public static bool SetClass<T>(ref T currentValue, T newValue) where T : class
            {
                if ((currentValue == null && newValue == null) || (currentValue != null && currentValue.Equals(newValue)))
                    return false;

                currentValue = newValue;
                return true;
            }
        }
        
        /// <summary>
        /// Setting that indicates one of four directions.
        /// </summary>
        public enum Direction
        {
            /// <summary>
            /// From the left to the right
            /// </summary>
            LeftToRight,

            /// <summary>
            /// From the right to the left
            /// </summary>
            RightToLeft,

            /// <summary>
            /// From the bottom to the top.
            /// </summary>
            BottomToTop,

            /// <summary>
            /// From the top to the bottom.
            /// </summary>
            TopToBottom,
        }

        [Serializable]
        /// <summary>
        /// Event type used by the UI.Slider.
        /// </summary>
        public class SliderEvent : UnityEvent<float> {}

        [SerializeField]
        private RectTransform m_FillRect;

        /// <summary>
        /// Optional RectTransform to use as fill for the slider.
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// using UnityEngine;
        /// using System.Collections;
        /// using UnityEngine.UI;  // Required when Using UI elements.
        ///
        /// public class Example : MonoBehaviour
        /// {
        ///     public Slider mainSlider;
        ///     //Reference to new "RectTransform"(Child of FillArea).
        ///     public RectTransform newFillRect;
        ///
        ///     //Deactivates the old FillRect and assigns a new one.
        ///     void Start()
        ///     {
        ///         mainSlider.fillRect.gameObject.SetActive(false);
        ///         mainSlider.fillRect = newFillRect;
        ///     }
        /// }
        /// ]]>
        ///</code>
        /// </example>
        public RectTransform fillRect { get { return m_FillRect; } set { if (SetPropertyUtility.SetClass(ref m_FillRect, value)) {UpdateCachedReferences(); UpdateVisuals(); } } }

        [SerializeField]
        private RectTransform m_HandleRect;

        /// <summary>
        /// Optional RectTransform to use as a handle for the slider.
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// using UnityEngine;
        /// using System.Collections;
        /// using UnityEngine.UI; // Required when Using UI elements.
        ///
        /// public class Example : MonoBehaviour
        /// {
        ///     public Slider mainSlider;
        ///     //Reference to new "RectTransform" (Child of "Handle Slide Area").
        ///     public RectTransform handleHighlighted;
        ///
        ///     //Deactivates the old Handle, then assigns and enables the new one.
        ///     void Start()
        ///     {
        ///         mainSlider.handleRect.gameObject.SetActive(false);
        ///         mainSlider.handleRect = handleHighlighted;
        ///         mainSlider.handleRect.gameObject.SetActive(true);
        ///     }
        /// }
        /// ]]>
        ///</code>
        /// </example>
        public RectTransform handleRect { get { return m_HandleRect; } set { if (SetPropertyUtility.SetClass(ref m_HandleRect, value)) { UpdateCachedReferences(); UpdateVisuals(); } } }

        [Space]

        [SerializeField]
        private Direction m_Direction = Direction.LeftToRight;

        /// <summary>
        /// The direction of the slider, from minimum to maximum value.
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// using UnityEngine;
        /// using System.Collections;
        /// using UnityEngine.UI; // Required when Using UI elements.
        ///
        /// public class Example : MonoBehaviour
        /// {
        ///     public Slider mainSlider;
        ///
        ///     public void Start()
        ///     {
        ///         //Changes the direction of the slider.
        ///         if (mainSlider.direction == Slider.Direction.BottomToTop)
        ///         {
        ///             mainSlider.direction = Slider.Direction.TopToBottom;
        ///         }
        ///     }
        /// }
        /// ]]>
        ///</code>
        /// </example>
        public Direction direction { get { return m_Direction; } set { if (SetPropertyUtility.SetStruct(ref m_Direction, value)) UpdateVisuals(); } }

        [SerializeField]
        private float m_MinValue = 0;

        /// <summary>
        /// The minimum allowed value of the slider.
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// using UnityEngine;
        /// using System.Collections;
        /// using UnityEngine.UI; // Required when Using UI elements.
        ///
        /// public class Example : MonoBehaviour
        /// {
        ///     public Slider mainSlider;
        ///
        ///     void Start()
        ///     {
        ///         // Changes the minimum value of the slider to 10;
        ///         mainSlider.minValue = 10;
        ///     }
        /// }
        /// ]]>
        ///</code>
        /// </example>
        public float minValue { get { return m_MinValue; } set { if (SetPropertyUtility.SetStruct(ref m_MinValue, value)) { Set(m_Value); UpdateVisuals(); } } }

        [SerializeField]
        private float m_MaxValue = 1;

        /// <summary>
        /// The maximum allowed value of the slider.
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// using UnityEngine;
        /// using System.Collections;
        /// using UnityEngine.UI; // Required when Using UI elements.
        ///
        /// public class Example : MonoBehaviour
        /// {
        ///     public Slider mainSlider;
        ///
        ///     void Start()
        ///     {
        ///         // Changes the max value of the slider to 20;
        ///         mainSlider.maxValue = 20;
        ///     }
        /// }
        /// ]]>
        ///</code>
        /// </example>
        public float maxValue { get { return m_MaxValue; } set { if (SetPropertyUtility.SetStruct(ref m_MaxValue, value)) { Set(m_Value); UpdateVisuals(); } } }

        [SerializeField]
        protected float m_Value;

        /// <summary>
        /// The current value of the slider.
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// using UnityEngine;
        /// using System.Collections;
        /// using UnityEngine.UI; // Required when Using UI elements.
        ///
        /// public class Example : MonoBehaviour
        /// {
        ///     public Slider mainSlider;
        ///
        ///     //Invoked when a submit button is clicked.
        ///     public void SubmitSliderSetting()
        ///     {
        ///         //Displays the value of the slider in the console.
        ///         Debug.Log(mainSlider.value);
        ///     }
        /// }
        /// ]]>
        ///</code>
        /// </example>
        public virtual float value
        {
            get
            {
                return m_Value;
            }
            set
            {
                Set(value);
            }
        }

        /// <summary>
        /// Set the value of the slider without invoking onValueChanged callback.
        /// </summary>
        /// <param name="input">The new value for the slider.</param>
        public virtual void SetValueWithoutNotify(float input)
        {
            Set(input, false);
        }

        /// <summary>
        /// The current value of the slider normalized into a value between 0 and 1.
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// using UnityEngine;
        /// using System.Collections;
        /// using UnityEngine.UI; // Required when Using UI elements.
        ///
        /// public class Example : MonoBehaviour
        /// {
        ///     public Slider mainSlider;
        ///
        ///     //Set to invoke when "OnValueChanged" method is called.
        ///     void CheckNormalisedValue()
        ///     {
        ///         //Displays the normalised value of the slider everytime the value changes.
        ///         Debug.Log(mainSlider.normalizedValue);
        ///     }
        /// }
        /// ]]>
        ///</code>
        /// </example>
        public virtual float normalizedValue
        {
            get
            {
                if (Mathf.Approximately(minValue, maxValue))
                    return 0;
                return Mathf.InverseLerp(minValue, maxValue, value);
            }
            set
            {
                this.value = Mathf.Lerp(minValue, maxValue, value);
            }
        }

        [Space]

        [SerializeField]
        private SliderEvent m_OnValueChanged = new SliderEvent();

        /// <summary>
        /// Callback executed when the value of the slider is changed.
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// using UnityEngine;
        /// using System.Collections;
        /// using UnityEngine.UI; // Required when Using UI elements.
        ///
        /// public class Example : MonoBehaviour
        /// {
        ///     public Slider mainSlider;
        ///
        ///     public void Start()
        ///     {
        ///         //Adds a listener to the main slider and invokes a method when the value changes.
        ///         mainSlider.onValueChanged.AddListener(delegate {ValueChangeCheck(); });
        ///     }
        ///
        ///     // Invoked when the value of the slider changes.
        ///     public void ValueChangeCheck()
        ///     {
        ///         Debug.Log(mainSlider.value);
        ///     }
        /// }
        /// ]]>
        ///</code>
        /// </example>
        public SliderEvent onValueChanged { get { return m_OnValueChanged; } set { m_OnValueChanged = value; } }

        // The offset from handle position to mouse down position
        private Vector2 m_Offset = Vector2.zero;

        // This "delayed" mechanism is required for case 1037681.
        private bool m_DelayedUpdateVisuals = false;

        // Size of each step.
        float stepSize { get { return (maxValue - minValue) * 0.1f; } }

        protected LSSlider()
        {}

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();

            //Onvalidate is called before OnEnabled. We need to make sure not to touch any other objects before OnEnable is run.
            if (IsActive())
            {
                UpdateCachedReferences();
                // Update rects in next update since other things might affect them even if value didn't change.
                m_DelayedUpdateVisuals = true;
            }

            if (!UnityEditor.PrefabUtility.IsPartOfPrefabAsset(this) && !Application.isPlaying)
            {
                CanvasUpdateRegistry.RegisterCanvasElementForLayoutRebuild(this);
            }
            
            onValueChanged.RemoveAllListeners();
            Init();
        }

#endif

        public virtual void Rebuild(CanvasUpdate executing)
        {
#if UNITY_EDITOR
            if (executing == CanvasUpdate.Prelayout)
                onValueChanged.Invoke(value);
#endif
        }

        /// <summary>
        /// See ICanvasElement.LayoutComplete
        /// </summary>
        public virtual void LayoutComplete()
        {}

        /// <summary>
        /// See ICanvasElement.GraphicUpdateComplete
        /// </summary>
        public virtual void GraphicUpdateComplete()
        {}

        protected override void OnDisable()
        {
            m_Tracker.Clear();
            base.OnDisable();
        }

        /// <summary>
        /// Update the rect based on the delayed update visuals.
        /// Got around issue of calling sendMessage from onValidate.
        /// </summary>
        protected virtual void Update()
        {
            if (m_DelayedUpdateVisuals)
            {
                m_DelayedUpdateVisuals = false;
                Set(m_Value, false);
                UpdateVisuals();
            }
        }

        protected override void OnDidApplyAnimationProperties()
        {
            // Has value changed? Various elements of the slider have the old normalisedValue assigned, we can use this to perform a comparison.
            // We also need to ensure the value stays within min/max.
            m_Value = ClampValue(m_Value);
            float oldNormalizedValue = normalizedValue;
            if (fillContainerRect != null)
            {
                if (fillImage != null && fillImage.type == Image.Type.Filled)
                    oldNormalizedValue = fillImage.fillAmount;
                else
                    oldNormalizedValue = (reverseValue ? 1 - m_FillRect.anchorMin[(int)axis] : m_FillRect.anchorMax[(int)axis]);
            }
            else if (handleContainerRect != null)
                oldNormalizedValue = (reverseValue ? 1 - m_HandleRect.anchorMin[(int)axis] : m_HandleRect.anchorMin[(int)axis]);

            UpdateVisuals();

            if (oldNormalizedValue != normalizedValue)
            {
                UISystemProfilerApi.AddMarker("Slider.value", this);
                onValueChanged.Invoke(m_Value);
            }
            // UUM-34170 Apparently, some properties on slider such as IsInteractable and Normalcolor Animation is broken.
            // We need to call base here to render the animation on Scene
            base.OnDidApplyAnimationProperties();
        }

        protected override void OnRectTransformDimensionsChange()
        {
            base.OnRectTransformDimensionsChange();

            //This can be invoked before OnEnabled is called. So we shouldn't be accessing other objects, before OnEnable is called.
            if (!IsActive())
                return;

            UpdateVisuals();
        }

        // Update the slider's position based on the mouse.
        void UpdateDrag(PointerEventData eventData, Camera cam)
        {
            RectTransform clickRect = handleContainerRect ?? fillContainerRect;
            if (clickRect != null && clickRect.rect.size[(int)axis] > 0)
            {
                Vector2 position = Vector2.zero;
                if (!LSMultipleDisplayUtilities.GetRelativeMousePositionForDrag(eventData, ref position))
                    return;

                Vector2 localCursor;
                if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(clickRect, position, cam, out localCursor))
                    return;
                localCursor -= clickRect.rect.position;

                float val = Mathf.Clamp01((localCursor - m_Offset)[(int)axis] / clickRect.rect.size[(int)axis]);
                normalizedValue = (reverseValue ? 1f - val : val);
            }
        }

        private bool MayDrag(PointerEventData eventData)
        {
            return IsActive() && IsInteractable() && eventData.button == PointerEventData.InputButton.Left;
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            if (!MayDrag(eventData))
                return;

            base.OnPointerDown(eventData);

            m_Offset = Vector2.zero;
            if (handleContainerRect != null && RectTransformUtility.RectangleContainsScreenPoint(m_HandleRect, eventData.pointerPressRaycast.screenPosition, eventData.enterEventCamera))
            {
                Vector2 localMousePos;
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(m_HandleRect, eventData.pointerPressRaycast.screenPosition, eventData.pressEventCamera, out localMousePos))
                    m_Offset = localMousePos;
            }
            else
            {
                // Outside the slider handle - jump to this point instead
                UpdateDrag(eventData, eventData.pressEventCamera);
            }
        }

        public virtual void OnDrag(PointerEventData eventData)
        {
            if (!MayDrag(eventData))
                return;
            UpdateDrag(eventData, eventData.pressEventCamera);
        }

        public override void OnMove(AxisEventData eventData)
        {
            if (!IsActive() || !IsInteractable())
            {
                base.OnMove(eventData);
                return;
            }

            switch (eventData.moveDir)
            {
                case MoveDirection.Left:
                    if (axis == Axis.Horizontal && FindSelectableOnLeft() == null)
                        Set(reverseValue ? value + stepSize : value - stepSize);
                    else
                        base.OnMove(eventData);
                    break;
                case MoveDirection.Right:
                    if (axis == Axis.Horizontal && FindSelectableOnRight() == null)
                        Set(reverseValue ? value - stepSize : value + stepSize);
                    else
                        base.OnMove(eventData);
                    break;
                case MoveDirection.Up:
                    if (axis == Axis.Vertical && FindSelectableOnUp() == null)
                        Set(reverseValue ? value - stepSize : value + stepSize);
                    else
                        base.OnMove(eventData);
                    break;
                case MoveDirection.Down:
                    if (axis == Axis.Vertical && FindSelectableOnDown() == null)
                        Set(reverseValue ? value + stepSize : value - stepSize);
                    else
                        base.OnMove(eventData);
                    break;
            }
        }

        /// <summary>
        /// See Selectable.FindSelectableOnLeft
        /// </summary>
        public override Selectable FindSelectableOnLeft()
        {
            if (navigation.mode == Navigation.Mode.Automatic && axis == Axis.Horizontal)
                return null;
            return base.FindSelectableOnLeft();
        }

        /// <summary>
        /// See Selectable.FindSelectableOnRight
        /// </summary>
        public override Selectable FindSelectableOnRight()
        {
            if (navigation.mode == Navigation.Mode.Automatic && axis == Axis.Horizontal)
                return null;
            return base.FindSelectableOnRight();
        }

        /// <summary>
        /// See Selectable.FindSelectableOnUp
        /// </summary>
        public override Selectable FindSelectableOnUp()
        {
            if (navigation.mode == Navigation.Mode.Automatic && axis == Axis.Vertical)
                return null;
            return base.FindSelectableOnUp();
        }

        /// <summary>
        /// See Selectable.FindSelectableOnDown
        /// </summary>
        public override Selectable FindSelectableOnDown()
        {
            if (navigation.mode == Navigation.Mode.Automatic && axis == Axis.Vertical)
                return null;
            return base.FindSelectableOnDown();
        }

        public virtual void OnInitializePotentialDrag(PointerEventData eventData)
        {
            eventData.useDragThreshold = false;
        }

        /// <summary>
        /// Sets the direction of this slider, optionally changing the layout as well.
        /// </summary>
        /// <param name="direction">The direction of the slider</param>
        /// <param name="includeRectLayouts">Should the layout be flipped together with the slider direction</param>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// using UnityEngine;
        /// using System.Collections;
        /// using UnityEngine.UI; // Required when Using UI elements.
        ///
        /// public class Example : MonoBehaviour
        /// {
        ///     public Slider mainSlider;
        ///
        ///     public void Start()
        ///     {
        ///         mainSlider.SetDirection(Slider.Direction.LeftToRight, false);
        ///     }
        /// }
        /// ]]>
        ///</code>
        /// </example>
        public void SetDirection(Direction direction, bool includeRectLayouts)
        {
            Axis oldAxis = axis;
            bool oldReverse = reverseValue;
            this.direction = direction;

            if (!includeRectLayouts)
                return;

            if (axis != oldAxis)
                RectTransformUtility.FlipLayoutAxes(transform as RectTransform, true, true);

            if (reverseValue != oldReverse)
                RectTransformUtility.FlipLayoutOnAxis(transform as RectTransform, (int)axis, true, true);
        }
    
        
        private float ClampValue(float input)
        {
            float newValue = Mathf.Clamp(input, minValue, maxValue);
            return newValue;
        }

        private Image fillImage;
        private Transform fillTransform;
        private RectTransform fillContainerRect;
        private Transform handleTransform;
        private RectTransform handleContainerRect;

        private void UpdateCachedReferences()
        {
            if (fillRect && fillRect != (RectTransform)transform)
            {
                fillTransform = fillRect.transform;
                fillImage = fillRect.GetComponent<Image>();
                if (fillTransform.parent != null)
                    fillContainerRect = fillTransform.parent.GetComponent<RectTransform>();
            }
            else
            {
                fillRect = null;
                fillContainerRect = null;
                fillImage = null;
            }

            if (handleRect && handleRect != (RectTransform)transform)
            {
                handleTransform = handleRect.transform;
                if (handleTransform.parent != null)
                    handleContainerRect = handleTransform.parent.GetComponent<RectTransform>();
            }
            else
            {
                handleRect = null;
                handleContainerRect = null;
            }
        }

        private enum Axis
        {
            Horizontal = 0,
            Vertical = 1
        }

        private bool reverseValue
        {
            get { return direction == Direction.RightToLeft || direction == Direction.TopToBottom; }
        }

        private Axis axis
        {
            get
            {
                return (direction == Direction.LeftToRight || direction == Direction.RightToLeft)
                    ? Axis.Horizontal
                    : Axis.Vertical;
            }
        }

        private DrivenRectTransformTracker m_Tracker;

        private void UpdateVisuals()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                UpdateCachedReferences();
#endif

            m_Tracker.Clear();

            if (fillContainerRect != null)
            {
                m_Tracker.Add(this, fillRect, DrivenTransformProperties.Anchors);
                Vector2 anchorMin = Vector2.zero;
                Vector2 anchorMax = Vector2.one;

                if (fillImage != null && fillImage.type == Image.Type.Filled)
                {
                    fillImage.fillAmount = normalizedValue;
                }
                else
                {
                    if (reverseValue)
                        anchorMin[(int)axis] = 1 - normalizedValue;
                    else
                        anchorMax[(int)axis] = normalizedValue;
                }

                fillRect.anchorMin = anchorMin;
                fillRect.anchorMax = anchorMax;
            }

            if (handleContainerRect != null)
            {
                m_Tracker.Add(this, handleRect, DrivenTransformProperties.Anchors);
                Vector2 anchorMin = Vector2.zero;
                Vector2 anchorMax = Vector2.one;
                anchorMin[(int)axis] = anchorMax[(int)axis] = (reverseValue ? (1 - normalizedValue) : normalizedValue);
                handleRect.anchorMin = anchorMin;
                handleRect.anchorMax = anchorMax;
            }
        }
    }
}