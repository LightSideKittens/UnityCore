//-----------------------------------------------------------------------
// <copyright file="ValidationResult.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#if UNITY_EDITOR
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
using System.Reflection;

namespace Sirenix.OdinInspector.Editor.Validation
{
#pragma warning disable

    using Sirenix.Utilities;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;
    using static Sirenix.OdinInspector.Editor.Validation.Validator;

    public delegate void ValidationUserActionPropDelegate();

    public delegate void ValidationUserActionAssetPathDelegate(string assetPath);

    public static class ResultItemExtensions
    {
        public static ref ResultItem WithFix(ref this ResultItem item, string title, Action fix, bool offerInInspector = true)
        {
            item.Fix = Fix.Create(title, fix, offerInInspector);
            return ref item;
        }

        public static ref ResultItem WithFix<T>(ref this ResultItem item, string title, Action<T> fix, bool offerInInspector = true)
            where T : new()
        {
            item.Fix = Fix.Create(title, fix, offerInInspector);
            return ref item;
        }

        public static ref ResultItem WithFix<T>(ref this ResultItem item, Action<T> fix, bool offerInInspector = true)
            where T : new()
        {
            item.Fix = Fix.Create(fix, offerInInspector);
            return ref item;
        }

        public static ref ResultItem WithFix(ref this ResultItem item, Action fix, bool offerInInspector = true)
        {
            item.Fix = Fix.Create(fix, offerInInspector);
            return ref item;
        }

        public static ref ResultItem WithFix(ref this ResultItem item, Fix fix)
        {
            item.Fix = fix;
            return ref item;
        }

        public static ref ResultItem WithContextClick(ref this ResultItem item, string path, Action onClick)
        {
            return ref item.WithContextClick(path, false, onClick);
        }

        public static ref ResultItem WithContextClick(ref this ResultItem item, string path, bool on, Action onClick)
        {
            item.OnContextClick += menu => { menu.AddItem(new GUIContent(path), on, () => onClick()); };
            return ref item;
        }

        public static ref ResultItem WithContextClick(ref this ResultItem item, Action<GenericMenu> onContextClick)
        {
            item.OnContextClick += onContextClick;
            return ref item;
        }

        public static ref ResultItem WithSceneGUI(ref this ResultItem item, Action onSceneGUI)
        {
            item.OnSceneGUI = onSceneGUI;
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

        public static ref ResultItem SetSelectionObject(ref this ResultItem resultItem, UnityEngine.Object obj)
        {
            resultItem.SelectionObject = obj;
            return ref resultItem;
        }

        public static ref ResultItem EnableRichText(ref this ResultItem resultItem)
        {
            resultItem.RichText = true;
            return ref resultItem;
        }
    }

    public struct ResultItem : System.Collections.IEnumerable
    {
        public string Message;
        public ValidationResultType ResultType;

        public ResultItemMetaData[] MetaData;
        public Fix Fix;
        public Action<GenericMenu> OnContextClick;
        public Action OnSceneGUI;
        public UnityEngine.Object SelectionObject;
        public bool RichText;

        public ResultItem(string message, ValidationResultType type)
        {
            this.Message = message;
            this.ResultType = type;
            this.MetaData = default;
            this.Fix = default;
            this.OnContextClick = default;
            this.OnSceneGUI = default;
            this.SelectionObject = default;
            this.RichText = default;
        }

        public IEnumerator GetEnumerator()
        {
            return this.MetaData?.GetEnumerator();
        }

        public static ResultItem Error(string errorMessage)
        {
            return new ResultItem(errorMessage, ValidationResultType.Error);
        }

        public static ResultItem Warning(string errorMessage)
        {
            return new ResultItem(errorMessage, ValidationResultType.Warning);
        }
    }

    public class NoFixArgs
    {
    }

    internal struct FixIdentifier : IEquatable<FixIdentifier>
    {
        public string Name;
        public MethodInfo MethodInfo;

        public FixIdentifier(MethodInfo methodInfo)
        {
            this.MethodInfo = methodInfo;
            this.Name = null;
        }

        public FixIdentifier(string name, MethodInfo methodInfo)
        {
            this.Name = name;
            this.MethodInfo = methodInfo;
        }

        public override string ToString() => this.Name;
        public bool Equals(FixIdentifier other) => Equals(MethodInfo, other.MethodInfo);
        public override bool Equals(object obj) => obj is FixIdentifier other && Equals(other);
        public static bool operator ==(FixIdentifier left, FixIdentifier right) => left.Equals(right);
        public static bool operator !=(FixIdentifier left, FixIdentifier right) => !left.Equals(right);

        public override int GetHashCode()
        {
            unchecked
            {
                return (MethodInfo != null ? MethodInfo.GetHashCode() : 0);
            }
        }
    }

    public class Fix
    {
        public bool OfferInInspector = true;
        public string Title = "Fix";
        public Delegate Action;
        public Type ArgType;

        public Fix()
        {
        }

        public Fix(Action action, bool offerInInspector)
        {
            this.Action = action;
            this.OfferInInspector = offerInInspector;
        }

        public Fix(string title, Action action, bool offerInInspector)
        {
            this.Action = action;
            this.Title = title;
            this.OfferInInspector = offerInInspector;
        }

        public static Fix Create(Action action, bool offerInInspector = true)
        {
            return new Fix(action, offerInInspector);
        }

        public static Fix Create<T>(Action<T> fix, bool offerInInspector = true)
            where T : new()
        {
            return new Fix()
            {
                Action = fix,
                OfferInInspector = offerInInspector,
                ArgType = typeof(T),
            };
        }

        public static Fix Create(string title, Action action, bool offerInInspector = true)
        {
            return new Fix(title, action, offerInInspector);
        }

        public static Fix Create<T>(string title, Action<T> fix, bool offerInInspector = true)
            where T : new()
        {
            return new Fix()
            {
                Title = title,
                Action = fix,
                OfferInInspector = offerInInspector,
                ArgType = typeof(T)
            };
        }

        internal FixIdentifier CreateIdentifier(string name)
        {
            return new FixIdentifier(name, this.Action.Method);
        }

        internal FixIdentifier CreateIdentifier()
        {
            return new FixIdentifier(this.Action.Method);
        }

        public object CreateEditorObject()
        {
            if (this.ArgType != null)
            {
                return Activator.CreateInstance(this.ArgType);
            }

            return null;
        }
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

    public sealed class ValidationResult : ICollection<ResultItem>
    {
        private static ResultItem NoResultItem;

        private ResultItem firstItem;

        private ResultItem[] items;

        private int itemsCount;

        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public string Path;

        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        [Obsolete("This value is no longer used for anything and is not passed further into the validation system.", OdinDefineSymbols.SIRENIX_INTERNAL)]
        public object ResultValue;

        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public double ValidationTimeMS;

        public ValidationSetup Setup;

        public int Count
        {
            get
            {
                if (this.FirstItemExists())
                {
                    return this.itemsCount + 1;
                }

                return this.itemsCount;
            }
        }

        public ref ResultItem this[int index]
        {
            get
            {
                if (this.FirstItemExists())
                {
                    if (index == 0) return ref this.firstItem;
                    if (this.items == null)
                    {
                        throw new IndexOutOfRangeException();
                    }

                    return ref this.items[index - 1];
                }
                else
                {
                    if (this.items == null) throw new IndexOutOfRangeException();
                    return ref this.items[index];
                }
            }
        }

        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public ref string Message => ref this.firstItem.Message;

        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public ref ValidationResultType ResultType => ref this.firstItem.ResultType;

        bool ICollection<ResultItem>.IsReadOnly => true;

        public ref ResultItem AddError(string error)
        {
            return ref this.Add(new ResultItem()
            {
                Message = error,
                ResultType = ValidationResultType.Error,
            });
        }

        public ref ResultItem AddWarning(string warning)
        {
            return ref this.Add(new ResultItem()
            {
                Message = warning,
                ResultType = ValidationResultType.Warning,
            });
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

        public ref ResultItem Add(ValidatorSeverity severity, string message)
        {
            if (severity == ValidatorSeverity.Error)
            {
                return ref this.Add(new ResultItem()
                {
                    Message = message,
                    ResultType = ValidationResultType.Error,
                });
            }
            else if (severity == ValidatorSeverity.Warning)
            {
                return ref this.Add(new ResultItem()
                {
                    Message = message,
                    ResultType = ValidationResultType.Warning,
                });
            }
            else
            {
                NoResultItem = default;
                return ref NoResultItem;
            }
        }

        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public void Explode(ref List<ValidationResult> results, double validationTimeMS = 0)
        {
            if (results == null) results = new List<ValidationResult>();

            var count = this.Count;

            if (count == 0) return;

            if (validationTimeMS == 0)
            {
                validationTimeMS = this.ValidationTimeMS;
            }

            ;

            double msSplit = validationTimeMS / count;

            for (int i = 0; i < count; i++)
            {
                ref var item = ref this[i];
                results.Add(new ValidationResult()
                {
                    firstItem = item,
                    Path = this.Path,
                    Setup = this.Setup,
                    ValidationTimeMS = msSplit,
                });
            }
        }

        public bool IsMatch(ValidationResult other)
        {
            if (object.ReferenceEquals(this, other)) return true;
            if (object.ReferenceEquals(this, null) != object.ReferenceEquals(other, null)) return false;
            if (object.ReferenceEquals(this, null)) return false;

            if (this.Message != other.Message
                || this.Path != other.Path
                || this.Setup.Validator != other.Setup.Validator
                || this.ResultType != other.ResultType
               //|| (this.ValidationActions == null) != (other.ValidationActions == null)
               //|| (this.ValidationActions != null && this.ValidationActions.Length != other.ValidationActions.Length)
               )
                return false;

            return true;
        }

        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        [Obsolete("Changes to the validation system mean that validation can no longer be safely rerun from a result.", true)]
        public void RerunValidation()
        {
            throw new NotSupportedException();
            //if (this.Setup.Validator == null)
            //    return;

            //var result = this;
            //var setupBackup = this.Setup;

            //try
            //{
            //    this.Setup.Validator.RunValidation(ref result);
            //}
            //catch (Exception ex)
            //{
            //    this.Setup = setupBackup;
            //    this.Message = "An exception was thrown during validation of " + this.Path + ": " + ex.ToString();
            //    this.ResultType = ValidationResultType.Error;
            //}
        }

        //public ArraySlice<ResultItem> GetItems()
        //{
        //    if (!this.isMultiResult) throw new InvalidOperationException("Cannot get the items of a non multi ValidationResult.");
        //    return new ArraySlice<ResultItem>(this.items, 0, this.count);
        //}

        public ValidationResult CreateCopy()
        {
            var copy = new ValidationResult();

            if (this.items != null)
            {
                var res = this.items;
                var copRes = new ResultItem[res.Length];
                for (int i = 0; i < res.Length; i++)
                {
                    copRes[i] = res[i];
                }

                copy.items = copRes;
            }

            copy.Path = this.Path;
            copy.Message = this.Message;
            copy.ResultType = this.ResultType;
            copy.Setup = this.Setup;

            //if (this.ValidationActions != null)
            //{
            //    copy.ValidationActions = new ValidationAction[this.ValidationActions.Length];

            //    for (int i = 0; i < this.ValidationActions.Length; i++)
            //    {
            //        copy.ValidationActions[i] = this.ValidationActions[i];
            //    }
            //}

            return copy;
        }

        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public bool FirstItemExists()
        {
            return this.firstItem.ResultType == ValidationResultType.Error || this.firstItem.ResultType == ValidationResultType.Warning
                                                                           || (this.itemsCount == 0 && this.firstItem.ResultType == ValidationResultType.Valid);
        }

        IEnumerator<ResultItem> IEnumerable<ResultItem>.GetEnumerator()
        {
            for (int i = 0; i < this.Count; i++)
            {
                yield return this[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            for (int i = 0; i < this.Count; i++)
            {
                yield return this[i];
            }
        }

        void ICollection<ResultItem>.Add(ResultItem item)
        {
            throw new NotSupportedException();
        }

        void ICollection<ResultItem>.Clear()
        {
            throw new NotSupportedException();
        }

        bool ICollection<ResultItem>.Contains(ResultItem item)
        {
            throw new NotSupportedException();
        }

        void ICollection<ResultItem>.CopyTo(ResultItem[] array, int arrayIndex)
        {
            throw new NotSupportedException();
        }

        bool ICollection<ResultItem>.Remove(ResultItem item)
        {
            throw new NotSupportedException();
        }
    }
}
#endif