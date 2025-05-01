//-----------------------------------------------------------------------
// <copyright file="UnitAttributeDrawer.cs" company="Sirenix ApS">
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

    using System.Globalization;
    using System.Linq;
    using Sirenix.OdinInspector;
    using Sirenix.OdinInspector.Editor;
    using Sirenix.OdinInspector.Editor.ValueResolvers;
    using Sirenix.Utilities;
    using Sirenix.Utilities.Editor;
    using UnityEditor;
    using UnityEngine;

    public abstract class UnitAttributeDrawer<TPrimitive> : OdinAttributeDrawer<UnitAttribute, TPrimitive>, IDefinesGenericMenuItems
    {
        private readonly static string FloatingFieldFormatString = "G7";
        private readonly static string IntegerFieldFormatString = "#,##0";

        private UnitInfo baseUnitInfo;
        private UnitInfo displayUnitInfo;

        private string lastResolvedUnitName;

        private ValueResolver<string> displayUnitNameResolver;
        private ValueResolver<Units> displayUnitEnumResolver;

        private string errorMessage;

        private bool isFloatingPointNumber;

        protected override void Initialize()
        {
            if (this.Attribute.Base != Units.Unset)
            {
                if (UnitNumberUtility.TryGetUnitInfo(this.Attribute.Base, out this.baseUnitInfo) == false)
                {
                    this.errorMessage = $"Failed to find unit '{this.Attribute.Base}'.";
                }
            }
            else
            {
                if (UnitNumberUtility.TryGetUnitInfoByName(this.Attribute.BaseName, out this.baseUnitInfo) == false)
                {
                    this.errorMessage = $"Failed to find unit by name '{this.Attribute.BaseName}'.";
                }
            }

            if (this.Attribute.Display != Units.Unset)
            {
                if (UnitNumberUtility.TryGetUnitInfo(this.Attribute.Display, out this.displayUnitInfo) == false)
                {
                    this.errorMessage = $"Failed to find unit '{this.Attribute.Display}'.";
                }
            }
            else
            {
                if (this.Attribute.DisplayName.Length == 0)
                {
                    this.errorMessage = "No display unit set.";
                }
                else if (this.Attribute.DisplayName[0] == '$' || this.Attribute.DisplayName[0] == '@')
                {
                    displayUnitEnumResolver = ValueResolver.Get<Units>(this.Property, this.Attribute.DisplayName);
                    displayUnitNameResolver = ValueResolver.Get<string>(this.Property, this.Attribute.DisplayName);

                    if (displayUnitNameResolver.HasError && this.displayUnitNameResolver.HasError)
                    {
                        this.errorMessage = displayUnitNameResolver.ErrorMessage;
                    }
                }
                else if (UnitNumberUtility.TryGetUnitInfoByName(this.Attribute.DisplayName, out this.displayUnitInfo) == false)
                {
                    this.errorMessage = $"Failed to find unit by name '{this.Attribute.DisplayName}'.";
                }
            }

            if (this.baseUnitInfo != null && this.displayUnitInfo != null && this.baseUnitInfo.UnitCategory != this.displayUnitInfo.UnitCategory)
            {
                this.errorMessage = $"Cannot convert between '{this.baseUnitInfo.Name}' and '{this.displayUnitInfo.Name}'.";
            }

            isFloatingPointNumber = typeof(TPrimitive) == typeof(float) || typeof(TPrimitive) == typeof(double) || typeof(TPrimitive) == typeof(decimal);
        }

        protected override void DrawPropertyLayout(GUIContent label)
        {
            if (Event.current.type == EventType.Layout && (this.displayUnitEnumResolver != null || this.displayUnitNameResolver != null))
            {
                if (this.displayUnitEnumResolver != null && this.displayUnitEnumResolver.HasError == false)
                {
                    var u = displayUnitEnumResolver.GetValue();

                    if (UnitNumberUtility.TryGetUnitInfo(u, out this.displayUnitInfo) == false)
                    {
                        Debug.LogError($"Failed to find unit: '{u}'.");

                    }
                }
                else if (this.displayUnitNameResolver != null && this.displayUnitNameResolver.HasError == false)
                {
                    var n = displayUnitNameResolver.GetValue();

                    if (n != this.lastResolvedUnitName && UnitNumberUtility.TryGetUnitInfoByName(n ?? "", out this.displayUnitInfo) == false)
                    {
                        Debug.LogError($"Failed to find unit with name: '{n}'.");
                    }

                    this.lastResolvedUnitName = n;
                }

                if (this.displayUnitInfo == null)
                {
                    this.displayUnitInfo = this.baseUnitInfo;
                }
                else if (UnitNumberUtility.CanConvertBetween(this.baseUnitInfo, this.displayUnitInfo) == false)
                {
                    Debug.LogError($"Cannot convert between {this.baseUnitInfo.Name} and {this.displayUnitInfo.Name}.");
                    this.displayUnitInfo = this.baseUnitInfo;
                }
            }

            if (string.IsNullOrWhiteSpace(this.errorMessage) == false)
            {
                SirenixEditorGUI.ErrorMessageBox(this.errorMessage);
                return;
            }

            if (this.Attribute.DisplayAsString)
            {
                string str = string.Empty;

                if (Event.current.type == EventType.Repaint)
                {
                    var d = ConvertUtility.Convert<decimal>(this.ValueEntry.SmartValue);
                    d = UnitNumberUtility.ConvertUnitFromTo(d, this.baseUnitInfo, this.displayUnitInfo);
                    str = d.ToString(isFloatingPointNumber ? FloatingFieldFormatString : IntegerFieldFormatString, CultureInfo.InvariantCulture) + " " + this.displayUnitInfo.Symbols[0];
                }

                var rect = EditorGUILayout.GetControlRect(GUILayoutOptions.MinWidth(0));

                if (label != null)
                {
                    rect = EditorGUI.PrefixLabel(rect, label);
                }

                EditorGUI.LabelField(rect, GUIHelper.TempContent(str));
            }
            else
            {
                EditorGUI.BeginChangeCheck();
                TPrimitive v = this.ValueEntry.SmartValue;
                v = DrawField(this.Property.ToFieldExpressionContext(), label, v, this.baseUnitInfo, this.displayUnitInfo);

                if (EditorGUI.EndChangeCheck())
                {
                    this.ValueEntry.SmartValue = v;
                }
            }
            //else if (isFloatingPointNumber)
            //{
            //    EditorGUI.BeginChangeCheck();
            //    decimal v = ConvertUtility.Convert<decimal>(this.ValueEntry.SmartValue);

            //    v = SirenixEditorFields.SmartDecimalUnitField(this.Property.ToFieldExpressionContext(), label, v, this.baseUnitInfo, this.displayUnitInfo);

            //    if (EditorGUI.EndChangeCheck())
            //    {
            //        this.ValueEntry.SmartValue = ConvertUtility.Convert<TPrimitive>(v);
            //    }
            //}
            //else
            //{
            //    EditorGUI.BeginChangeCheck();
            //    long v = ConvertUtility.Convert<long>(this.ValueEntry.SmartValue);

            //    v = SirenixEditorFields.SmartLongUnitField(this.Property.ToFieldExpressionContext(), label, v, this.baseUnitInfo, this.displayUnitInfo);

            //    if (EditorGUI.EndChangeCheck())
            //    {
            //        this.ValueEntry.SmartValue = ConvertUtility.Convert<TPrimitive>(v);
            //    }
            //}
        }

        protected abstract TPrimitive DrawField(FieldExpressionContext fieldExpressionContext, GUIContent label, TPrimitive value, UnitInfo baseUnitInfo, UnitInfo displayUnitInfo);

        public void PopulateGenericMenu(InspectorProperty property, GenericMenu genericMenu)
        {
            if (this.Attribute.ForceDisplayUnit == false && this.errorMessage == null && this.displayUnitNameResolver == null && this.displayUnitEnumResolver == null)
            {
                foreach (var x in UnitNumberUtility.GetAllUnitInfos().Where(ui => ui.UnitCategory == this.baseUnitInfo.UnitCategory))
                {
                    var unitInfo = x;
                    genericMenu.AddItem(new GUIContent($"Change Unit/{unitInfo.Name}"), unitInfo == this.displayUnitInfo, () =>
                    {
                        this.displayUnitInfo = unitInfo;
                    });
                }
            }

            genericMenu.AddItem(new GUIContent("Open In Unit Overview Window"), false, () =>
            {
                var value = UnitNumberUtility.ConvertUnitFromTo(
                    ConvertUtility.Convert<decimal>(this.ValueEntry.SmartValue),
                    this.baseUnitInfo,
                    this.displayUnitInfo);

                UnitOverviewWindow.SelectUnitInfo(this.displayUnitInfo, value);
            });
        }
    }

    public class UnitAttributeIntDrawer : UnitAttributeDrawer<int>
    {
        protected override int DrawField(FieldExpressionContext fieldExpressionContext, GUIContent label, int value, UnitInfo baseUnitInfo, UnitInfo displayUnitInfo)
        {
            return SirenixEditorFields.SmartIntUnitField(fieldExpressionContext, label, value, baseUnitInfo, displayUnitInfo);
        }
    }
    public class UnitAttributeUIntDrawer : UnitAttributeDrawer<uint> 
    {
        protected override uint DrawField(FieldExpressionContext fieldExpressionContext, GUIContent label, uint value, UnitInfo baseUnitInfo, UnitInfo displayUnitInfo)
        {
            return (uint)SirenixEditorFields.SmartLongUnitField(fieldExpressionContext, label, value, baseUnitInfo, displayUnitInfo);
        }
    }
    public class UnitAttributeLongDrawer : UnitAttributeDrawer<long> 
    {
        protected override long DrawField(FieldExpressionContext fieldExpressionContext, GUIContent label, long value, UnitInfo baseUnitInfo, UnitInfo displayUnitInfo)
        {
            return SirenixEditorFields.SmartLongUnitField(fieldExpressionContext, label, value, baseUnitInfo, displayUnitInfo);
        }
    }
    public class UnitAttributeULongDrawer : UnitAttributeDrawer<ulong> 
    {
        protected override ulong DrawField(FieldExpressionContext fieldExpressionContext, GUIContent label, ulong value, UnitInfo baseUnitInfo, UnitInfo displayUnitInfo)
        {
            return (ulong)SirenixEditorFields.SmartLongUnitField(fieldExpressionContext, label, (long)value, baseUnitInfo, displayUnitInfo);
        }
    }
    public class UnitAttributeFloatDrawer : UnitAttributeDrawer<float> 
    {
        protected override float DrawField(FieldExpressionContext fieldExpressionContext, GUIContent label, float value, UnitInfo baseUnitInfo, UnitInfo displayUnitInfo)
        {
            return SirenixEditorFields.SmartFloatUnitField(fieldExpressionContext, label, value, baseUnitInfo, displayUnitInfo);
        }
    }
    public class UnitAttributeDoubleDrawer : UnitAttributeDrawer<double> 
    {
        protected override double DrawField(FieldExpressionContext fieldExpressionContext, GUIContent label, double value, UnitInfo baseUnitInfo, UnitInfo displayUnitInfo)
        {
            return SirenixEditorFields.SmartDoubleUnitField(fieldExpressionContext, label, value, baseUnitInfo, displayUnitInfo);
        }
    }
    public class UnitAttributeDecimalDrawer : UnitAttributeDrawer<decimal> 
    {
        protected override decimal DrawField(FieldExpressionContext fieldExpressionContext, GUIContent label, decimal value, UnitInfo baseUnitInfo, UnitInfo displayUnitInfo)
        {
            return SirenixEditorFields.SmartDecimalUnitField(fieldExpressionContext, label, value, baseUnitInfo, displayUnitInfo);
        }
    }
}
#endif