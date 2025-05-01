//-----------------------------------------------------------------------
// <copyright file="UnityLocalizationModuleDefinition.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#if UNITY_EDITOR
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
using Sirenix.Utilities.Editor;
using System;

namespace Sirenix.OdinInspector.Editor.Modules
{
#pragma warning disable

    public class UnityLocalizationModuleDefinition : ModuleDefinition
    {
        public override string ID =>  "Unity.Localization";
        public override string NiceName => "Unity.Localization support";
        public override Version LatestVersion => new Version(2, 0, 0, 5);
        public override string Description => "This massive module contains a total overhaul of and new workflow for the Unity.Localization package.";
        public override string DependenciesDescription => "com.unity.localization package v1.1.0 or above";
        public override string BuildFromPath => "Assets/Plugins/Sirenix/Odin Inspector/Modules/Unity.Localization/";

        public override bool CheckSupportsCurrentEnvironment()
        {
            return UnityPackageUtility.HasPackageInstalled("com.unity.localization", new Version(1, 1, 0));
        }
    }
}
#endif