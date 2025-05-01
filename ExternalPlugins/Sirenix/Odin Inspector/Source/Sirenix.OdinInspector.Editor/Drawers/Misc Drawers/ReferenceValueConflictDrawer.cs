//-----------------------------------------------------------------------
// <copyright file="ReferenceValueConflictDrawer.cs" company="Sirenix ApS">
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

    using Utilities.Editor;
    using UnityEditor;
    using UnityEngine;
    using Utilities;
	 using Sirenix.OdinInspector.Editor.Internal;
	 using Sirenix.Serialization;

	 // NOTE: this using statement is important for later Unity versions
	 using SerializationUtility = Sirenix.Serialization.SerializationUtility;

    /// <summary>
    /// <para>
    /// When multiple objects are selected and inspected, this his drawer ensures UnityEditor.EditorGUI.showMixedValue
    /// gets set to true if there are any conflicts in the selection for any given property.
    /// Otherwise the next drawer is called.
    /// </para>
    /// <para>This drawer also implements <see cref="IDefinesGenericMenuItems"/> and provides a right-click context menu item for resolving conflicts if any.</para>
    /// </summary>
    [DrawerPriority(0.5, 0, 0)]
    [AllowGUIEnabledForReadonly]
    public sealed class ReferenceValueConflictDrawer<T> : OdinValueDrawer<T>, IDefinesGenericMenuItems where T : class
	 {
		 private bool allowSceneObjects;
		 
        protected override bool CanDrawValueProperty(InspectorProperty property)
        {
            return !property.IsTreeRoot && property.Tree.WeakTargets.Count > 1;
        }

        protected override void Initialize()
        {
			  //this.SkipWhenDrawin
            this.allowSceneObjects = InspectorPropertyInfoUtility.InspectorPropertySupportsAssigningSceneReferences(this.Property);
        }

		  /// <summary>
		  /// Draws the property.
		  /// </summary>
		  protected override void DrawPropertyLayout(GUIContent label)
		  {
			  IPropertyValueEntry<T> entry = this.ValueEntry;
			  
			  if (entry.ValueState == PropertyValueState.ReferenceValueConflict)
			  {
				  GUIHelper.PushGUIEnabled(GUI.enabled && entry.IsEditable);

				  if (typeof(UnityEngine.Object).IsAssignableFrom(entry.TypeOfValue))
				  {
					  bool prev = EditorGUI.showMixedValue;
					  EditorGUI.showMixedValue = true;
					  this.CallNextDrawer(label);
					  EditorGUI.showMixedValue = prev;
				  }
				  else
				  {
					  var position = EditorGUILayout.GetControlRect();

					  bool prev = EditorGUI.showMixedValue;

					  EditorGUI.showMixedValue = true;

					  if (GeneralDrawerConfig.Instance.useOldPolymorphicField)
					  {
						  if (label != null)
						  {
							  position = EditorGUI.PrefixLabel(position, label);
						  }

						  EditorGUI.BeginChangeCheck();
						  var newValue = OdinInternalEditorFields.PolymorphicObjectField(this.Property,
																											  OdinObjectSelectorIds.ODIN_DRAWER_FIELD,
																											  position,
																											  label,
																											  this.Property,
																											  this.allowSceneObjects);
						  if (EditorGUI.EndChangeCheck())
						  {
							  this.ValueEntry.Property.Tree.DelayActionUntilRepaint(() =>
							  {
								  this.ValueEntry.WeakValues[0] = newValue;
								  for (int j = 1; j < this.ValueEntry.ValueCount; j++)
								  {
									  // NOTE: "Sirenix.Serialization." is important for later Unity versions
									  this.ValueEntry.WeakValues[j] = Sirenix.Serialization.SerializationUtility.CreateCopy(newValue);
								  }
							  });
						  }
					  }
					  else
					  {
						  OdinInternalEditorFields.PolymorphicObjectField(this.Property, OdinObjectSelectorIds.ODIN_DRAWER_FIELD, position, label, this.Property, this.allowSceneObjects);
					  }

					  EditorGUI.showMixedValue = prev;
				  }

				  GUIHelper.PopGUIEnabled();
			  }
			  else
			  {
				  this.CallNextDrawer(label);
			  }
		  }

		  void IDefinesGenericMenuItems.PopulateGenericMenu(InspectorProperty property, GenericMenu genericMenu)
        {
            if (property.ValueEntry.ValueState == PropertyValueState.ReferenceValueConflict)
            {
                var tree = property.Tree;

                if (typeof(UnityEngine.Object).IsAssignableFrom(tree.TargetType))
                {
                    for (int i = 0; i < tree.WeakTargets.Count; i++)
                    {
                        object value = property.ValueEntry.WeakValues[i];
                        string valueString = value == null ? "null" : value.GetType().GetNiceName();
                        string contentString = "Resolve type conflict with.../" + ((UnityEngine.Object)tree.WeakTargets[i]).name + " (" + valueString + ")";

                        genericMenu.AddItem(new GUIContent(contentString), false, () =>
                        {
                            property.Tree.DelayActionUntilRepaint(() =>
									 {
										 property.ValueEntry.WeakValues[0] = value;

										 for (var j = 1; j < property.ValueEntry.WeakValues.Count; j++)
										 {
											 property.ValueEntry.WeakValues[j] = SerializationUtility.CreateCopy(value);
										 }
                            });
                        });
                    }
                }
            }
        }
    }
}
#endif