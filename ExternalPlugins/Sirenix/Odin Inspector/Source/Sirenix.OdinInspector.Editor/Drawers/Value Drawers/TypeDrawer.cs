//-----------------------------------------------------------------------
// <copyright file="TypeDrawer.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#if UNITY_EDITOR
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
namespace Sirenix.OdinInspector.Editor.Drawers
{
#pragma warning disable

    using Sirenix.Serialization;
    using Sirenix.Utilities;
    using Sirenix.Utilities.Editor;
    using Sirenix.Utilities.Editor.Expressions;
    using System;
    using System.Linq;
    using UnityEditor;
    using UnityEngine;
    using Sirenix.OdinInspector.Editor.Internal;
	 using System.Collections.Generic;

    /// <summary>
    /// Type property drawer
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [DrawerPriority(0, 0, 2001)]
	 public class TypeDrawer<T> : OdinValueDrawer<T>, IDefinesGenericMenuItems where T : Type
    {
        private TypeDrawerSettingsAttribute settings = null;

        private static readonly TwoWaySerializationBinder Binder = new DefaultSerializationBinder();

		  public string TypeNameTemp;
		  public bool IsValid = true;
		  public string UniqueControlName;
		  public bool WasFocusedControl;

		  public void PopulateGenericMenu(InspectorProperty property, GenericMenu genericMenu)
		  {
			  var entry = property.ValueEntry as IPropertyValueEntry<T>;

			  if (entry.IsEditable)
			  {
				  Rect rect = entry.Property.LastDrawnValueRect;
				  //rect.position = GUIUtility.GUIToScreenPoint(rect.position);
				  //rect.height = 20;

				  genericMenu.AddItem(new GUIContent("Change Type"), false,
											 () => { this.Property.Tree.DelayActionUntilRepaint(() => { this.ShowSelectorInPopup(rect); }); });
			  }
			  else
			  {
				  genericMenu.AddDisabledItem(new GUIContent("Change Type"));
			  }
		  }

        protected override void Initialize()
        {
            this.UniqueControlName = Guid.NewGuid().ToString();

            this.settings = this.Property.GetAttribute<TypeDrawerSettingsAttribute>();
        }

        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            var entry = this.ValueEntry;

            if (!this.IsValid)
            {
                GUIHelper.PushColor(Color.red);
            }

            GUI.SetNextControlName(this.UniqueControlName);
            
            Rect rect = EditorGUILayout.GetControlRect();

            if (label != null)
            {
                rect = EditorGUI.PrefixLabel(rect, label);
            }
            
            Rect fieldRect = rect;
            Rect dropdownRect = rect.AlignRight(18);

            // Dropdown button.
            EditorGUIUtility.AddCursorRect(dropdownRect, MouseCursor.Arrow);

            bool isPressed = GUI.Button(dropdownRect, GUIContent.none, GUIStyle.none);

            if (isPressed)
            {
					this.ShowSelectorInPopup(rect);
            }

            // Reset type name.
            if (Event.current.type == EventType.Layout)
            {
                this.TypeNameTemp = entry.SmartValue != null ? Binder.BindToName(entry.SmartValue) : null;
            }

            EditorGUI.BeginChangeCheck();
            this.TypeNameTemp = SirenixEditorFields.DelayedTextField(fieldRect, this.TypeNameTemp);

            // Draw dropdown button.
            EditorIcons.TriangleDown.Draw(dropdownRect);

            if (!this.IsValid)
            {
                GUIHelper.PopColor();
            }

            bool isFocused = GUI.GetNameOfFocusedControl() == this.UniqueControlName;
            bool defocused = false;

            if (isFocused != this.WasFocusedControl)
            {
                defocused = !isFocused;
                this.WasFocusedControl = isFocused;
            }

            if (EditorGUI.EndChangeCheck())
            {
                if (this.TypeNameTemp == null || string.IsNullOrEmpty(this.TypeNameTemp.Trim()))
                {
                    // String is empty
                    entry.SmartValue = null;
                    this.IsValid = true;
                }
                else
                {
                    Type type = Binder.BindToType(this.TypeNameTemp);

                    if (type == null)
                    {
                        type = AssemblyUtilities.GetTypeByCachedFullName(this.TypeNameTemp);
                    }

                    if (type == null)
                    {
                        ExpressionUtility.TryParseTypeNameAsCSharpIdentifier(this.TypeNameTemp, out type);
                    }

                    if (type == null)
                    {
                        this.IsValid = false;
                    }
                    else
                    {
                        // Use WeakSmartValue in case of a different Type-derived instance showing up somehow, so we don't get cast errors
                        entry.WeakSmartValue = type;
                        this.IsValid = true;
                    }
                }
            }

            if (defocused)
            {
                // Ensure we show the full type name when the control is defocused
                this.TypeNameTemp = entry.SmartValue == null ? "" : Binder.BindToName(entry.SmartValue);
                this.IsValid = true;
            }
        }

		  private void ShowSelectorInPopup(Rect position)
		  {
			  var entry = this.Property.ValueEntry as IPropertyValueEntry<T>;


			  List<Type> types;

			  if (GeneralDrawerConfig.Instance.useOldTypeSelector || this.settings == null)
			  {
				  types = TypeRegistry.GetValidTypesInCategory(AssemblyCategory.All);
			  }
			  else
			  {
				  if (this.settings.BaseType != null)
				  {
					  types = TypeRegistry.GetInheritors(this.settings.BaseType);
				  }
				  else
				  {
					  types = TypeRegistry.GetValidTypesInCategory(AssemblyCategory.All);
				  }

				  for (int i = types.Count - 1; i >= 0; i--)
				  {
					  if (!this.settings.Filter.IsValidType(types[i]))
					  {
						  types.RemoveAt(i);
					  }
				  }
			  }

			  OdinSelector<Type> selector = TypeSelectorHandler_WILL_BE_DEPRECATED.InstantiateSelector(types,
																																	 showNoneItem: true,
																																	 property: this.Property);

			  selector.SelectionConfirmed += t =>
			  {
				  var type = t.FirstOrDefault();

				  if (type == typeof(TypeSelectorV2.TypeSelectorNoneValue))
				  {
					  type = null;
				  }

				  entry.Property.Tree.DelayAction(() =>
				  {
					  entry.WeakSmartValue = type;
					  this.IsValid = true;
					  entry.ApplyChanges();
				  });
			  };

			  selector.SetSelection(entry.SmartValue);
			  selector.ShowInPopup(position);
		  }
	 }
}
#endif