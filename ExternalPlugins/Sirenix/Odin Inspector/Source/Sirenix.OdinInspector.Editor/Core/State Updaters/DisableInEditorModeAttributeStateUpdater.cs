//-----------------------------------------------------------------------
// <copyright file="DisableInEditorModeAttributeStateUpdater.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#if UNITY_EDITOR
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
[assembly: Sirenix.OdinInspector.Editor.RegisterStateUpdater(typeof(Sirenix.OdinInspector.Editor.StateUpdaters.DisableInEditorModeAttributeStateUpdater))]

namespace Sirenix.OdinInspector.Editor.StateUpdaters
{
#pragma warning disable

    using UnityEngine;

    public class DisableInEditorModeAttributeStateUpdater : AttributeStateUpdater<DisableInEditorModeAttribute>
    {
        public override void OnStateUpdate()
        {
            // Only disable, never enable
            if (this.Property.State.Enabled && !Application.isPlaying)
            {
                this.Property.State.Enabled = false;
            }
        }
    }
}
#endif