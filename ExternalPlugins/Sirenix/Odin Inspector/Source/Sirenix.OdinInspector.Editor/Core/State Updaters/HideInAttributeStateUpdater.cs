//-----------------------------------------------------------------------
// <copyright file="HideInAttributeStateUpdater.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#if UNITY_EDITOR
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
[assembly: Sirenix.OdinInspector.Editor.RegisterStateUpdater(typeof(Sirenix.OdinInspector.Editor.StateUpdaters.HideInAttributeStateUpdater))]

namespace Sirenix.OdinInspector.Editor.StateUpdaters
{
#pragma warning disable

    public sealed class HideInAttributeStateUpdater : AttributeStateUpdater<HideInAttribute>
    {
        private bool hide;

        protected override void Initialize()
        {
            this.hide = (OdinPrefabUtility.GetPrefabKind(this.Property) & this.Attribute.PrefabKind) != 0;
        }

        public override void OnStateUpdate()
        {
            this.Property.State.Visible = !this.hide;
        }
    }
}
#endif