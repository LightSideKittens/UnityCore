//-----------------------------------------------------------------------
// <copyright file="IValidatorLocator.cs" company="Sirenix ApS">
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

    using System;
    using System.Collections.Generic;
    using System.Reflection;

    public interface IValidatorLocator
    {
        IList<SceneValidator> GetSceneValidators(SceneReference scene);

        bool PotentiallyHasValidatorsFor(InspectorProperty property);

        IList<Validator> GetValidators(InspectorProperty property);

        Func<Type, bool> CustomValidatorFilter { get; set; }
    }
}
#endif