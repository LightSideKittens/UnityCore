//-----------------------------------------------------------------------
// <copyright file="UnityAddressablesModuleDefinition.cs" company="Sirenix ApS">
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

    public class UnityAddressablesModuleDefinition : ModuleDefinition
    {
        public override string ID
        {
            get
            {
                return "Unity.Addressables";
            }
        }

        public override string NiceName
        {
            get
            {
                return "Unity.Addressables support";
            }
        }

        public override Version LatestVersion
        {
            get
            {
                return new Version(1, 1, 0, 11);
            }
        }

        public override string Description
        {
            get
            {
                return "This small module contains a set of custom drawer and validator implementations to bring Odin support to Unity.Addressables in the inspector.";
            }
        }

        public override string DependenciesDescription
        {
            get
            {
                return "com.unity.addressables package v1.20+";
            }
        }

        public override string BuildFromPath
        {
            get
            {
                return "Assets/Plugins/Sirenix/Odin Inspector/Modules/Unity.Addressables/";
            }
        }

        public override bool CheckSupportsCurrentEnvironment()
        {
            return UnityPackageUtility.HasPackageInstalled("com.unity.addressables", new Version(1, 20));
        }

        public override bool UnstableExperimental { get { return false; } }
    }
}
#endif