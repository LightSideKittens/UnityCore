//-----------------------------------------------------------------------
// <copyright file="ActionResolverContext.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#if UNITY_EDITOR
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
namespace Sirenix.OdinInspector.Editor.ActionResolvers
{
#pragma warning disable

    using System;

    /// <summary>
    /// This struct contains all of an ActionResolver's configurations and values it needs to function. For performance and simplicity reasons, this is a single very large struct that lives on an ActionResolver instance and is passed around by ref to anything that needs it.
    /// </summary>
    public struct ActionResolverContext
    {
        /// <summary>
        /// The property that *provides* the context for the action resolution. This is the instance that was passed to the resolver when it was created. Note that this is different from <see cref="ContextProperty"/>, which is based on this value, but almost always isn't the same InspectorProperty instance.
        /// </summary>
        public InspectorProperty Property;

        /// <summary>
        /// The error message, if a valid action resolution wasn't found, or if creation of the action resolver failed because <see cref="ResolvedString"/> was invalid, or if the action was executed but threw an exception. (In this last case, <see cref="ErrorMessageIsDueToException"/> will be true.)
        /// </summary>
        public string ErrorMessage;

        /// <summary>
        /// The named values that are available to the action resolver. Use this field only to get and set named values - once the ValueResolver has been created, new named values will have no effect.
        /// </summary>
        public NamedValues NamedValues;

        /// <summary>
        /// This will be true if <see cref="ErrorMessage"/> is not null and the message was caused by an exception thrown by code invoked during execution of the resolved action.
        /// </summary>
        public bool ErrorMessageIsDueToException;

        /// <summary>
        /// Whether exceptions thrown during action execution should be logged to the console.
        /// </summary>
        public bool LogExceptions;

        /// <summary>
        /// The string that is resolved to perform an action.
        /// </summary>
        public string ResolvedString
        {
            get
            {
                return this.resolvedString;
            }

            set
            {
                if (this.IsResolved) throw new InvalidOperationException("ResolvedString cannot be set after a context has been resolved!");
                this.resolvedString = value;
            }
        }

        /// <summary>
        /// Whether the action resolver should sync ref parameters of invoked methods with named values. If this is true, then if a ref or out parameter value is changed during action execution, the named value associated with that parameter will also be changed to the same value.
        /// </summary>
        public bool SyncRefParametersWithNamedValues
        {
            get
            {
                return this.syncRefParametersWithNamedValues;
            }

            set
            {
                if (this.IsResolved) throw new InvalidOperationException("SyncRefParametersWithNamedValues cannot be set after a context has been resolved!");
                this.syncRefParametersWithNamedValues = value;
            }
        }

        private InspectorProperty propertyUsedForContextProperty;
        private InspectorProperty contextProperty;

        private bool syncRefParametersWithNamedValues;
        private string resolvedString;

        /// <summary>
        /// Whether this context has been resolved.
        /// </summary>
        public bool IsResolved { get; private set; }

        /// <summary>
        /// The type that is the parent of the action resolution, ie, the type that is the context. This is the same as <see cref="ContextProperty"/>.ValueEntry.TypeOfValue.
        /// </summary>
        public Type ParentType { get { return this.ContextProperty.ValueEntry.TypeOfValue; } }

        /// <summary>
        /// The property that *is* the context for the action resolution. This is not the instance that was passed to the resolver when it was created, but this value is based on that instance. This is the property that provides the actual context - for example, if <see cref="Property"/> is for a member of a type - or for an element in a collection contained by a member - this value will be the parent property for the type that contains that member. Only if <see cref="Property"/> is the tree's root property is <see cref="ContextProperty"/> the same as <see cref="Property"/>.
        /// </summary>
        public InspectorProperty ContextProperty
        {
            get
            {
                if (this.contextProperty == null || this.propertyUsedForContextProperty != this.Property)
                {
                    this.propertyUsedForContextProperty = this.Property;
                    var nearestValueProperty = this.Property.ParentValueProperty;

                    while (nearestValueProperty != null && nearestValueProperty.ChildResolver is ICollectionResolver)
                    {
                        nearestValueProperty = nearestValueProperty.ParentValueProperty;
                    }

                    if (nearestValueProperty == null)
                    {
                        this.contextProperty = this.Property.Tree.RootProperty;
                    }
                    else
                    {
                        this.contextProperty = nearestValueProperty;
                    }
                }

                return this.contextProperty;
            }
        }

        /// <summary>
        /// Gets the parent value which provides the context of the resolver.
        /// </summary>
        /// <param name="selectionIndex">The selection index of the parent value to get.</param>
        public object GetParentValue(int selectionIndex)
        {
            return this.ContextProperty.ValueEntry.WeakValues[selectionIndex];
        }

        /// <summary>
        /// Sets the parent value which provides the context of the resolver.
        /// </summary>
        /// <param name="selectionIndex">The selection index of the parent value to set.</param>
        /// <param name="value">The value to set.</param>
        public void SetParentValue(int selectionIndex, object value)
        {
            this.ContextProperty.ValueEntry.WeakValues[selectionIndex] = value;
        }

        private static readonly NamedValueGetter PropertyGetter = (ref ActionResolverContext context, int selectionIndex) => context.Property;
        private static readonly NamedValueGetter ValueGetter = (ref ActionResolverContext context, int selectionIndex) => context.Property.ValueEntry.WeakValues[selectionIndex];
        private static readonly NamedValueGetter RootGetter = (ref ActionResolverContext context, int selectionIndex) => context.Property.Tree.RootProperty.ValueEntry.WeakValues[selectionIndex];

        /// <summary>
        /// Adds the default named values of "property" and "value" to the context's named values.
        /// This method is usually automatically invoked when a resolver is created, so there
        /// is no need to invoke it manually.
        /// </summary>
        public void AddDefaultContextValues()
        {
            this.NamedValues.Add("property", typeof(InspectorProperty), PropertyGetter);

            if (this.Property.ValueEntry != null)
            {
                this.NamedValues.Add("value", this.Property.ValueEntry.BaseValueType, ValueGetter);
            }
            
            this.NamedValues.Add("root", this.Property.Tree.RootProperty.ValueEntry.BaseValueType, RootGetter);
        }

        public void MarkResolved()
        {
            if (this.IsResolved) throw new InvalidOperationException("This context has already been marked resolved!");
            this.IsResolved = true;
        }

        public static ActionResolverContext CreateDefault(InspectorProperty property, string resolvedString, params NamedValue[] namedValues)
        {
            var result = new ActionResolverContext();

            result.Property = property;
            result.ResolvedString = resolvedString;
            result.LogExceptions = true;

            if (namedValues != null)
            {
                for (int i = 0; i < namedValues.Length; i++)
                {
                    result.NamedValues.Add(namedValues[i]);
                }
            }

            result.AddDefaultContextValues();

            return result;
        }
    }
}
#endif