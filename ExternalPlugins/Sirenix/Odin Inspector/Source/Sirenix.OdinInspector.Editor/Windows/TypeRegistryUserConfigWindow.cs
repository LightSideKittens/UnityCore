//-----------------------------------------------------------------------
// <copyright file="TypeRegistryUserConfigWindow.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#if UNITY_EDITOR
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.Config;
using Sirenix.Reflection.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

// TODO: optimize, lots of easy optimization to do to make this fast.
namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

	public class TypeRegistryUserConfigWindow : OdinMenuEditorWindow
	{
		[Flags]
		internal enum FilterMode
		{
			None = 0,
			Visible = 1 << 0,
			Hidden = 1 << 1,
			Illegal = 1 << 2,
			Valid = Visible | Hidden,
			All = Valid | Illegal
		}

		internal class TypeRegistryMenuItem : OdinMenuItem
		{
			public readonly TypeItemSettingsAccessor Accessor;
			public readonly Type Type;

			private bool isInitialized;

			public TypeRegistryMenuItem(OdinMenuTree tree, string name, Type value) : base(tree, name, value)
			{
				this.Type = value;
				this.Accessor = new TypeItemSettingsAccessor(value);
				this.Value = this.Accessor;
			}

			public override void DrawMenuItem(int indentLevel)
			{
				if (!this.isInitialized)
				{
					this.Initialize();

					this.isInitialized = true;
				}

				bool isModified = this.Accessor.IsModified;

				if (isModified)
				{
					GUIHelper.PushIsBoldLabel(true);
				}

				Rect visibilityToggleRect = this.rect.AlignRight(20).SubX(this.MenuTree.Config.DefaultMenuStyle.BorderPadding).Padding(ICON_PADDING);

				if (Event.current.OnMouseDown(visibilityToggleRect, 0))
				{
					this.Accessor.Visible = !this.Accessor.Visible;
				}

				base.DrawMenuItem(indentLevel);

				if (isModified)
				{
					GUIHelper.PopIsBoldLabel();
				}

				if (!this.Accessor.Visible)
				{
					EditorGUI.DrawRect(this.rect, ItemHiddenOverlay);
				}

				SdfIconType visibleIcon = this.Accessor.Visible ? SdfIconType.EyeFill : SdfIconType.EyeSlashFill;

				if (Event.current.IsMouseOver(visibilityToggleRect))
				{
					SdfIcons.DrawIcon(visibilityToggleRect, visibleIcon, Color.white);
				}
				else
				{
					SdfIcons.DrawIcon(visibilityToggleRect, visibleIcon);
				}

				if (this.Accessor.IsIllegal)
				{
					EditorGUI.DrawRect(this.rect.AlignLeft(2), ItemIllegal);
				}
			}


			private void Initialize()
			{
				if (string.IsNullOrEmpty(this.Type.Namespace))
				{
					this.Name = $"{this.Type.GetNiceName()} ({this.Type.Assembly.GetName().Name})";
				}
				else
				{
					this.Name = this.Type.GetNiceName();
				}

				this.Accessor?.Initialize();
			}
		}

		internal class TypeItemSettingsAccessor
		{
			[HideInInspector]
			public bool IsRemoved;

			public bool IsModified => TypeRegistryUserConfig.Instance.IsModified(this.Type);

			[ShowInInspector]
			[DisableIf("@$property.ValueEntry.ValueCount > 1")]
			public string DisplayName
			{
				get
				{
					if (this.Settings == null)
					{
						return this.NiceName;
					}

					return string.IsNullOrEmpty(this.Settings.Name) ? this.NiceName : this.Settings.Name;
				}
				
				set
				{
					if (this.Settings is null)
					{
						this.Settings = new TypeSettings();
					}

					this.Settings.Name = value;
					TypeRegistryUserConfig.Instance.HandleDefaultSettings(this.Type, this.Settings, this.ItemAttribute);
				}
			}

			[ShowInInspector]
			public string Category
			{
				get
				{
					if (this.Settings != null && !string.IsNullOrEmpty(this.Settings.Category))
					{
						return this.Settings.Category;
					}

					if (this.ItemAttribute != null && !string.IsNullOrEmpty(this.ItemAttribute.CategoryPath))
					{
						return this.ItemAttribute.CategoryPath;
					}

					return string.Empty;
				}
				
				set
				{
					if (this.Settings is null)
					{
						this.Settings = new TypeSettings();
					}

					this.Settings.Category = value;
					TypeRegistryUserConfig.Instance.HandleDefaultSettings(this.Type, this.Settings, this.ItemAttribute);
				}
			}

			[ShowInInspector]
			[BoxGroup("IconGroup", LabelText = "Icon Settings")]
			public SdfIconType Icon
			{
				get
				{
					if (this.Settings != null && this.Settings.Icon != SdfIconType.None)
					{
						return this.Settings.Icon;
					}

					if (this.ItemAttribute != null && this.ItemAttribute.Icon != SdfIconType.None)
					{
						return this.ItemAttribute.Icon;
					}

					return SdfIconType.None;
				}
				
				set
				{
					if (this.Settings is null)
					{
						this.Settings = new TypeSettings();
					}

					this.Settings.Icon = value;
					TypeRegistryUserConfig.Instance.HandleDefaultSettings(this.Type, this.Settings, this.ItemAttribute);
				}
			}

			[ShowInInspector]
			[BoxGroup("IconGroup")]
			public Color LightModeColor
			{
				get => this.Settings?.LightIconColor ?? this.ItemAttribute?.LightIconColor ?? Color.clear;
				
				set
				{
					if (this.Settings is null)
					{
						this.Settings = new TypeSettings();
					}

					this.Settings.LightIconColor = value;
					TypeRegistryUserConfig.Instance.HandleDefaultSettings(this.Type, this.Settings, this.ItemAttribute);
				}
			}

			[ShowInInspector]
			[BoxGroup("IconGroup")]
			public Color DarkModeColor
			{
				get => this.Settings?.DarkIconColor ?? this.ItemAttribute?.DarkIconColor ?? Color.clear;
				
				set
				{
					if (this.Settings is null)
					{
						this.Settings = new TypeSettings();
					}

					this.Settings.DarkIconColor = value;
					TypeRegistryUserConfig.Instance.HandleDefaultSettings(this.Type, this.Settings, this.ItemAttribute);
				}
			}

			[ShowInInspector]
			public bool Visible
			{
				get => TypeRegistryUserConfig.Instance.IsVisible(this.Type);
				set => TypeRegistryUserConfig.Instance.SetVisibility(this.Type, value);
			}

			[ShowInInspector]
			public bool IsIllegal
			{
				get => TypeRegistryUserConfig.Instance.IsIllegal(this.Type);
				set => TypeRegistryUserConfig.Instance.SetIllegal(this.Type, value);
			}

			[ShowInInspector]
			public int Priority
			{
				get
				{
					int priority = TypeRegistryUserConfig.Instance.GetPriority(this.Type);

					if (priority != 0)
					{
						return priority;
					}

					if (this.ItemAttribute != null)
					{
						return this.ItemAttribute.Priority;
					}

					return 0;
				}

				set => TypeRegistryUserConfig.Instance.SetPriority(this.Type, value, this.ItemAttribute);
			}

			[EnableIf("@this." + nameof(CanReset) + "($property.Parent)")]
			[Button(ButtonSizes.Large)]
			public void Reset()
			{
				TypeRegistryUserConfig.Instance.ResetType(this.Type);
			}

			internal TypeSettings Settings
			{
				get => TypeRegistryUserConfig.Instance.TryGetSettings(this.Type);
				set => TypeRegistryUserConfig.Instance.SetSettings(this.Type, value);
			}

			internal Type Type;
			internal TypeRegistryItemAttribute ItemAttribute = null;
			internal string NiceName;

			internal TypeItemSettingsAccessor(Type type)
			{
				this.Type = type;

				this.ItemAttribute = type.GetCustomAttribute<TypeRegistryItemAttribute>();

				this.NiceName = type.Name;
			}

			internal bool CanReset(InspectorProperty property)
			{
				if (property == null)
				{
					return false;
				}

				for (var i = 0; i < property.ValueEntry.ValueCount; i++)
				{
					if (((TypeItemSettingsAccessor) property.ValueEntry.WeakValues[i]).IsModified)
					{
						return true;
					}
				}

				return false;
			}

			private bool isInitialized;

			[OnInspectorGUI]
			internal void Initialize()
			{
				if (this.isInitialized)
				{
					return;
				}

				if (this.ItemAttribute != null && !string.IsNullOrEmpty(this.ItemAttribute.Name))
				{
					this.NiceName = this.ItemAttribute.Name;
				}
				else
				{
					this.NiceName = this.Type.GetNiceName();
				}

				this.isInitialized = true;
			}
		}

#region Styling

		public const int TOOLBAR_HEIGHT = 28;
		public const int ITEM_HEIGHT = 24;
		public const float ICON_PADDING = 2.0f;

		public static Color Background => EditorGUIUtility.isProSkin ? BackgroundDark : BackgroundLight;
		public static Color Header => EditorGUIUtility.isProSkin ? HeaderDark : HeaderLight;
		public static Color ItemHiddenOverlay => EditorGUIUtility.isProSkin ? ItemHiddenOverlayDark : ItemHiddenOverlayLight;
		public static Color ItemIllegal => EditorGUIUtility.isProSkin ? ItemIllegalDark : ItemIllegalLight;

		private static readonly Color BackgroundLight = new Color(0.7607844f, 0.7607844f, 0.7607844f, 1.0f);
		private static readonly Color HeaderLight = new Color(0.8235295f, 0.8235295f, 0.8235295f, 1.0f);
		private static readonly Color ItemHiddenOverlayLight = new Color(0, 0, 0, 0.15f);
		private static readonly Color ItemIllegalLight = new Color(1.0f, 1.0f, 0.0f, 1.0f);

		private static readonly Color BackgroundDark = new Color(0.171f, 0.171f, 0.171f, 1.0f);
		private static readonly Color HeaderDark = new Color(0.2431373f, 0.2431373f, 0.2431373f, 1.0f);
		private static readonly Color ItemHiddenOverlayDark = new Color(0, 0, 0, 0.1803922f);
		private static readonly Color ItemIllegalDark = new Color(1, 1, 0, 1);

#endregion

		public static HashSet<Type> additionalTypes = new HashSet<Type>();
		public Type TypeToScrollTo = null;
		
		private OdinMenuItem selectedNamespaceItem;
		private string selectedNamespace;

		private FilterMode filterMode = FilterMode.Visible | FilterMode.Illegal;
		
		protected override void OnEnable()
		{
			base.OnEnable();
			this.filterMode = FilterMode.Visible | FilterMode.Illegal;
			this.minSize = new Vector2(960, 600);
			this.titleContent = new GUIContent("Type Registry Editor");
		}

		private void Awake()
		{
			this.filterMode = FilterMode.Visible | FilterMode.Illegal;
			this.minSize = new Vector2(960, 600);
		}

		protected override IEnumerable<object> GetTargets()
		{
			if (this.MenuTree.Selection.Count > 0)
			{
				if (this.MenuTree.Selection.SelectedValue == null)
				{
					yield return null;
				}
				else
				{
					yield return this.MenuTree.Selection.SelectedValues.ToList();
				}
			}
		}

		private Rect topRect;

		protected override void OnImGUI()
		{
			if (this.TypeToScrollTo != null && this.MenuTree != null)
			{
				foreach (OdinMenuItem odinMenuItem in this.MenuTree.EnumerateTree())
				{
					if (odinMenuItem is TypeRegistryMenuItem item)
					{
						if (this.TypeToScrollTo == item.Type)
						{
							this.MenuTree.Selection.Clear();

							odinMenuItem.Select();

							this.MenuTree.ScrollToMenuItem(odinMenuItem, true);

							break;
						}
					}
				}

				this.TypeToScrollTo = null;
			}
			
			float windowWidth = this.position.width;

			this.MenuWidth = Mathf.Clamp(this.MenuWidth, windowWidth - 500, windowWidth - 200);

			Rect topRectPlaceholder = GUILayoutUtility.GetRect(0, TOOLBAR_HEIGHT, GUILayoutOptions.ExpandWidth());

			if (!topRectPlaceholder.IsPlaceholder())
			{
				this.topRect = topRectPlaceholder;
			}

			EditorGUI.DrawRect(this.position.SetPosition(Vector2.zero).AlignLeft(this.MenuWidth), Background);

			base.OnImGUI();

			EditorGUI.DrawRect(this.topRect, Header);

			// left side of the Header
			{
				Rect leftTop = this.topRect.AlignLeft(this.MenuWidth + 1.0f);

				Rect namespaceSelectorRect = leftTop.TakeFromRight(240);
				Rect filterRect = leftTop.TakeFromRight(180);

				GUILayout.BeginArea(leftTop);
				{
					if (this.MenuTree != null)
					{
						this.MenuTree.DrawSearchToolbar();
					}
				}
				GUILayout.EndArea();

				OdinSelector<OdinMenuItem>.DrawSelectorDropdown(namespaceSelectorRect,
																				this.selectedNamespaceItem?.Name ?? "All Namespaces",
																				this.NamespaceCategorySelector,
																				EditorStyles.toolbarDropDown);

				EditorGUI.BeginChangeCheck();

				this.filterMode = EnumSelector<FilterMode>.DrawEnumField(filterRect,
																							null,
																							GUIHelper.TempContent($"Show: {this.filterMode}"),
																							this.filterMode,
																							EditorStyles.toolbarDropDown);

				if (EditorGUI.EndChangeCheck())
				{
					this.RebuildMenuTree();
				}
			}

			// right side of the Header
			{
				var topRight = this.topRect.AlignRight(this.topRect.width - this.MenuWidth);

				topRight = topRight.Padding(4);

				Rect iconPosition = topRight.TakeFromLeft(30);

				if (this.MenuTree.Selection.Count > 0 && this.MenuTree.Selection[0] is TypeRegistryMenuItem item)
				{
					if (typeof(UnityEngine.Object).IsAssignableFrom(item.Type))
					{
						GUI.DrawTexture(iconPosition, EditorIcons.UnityLogo, ScaleMode.ScaleToFit);
					}
					else
					{
						SdfIcons.DrawIcon(iconPosition, SdfIconType.PuzzleFill);
					}
				}
				else
				{
					SdfIcons.DrawIcon(iconPosition, SdfIconType.PuzzleFill);
				}

				topRight.width -= 30;

				if (this.MenuTree.Selection.Count == 1)
				{
					GUI.Label(topRight, this.MenuTree.Selection[0].Name, SirenixGUIStyles.TitleCentered);
				}
				else if (this.MenuTree.Selection.Count > 1)
				{
					GUI.Label(topRight, $"{this.MenuTree.Selection[0].Name} (+{this.MenuTree.Selection.Count - 1})", SirenixGUIStyles.TitleCentered);
				}
				else
				{
					GUI.Label(topRight, "None", SirenixGUIStyles.TitleCentered);
				}
			}
		}

		private void RebuildMenuTree()
		{
			this.MenuTree?.Selection?.Clear();

			this.ForceMenuTreeRebuild();
		}

		protected override OdinMenuTree BuildMenuTree()
		{
			var tree = new OdinMenuTree
			{
				Selection =
				{
					SupportsMultiSelect = true
				},
				Config = new OdinMenuTreeDrawingConfig
				{
					EXPERIMENTAL_INTERNAL_SparseFixedLayouting = true,
					DrawSearchToolbar = false,
					DefaultMenuStyle =
					{
						IconPadding = ICON_PADDING,
						Height = ITEM_HEIGHT
					}
				}
			};

			if (this.TypeToScrollTo != null)
			{
				if (!TypeRegistryUserConfig.Instance.IsVisible(this.TypeToScrollTo))
				{
					this.filterMode = FilterMode.All;
				}
			}

			this.WindowPadding = new Vector4(8, 8, 8, 8);

			bool validateNamespace = !string.IsNullOrEmpty(this.selectedNamespace);

			// TODO ensure when you right-click && customize type it checks for this whether to use a disabled button or not
			foreach (Type type in AssemblyUtilities.GetTypes(AssemblyCategory.All).Concat(additionalTypes)) //TypeRegistry.ValidTypes)
			{
				if (!TypeRegistry.IsModifiableType(type) && !additionalTypes.Contains(type))
				{
					continue;
				}

				if (!this.IsTypeVisible(type))
				{
					continue;
				}

				if (validateNamespace)
				{
					if (string.IsNullOrEmpty(type.Namespace))
					{
						continue;
					}

					if (!type.Namespace.StartsWith(this.selectedNamespace))
					{
						continue;
					}
				}

				TypeRegistryMenuItem item = AddTypeToTree(tree, type);

				if (this.TypeToScrollTo != null)
				{
					if (type == this.TypeToScrollTo && item != null)
					{
						item.Select();
					}
				}
			}

			this.TypeToScrollTo = null;
			
			tree.CollapseEmptyItems();

			tree.SortMenuItemsByName();

			foreach (OdinMenuItem item in tree.EnumerateTree())
			{
				if (item.Value == null)
				{
					item.SdfIcon = SdfIconType.CollectionFill;
					item.IsSelectable = false;

					item.OnDrawItem += menuItem =>
					{
						if (Event.current.OnMouseDown(menuItem.rect, 0))
						{
							menuItem.Toggled = !menuItem.Toggled;
						}
					};
				}
			}

			tree.UpdateMenuTree();

			return tree;
		}

		private static string GetFullNameWithoutSlashes(OdinMenuItem item)
		{
			OdinMenuItem parent = item.Parent;

			string result = item.Name.Replace('/', '.');

			while (parent != null)
			{
				result = $"{parent.Name.Replace('/', '.')}/{result}";

				parent = parent.Parent;
			}

			return result;
		}

		private static string GetFullNameAsNamespace(OdinMenuItem item)
		{
			OdinMenuItem parent = item.Parent;

			string result = item.Name;

			while (parent != null)
			{
				result = $"{parent.Name}/{result}";

				parent = parent.Parent;
			}

			result = result.Replace('/', '.');

			return result;
		}


		private OdinSelector<OdinMenuItem> NamespaceCategorySelector(Rect rect)
		{
			var result = new GenericSelector<OdinMenuItem>();

			result.SelectionTree.Add("<All>", null);

			foreach (OdinMenuItem item in this.MenuTree.EnumerateTree())
			{
				if (item.ChildMenuItems.Count > 0)
				{
					result.SelectionTree.Add(GetFullNameWithoutSlashes(item), item);
				}
			}

			foreach (OdinMenuItem item in result.SelectionTree.EnumerateTree())
			{
				item.Name = item.Name.Replace('.', '/');
			}

			result.SelectionConfirmed += enumerable =>
			{
				OdinMenuItem item = enumerable.FirstOrDefault();

				this.selectedNamespaceItem = item;

				this.selectedNamespace = item == null ? string.Empty : GetFullNameAsNamespace(item);

				this.RebuildMenuTree();
			};

			result.ShowInPopup(rect);

			return result;
		}

		private bool IsTypeVisible(Type type)
		{
			TypeRegistryUserConfig userConfig = TypeRegistryUserConfig.Instance;

			switch (this.filterMode)
			{
				case FilterMode.None:
					return false;

				case FilterMode.Visible:
					return userConfig.IsVisible(type);

				case FilterMode.Hidden:
					return !userConfig.IsVisible(type);

				case FilterMode.Illegal:
					return userConfig.IsIllegal(type);

				case FilterMode.Illegal | FilterMode.Visible:
					return userConfig.IsVisible(type) || userConfig.IsIllegal(type);

				case FilterMode.Illegal | FilterMode.Hidden:
					return !userConfig.IsVisible(type) || userConfig.IsIllegal(type);

				case FilterMode.Valid:
					return !userConfig.IsIllegal(type);

				case FilterMode.All:
					return true;

				default:
					throw new ArgumentOutOfRangeException(nameof(this.filterMode), this.filterMode, null);
			}
		}

		private static TypeRegistryMenuItem AddTypeToTree(OdinMenuTree tree, Type type)
		{
			if (TypeRegistry.IsGeneratedType(type))
			{
				return null;
			}

			string name = type.Name;

			var item = new TypeRegistryMenuItem(tree, name, type)
			{
				SearchString = $"{name}, {type.FullName}"
			};

			if (typeof(UnityEngine.Object).IsAssignableFrom(type))
			{
				item.Icon = EditorIcons.UnityLogo;
			}
			else
			{
				item.SdfIcon = SdfIconType.PuzzleFill;
			}

			if (!string.IsNullOrEmpty(type.Namespace))
			{
				string path = TypeRegistry.GetNamespacePath(type);

				tree.AddMenuItemAtPath(path, item);
			}
			else
			{
				item.Name = $"{name} ({type.Assembly.GetName().Name})";

				tree.MenuItems.Add(item);
			}

			return item;
		}
	}
}
#endif