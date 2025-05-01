//-----------------------------------------------------------------------
// <copyright file="HideInPlayModeAttributeStateUpdater.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#if UNITY_EDITOR
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
[assembly: Sirenix.OdinInspector.Editor.RegisterStateUpdater(typeof(Sirenix.OdinInspector.Editor.StateUpdaters.HideInPlayModeAttributeStateUpdater))]

namespace Sirenix.OdinInspector.Editor.StateUpdaters
{
#pragma warning disable

    using UnityEngine;

    public sealed class HideInPlayModeAttributeStateUpdater : AttributeStateUpdater<HideInPlayModeAttribute>
    {
        public override void OnStateUpdate()
        {
            this.Property.State.Visible = !Application.isPlaying;
        }
    }
}
#endif