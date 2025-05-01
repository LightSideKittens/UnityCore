//-----------------------------------------------------------------------
// <copyright file="OdinPropertyResolver.cs" company="Sirenix ApS">
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

    using Sirenix.Utilities.Editor;
    using System;
    using System.Collections.Generic;
    using System.Reflection.Emit;
    using System.Runtime.CompilerServices;

    public abstract class OdinPropertyResolver
    {
        private bool hasUpdatedChildCountEver = false;
        private int lastUpdatedTreeID = -1;
        private int childCount;
        private bool hasChildCountConflict;
        private int maxChildCountSeen;
        private Type resolverForType;

        private static readonly Dictionary<Type, Func<OdinPropertyResolver>> Resolver_EmittedCreator_Cache = new Dictionary<Type, Func<OdinPropertyResolver>>(FastTypeComparer.Instance);

        public bool HasChildCountConflict 
        { 
            get
            {
                this.UpdateChildCountIfNeeded();
                return this.hasChildCountConflict;
            }
            protected set
            {
                this.hasChildCountConflict = value;
            }
        }

        public int MaxChildCountSeen
        { 
            get
            {
                this.UpdateChildCountIfNeeded();
                return this.maxChildCountSeen;
            }
            protected set
            {
                this.maxChildCountSeen = value;
            }
        }

        public static OdinPropertyResolver Create(Type resolverType, InspectorProperty property)
        {
            if (resolverType == null)
            {
                throw new ArgumentNullException("resolverType");
            }

            if (property == null)
            {
                throw new ArgumentNullException("property");
            }

            if (!typeof(OdinPropertyResolver).IsAssignableFrom(resolverType))
            {
                throw new ArgumentException("Type is not a PropertyResolver");
            }

            //var result = (OdinPropertyResolver)Activator.CreateInstance(resolverType);

            Func<OdinPropertyResolver> creator;

            if (!Resolver_EmittedCreator_Cache.TryGetValue(resolverType, out creator))
            {
                var builder = new DynamicMethod("OdinPropertyResolver_EmittedCreator_" + Guid.NewGuid(), typeof(OdinPropertyResolver), Type.EmptyTypes);
                var il = builder.GetILGenerator();

                il.Emit(OpCodes.Newobj, resolverType.GetConstructor(Type.EmptyTypes));
                il.Emit(OpCodes.Ret);

                creator = (Func<OdinPropertyResolver>)builder.CreateDelegate(typeof(Func<OdinPropertyResolver>));
                Resolver_EmittedCreator_Cache.Add(resolverType, creator);
            }

            var result = creator();
            result.Property = property;
			if (result.Property.ValueEntry != null)
				result.resolverForType = property.ValueEntry.TypeOfValue;
			result.Initialize();
            return result;
        }

        public static T Create<T>(InspectorProperty property) where T : OdinPropertyResolver, new()
        {
            if (property == null)
            {
                throw new ArgumentNullException("property");
            }

            var result = new T();
            result.Property = property;
            if (result.Property.ValueEntry != null)
                result.resolverForType = property.ValueEntry.TypeOfValue;
            result.Initialize();
            return result;
        }

        protected virtual void Initialize()
        {
        }

        public virtual Type ResolverForType { get { return this.resolverForType; } }

        public InspectorProperty Property { get; private set; }

        public virtual bool IsCollection { get { return this is ICollectionResolver; } }

        public int ChildCount
        {
            get
            {
                this.UpdateChildCountIfNeeded();
                return this.childCount;
            }
        }

        [MethodImpl((MethodImplOptions)0x100)]  // Set aggressive inlining flag, for the runtimes that understand that
        private void UpdateChildCountIfNeeded()
        {
            var treeId = this.Property.Tree.UpdateID;

            if (this.lastUpdatedTreeID != treeId || !this.hasUpdatedChildCountEver)
            {
                this.lastUpdatedTreeID = treeId;
                this.hasUpdatedChildCountEver = true;
                this.childCount = this.CalculateChildCount();
            }
        }

        public abstract InspectorPropertyInfo GetChildInfo(int childIndex);

        public abstract int ChildNameToIndex(string name);

#if SIRENIX_INTERNAL
        // We force our internal code to implement this for better overall performance
        public abstract int ChildNameToIndex(ref StringSlice name);
#else
        public virtual int ChildNameToIndex(ref StringSlice name)
        {
            return this.ChildNameToIndex(name.ToString());
        }
#endif

        protected abstract int CalculateChildCount();

        public virtual bool CanResolveForPropertyFilter(InspectorProperty property)
        {
            return true;
        }

        public void ForceUpdateChildCount()
        {
            if (this.hasUpdatedChildCountEver) // If we've never updated the child count yet, there's no reason to actually do this, as the latest value will be given by ChildCount anyways
            {
                this.lastUpdatedTreeID = this.Property.Tree.UpdateID;
                this.childCount = this.CalculateChildCount();
            }
        }
    }

    public abstract class OdinPropertyResolver<TValue> : OdinPropertyResolver
    {
        private IPropertyValueEntry<TValue> valueEntry;

        public IPropertyValueEntry<TValue> ValueEntry 
        { 
            get 
            {
                if (this.valueEntry == null)
                {
                    this.valueEntry = this.Property.TryGetTypedValueEntry<TValue>();
                }

                return this.valueEntry;
            }
        }

        protected virtual bool AllowNullValues { get { return false; } }

        protected sealed override int CalculateChildCount()
        {
            var valueEntry = this.ValueEntry;

            this.HasChildCountConflict = false;
            int count = int.MaxValue;
            this.MaxChildCountSeen = int.MinValue;

            for (int i = 0; i < valueEntry.ValueCount; i++)
            {
                var value = valueEntry.Values[i];
                
                int indexCount;

                if (this.AllowNullValues)
                {
                    indexCount = this.GetChildCount(value);
                }
                else
                {
                    indexCount = value != null ? this.GetChildCount(value) : 0;
                }

                if (count != int.MaxValue && count != indexCount)
                {
                    this.HasChildCountConflict = true;
                }

                if (indexCount < count)
                {
                    count = indexCount;
                }

                if (indexCount > this.MaxChildCountSeen)
                {
                    this.MaxChildCountSeen = indexCount;
                }
            }

            return count;
        }

        protected abstract int GetChildCount(TValue value);
    }

    public abstract class OdinPropertyResolver<TValue, TAttribute> : OdinPropertyResolver<TValue> where TAttribute : Attribute
    {
    }
}
#endif