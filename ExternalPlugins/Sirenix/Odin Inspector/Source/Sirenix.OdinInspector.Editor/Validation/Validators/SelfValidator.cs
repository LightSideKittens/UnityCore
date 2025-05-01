//-----------------------------------------------------------------------
// <copyright file="SelfValidator.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#if UNITY_EDITOR
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
[assembly: Sirenix.OdinInspector.Editor.Validation.RegisterValidator(typeof(Sirenix.OdinInspector.Editor.Validation.SelfValidator<>))]

namespace Sirenix.OdinInspector.Editor.Validation
{
#pragma warning disable

    using Sirenix.Utilities;
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEditor;

    public class SelfValidator<T> : ValueValidator<T> where T : ISelfValidator
    {
        public override bool CanValidateProperty(InspectorProperty property)
        {
            if (!property.IsTreeRoot && typeof(UnityEngine.Object).IsAssignableFrom(property.ValueEntry.TypeOfValue))
            {
                return false;
            }

            return true;
        }

        protected override void Validate(ValidationResult result)
        {
            if (this.ValueEntry.SmartValue != null)
            {
                var selfResult = new SelfValidationResult();

                this.ValueEntry.SmartValue.Validate(selfResult);

                var count = selfResult.Count;

                for (int i = 0; i < count; i++)
                {
                    ref var selfEntry = ref selfResult[i];

                    var item = new ResultItem();

                    item.Message = selfEntry.Message;

                    switch (selfEntry.ResultType)
                    {
                        case SelfValidationResult.ResultType.Error:
                        item.ResultType = ValidationResultType.Error;
                        break;
                        case SelfValidationResult.ResultType.Warning:
                        item.ResultType = ValidationResultType.Warning;
                        break;
                        case SelfValidationResult.ResultType.Valid:
                        item.ResultType = ValidationResultType.Valid;
                        break;
                        default:
                        throw new NotImplementedException(selfEntry.ResultType.ToString());
                    }

                    if (selfEntry.MetaData != null)
                    {
                        var metaData = new ResultItemMetaData[selfEntry.MetaData.Length];

                        for (int j = 0; j < metaData.Length; j++)
                        {
                            var metaDataItem = selfEntry.MetaData[j];
                            metaData[j] = new ResultItemMetaData(metaDataItem.Name, metaDataItem.Value, metaDataItem.Attributes);
                        }

                        item.MetaData = metaData;
                    }

                    if (selfEntry.Fix.HasValue)
                    {
                        var fix = selfEntry.Fix.Value;

                        if (fix.Action.GetType() == typeof(Action))
                        {
                            item.Fix = new Fix(fix.Title, (Action)fix.Action, fix.OfferInInspector);
                        }
                        else if (fix.Action.GetType().IsGenericType && fix.Action.GetType().GetGenericTypeDefinition() == typeof(Action<>))
                        {
                            item.Fix = new Fix()
                            {
                                Title = fix.Title,
                                Action = fix.Action,
                                OfferInInspector = fix.OfferInInspector,
                                ArgType = fix.Action.GetType().GetGenericArguments()[0]
                            };
                        }
                        else
                        {
                            result.AddError($"Given fix '{fix.Title}' had an invalid delegate type of '{fix.Action.GetType().GetNiceName()}', for validation {selfEntry.ResultType.ToString().ToLower()} result with message '{selfEntry.Message}'; only System.Action and System.Action<T> are allowed.");
                            continue;
                        }
                    }

                    if (selfEntry.OnContextClick != null)
                    {
                        item.OnContextClick = CreateOnContextClickInvoker(selfEntry.OnContextClick);
                    }

                    item.OnSceneGUI = selfEntry.OnSceneGUI;
                    item.SelectionObject = selfEntry.SelectionObject;
                    item.RichText = selfEntry.RichText;

                    result.Add(item);
                }
            }
        }

        // This just exists to ensure there's no needless closure capturing going on
        private static Action<GenericMenu> CreateOnContextClickInvoker(Func<IEnumerable<SelfValidationResult.ContextMenuItem>> onContextClick)
        {
            return menu =>
            {
                foreach (var contextClickAction in onContextClick.GetInvocationList())
                {
                    var items = (IEnumerable<SelfValidationResult.ContextMenuItem>)contextClickAction.DynamicInvoke();
                    foreach (var item in items)
                    {
                        if (item.AddSeparatorBefore)
                        {
                            var lastForwardSlash = item.Path.LastIndexOf('/');

                            if (lastForwardSlash > 0)
                            {
                                menu.AddSeparator(item.Path.Substring(0, lastForwardSlash));
                            }
                            else
                            {
                                menu.AddSeparator("");
                            }
                        }

                        menu.AddItem(new GUIContent(item.Path), item.On, CreateMenuFunction(item.OnClick));
                    }
                }
            };
        }

        private static GenericMenu.MenuFunction CreateMenuFunction(Action onClick)
        {
            return (GenericMenu.MenuFunction)Delegate.CreateDelegate(typeof(GenericMenu.MenuFunction), onClick.Target, onClick.Method);
        }
    }
}
#endif