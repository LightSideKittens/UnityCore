//-----------------------------------------------------------------------
// <copyright file="OnStateUpdateAttributeStateUpdater.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#if UNITY_EDITOR
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
[assembly: Sirenix.OdinInspector.Editor.RegisterStateUpdater(typeof(Sirenix.OdinInspector.Editor.StateUpdaters.OnStateUpdateAttributeStateUpdater))]

namespace Sirenix.OdinInspector.Editor.StateUpdaters
{
#pragma warning disable

    using Sirenix.OdinInspector.Editor.ActionResolvers;

    public sealed class OnStateUpdateAttributeStateUpdater : AttributeStateUpdater<OnStateUpdateAttribute>
    {
        private ActionResolver action;

        protected override void Initialize()
        {
            this.action = ActionResolver.Get(this.Property, this.Attribute.Action);
            this.ErrorMessage = this.action.ErrorMessage;
        }

        public override void OnStateUpdate()
        {
            this.action.DoActionForAllSelectionIndices();
            this.ErrorMessage = this.action.ErrorMessage;
        }
    }
}
#endif