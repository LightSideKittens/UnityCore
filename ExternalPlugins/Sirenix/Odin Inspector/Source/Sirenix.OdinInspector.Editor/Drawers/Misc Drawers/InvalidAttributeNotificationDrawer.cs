//-----------------------------------------------------------------------
// <copyright file="InvalidAttributeNotificationDrawer.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#if UNITY_EDITOR
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    using Sirenix.Utilities;
    using Sirenix.Utilities.Editor;
    using System.Text;
    using UnityEditor;
    using UnityEngine;

    [DrawerPriority(-1, -1, -1), OdinDontRegister]
    public class InvalidAttributeNotificationDrawer<TInvalidAttribute> : OdinDrawer
    {
        public string errorMessage;
        public string validTypeMessage;
        public bool isFolded = true;

        protected override void Initialize()
        {
            var sb = new StringBuilder("Attribute '")
                .Append(typeof(TInvalidAttribute).GetNiceName())
                .Append("' cannot be put on property '")
                .Append(Property.Name)
                .Append("'");

            if (Property.ValueEntry != null)
            {
                sb.Append(" of base type '")
                  .Append(Property.ValueEntry.BaseValueType.GetNiceName())
                  .Append("'");
            }
            sb.Append('.');

            errorMessage = sb.ToString();

            sb.Length = 0;

            var validTypes = DrawerUtilities.InvalidAttributeTargetUtility.GetValidTargets(typeof(TInvalidAttribute));
            sb.AppendLine("The following types are valid:");
            sb.AppendLine();

            for (int i = 0; i < validTypes.Count; i++)
            {
                var type = validTypes[i];
                sb.Append(type.GetNiceName());

                if (type.IsGenericParameter)
                {
                    sb.Append(" ")
                      .Append(type.GetGenericParameterConstraintsString(useFullTypeNames: true));
                }

                sb.AppendLine();
            }

            sb.Append("Supported collections where the element type is any of the above types");

            validTypeMessage = sb.ToString();
        }

        protected override void DrawPropertyLayout(GUIContent label)
        {
            isFolded = SirenixEditorGUI.DetailedMessageBox(errorMessage, validTypeMessage, MessageType.Error, isFolded);

            this.CallNextDrawer(label);
        }
    }
}
#endif