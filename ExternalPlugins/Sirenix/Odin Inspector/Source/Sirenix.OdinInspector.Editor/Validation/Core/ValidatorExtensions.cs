//-----------------------------------------------------------------------
// <copyright file="ValidatorExtensions.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#if UNITY_EDITOR
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
using System;
using Sirenix.Utilities;
using UnityEditor;

namespace Sirenix.OdinInspector.Editor.Validation
{
#pragma warning disable

    using Sirenix.OdinInspector;

    public static class ValidatorExtensions
    {
        public static ValidationResultType ToValidationResultType(this InfoMessageType messageType)
        {
            if (messageType == InfoMessageType.Error)
                return ValidationResultType.Error;
            else if (messageType == InfoMessageType.Warning)
                return ValidationResultType.Warning;
            else return ValidationResultType.Valid;
        }
        public static ValidatorSeverity ToValidatorSeverity(this InfoMessageType messageType)
        {
            if (messageType == InfoMessageType.Error)
                return ValidatorSeverity.Error;
            else if (messageType == InfoMessageType.Warning)
                return ValidatorSeverity.Warning;
            else return ValidatorSeverity.Ignore;
        }

        internal static string GetNiceValidatorTypeName(this Type t)
        {
            // Yeah I know, don't judge...

            if (t == null)
            {
                return "-";
            }

            var k = "";

            k = t.GetNiceName();
            var i = k.IndexOf('<');

            if (i >= 0)
            {
                k = k.Substring(0, i);
            }

            for (int j = 0; j < 2; j++)
            {
                if (k.FastEndsWith("Validator")) k = k.Substring(0, k.Length - "Validator".Length);
                if (k.FastEndsWith("Attribute")) k = k.Substring(0, k.Length - "Attribute".Length);
            }

            return ObjectNames.NicifyVariableName(k);
        }
    }
}
#endif