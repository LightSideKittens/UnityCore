//-----------------------------------------------------------------------
// <copyright file="NonSerializedShowInInspectorWarningAttributeDrawer.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#if UNITY_EDITOR
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
namespace Sirenix.OdinInspector.Editor.Drawers
{
#pragma warning disable

    using UnityEngine;
    using Sirenix.Utilities.Editor;
    using Sirenix.Serialization;

    /// <summary>
    /// Draws a warning message for non-serialized properties that sports both the SerializeField and the ShowInInspector attribute.
    /// </summary>
    [DrawerPriority(1, 0, 0)]
    public class NonSerializedShowInInspectorWarningAttributeDrawer : OdinAttributeDrawer<ShowInInspectorAttribute>
    {
        /// <summary>
        /// Determines if the drawer can draw the property.
        /// </summary>
        /// <param name="property">The property to test.</param>
        /// <returns><c>true</c> if the drawer can draw the property; otherwise <c>false</c>.</returns>
        protected override bool CanDrawAttributeProperty(InspectorProperty property)
        {
            return property.Info.PropertyType == PropertyType.Value;
        }

        /// <summary>
        /// Initializes the drawer.
        /// </summary>
        protected override void Initialize()
        {        
            if ( GlobalSerializationConfig.Instance.HideNonSerializedShowInInspectorWarningMessages ||
                (this.Property.Info.HasSingleBackingMember
                && this.Property.Info.SerializationBackend == SerializationBackend.None
                && this.Property.Info.GetMemberInfo().IsDefined(typeof(SerializeField), true)) == false)
            {
                this.SkipWhenDrawing = true;
            }
        }

        /// <summary>
        /// Draws the warning message and calls the next drawer.
        /// </summary>
        /// <param name="label">The label for the property.</param>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            SirenixEditorGUI.WarningMessageBox(
                "You have used the SerializeField and the ShowInInspector attributes together, but the member is not serialized.\n" +
                "Are you certain that is correct?\n" +
                "You can try using the Serialization Debugger, or you can disable this message from the preferences window.");

            this.CallNextDrawer(label);
        }
    }
}
#endif