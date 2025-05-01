//-----------------------------------------------------------------------
// <copyright file="UIToolkitIntegration.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#if UNITY_EDITOR
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
namespace Sirenix.OdinInspector.Editor.Internal.UIToolkitIntegration
{
#pragma warning disable

    using System;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEditor.UIElements;
    using UnityEngine.UIElements;
    using UnityEngine;
    using Sirenix.Utilities.Editor;
    using Sirenix.Utilities;
    using Sirenix.Reflection.Editor;
	using Sirenix.Serialization;
	using System.Reflection;
	using System.Reflection.Emit;
    using Label = UnityEngine.UIElements.Label;

	public interface IUnityPropertyFieldDrawer
	{
		bool WillDrawPropertyField { get; }
	}

	public class DrawWithVisualElementsAttributeDrawer<T> : OdinAttributeDrawer<DrawWithVisualElementsAttribute, T>, IDisposable, IUnityPropertyFieldDrawer
	{
        private OdinImGuiElement element;

		public bool WillDrawPropertyField => element != null;

        protected override bool CanDrawAttributeValueProperty(InspectorProperty property)
        {
            if (property.GetAttribute<DrawWithVisualElementsAttribute>().DrawCollectionWithImGUI && property.ChildResolver is ICollectionResolver)
            {
                return false;
            }

            return true;
        }

        protected override void Initialize()
        {
			if (!ImguiElementUtils.IsSupported) return;

			var unityProperty = this.Property.Tree.GetUnityPropertyForPath(this.Property.Path, out _);

            if (unityProperty != null)
            {
                var propField = new PropertyField(unityProperty);

                ImguiElementUtils.RegisterSerializedPropertyChangeEventCallback(propField, (prop) =>
                {
                    if (this.ValueEntry.IsEditable && prop.serializedObject.targetObject is EmittedScriptableObject<T>)
                    {
                        var targetObjects = prop.serializedObject.targetObjects;

                        for (int i = 0; i < targetObjects.Length; i++)
                        {
                            EmittedScriptableObject<T> target = (EmittedScriptableObject<T>)targetObjects[i];
                            this.ValueEntry.Values[i] = target.GetValue();
                        }

                        this.ValueEntry.Values.ForceMarkDirty();
                    }
                });

                this.element = new OdinImGuiElement(propField, unityProperty);
                this.element.Bind(unityProperty.serializedObject);
            }
        }

        protected override void DrawPropertyLayout(GUIContent label)
        {
			if (!ImguiElementUtils.IsSupported)
			{
                SirenixEditorGUI.ErrorMessageBox($"Unable to draw visual element for this property since UIToolkit is not supported by Odin in this version of Unity (requires Unity 2020.2+).");
				this.CallNextDrawer(label);
				return;
			}

            if (this.element == null)
            {
                var path = this.Property.UnityPropertyPath;
                SirenixEditorGUI.ErrorMessageBox($"Unable to draw visual element for {path} of type {typeof(T).GetNiceFullName()} because no Unity SerializedProperty existed for path and no property wrapper could be emitted.");
                return;
            }

            SerializedProperty unityProperty = this.Property.Tree.GetUnityPropertyForPath(this.Property.Path, out var fieldInfo);

            if (unityProperty == null)
            {
                SirenixEditorGUI.ErrorMessageBox("Could not get a Unity SerializedProperty for the property '" + this.Property.NiceName + "' of type '" + this.Property.ValueEntry.TypeOfValue.GetNiceName() + "' at path '" + this.Property.Path + "'.");
                return;
            }

            if (unityProperty.serializedObject.targetObject is EmittedScriptableObject<T>)
            {
                if (!unityProperty.isArray)
                {
                    var targetObjects = unityProperty.serializedObject.targetObjects;

                    for (int i = 0; i < targetObjects.Length; i++)
                    {
                        EmittedScriptableObject<T> target = (EmittedScriptableObject<T>)targetObjects[i];
                        target.SetValue(this.ValueEntry.Values[i]);
                    }

                    unityProperty.serializedObject.Update();
                }
            }

            ImguiElementUtils.EmbedVisualElementAndDrawItHere(this.element, label);
        }

        public void Dispose()
        {
            if (this.element != null)
            {
                this.element.Unbind();
                this.element.RemoveFromHierarchy();
                this.element = null;
            }
        }
    }

    public class OdinImGuiElement : VisualElement
    {
        static int idCounter = 0;

        private Vector2 measuredSize;
        private string expectedLabel;
        private bool isMeasuredBeforeAdd;

        internal float CachedOpacity = -1;
        internal bool CachedEnabled = true;
        internal DisplayStyle CachedDisplay;
        internal string CachedText;
        internal string CachedTooltip;
        internal Rect CurrClip;
        internal Rect RectAtPostLayoutTime;
        internal Rect? ClipRect;
        internal Rect AbsoluteRect;
        internal Rect PrevAbsoluteRect;
        internal List<GUILayoutGroup_Internal> ParentLayoutGroups = new List<GUILayoutGroup_Internal>();
        internal List<ScrollViewState_Internal> ScrollViewStates = new List<ScrollViewState_Internal>();
        internal IMGUIContainer LastImGUIContainer;

        public VisualElement PositioningContainer;
        public Label LabelElement;

        private static string GetName(VisualElement element)
        {
            if (element is PropertyField p)
            {
                return p.bindingPath;
            }

            return element.name;
        }

        public OdinImGuiElement(VisualElement element, SerializedProperty unityProperty)
        {
            this.name = $"({idCounter++}) : " + unityProperty.propertyPath;
            this.PositioningContainer = new VisualElement();
            this.PositioningContainer.name = "Odin Positioning Container (Value)";
            this.Add(this.PositioningContainer);
            this.PositioningContainer.Add(element);
            this.expectedLabel = unityProperty.displayName;
        }

        public OdinImGuiElement(SerializedProperty unityProperty)
        {
            this.name = $"Odin Clip Container ({idCounter++}) : " + unityProperty.propertyPath;
            this.PositioningContainer = new VisualElement();
            this.PositioningContainer.name = "Odin Positioning Container (Value)";
            this.Add(this.PositioningContainer);
            this.PositioningContainer.Add(new PropertyField(unityProperty));
            this.expectedLabel = unityProperty.displayName;
        }

        public OdinImGuiElement(VisualElement element)
        {
            this.name = $"Odin Clip ({idCounter++}) : " + GetName(element);
            this.PositioningContainer = new VisualElement();
            this.PositioningContainer.name = "Odin Element (Value)";
            this.Add(this.PositioningContainer);
            this.PositioningContainer.Add(element);
        }

        internal Vector2 GetSize(float width)
        {
            if (!isMeasuredBeforeAdd)
            {
                isMeasuredBeforeAdd = true;

                if (this.LastImGUIContainer != null)
                {
                    this.measuredSize = ImguiElementUtils.MeasureSizeOfVisualElement(this.LastImGUIContainer, this, width).size;
                    return this.measuredSize;
                }
            }

            var size = this.PositioningContainer.contentRect.size;
            if (size.x == 0 || size.y == 0 || float.IsNaN(size.x) || float.IsNaN(size.y))
            {
                return this.measuredSize;
            }
            return size;
        }

        internal void OnAdd()
        {
            this.style.position = Position.Absolute;
            this.style.left = 0;
            this.style.top = 0;
            this.style.width = Length.Percent(100);
            this.style.height = Length.Percent(100);
            this.style.overflow = Overflow.Hidden;
            this.pickingMode = PickingMode.Ignore;
            this.PositioningContainer.style.position = Position.Absolute;
				this.PositioningContainer.pickingMode = PickingMode.Ignore;

            if (!string.IsNullOrWhiteSpace(this.expectedLabel))
            {
                var labelCandidate = FastRecursiveLabelFinder(this.PositioningContainer);
                if (labelCandidate != null)
                {
                    if (labelCandidate.text == this.expectedLabel)
                    {
                        this.LabelElement = labelCandidate;
                        this.CachedDisplay = this.LabelElement.style.display.value;
                        this.CachedText = this.LabelElement.text;
                    }
                }
            }
        }

        static Label FastRecursiveLabelFinder(VisualElement element, int depth = 0)
        {
            const int maxDepth = 6;
            const int maxBreadth = 10;

            if (element is Label lbl)
            {
                return lbl;
            }

            if (depth >= maxDepth)
            {
                return null;
            }

            var childCount = Math.Min(element.childCount, maxBreadth);
            for (int i = 0; i < childCount; i++)
            {
                var label = FastRecursiveLabelFinder(element.hierarchy[i]);
                if (label != null)
                {
                    return label;
                }
            }

            return null;
        }
    }

	// OdinUIToolkitIntegrationUtils

	public static class ImguiElementUtils
	{
		private const string NotSupportedMessage = "UIToolkit is not supported by Odin in this version of Unity; it requires version 2020.2 or above.";
		public static readonly bool IsSupported = true;

		private static readonly Type SerializedPropertyChangeEvent_Type;
		private static readonly PropertyInfo SerializedPropertyChangeEvent_changedProperty_Prop;
		private static readonly MethodInfo CallbackEventHandler_RegisterCallback_MethodDefinition;
		private static readonly MethodInfo CallbackEventHandler_RegisterCallback_SerializedPropertyChangeEvent_Method;
		private static readonly Type EventCallback_SerializedPropertyChangeEvent_Type;

		private static readonly DynamicMethod EmittedCallbackInvoker;
		//private static readonly Type CallbackInvoker_SerializedPropertyChangeEvent_Type;

		static ImguiElementUtils()
		{
			SerializedPropertyChangeEvent_Type = TwoWaySerializationBinder.Default.BindToType("UnityEditor.UIElements.SerializedPropertyChangeEvent");
			SerializedPropertyChangeEvent_changedProperty_Prop = SerializedPropertyChangeEvent_Type?.GetProperty("changedProperty", BindingFlags.Instance | BindingFlags.Public);

			foreach (var method in typeof(CallbackEventHandler).GetMethods(BindingFlags.Public | BindingFlags.Instance))
			{
				if (method.Name == "RegisterCallback" && method.IsGenericMethodDefinition)
				{
					var parameters = method.GetParameters();
					if (parameters.Length == 2 && parameters[0].ParameterType.IsGenericType && parameters[0].ParameterType.GetGenericTypeDefinition() == typeof(EventCallback<>) && parameters[1].ParameterType == typeof(TrickleDown))
					{
						CallbackEventHandler_RegisterCallback_MethodDefinition = method;
						break;
					}
				}
			}

			if (SerializedPropertyChangeEvent_Type == null
				|| SerializedPropertyChangeEvent_changedProperty_Prop == null
				|| CallbackEventHandler_RegisterCallback_MethodDefinition == null)
			{
				IsSupported = false;
			}
			else
			{
				EventCallback_SerializedPropertyChangeEvent_Type = typeof(EventCallback<>).MakeGenericType(SerializedPropertyChangeEvent_Type);
				CallbackEventHandler_RegisterCallback_SerializedPropertyChangeEvent_Method = CallbackEventHandler_RegisterCallback_MethodDefinition.MakeGenericMethod(SerializedPropertyChangeEvent_Type);

				var method = new DynamicMethod("ImguiElementUtils.EmittedCallbackInvoker", typeof(void), new Type[] { typeof(CallbackInvokerContext), SerializedPropertyChangeEvent_Type }, true);
				var il = method.GetILGenerator();

				il.Emit(OpCodes.Ldarg_0);
				il.Emit(OpCodes.Ldfld, typeof(CallbackInvokerContext).GetField("Action", BindingFlags.Instance | BindingFlags.Public));
				il.Emit(OpCodes.Ldarg_1);
				il.Emit(OpCodes.Callvirt, SerializedPropertyChangeEvent_changedProperty_Prop.GetGetMethod(true));
				il.Emit(OpCodes.Call, typeof(Action<SerializedProperty>).GetMethod("Invoke", BindingFlags.Instance | BindingFlags.Public));
				il.Emit(OpCodes.Ret);

				EmittedCallbackInvoker = method;
			}
		}

		private static Dictionary<IMGUIContainer, ImGUIElementDrawer> imGUIElementDrawerLookup = new Dictionary<IMGUIContainer, ImGUIElementDrawer>();

		[InitializeOnLoadMethod]
		private static void Init()
		{
			if (IsSupported)
			{
				UIElementsUtility_Internals.BeginContainerCallback += container =>
				{
					if (imGUIElementDrawerLookup.TryGetValue(container, out var drawer))
						drawer.BeginContainerCallback();
				};

				UIElementsUtility_Internals.EndContainerCallback += container =>
				{
					if (imGUIElementDrawerLookup.TryGetValue(container, out var drawer))
						drawer.ProcessElements();
				};
			}
		}

		private class CallbackInvokerContext
		{
			public Action<SerializedProperty> Action;
		}

		public static void RegisterSerializedPropertyChangeEventCallback(PropertyField propertyField, Action<SerializedProperty> callback)
		{
			if (!IsSupported) throw new NotSupportedException(NotSupportedMessage);

			CallbackInvokerContext ctx = new CallbackInvokerContext() { Action = callback };
			var del = EmittedCallbackInvoker.CreateDelegate(EventCallback_SerializedPropertyChangeEvent_Type, ctx);
			CallbackEventHandler_RegisterCallback_SerializedPropertyChangeEvent_Method.Invoke(propertyField, new object[] { del, TrickleDown.NoTrickleDown });
		}

		public static Rect EmbedVisualElementAndDrawItHere(OdinImGuiElement element, GUIContent label)
		{
			if (!IsSupported) throw new NotSupportedException(NotSupportedMessage);

			var rect = EmbedVisualElementAndDrawItHere(element);

			if (Event.current.type == EventType.Layout)
			{
				if (element.LabelElement != null)
				{
					var display = label == null || label.text == "" ? DisplayStyle.None : DisplayStyle.Flex;

					if (element.CachedDisplay != display)
					{
						element.LabelElement.style.display = display;
						element.CachedDisplay = display;
					}

					if (display == DisplayStyle.Flex)
					{
						var text = label?.text ?? "";
						var tooltip = label?.tooltip ?? "";

						if (element.CachedText != text)
						{
							element.LabelElement.text = text;
							element.CachedText = text;
						}

						if (element.CachedTooltip != tooltip)
						{
							element.LabelElement.tooltip = tooltip;
							element.CachedTooltip = tooltip;
						}
					}
				}
			}

			return rect;
		}

		public static Rect EmbedVisualElementAndDrawItHere(OdinImGuiElement element)
		{
			if (!IsSupported) throw new NotSupportedException(NotSupportedMessage);

			var imGuiContainer = UIElementsUtility_Internals.GetCurrentIMGUIContainer();

			if (imGuiContainer == null)
			{
				Debug.LogError("You are not allowed to embed visual elements into IMGUI outside of an IMGUI container draw call.");
				return default;
			}

			if (!imGUIElementDrawerLookup.TryGetValue(imGuiContainer, out var drawer))
			{
				if (Event.current.type == EventType.Layout)
				{
					drawer = new ImGUIElementDrawer(imGuiContainer);
					imGUIElementDrawerLookup[imGuiContainer] = drawer;
				}
				else
				{
					return default;
				}
			}

			element.LastImGUIContainer = imGuiContainer;

			if (drawer.CurrentIndex == drawer.ToDraw.Count)
			{
				drawer.ToDraw.Add(element);
				drawer.ToDrawListChanged();
			}
			else if (drawer.ToDraw[drawer.CurrentIndex] != element)
			{
				drawer.ToDraw[drawer.CurrentIndex] = element;
				drawer.ToDrawListChanged();
			}

			drawer.CurrentIndex++;

			var rect = ImguiElementUtils.GetImGUILayoutRectForElement(element);

			if (Event.current.type == EventType.Repaint)
			{
				var alpha = GUI.color.a;
				var enabled = GUI.enabled;

				if (element.CachedOpacity != alpha)
					element.PositioningContainer.style.opacity = alpha;

				if (element.CachedEnabled != enabled)
				{
					element.CachedEnabled = enabled;
					element.SetEnabled(enabled);
				}

				element.ClipRect = GUIClipInfo.TopMostRect;
			}

			return rect;
		}

		static ScriptableObject owner;
		static Panel_Internal panel;
		static VisualElement visualTree;

		static void EnsureInit()
		{
			if (panel.IPanel == null)
			{
				owner = ScriptableObject.CreateInstance<ScriptableObject>();
				panel = Panel_Internals.CreateEditorPanel(owner);
				visualTree = panel.IPanel.visualTree;

				visualTree.style.left = 0;
				visualTree.style.top = 0;
				visualTree.style.width = 100;
				visualTree.style.height = 100;
				visualTree.style.minHeight = 100;
				visualTree.style.position = Position.Absolute;
				visualTree.style.top = 0;
				visualTree.style.left = 0;
				visualTree.style.right = 0;
				visualTree.style.bottom = 0;
				visualTree.style.backgroundColor = Color.clear;
			}
		}

		public static IEnumerable<StyleSheet> GetAllStyleSheets(VisualElement e)
		{
			if (!IsSupported) throw new NotSupportedException(NotSupportedMessage);
			for (; e != null; e = e.parent)
				for (int i = 0; i < e.styleSheets.count; i++)
					yield return e.styleSheets[i];
		}

		public static Rect MeasureSizeOfVisualElement(VisualElement targetContainer, VisualElement element, float width)
		{
			if (!IsSupported) throw new NotSupportedException(NotSupportedMessage);
			//return new Rect(0, 0, width, 30);
			EnsureInit();

			if (panel.duringLayoutPhase)
			{
				Debug.LogError("TODO: Support recursive MeasureNow() (or don't) which could happen in UpdateVisualTreePhaseLayout()");
				return new Rect(0, 0, width, 30);
			}

			//if (visualTree.styleSheets.count == 0)
			{
				visualTree.styleSheets.Clear();
				foreach (var item in GetAllStyleSheets(targetContainer))
					visualTree.styleSheets.Add(item);
				panel.ApplyStyles();
			}

			visualTree.Add(element);
			visualTree.style.width = width;
			panel.VisualTreeSetSize(new Vector2(width, 1000));

			{
				//m_VisualTreeUpdater.UpdateVisualTreePhase(VisualTreeUpdatePhase.ViewData);
				//m_VisualTreeUpdater.UpdateVisualTreePhase(VisualTreeUpdatePhase.Bindings);
				//m_VisualTreeUpdater.UpdateVisualTreePhase(VisualTreeUpdatePhase.Animation);
				//m_VisualTreeUpdater.UpdateVisualTreePhase(VisualTreeUpdatePhase.Styles);
				//m_VisualTreeUpdater.UpdateVisualTreePhase(VisualTreeUpdatePhase.Layout);

				panel.UpdateVisualTreePhaseViewData();
				panel.UpdateVisualTreePhaseBindings();
				panel.UpdateVisualTreePhaseAnimation();
				panel.UpdateVisualTreePhaseStyles();
				panel.UpdateVisualTreePhaseLayout();
			}
			var rect = element.contentRect;
			visualTree.Clear();

			return rect;
		}

		public static Rect GetImGUILayoutRectForElement(OdinImGuiElement element)
		{
			if (!IsSupported) throw new NotSupportedException(NotSupportedMessage);
			var e = Event.current.type;
			//element.Matrix = GUI.matrix;

			if (e == EventType.Layout)
			{
				var entry = ImGuiVisualElementLayoutEntry.Create(element);
				GUILayoutUtility_Internals.TopLevel.Add(entry);

				element.ParentLayoutGroups.Clear();
				foreach (var item in GUILayoutUtility_Internals.Current.LayoutGroups)
					element.ParentLayoutGroups.Add(item);

				element.ScrollViewStates.Clear();
				foreach (var item in GUI_Internals.ScrollViewStates)
					element.ScrollViewStates.Add(item);

				return default;
			}
			else if (e == EventType.Used)
			{
				return new Rect();
			}
			else
			{
				return GUILayoutUtility_Internals.TopLevel.GetNext().rect;
			}
		}

		public static class ImGuiVisualElementLayoutEntry
		{
			public static GUILayoutEntry_Internal<OdinImGuiElement> Create(OdinImGuiElement element)
			{
				return GUILayoutEntry_Internal<OdinImGuiElement>.CreateCustom(element, SetVertical, SetHorizontal, CalcWidth, CalcHeight);
			}

			private static void CalcWidth(ref GUILayoutEntry_Internal<OdinImGuiElement> entry) { }

			private static void CalcHeight(ref GUILayoutEntry_Internal<OdinImGuiElement> entry)
			{
			}

			private static void SetVertical(ref GUILayoutEntry_Internal<OdinImGuiElement> entry, float y, float height)
			{
				entry.rect.y = y;
				Update(ref entry);
			}

			private static void SetHorizontal(ref GUILayoutEntry_Internal<OdinImGuiElement> entry, float x, float width)
			{
				entry.rect.x = x;
				entry.minWidth = width;
				entry.maxWidth = width;
				Update(ref entry);
			}

			private static void Update(ref GUILayoutEntry_Internal<OdinImGuiElement> entry)
			{
				var size = entry.Value.GetSize(entry.minWidth);
				entry.maxHeight = size.y;
				entry.minHeight = size.y;
				entry.rect.width = entry.minWidth;
				entry.rect.height = entry.minHeight;
				entry.Value.RectAtPostLayoutTime = entry.rect;
			}
		}

		private class ImGUIElementDrawer
		{
			private bool toDrawListChanged;
			public int CurrentIndex;
			public Panel_Internal Panel;
			public IMGUIContainer ImGuiContainer;
			public VisualElement ElementContainer;
			public List<OdinImGuiElement> ToDraw = new List<OdinImGuiElement>();
			public HashSet<OdinImGuiElement> CurrentElements = new HashSet<OdinImGuiElement>();

			public ImGUIElementDrawer(IMGUIContainer container)
			{
				if (!IsSupported) throw new NotSupportedException(NotSupportedMessage);

				this.ImGuiContainer = container;
				this.ImGuiContainer.RegisterCallback<GeometryChangedEvent>(x =>
				{
					this.Hook();
					this.ProcessElements();
				});
				this.Panel = new Panel_Internal(container.panel);
			}

			public void BeginContainerCallback()
			{
				this.CurrentIndex = 0;
			}

			public void ProcessElements()
			{
				if (this.ElementContainer != null && this.ElementContainer.hierarchy.childCount != this.CurrentIndex)
				{
					this.toDrawListChanged = true;
				}

				// Add new elements!
				if (this.toDrawListChanged && (Event.current == null || Event.current.type != EventType.Layout))
				{
					Hook();

					this.ToDraw.SetLength(this.CurrentIndex);

					for (int k = 0; k < 3; k++)
					{
						var changed = false;

						for (int i = 0; i < this.ElementContainer.hierarchy.childCount && i < this.ToDraw.Count; i++)
						{
							if (this.ElementContainer.hierarchy[i] != ToDraw[i])
							{
								var needsAdd = ToDraw[i].hierarchy.parent != this.ElementContainer;
								if (needsAdd)
								{
									this.ElementContainer.Insert(i, ToDraw[i]);
									this.ToDraw[i].OnAdd();
									changed = true;
								}
								else
								{
									this.ElementContainer.RemoveAt(i);

									if (i >= this.ElementContainer.hierarchy.childCount || this.ElementContainer.hierarchy[i] != ToDraw[i])
									{
										this.ElementContainer.Insert(i, ToDraw[i]);
										this.ToDraw[i].OnAdd();
									}
									else
									{
										i--;
									}

									changed = true;
								}
							}
						}

#if SIRENIX_INTERNAL
						if (changed && k == 1)
						{
							Debug.Log("This should not happen. What is up with that.");
							break;
						}
#else
                        break;
#endif
					}

					for (int i = this.ToDraw.Count; i < this.ElementContainer.hierarchy.childCount; i++)
					{
						this.ElementContainer.RemoveAt(i);
					}

					for (int i = this.ElementContainer.hierarchy.childCount; i < this.ToDraw.Count; i++)
					{
						this.ElementContainer.Add(this.ToDraw[i]);
						this.ToDraw[i].OnAdd();
					}

					this.toDrawListChanged = false;
				}

				//if (Event.current.type == EventType.Layout)
				{
					for (int i = 0; i < this.CurrentIndex; i++)
					{
						var e = this.ToDraw[i];

						e.AbsoluteRect = e.RectAtPostLayoutTime;
						var layoutOffset = Vector2.zero;
						var viewOffset = Vector2.zero;
						var clipOffset = Vector2.zero;

						if (e.ClipRect != null)
						{
							clipOffset += e.ClipRect.Value.position;
						}

						for (int j = 0; j < e.ParentLayoutGroups.Count; j++)
						{
							var g = e.ParentLayoutGroups[j];
							if (g.resetCoords)
								layoutOffset += g.rect.position;
						}

						for (int j = 0; j < e.ScrollViewStates.Count; j++)
						{
							var g = e.ScrollViewStates[j];
							viewOffset += g.scrollPosition;
						}

						e.AbsoluteRect.position += layoutOffset - viewOffset - clipOffset;

						{
							var left = e.style.left.value.value;
							var top = e.style.top.value.value;
							var width = e.style.width.value.value;
							var height = e.style.height.value.value;

							//if (e.ClipRect != null/* && e.CurrClip != e.ClipRect.Value*/)
							if (e.ClipRect != null && (left != e.ClipRect.Value.x || top != e.ClipRect.Value.y || width != e.ClipRect.Value.width || height != e.ClipRect.Value.height))
							{
								e.style.left = e.ClipRect.Value.x;
								e.style.top = e.ClipRect.Value.y;
								e.style.width = e.ClipRect.Value.width;
								e.style.height = e.ClipRect.Value.height;
								e.CurrClip = e.ClipRect.Value;
							}
						}

						{
							//if (e.PrevAbsoluteRect != e.AbsoluteRect)
							var left = e.PositioningContainer.style.left;
							var top = e.PositioningContainer.style.top;
							var width = e.PositioningContainer.style.width;

							if (left != e.AbsoluteRect.x || top != e.AbsoluteRect.y || width != e.AbsoluteRect.width)
							{
								//var offset = e.Matrix.GetPosition();
								e.PositioningContainer.style.left = e.AbsoluteRect.x;
								e.PositioningContainer.style.top = e.AbsoluteRect.y;
								e.PositioningContainer.style.width = e.AbsoluteRect.width;
							}
						}
					}
				}
			}

			public void ToDrawListChanged()
			{
				this.toDrawListChanged = true;
			}

			private void Hook()
			{
				if (this.ElementContainer == null)
				{
					this.ElementContainer = new VisualElement();
					this.ElementContainer.style.position = Position.Absolute;
					this.ElementContainer.style.left = 0;
					this.ElementContainer.style.top = 0;
					this.ElementContainer.style.width = Length.Percent(100);
					this.ElementContainer.style.height = Length.Percent(100);
					this.ElementContainer.style.overflow = Overflow.Hidden;
					this.ElementContainer.pickingMode = PickingMode.Ignore;

					this.ImGuiContainer.parent.Add(this.ElementContainer);
					this.ImGuiContainer.parent.RemoveFromClassList("unity-inspector-element");              // Removes silly label behaviour.
				}
			}

		}
	}
}
#endif