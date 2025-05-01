//-----------------------------------------------------------------------
// <copyright file="DisableInNonPrefabsAttributeStateUpdater.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#if UNITY_EDITOR
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
[assembly: Sirenix.OdinInspector.Editor.RegisterStateUpdater(typeof(Sirenix.OdinInspector.Editor.StateUpdaters.DisableInNonPrefabsAttributeStateUpdater))]

namespace Sirenix.OdinInspector.Editor.StateUpdaters
{
#pragma warning disable

#pragma warning disable CS0618 // Type or member is obsolete
    public sealed class DisableInNonPrefabsAttributeStateUpdater : AttributeStateUpdater<DisableInNonPrefabsAttribute>
#pragma warning restore CS0618 // Type or member is obsolete
    {
        private bool disable;

        protected override void Initialize()
        {
            this.disable = (OdinPrefabUtility.GetPrefabKind(this.Property) & (PrefabKind.NonPrefabInstance)) != 0;
        }

        public override void OnStateUpdate()
        {
            // Only disable, never enable
            if (this.Property.State.Enabled && this.disable)
            {
                this.Property.State.Enabled = false;
            }
        }
    }
}
#endif