//-----------------------------------------------------------------------
// <copyright file="DisableInInlineEditorsAttributeStateUpdater.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#if UNITY_EDITOR
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
using Sirenix.OdinInspector.Editor.Drawers;

[assembly: Sirenix.OdinInspector.Editor.RegisterStateUpdater(typeof(Sirenix.OdinInspector.Editor.StateUpdaters.DisableInInlineEditorsAttributeStateUpdater))]

namespace Sirenix.OdinInspector.Editor.StateUpdaters
{
#pragma warning disable

    public sealed class DisableInInlineEditorsAttributeStateUpdater : AttributeStateUpdater<DisableInInlineEditorsAttribute>
    {
        public override void OnStateUpdate()
        {
            // Only disable, never enable
            if (this.Property.State.Enabled && InlineEditorAttributeDrawer.CurrentInlineEditorDrawDepth > 0)
            {
                this.Property.State.Enabled = false;
            }
        }
    }
}
#endif