//-----------------------------------------------------------------------
// <copyright file="StrongListPropertyResolver.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#if UNITY_EDITOR
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    using Sirenix.Utilities;
    using Sirenix.Utilities.Editor;
    using System;
    using System.Collections.Generic;

    [ResolverPriority(-1)]
    public class StrongListPropertyResolver<TList, TElement> : BaseOrderedCollectionResolver<TList>, IMaySupportPrefabModifications
        where TList : IList<TElement>
    {
        private static bool IsArray = typeof(TList).IsArray;

        private Dictionary<int, InspectorPropertyInfo> childInfos = new Dictionary<int, InspectorPropertyInfo>();
        private List<Attribute> childAttrs;

        public bool MaySupportPrefabModifications { get { return true; } }

        public override Type ElementType { get { return typeof(TElement); } }

        protected override void Initialize()
        {
            base.Initialize();

            var propAttrs = this.Property.Attributes;
            List<Attribute> attrs = new List<Attribute>(propAttrs.Count);

            for (int i = 0; i < propAttrs.Count; i++)
            {
                var attr = propAttrs[i];
                if (attr.GetType().IsDefined(typeof(DontApplyToListElementsAttribute), true)) continue;
                attrs.Add(attr);
            }

            this.childAttrs = attrs;
        }

        public override InspectorPropertyInfo GetChildInfo(int childIndex)
        {
            if (childIndex < 0 || childIndex >= this.ChildCount)
            {
                throw new IndexOutOfRangeException();
            }

            InspectorPropertyInfo result;

            if (!this.childInfos.TryGetValue(childIndex, out result))
            {
                result = InspectorPropertyInfo.CreateValue(
                    name: CollectionResolverUtilities.DefaultIndexToChildName(childIndex),
                    order: childIndex,
                    serializationBackend: this.Property.BaseValueEntry.SerializationBackend,
                    getterSetter: new GetterSetter<TList, TElement>(
                        getter: (ref TList list) => list[childIndex],
                        setter: (ref TList list, TElement element) => list[childIndex] = element),
                    attributes: this.childAttrs);

                this.childInfos[childIndex] = result;
            }

            return result;
        }

        public override bool ChildPropertyRequiresRefresh(int index, InspectorPropertyInfo info)
        {
            return false;
        }

        public override int ChildNameToIndex(string name)
        {
            return CollectionResolverUtilities.DefaultChildNameToIndex(name);
        }

        public override int ChildNameToIndex(ref StringSlice name)
        {
            return CollectionResolverUtilities.DefaultChildNameToIndex(ref name);
        }

        protected override int GetChildCount(TList value)
        {
            return value.Count;
        }

        protected override void Add(TList collection, object value)
        {
            if (IsArray)
            {
                try
                {
                    TList newArray = (TList)(object)ArrayUtilities.CreateNewArrayWithAddedElement((TElement[])(object)collection, (TElement)value);
                    this.ReplaceArray(collection, newArray);
                }
                catch (Exception e)
                {

                }
            }
            else
            {
                collection.Add((TElement)value);
            }
        }

        protected override void InsertAt(TList collection, int index, object value)
        {
            if (IsArray)
            {
                TList newArray = (TList)(object)ArrayUtilities.CreateNewArrayWithInsertedElement((TElement[])(object)collection, index, (TElement)value);
                this.ReplaceArray(collection, newArray);
            }
            else
            {
                collection.Insert(index, (TElement)value);
            }
        }

        protected override void Remove(TList collection, object value)
        {
            if (IsArray)
            {
                int index = collection.IndexOf((TElement)value);

                if (index >= 0)
                {
                    TList newArray = (TList)(object)ArrayUtilities.CreateNewArrayWithRemovedElement((TElement[])(object)collection, index);
                    this.ReplaceArray(collection, newArray);
                }
            }
            else
            {
                collection.Remove((TElement)value);
            }
        }

        protected override void RemoveAt(TList collection, int index)
        {
            if (IsArray)
            {
                TList newArray = (TList)(object)ArrayUtilities.CreateNewArrayWithRemovedElement((TElement[])(object)collection, index);
                this.ReplaceArray(collection, newArray);
            }
            else
            {
                collection.RemoveAt(index);
            }
        }

        protected override void Clear(TList collection)
        {
            if (IsArray)
            {
                this.ReplaceArray(collection, (TList)(object)new TElement[0]);
            }
            else
            {
                collection.Clear();
            }
        }

        protected override bool CollectionIsReadOnly(TList collection)
        {
            // An array's strongly typed ICollection<T>.IsReadOnly member faultily returns true.
            // IsReadOnly is always supposed to be false on arrays (!!!!), so we enforce that manually.
            // Many Bothans died to bring us this information.
            if (IsArray) return false;
            return collection.IsReadOnly;
        }

        private void ReplaceArray(TList oldArray, TList newArray)
        {
            if (!this.Property.ValueEntry.SerializationBackend.SupportsCyclicReferences)
            {
                for (int i = 0; i < this.ValueEntry.ValueCount; i++)
                {
                    if (object.ReferenceEquals(this.ValueEntry.Values[i], oldArray))
                    {
                        this.ValueEntry.Values[i] = newArray;
                        (this.ValueEntry as IValueEntryActualValueSetter).SetActualValue(i, newArray);
                    }
                }
            }
            else
            {
                this.ReplaceArrayRecursive(this.Property.Tree.RootProperty, oldArray, newArray);
                //foreach (var prop in this.Property.Tree.EnumerateTree(true))
                //{
                //    if (prop.Info.PropertyType == PropertyType.Value && !prop.Info.TypeOfValue.IsValueType)
                //    {
                //        var valueEntry = prop.ValueEntry;

                //        if (!valueEntry.SerializationBackend.SupportsCyclicReferences) continue;

                //        if (prop.ChildResolver is ICollectionResolver)
                //        {
                //            prop.Children.Update();
                //        }

                //        for (int i = 0; i < valueEntry.ValueCount; i++)
                //        {
                //            object obj = valueEntry.WeakValues[i];

                //            if (object.ReferenceEquals(oldArray, obj))
                //            {
                //                valueEntry.WeakValues[i] = newArray;
                //                (valueEntry as IValueEntryActualValueSetter).SetActualValue(i, newArray);
                //            }
                //        }
                //    }
                //}
            }
        }

        private void ReplaceArrayRecursive(InspectorProperty prop, TList oldArray, TList newArray)
        {
            if (prop.Info.PropertyType == PropertyType.Value && !prop.Info.TypeOfValue.IsValueType)
            {
                var valueEntry = prop.ValueEntry;

                if (valueEntry.SerializationBackend.SupportsCyclicReferences)
                {
                    for (int i = 0; i < valueEntry.ValueCount; i++)
                    {
                        object obj = valueEntry.WeakValues[i];

                        if (object.ReferenceEquals(oldArray, obj))
                        {
                            valueEntry.WeakValues[i] = newArray;
                            (valueEntry as IValueEntryActualValueSetter).SetActualValue(i, newArray);
                        }
                    }
                }

            }

            if (prop.ChildResolver is ICollectionResolver)
            {
                prop.Children.Update();
            }

            for (int i = 0; i < prop.Children.Count; i++)
            {
                ReplaceArrayRecursive(prop.Children[i], oldArray, newArray);
            }
        }
    }
}
#endif