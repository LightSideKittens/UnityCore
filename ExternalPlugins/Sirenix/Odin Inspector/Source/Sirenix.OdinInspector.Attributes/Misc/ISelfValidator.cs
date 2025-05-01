//-----------------------------------------------------------------------
// <copyright file="ISelfValidator.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
namespace Sirenix.OdinInspector
{
#pragma warning disable

    using System;
    using System.Collections.Generic;
    using ResultItem = SelfValidationResult.ResultItem;
    using ContextMenuItem = SelfValidationResult.ContextMenuItem;
    using ResultItemMetaData = SelfValidationResult.ResultItemMetaData;

    /// <summary>
    /// Any type implementing this interface will be considered to be validating itself using the implemented logic, as if a custom validator had been written for it.
    /// </summary>
    public interface ISelfValidator
    {
        void Validate(SelfValidationResult result);
    }

    [EnumToggleButtons]
    public enum ValidatorSeverity
    {
        Error,
        Warning,
        Ignore,
    }

    public static class SelfValidationResultItemExtensions
    {
        // TODO: Comment on offerInInspector not yet implemented.
        public static ref ResultItem WithFix(ref this ResultItem item, string title, Action fix, bool offerInInspector = true)
        {
            item.Fix = SelfFix.Create(title, fix, offerInInspector);
            return ref item;
        }

        public static ref ResultItem WithFix<T>(ref this ResultItem item, string title, Action<T> fix, bool offerInInspector = true)
            where T : new()
        {
            item.Fix = SelfFix.Create(title, fix, offerInInspector);
            return ref item;
        }

        public static ref ResultItem WithFix(ref this ResultItem item, Action fix, bool offerInInspector = true)
        {
            item.Fix = SelfFix.Create(fix, offerInInspector);
            return ref item;
        }

        public static ref ResultItem WithFix<T>(ref this ResultItem item, Action<T> fix, bool offerInInspector = true)
            where T : new()
        {
            item.Fix = SelfFix.Create(fix, offerInInspector);
            return ref item;
        }

        public static ref ResultItem WithFix(ref this ResultItem item, SelfFix fix)
        {
            item.Fix = fix;
            return ref item;
        }

        public static ref ResultItem WithContextClick(ref this ResultItem item, Func<IEnumerable<ContextMenuItem>> onContextClick)
        {
            item.OnContextClick += onContextClick;
            return ref item;
        }

        public static ref ResultItem WithContextClick(ref this ResultItem item, string path, Action onClick)
        {
            item.OnContextClick += () => new ContextMenuItem[]
            {
                new ContextMenuItem()
                {
                    Path    = path,
                    OnClick = onClick
                }
            };
            return ref item;
        }

        public static ref ResultItem WithContextClick(ref this ResultItem item, string path, bool on, Action onClick)
        {
            item.OnContextClick += () => new ContextMenuItem[]
            {
                new ContextMenuItem()
                {
                    Path    = path,
                    On      = on,
                    OnClick = onClick
                }
            };
            return ref item;
        }

        public static ref ResultItem WithContextClick(ref this ResultItem item, ContextMenuItem onContextClick)
        {
            item.OnContextClick += () => new ContextMenuItem[] { onContextClick };
            return ref item;
        }

        public static ref ResultItem WithSceneGUI(ref this ResultItem item, Action onSceneGUI)
        {
            item.OnSceneGUI = onSceneGUI;
            return ref item;
        }

        public static ref ResultItem SetSelectionObject(ref this ResultItem item, UnityEngine.Object uObj)
        {
            item.SelectionObject = uObj;
            return ref item;
        }

        public static ref ResultItem EnableRichText(ref this ResultItem item)
        {
            item.RichText = true;
            return ref item;
        }

        public static ref ResultItem WithMetaData(ref this ResultItem resultItem, string name, object value, params Attribute[] attributes)
        {
            resultItem.MetaData = resultItem.MetaData ?? new ResultItemMetaData[0];
            Array.Resize(ref resultItem.MetaData, resultItem.MetaData.Length + 1);
            resultItem.MetaData[resultItem.MetaData.Length - 1] = new ResultItemMetaData(name, value, attributes);
            return ref resultItem;
        }

        public static ref ResultItem WithMetaData(ref this ResultItem resultItem, object value, params Attribute[] attributes)
        {
            resultItem.MetaData = resultItem.MetaData ?? new ResultItemMetaData[0];
            Array.Resize(ref resultItem.MetaData, resultItem.MetaData.Length + 1);
            resultItem.MetaData[resultItem.MetaData.Length - 1] = new ResultItemMetaData(null, value, attributes);
            return ref resultItem;
        }

        public static ref ResultItem WithButton(ref this ResultItem resultItem, string name, Action onClick)
        {
            resultItem.MetaData = resultItem.MetaData ?? new ResultItemMetaData[0];
            Array.Resize(ref resultItem.MetaData, resultItem.MetaData.Length + 1);
            resultItem.MetaData[resultItem.MetaData.Length - 1] = new ResultItemMetaData(name, onClick);
            return ref resultItem;
        }
    }

    public class SelfValidationResult
    {
        private static ResultItem NoResultItem;

        public struct ContextMenuItem
        {
            public string Path;
            public bool On;
            public bool AddSeparatorBefore;
            public Action OnClick;
        }

        private ResultItem[] items;
        private int itemsCount;

        public int Count => this.itemsCount;

        public ref ResultItem this[int index]
        {
            get { return ref this.items[index]; }
        }

        public enum ResultType
        {
            Error,
            Warning,
            Valid
        }

        public struct ResultItem
        {
            public string Message;
            public ResultType ResultType;
            public SelfFix? Fix;
            public ResultItemMetaData[] MetaData;
            public Func<IEnumerable<ContextMenuItem>> OnContextClick;
            public Action OnSceneGUI;
            public UnityEngine.Object SelectionObject;
            public bool RichText;
        }

        public struct ResultItemMetaData
        {
            public string Name;
            public object Value;
            public Attribute[] Attributes;

            public ResultItemMetaData(string name, object value, params Attribute[] attributes)
            {
                this.Name = name;
                this.Value = value;
                this.Attributes = attributes;
            }
        }

        public ref ResultItem AddError(string error)
        {
            return ref this.Add(new ResultItem()
            {
                Message = error,
                ResultType = ResultType.Error,
            });
        }

        public ref ResultItem AddWarning(string warning)
        {
            return ref this.Add(new ResultItem()
            {
                Message = warning,
                ResultType = ResultType.Warning,
            });
        }

        public ref ResultItem Add(ValidatorSeverity severity, string message)
        {
            if (severity == ValidatorSeverity.Error)
            {
                return ref this.Add(new ResultItem()
                {
                    Message = message,
                    ResultType = ResultType.Error,
                });
            }
            else if (severity == ValidatorSeverity.Warning)
            {
                return ref this.Add(new ResultItem()
                {
                    Message = message,
                    ResultType = ResultType.Warning,
                });
            }
            else
            {
                NoResultItem = default;
                return ref NoResultItem;
            }
        }

        public ref ResultItem Add(ResultItem item)
        {
            var its = this.items;

            if (its == null)
            {
                its = new ResultItem[2];
                this.items = its;
            }

            while (its.Length <= this.itemsCount + 1)
            {
                var expand = new ResultItem[its.Length * 2];
                for (int i = 0; i < its.Length; i++)
                {
                    expand[i] = its[i];
                }

                its = expand;
                this.items = expand;
            }

            its[this.itemsCount] = item;
            return ref its[this.itemsCount++];
        }
    }

    public class SelfMetaData : List<SelfValidationResult.ResultItemMetaData>
    {
        public void Add(string key, object value)
        {
            this.Add(new SelfValidationResult.ResultItemMetaData(key, value));
        }
    }

    public struct SelfFix
    {
        public string Title;
        public Delegate Action;
        public bool OfferInInspector;

        public SelfFix(string name, Action action, bool offerInInspector)
        {
            this.Title = name;
            this.Action = action;
            this.OfferInInspector = offerInInspector;
        }

        public SelfFix(string name, Delegate action, bool offerInInspector)
        {
            this.Title = name;
            this.Action = action;
            this.OfferInInspector = offerInInspector;
        }

        public static SelfFix Create(Action action, bool offerInInspector = true)
        {
            return new SelfFix("Fix", action, offerInInspector);
        }

        public static SelfFix Create(string title, Action action, bool offerInInspector = true)
        {
            return new SelfFix(title, action, offerInInspector);
        }

        public static SelfFix Create<T>(Action<T> action, bool offerInInspector = true)
            where T : new()
        {
            return new SelfFix("Fix", action, offerInInspector);
        }

        public static SelfFix Create<T>(string title, Action<T> action, bool offerInInspector = true)
            where T : new()
        {
            return new SelfFix(title, action, offerInInspector);
        }
    }
}