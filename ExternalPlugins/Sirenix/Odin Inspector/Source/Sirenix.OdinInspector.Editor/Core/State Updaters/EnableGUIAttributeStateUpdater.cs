//-----------------------------------------------------------------------
// <copyright file="EnableGUIAttributeStateUpdater.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#if UNITY_EDITOR
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
[assembly: Sirenix.OdinInspector.Editor.RegisterStateUpdater(typeof(Sirenix.OdinInspector.Editor.StateUpdaters.EnableGUIAttributeStateUpdater))]

namespace Sirenix.OdinInspector.Editor.StateUpdaters
{
#pragma warning disable


    public sealed class EnableGUIAttributeStateUpdater : AttributeStateUpdater<EnableGUIAttribute>
    {
        public override void OnStateUpdate()
        {
            this.Property.State.Enabled = true;
        }
    }
}
#endif