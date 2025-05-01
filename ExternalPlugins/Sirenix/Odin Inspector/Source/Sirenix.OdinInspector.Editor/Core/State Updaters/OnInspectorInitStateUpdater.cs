//-----------------------------------------------------------------------
// <copyright file="OnInspectorInitStateUpdater.cs" company="Sirenix ApS">
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

[assembly: Sirenix.OdinInspector.Editor.RegisterStateUpdater(typeof(Sirenix.OdinInspector.Editor.StateUpdaters.OnInspectorInitStateUpdater), 10000)]

namespace Sirenix.OdinInspector.Editor.StateUpdaters
{
#pragma warning disable

    public sealed class OnInspectorInitStateUpdater : AttributeStateUpdater<OnInspectorInitAttribute>
    {
        protected override void Initialize()
        {
            var action = ActionResolver.Get(this.Property, this.Attribute.Action);
            action.DoActionForAllSelectionIndices();
            this.ErrorMessage = action.ErrorMessage;
        }
    }
}
#endif