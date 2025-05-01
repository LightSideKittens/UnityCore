//-----------------------------------------------------------------------
// <copyright file="TypeSelectorV2.cs" company="Sirenix ApS">
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

	using System;
	using System.Collections.Generic;
	using Sirenix.Utilities;
	using UnityEngine;
	using Sirenix.Utilities.Editor;
	using UnityEditor;
	using Sirenix.Config;
	using Sirenix.OdinInspector.Editor.ValueResolvers;

	[HideReferenceObjectPicker]
	public class TypeSelectorV2 : OdinSelector<Type>
	{
		private const string FIND_UNITY_OBJECT_ITEM_NAME = "Find Unity Object";

		internal class TypeSelectorMenuItem : OdinMenuItem
		{
			public string GetPath()
			{
				string result = this.Name;

				OdinMenuItem nextParent = this.Parent;

				while (nextParent != null)
				{
					result = nextParent.Name + '/' + result;
					nextParent = nextParent.Parent;
				}

				return result;
			}

			public bool HasDefaultConstructor;
			public TypeSelectorV2 CurrentSelector;
			public readonly Type Type;

			private bool isInitialized;

			public TypeSelectorMenuItem(TypeSelectorV2 selector, OdinMenuTree tree, string name, Type value) : base(tree, name, value)
			{
				this.CurrentSelector = selector;
				this.SearchString = $"{name}, {value.FullName}";
				this.Type = value;
			}

			public override void DrawMenuItem(int indentLevel)
			{
				if (!this.isInitialized)
				{
					this.Initialize();

					this.isInitialized = true;
				}

				base.DrawMenuItem(indentLevel);

				var showNoDefaultCtorTooltip = false;
				bool isIllegal = TypeRegistryUserConfig.Instance.IsIllegal(this.Type);

				if (!this.Type.IsInterface && !this.HasDefaultConstructor)
				{
					Rect defaultCtorPosition = this.rect.AlignLeft(this.rect.height);

					SdfIcons.DrawIcon(defaultCtorPosition.Padding(6, 8, 6, 6), SdfIconType.Exclamation);

					showNoDefaultCtorTooltip = true;
				}

				if (isIllegal)
				{
					EditorGUI.DrawRect(this.rect, new Color(1, 1, 0, 0.05f));
				}

				if (isIllegal && showNoDefaultCtorTooltip)
				{
					GUI.Label(this.rect, GUIHelper.TempContent(string.Empty, "This type is illegal, and has no default constructor."));
				}
				else if (isIllegal)
				{
					GUI.Label(this.rect, GUIHelper.TempContent(string.Empty, "This type is illegal."));
				}
				else if (showNoDefaultCtorTooltip)
				{
					GUI.Label(this.rect, GUIHelper.TempContent(string.Empty, "This type has no default constructor."));
				}
			}

			private void Initialize()
			{
				if (this.CurrentSelector.duplicatePaths.Contains(this.GetPath()))
				{
					if (this.CurrentSelector.ShowCategories)
					{
						if (TypeRegistry.HasCustomName(this.Type))
						{
							this.Name = $"{TypeRegistry.GetNiceName(this.Type)} ({this.Type.Name})";
						}
						else
						{
							this.Name = $"{TypeRegistry.GetNiceName(this.Type)} ({this.Type.Assembly.GetName().Name}, {this.Type.Namespace})";
						}
					}
					else
					{
						if (TypeRegistry.HasCustomName(this.Type))
						{
							this.Name = $"{TypeRegistry.GetNiceName(this.Type)} ({this.Type.FullName})";
						}
						else
						{
							this.Name = $"{TypeRegistry.GetNiceName(this.Type)} ({this.Type.Assembly.GetName().Name}, {this.Type.Namespace})";
						}
					}
				}
				else
				{
					this.Name = TypeRegistry.GetNiceName(this.Type);
				}

				if (this.CurrentSelector.HideNonDefaultCtorInfo)
				{
					this.HasDefaultConstructor = true;
				}
				else
				{
					if (this.Type.IsInterface || this.Type.IsAbstract)
					{
						this.HasDefaultConstructor = true;
					}
					else
					{
						this.HasDefaultConstructor = this.Type.HasDefaultConstructor();
					}
				}

				TypeRegistryUserConfig userConfig = TypeRegistryUserConfig.Instance;

				if (userConfig.IsIllegal(this.Type))
				{
					this.SdfIcon = SdfIconType.ExclamationTriangleFill;
					this.SdfIconColor = SirenixGUIStyles.YellowWarningColor;
					return;
				}

				bool isUnityType = typeof(UnityEngine.Object).IsAssignableFrom(this.Type);

				bool hasIcon = TypeRegistry.TryGetIcon(this.Type, out SdfIconType icon, out Color? iconColor);

				if (hasIcon)
				{
					this.SdfIcon = icon;
					this.SdfIconColor = iconColor;
					return;
				}

				if (!isUnityType)
				{
					this.SdfIcon = SdfIconType.PuzzleFill;
				}
				else
				{
					this.Icon = GUIHelper.GetAssetThumbnail(null, this.Type, false);

					if (this.Icon == null)
					{
						this.Icon = EditorIcons.UnityLogo;
					}
				}
			}
		}

		internal abstract class TypeSelectorNoneValue { }

		internal abstract class TypeSelectorAllUnityTypes { }

		[HideInInspector]
		public bool SupportsMultiSelect;

		[HideInInspector]
		public Type SelectedType;

		[HideInInspector]
		public bool ShowNoneItem;

		[HideInInspector]
		public bool ShowCategories;

		[HideInInspector]
		public bool PreferNamespaces;

		[HideInInspector]
		public bool ShowHiddenTypes;

		public override string Title => null;

		internal bool CategorizeUnityObjects = false;

		internal bool HideNonDefaultCtorInfo = true;

		internal bool useSingleClick = false;

		private OdinMenuItem noneMenuItem;
		private OdinMenuItem findUnityObjectItem;
		private ValueResolver<bool> filterItemsFunction = null;
		private IEnumerable<Type> types;

		public TypeSelectorV2(AssemblyCategory assemblyCategory,
									 bool supportsMultiSelect = false,
									 Type selectedType = null,
									 bool? showCategories = null,
									 bool showHidden = false,
									 bool? preferNamespaces = null,
									 bool? showNoneItem = null) : this(supportsMultiSelect,
																				  selectedType,
																				  showCategories,
																				  showHidden,
																				  preferNamespaces,
																				  showNoneItem,
																				  null)
		{
			this.types = TypeRegistry.GetValidTypesInCategory(assemblyCategory);
		}

		public TypeSelectorV2(IEnumerable<Type> types,
									 bool supportsMultiSelect = false,
									 Type selectedType = null,
									 bool? showCategories = null,
									 bool showHidden = false,
									 bool? preferNamespaces = null,
									 bool? showNoneItem = null) : this(supportsMultiSelect,
																				  selectedType,
																				  showCategories,
																				  showHidden,
																				  preferNamespaces,
																				  showNoneItem,
																				  null)
		{
			this.types = types;
		}

		internal TypeSelectorV2(AssemblyCategory assemblyCategory,
										bool supportsMultiSelect,
										Type selectedType,
										bool? showCategories,
										bool showHidden,
										bool? preferNamespaces,
										bool? showNoneItem,
										InspectorProperty property) : this(supportsMultiSelect,
																					  selectedType,
																					  showCategories,
																					  showHidden,
																					  preferNamespaces,
																					  showNoneItem,
																					  property)
		{
			this.types = TypeRegistry.GetValidTypesInCategory(assemblyCategory);
		}

		internal TypeSelectorV2(IEnumerable<Type> types,
										bool supportsMultiSelect,
										Type selectedType,
										bool? showCategories,
										bool showHidden,
										bool? preferNamespaces,
										bool? showNoneItem,
										InspectorProperty property) : this(supportsMultiSelect,
																					  selectedType,
																					  showCategories,
																					  showHidden,
																					  preferNamespaces,
																					  showNoneItem,
																					  property)
		{
			this.types = types;
		}

		protected TypeSelectorV2(bool supportsMultiSelect,
										 Type selectedType,
										 bool? showCategories,
										 bool showHidden,
										 bool? preferNamespaces,
										 bool? showNoneItem,
										 InspectorProperty property)
		{
			this.SupportsMultiSelect = supportsMultiSelect;
			this.SelectedType = selectedType;
			this.ShowNoneItem = showNoneItem ?? GeneralDrawerConfig.Instance.showNoneItem;
			this.ShowCategories = showCategories ?? GeneralDrawerConfig.Instance.showCategoriesByDefault;
			this.ShowHiddenTypes = showHidden;
			this.PreferNamespaces = preferNamespaces ?? GeneralDrawerConfig.Instance.preferNamespacesOverAssemblyCategories;

			var settings = property?.GetAttribute<TypeSelectorSettingsAttribute>();

			if (settings != null)
			{
				if (settings.ShowCategoriesIsSet)
				{
					this.ShowCategories = settings.ShowCategories;
				}

				if (settings.PreferNamespacesIsSet)
				{
					this.PreferNamespaces = settings.PreferNamespaces;
				}

				if (settings.ShowNoneItemIsSet)
				{
					this.ShowNoneItem = settings.ShowNoneItem;
				}

				if (!string.IsNullOrEmpty(settings.FilterTypesFunction))
				{
					var typeNamedValue = new NamedValue(TypeSelectorSettingsAttribute.FILTER_TYPES_FUNCTION_NAMED_VALUE, typeof(Type), null);

					this.filterItemsFunction = ValueResolver.Get<bool>(property, settings.FilterTypesFunction, typeNamedValue);

					if (this.filterItemsFunction.HasError)
					{
						Debug.LogWarning(this.filterItemsFunction.ErrorMessage);

						this.filterItemsFunction = null;
					}
				}
			}
		}

		public OdinEditorWindow ShowInAux()
		{
			var windowSize = new Vector2(700, 500);

			Rect windowRect = GUIHelper.GetEditorWindowRect();

			OdinEditorWindow window = this.ShowInPopup();

			window.position = new Rect(windowRect.center - windowSize * 0.5f, new Vector2(windowSize.x, window.position.height));

			Vector2 minSize = window.minSize;
			Vector2 maxSize = window.maxSize;

			window.minSize = new Vector2(windowSize.x, minSize.y);
			window.maxSize = new Vector2(windowSize.x, maxSize.y);

			return window;
		}

		protected override float DefaultWindowWidth()
		{
			return 450;
		}

		// NOTE: These should all be reset on BuildSelectionTree
		private readonly Dictionary<string, OdinMenuItem> categories = new Dictionary<string, OdinMenuItem>();
		private readonly List<OdinMenuItem> pathCache = new List<OdinMenuItem>(8);
		private readonly HashSet<string> duplicatePaths = new HashSet<string>();
		private readonly HashSet<string> addedNamesForFlatTree = new HashSet<string>();

		protected override void BuildSelectionTree(OdinMenuTree tree)
		{
			this.categories.Clear();
			this.pathCache.Clear();
			this.duplicatePaths.Clear();
			this.addedNamesForFlatTree.Clear();

			tree.Config.SelectMenuItemsOnMouseDown = true;
			tree.Config.EXPERIMENTAL_INTERNAL_SparseFixedLayouting = true;
			tree.Selection.SupportsMultiSelect = this.SupportsMultiSelect;

#if true
			if (this.CategorizeUnityObjects)
			{
				this.findUnityObjectItem = new OdinMenuItem(tree, FIND_UNITY_OBJECT_ITEM_NAME, typeof(TypeSelectorAllUnityTypes));

				tree.MenuItems.Add(this.findUnityObjectItem);

				this.categories[FIND_UNITY_OBJECT_ITEM_NAME] = this.findUnityObjectItem;
			}
			else
			{
				this.findUnityObjectItem = null;
			}
#endif

			bool hasFilterFunction = this.filterItemsFunction != null;

#if false
            TypeInclusionFilterUtility.Deconstruct(this.Filter,
                                                   out bool includeConcreteTypes,
                                                   out bool includeGenerics,
                                                   out bool includeAbstracts,
                                                   out bool includeInterfaces,
                                                   out bool includeGenerated);
#endif

			foreach (Type type in this.types)
			{
				if (hasFilterFunction)
				{
					this.filterItemsFunction.Context.NamedValues.Set(TypeSelectorSettingsAttribute.FILTER_TYPES_FUNCTION_NAMED_VALUE, type);

					if (!this.filterItemsFunction.GetValue())
					{
						continue;
					}
				}

				this.AddType(tree, type);
			}

			if (this.ShowCategories)
			{
				tree.AssignIconToEmptyItems(SdfIconType.FolderFill);
			}

			this.SortItemsByPriorityAndName();

			if (this.ShowCategories)
			{
				tree.CollapseEmptyItems();
			}

			if (this.findUnityObjectItem != null)
			{
				this.findUnityObjectItem.Icon = EditorIcons.UnityLogo;
				this.findUnityObjectItem.SdfIcon = SdfIconType.None;
			}

			tree.UpdateMenuTree();

			if (this.SelectedType != null)
			{
				this.SetSelection(this.SelectedType);
			}

			this.noneMenuItem = new OdinMenuItem(tree, "None", typeof(TypeSelectorNoneValue))
			{
				Icon = EditorIcons.Transparent.Raw
			};

#if false
            string unityInterfaceName = this.BaseType != null ? $"Unity Object types that implement {TypeRegistry.GetNiceName(this.BaseType)}" : "Unity types";

            this.unityInterfaceSelectorItem = new OdinMenuItem(tree, unityInterfaceName, typeof(TypeSelectorAllUnityTypes))
            {
                Icon = EditorIcons.Transparent.Raw
            };
#endif

			this.noneMenuItem.UpdateMenuTreeRecursive();

			if (!this.SupportsMultiSelect && this.useSingleClick)
			{
				this.noneMenuItem.OnDrawItem += menuItem =>
				{
					if (Event.current.OnMouseDown(menuItem.rect, 0))
					{
						menuItem.Select();
						tree.Selection.ConfirmSelection();
					}
				};

				if (this.CategorizeUnityObjects)
				{
					this.findUnityObjectItem.OnDrawItem += menuItem =>
					{
						var clickableRect = menuItem.rect;

						if (menuItem.Style.AlignTriangleLeft)
						{
							clickableRect.xMin += menuItem.Style.TrianglePadding + menuItem.Style.TriangleSize;
						}
						else
						{
							clickableRect.width -= menuItem.Style.TrianglePadding + menuItem.Style.TriangleSize;
						}

						if (Event.current.OnMouseDown(clickableRect, 0))
						{
							menuItem.Select();
							tree.Selection.ConfirmSelection();
						}
					};
				}
			}
		}

		private Rect specialItemsRect = Rect.zero;

		protected override void DrawSelectionTree()
		{
			var rect = EditorGUILayout.BeginVertical();
			{
				EditorGUI.DrawRect(rect, SirenixGUIStyles.DarkEditorBackground);
				GUILayout.Space(1);

				DrawToolbar();

				var prev = this.SelectionTree.Config.DrawSearchToolbar;
				this.SelectionTree.Config.DrawSearchToolbar = false;

				try
				{
					var desiredSpecialItemHeight = 0.0f;

					if (this.ShowNoneItem)
					{
						desiredSpecialItemHeight += this.SelectionTree.DefaultMenuStyle.Height + 4.0f;
					}

					Rect tmpSpecialItemsRect = GUILayoutUtility.GetRect(0, desiredSpecialItemHeight);

					if (!tmpSpecialItemsRect.IsPlaceholder())
					{
						this.specialItemsRect = tmpSpecialItemsRect;
					}

					if (this.SelectionTree.MenuItems.Count == 0)
					{
						GUILayout.BeginVertical(SirenixGUIStyles.ContentPadding);
						SirenixEditorGUI.InfoMessageBox("There are no possible values to select.");
						GUILayout.EndVertical();
					}

					this.SelectionTree.DrawMenuTree();

					GUILayout.BeginArea(this.specialItemsRect);
					{
						if (this.ShowNoneItem)
						{
							this.noneMenuItem.DrawMenuItem(0);

							SirenixEditorGUI.DrawThickHorizontalSeperator(4.0f, 1.0f, 1.0f);
						}
					}
					GUILayout.EndArea();
				} finally
				{
					this.SelectionTree.Config.DrawSearchToolbar = prev;
				}

				SirenixEditorGUI.DrawBorders(rect, 1);
			}
			EditorGUILayout.EndVertical();
		}

		protected override void DrawToolbar()
		{
			bool drawTitle = !string.IsNullOrEmpty(this.Title);
			bool drawSearchToolbar = this.SelectionTree.Config.DrawSearchToolbar;
			bool drawButton = this.DrawConfirmSelectionButton;

			if (!drawTitle && !drawSearchToolbar && !drawButton)
			{
				return;
			}

			SirenixEditorGUI.BeginHorizontalToolbar(this.SelectionTree.Config.SearchToolbarHeight);
			{
				this.DrawToolbarTitle();
				this.DrawToolbarSearch();
				EditorGUI.DrawRect(GUILayoutUtility.GetLastRect().AlignLeft(1), SirenixGUIStyles.BorderColor);
				this.DrawToolbarConfirmButton();
				this.DrawToolbarButtons();
			}
			SirenixEditorGUI.EndHorizontalToolbar();
		}

		protected void DrawToolbarButtons()
		{
			if (ToolbarToggle(this.ShowCategories, SdfIconType.ListNested, GUIHelper.TempContent("", "Toggle Categories")))
			{
				this.ShowCategories = !this.ShowCategories;
				this.RebuildMenuTree();
				this.SelectionTree.FocusSearchField();
			}

			if (ToolbarToggle(this.ShowHiddenTypes, SdfIconType.EyeFill, GUIHelper.TempContent("", "Customize Visible Types"), iconYOffset: -1))
			{
				this.ShowHiddenTypes = !this.ShowHiddenTypes;
				this.RebuildMenuTree();
				this.SelectionTree.FocusSearchField();
			}

			//if (SirenixEditorGUI.ToolbarButton(SdfIconType.GearFill))
			if (ToolbarToggle(false, SdfIconType.GearFill, GUIHelper.TempContent("", "Goto 'Type Selector' Tab In Preferences")))
			{
				EditorWindow.GetWindow<SirenixPreferencesWindow>().GotoPreferencesTab("Type Selector");
			}
		}

		private string GetTypePath(Type type)
		{
			bool isUnityObject = typeof(UnityEngine.Object).IsAssignableFrom(type);

			if (this.ShowCategories)
			{
				string path = TypeRegistry.GetCategoryPath(type, this.PreferNamespaces);

				if (this.CategorizeUnityObjects && isUnityObject)
				{
					return string.IsNullOrEmpty(path) ? FIND_UNITY_OBJECT_ITEM_NAME : $"{FIND_UNITY_OBJECT_ITEM_NAME}/{path}";
				}

				return path;
			}

			return this.CategorizeUnityObjects && isUnityObject ? FIND_UNITY_OBJECT_ITEM_NAME : string.Empty;
		}

		protected void AddType(OdinMenuTree tree, Type type)
		{
			TypeRegistryUserConfig userConfig = TypeRegistryUserConfig.Instance;

			bool isHidden = !userConfig.IsVisible(type);

			if (!this.ShowHiddenTypes && isHidden)
			{
				return;
			}

			string name = TypeRegistry.GetName(type);

			TypeSelectorMenuItem typeSelectorMenuItem;

			if (this.ShowCategories || this.CategorizeUnityObjects)
			{
				string path = this.GetTypePath(type); //TypeRegistry.GetCategoryPath(type, this.PreferNamespaces);

				if (string.IsNullOrEmpty(path))
				{
					if (this.addedNamesForFlatTree.Contains(name))
					{
						this.duplicatePaths.Add(name);
					}

					typeSelectorMenuItem = new TypeSelectorMenuItem(this, tree, name, type);

					tree.MenuItems.Add(typeSelectorMenuItem);

					this.addedNamesForFlatTree.Add(name);
				}
				else
				{
					path = path.Trim('/');

					OdinMenuItem pathItem = this.GetOrMakePathItem(tree, path);

					bool hasItemWithName = this.TryGetItemWithName(pathItem, name, out OdinMenuItem foundItem, out int foundItemIndex);
					bool isFoundItemValueNull = hasItemWithName && foundItem.Value == null;

					if (this.PreferNamespaces)
					{
						if (hasItemWithName && !isFoundItemValueNull)
						{
							this.duplicatePaths.Add($"{path}/{name}");
						}
					}
					else
					{
						var fullPath = $"{path}/{name}";

						if (this.addedNamesForFlatTree.Contains(fullPath))
						{
							this.duplicatePaths.Add(fullPath);
						}

						this.addedNamesForFlatTree.Add(fullPath);
					}

					typeSelectorMenuItem = new TypeSelectorMenuItem(this, tree, name, type);

					if (isFoundItemValueNull)
					{
						typeSelectorMenuItem.ChildMenuItems.AddRange(foundItem.ChildMenuItems);
						pathItem.ChildMenuItems.RemoveAt(foundItemIndex);
						pathItem.ChildMenuItems.Insert(foundItemIndex, typeSelectorMenuItem);
					}
					else
					{
						pathItem.ChildMenuItems.Add(typeSelectorMenuItem);
					}
				}
			}
			else
			{
				if (this.addedNamesForFlatTree.Contains(name))
				{
					this.duplicatePaths.Add(name);
				}

				typeSelectorMenuItem = new TypeSelectorMenuItem(this, tree, name, type);

				tree.MenuItems.Add(typeSelectorMenuItem);

				this.addedNamesForFlatTree.Add(name);
			}

			if (this.ShowHiddenTypes && TypeRegistry.IsModifiableType(type))
			{
				typeSelectorMenuItem.OnDrawItem += menuItem =>
				{
					bool localIsVisible = TypeRegistryUserConfig.Instance.IsVisible(type);

					Rect visibilityRect = menuItem.Rect.AlignRight(menuItem.Style.IconSize);

					if (typeSelectorMenuItem.ChildMenuItems.Count > 0)
					{
						visibilityRect.x -= typeSelectorMenuItem.Style.TriangleSize;
					}

					visibilityRect.x -= menuItem.Style.BorderPadding;

					bool isMouseOver = Event.current.IsMouseOver(visibilityRect);

					visibilityRect = visibilityRect.AlignMiddle(menuItem.Style.IconSize).Padding(1);

					if (isMouseOver)
					{
						if (EditorGUIUtility.isProSkin)
						{
							SdfIcons.DrawIcon(visibilityRect, !localIsVisible ? SdfIconType.EyeSlashFill : SdfIconType.EyeFill, new Color(1, 1, 1));
						}
						else
						{
							SdfIcons.DrawIcon(visibilityRect, !localIsVisible ? SdfIconType.EyeSlashFill : SdfIconType.EyeFill, new Color(0, 0, 0));
						}
					}
					else
					{
						if (EditorGUIUtility.isProSkin)
						{
							SdfIcons.DrawIcon(visibilityRect, !localIsVisible ? SdfIconType.EyeSlashFill : SdfIconType.EyeFill, new Color(0.75f, 0.75f, 0.75f));
						}
						else
						{
							SdfIcons.DrawIcon(visibilityRect, !localIsVisible ? SdfIconType.EyeSlashFill : SdfIconType.EyeFill, new Color(0.25f, 0.25f, 0.25f));
						}
					}

					if (Event.current.OnMouseDown(visibilityRect, 0))
					{
						TypeRegistryUserConfig.Instance.SetVisibility(type, !localIsVisible);
						GUIHelper.RemoveFocusControl();
						Event.current.Use();
					}

					if (!this.SupportsMultiSelect && this.useSingleClick)
					{
						Rect clickPosition = menuItem.rect;
						clickPosition.width -= menuItem.Style.IconSize;

						if (menuItem.ChildMenuItems.Count > 0)
						{
							if (menuItem.Style.AlignTriangleLeft)
							{
								clickPosition.xMin += menuItem.Style.TriangleSize + menuItem.Style.TrianglePadding;
							}
							else
							{
								clickPosition.width -= menuItem.Style.TriangleSize + menuItem.Style.TrianglePadding;
							}
						}

						if (Event.current.OnMouseDown(clickPosition, 0))
						{
							menuItem.Select();
							this.SelectionTree.Selection.ConfirmSelection();
						}
					}

					if (!localIsVisible)
					{
						if (EditorGUIUtility.isProSkin)
						{
							EditorGUI.DrawRect(menuItem.rect, new Color(0, 0, 0, 0.25f));
						}
						else
						{
							EditorGUI.DrawRect(menuItem.rect, new Color(0, 0, 0, 0.075f));
						}
					}
				};
			}
			else if (!this.SupportsMultiSelect && this.useSingleClick)
			{
				typeSelectorMenuItem.OnDrawItem += menuItem =>
				{
					Rect clickPosition = menuItem.rect;

					if (menuItem.ChildMenuItems.Count > 0)
					{
						if (menuItem.Style.AlignTriangleLeft)
						{
							clickPosition.xMin += menuItem.Style.TriangleSize + menuItem.Style.TrianglePadding;
						}
						else
						{
							clickPosition.width -= menuItem.Style.TriangleSize + menuItem.Style.TrianglePadding;
						}
					}

					if (Event.current.OnMouseDown(clickPosition, 0))
					{
						menuItem.Select();
						this.SelectionTree.Selection.ConfirmSelection();
					}
				};
			}
		}

		private bool TryGetItemWithName(OdinMenuItem parent, string name, out OdinMenuItem item, out int index)
		{
			if (parent == null)
			{
				item = null;
				index = -1;
				return false;
			}

			for (var i = 0; i < parent.ChildMenuItems.Count; i++)
			{
				OdinMenuItem child = parent.ChildMenuItems[i];
				if (child.Name == name)
				{
					item = child;
					index = i;
					return true;
				}
			}

			item = null;
			index = -1;
			return false;
		}

		private OdinMenuItem GetOrMakePathItem(OdinMenuTree tree, string path)
		{
			if (this.categories.TryGetValue(path, out OdinMenuItem item))
			{
				return item;
			}

			item = tree.GetMenuItem(path);

			if (item != null)
			{
				return this.categories[path] = item;
			}

			OdinMenuTreeExtensions.SplitMenuPath(path, out string itemPath, out string itemName);

			item = new OdinMenuItem(tree, itemName, null) {SdfIcon = SdfIconType.FolderFill};

			tree.AddMenuItemAtPath(this.pathCache, itemPath, item);

			foreach (OdinMenuItem pathItem in this.pathCache)
			{
				pathItem.SdfIcon = SdfIconType.FolderFill;
			}

			this.pathCache.Clear();

			return this.categories[path] = item;
		}

		private void SortItemsByPriorityAndName(OdinMenuItem item = null)
		{
			if (item is null)
			{
				this.SelectionTree.MenuItems.Sort(this.CompareItemNameAndPriority);

				for (var i = 0; i < this.SelectionTree.MenuItems.Count; i++)
				{
					OdinMenuItem currentItem = this.SelectionTree.MenuItems[i];

					if (currentItem.ChildMenuItems.Count > 0)
					{
						this.SortItemsByPriorityAndName(currentItem);
					}
				}

				return;
			}

			item.ChildMenuItems.Sort(this.CompareItemNameAndPriority);

			for (var i = 0; i < item.ChildMenuItems.Count; i++)
			{
				OdinMenuItem currentSubItem = item.ChildMenuItems[i];

				if (currentSubItem.ChildMenuItems.Count > 0)
				{
					this.SortItemsByPriorityAndName(currentSubItem);
				}
			}
		}

		private int CompareItemNameAndPriority(OdinMenuItem a, OdinMenuItem b)
		{
			bool isANull = a == null;
			bool isBNull = b == null;

			if (isANull && isBNull)
			{
				return 0;
			}

			if (isANull)
			{
				return -1;
			}

			if (isBNull)
			{
				return 1;
			}

			if (this.CategorizeUnityObjects)
			{
				if (a == this.findUnityObjectItem)
				{
					return -1;
				}

				if (b == this.findUnityObjectItem)
				{
					return 1;
				}
			}

			bool isAValueNull = a.Value == null;
			bool isBValueNull = b.Value == null;

			if (isAValueNull && isBValueNull)
			{
				return string.Compare(a.Name, b.Name, StringComparison.Ordinal);
			}

			if (isAValueNull)
			{
				return -1;
			}

			if (isBValueNull)
			{
				return 1;
			}

			int aPriority = TypeRegistry.GetPriority((Type) a.Value);
			int bPriority = TypeRegistry.GetPriority((Type) b.Value);

			int priorityWeight = bPriority.CompareTo(aPriority);

			if (priorityWeight == 0)
			{
				return string.Compare(a.Name, b.Name, StringComparison.Ordinal);
			}

			return priorityWeight;
		}

		private static bool ToolbarToggle(bool isActive, SdfIconType icon, GUIContent content,
													 bool ignoreGUIEnabled = false,
													 float iconXOffset = 0.0f,
													 float iconYOffset = 0.0f)
		{
			Rect rect = GUILayoutUtility.GetRect(SirenixEditorGUI.currentDrawingToolbarHeight,
															 SirenixEditorGUI.currentDrawingToolbarHeight,
															 GUILayoutOptions.ExpandWidth(false).ExpandHeight(false));

			bool isPressed = GUI.Toggle(rect, isActive, content, SirenixGUIStyles.ToolbarButton) != isActive;

			if (ignoreGUIEnabled && !GUI.enabled)
			{
				if (Event.current.rawType == EventType.MouseDown && Event.current.button == 0 && Event.current.IsMouseOver(rect))
				{
					GUIHelper.PushGUIEnabled(true);
					Event.current.Use();
					GUIHelper.PopGUIEnabled();

					isPressed = true;
				}
			}

			if (isPressed && !isActive)
			{
				GUIHelper.RemoveFocusControl();
				GUIHelper.RequestRepaint();
			}

			if (Event.current.type != EventType.Repaint)
			{
				return isPressed;
			}

			RectOffset stylePadding = SirenixGUIStyles.ToolbarButton.padding;

			Rect iconRect = rect.Padding(stylePadding.left, stylePadding.right, stylePadding.top, stylePadding.bottom);

			if (iconXOffset != 0.0f)
			{
				iconRect.x += iconXOffset;
			}

			if (iconYOffset != 0.0f)
			{
				iconRect.y += iconYOffset;
			}

			SdfIcons.DrawIcon(iconRect, icon);

			return isPressed;
		}

		[OnInspectorGUI, PropertyOrder(10)]
		private void ShowTypeInfo()
		{
			if (this.SelectionTree.Selection.Count < 1)
			{
				return;
			}

			OdinMenuItem selectedItem = this.SelectionTree.Selection[0];

			if (!(selectedItem is TypeSelectorMenuItem item))
			{
				return;
			}

			const int LABEL_HEIGHT = 16;
			const int LABEL_WIDTH = 75;

			var fullTypeName = string.Empty;
			var assembly = string.Empty;
			var baseType = string.Empty;
			Rect rect = GUILayoutUtility.GetRect(0, LABEL_HEIGHT * 3 + 8).Padding(10, 4).AlignTop(LABEL_HEIGHT);

			Type type = item.Type;

			if (type != null)
			{
				fullTypeName = type.GetNiceFullName();
				assembly = type.Assembly.GetName().Name;
				baseType = type.BaseType == null ? string.Empty : type.BaseType.GetNiceFullName();
			}

			GUIStyle style = SirenixGUIStyles.LeftAlignedGreyMiniLabel;

			GUI.Label(rect.AlignLeft(LABEL_WIDTH), "Type Name", style);
			GUI.Label(rect.AlignRight(rect.width - LABEL_WIDTH), fullTypeName, style);

			rect.y += LABEL_HEIGHT;

			GUI.Label(rect.AlignLeft(LABEL_WIDTH), "Base Type", style);
			GUI.Label(rect.AlignRight(rect.width - LABEL_WIDTH), baseType, style);

			rect.y += LABEL_HEIGHT;

			GUI.Label(rect.AlignLeft(LABEL_WIDTH), "Assembly", style);
			GUI.Label(rect.AlignRight(rect.width - LABEL_WIDTH), assembly, style);
		}
	}
}
#endif