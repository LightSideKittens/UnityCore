//-----------------------------------------------------------------------
// <copyright file="OnInspectorDisposeStateUpdater.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#if UNITY_EDITOR
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
using Sirenix.OdinInspector.Editor.ActionResolvers;
using System;

[assembly: Sirenix.OdinInspector.Editor.RegisterStateUpdater(typeof(Sirenix.OdinInspector.Editor.StateUpdaters.OnInspectorDisposeStateUpdater), -10000)]

namespace Sirenix.OdinInspector.Editor.StateUpdaters
{
#pragma warning disable

    public sealed class OnInspectorDisposeStateUpdater : AttributeStateUpdater<OnInspectorDisposeAttribute>, IDisposable
    {
        private ActionResolver action;

        protected override void Initialize()
        {
            this.action = ActionResolver.Get(this.Property, this.Attribute.Action);
            this.ErrorMessage = this.action.ErrorMessage;
        }

        public void Dispose()
        {
            this.action.DoActionForAllSelectionIndices();
        }
    }
}
#endif