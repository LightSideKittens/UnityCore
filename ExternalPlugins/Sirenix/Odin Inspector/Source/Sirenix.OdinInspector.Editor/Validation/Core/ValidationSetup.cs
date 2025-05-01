//-----------------------------------------------------------------------
// <copyright file="ValidationSetup.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#if UNITY_EDITOR
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
namespace Sirenix.OdinInspector.Editor.Validation
{
#pragma warning disable

    using Sirenix.Utilities;

    /// <summary>
    /// Use <see cref="Validator.InitializeResult(ref ValidationResult)"/> to initialize an empty <see cref="ValidationResult"/>. 
    /// </summary>
    public struct ValidationSetup
    {
        public object Validator;
        [System.Obsolete("This field is no longer populated by the validation system, as it was never used and caused a lot of garbage allocation.",
            OdinDefineSymbols.SIRENIX_INTERNAL
            )]
        public object Value;
        public object ParentInstance;
        public object Root;
    }
}
#endif