//-----------------------------------------------------------------------
// <copyright file="ECSModuleDefinition.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#if UNITY_EDITOR
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
namespace Sirenix.OdinInspector.Editor.Modules
{
#pragma warning disable

    using Sirenix.Utilities.Editor;
    using System;

    public class ECSModuleDefinition : ModuleDefinition
    {
        public override string ID { get { return "Unity.Entities"; } }

        public override string BuildFromPath
        {
            get
            {
                return "../Sirenix Solution/Sirenix.OdinInspector.SmallModules/Packages/com.unity.entities/";
            }
        }

        public override Version LatestVersion
        {
            get
            {
                return new Version(1, 0, 1, 0);
            }
        }

        public override bool UnstableExperimental { get { return true; } }

        public override string NiceName
        {
            get
            {
                return "Unity.Entities support";
            }
        }

        public override string Description
        {
            get
            {
                return @"This module adds an Entity Component System inspector integration to Odin.

PLEASE NOTE that since Unity's ECS systems are still unstable and under development, this module is currently considered EXPERIMENTAL, and is *KNOWN* to be unstable, particularly in cases where entities are added/removed every frame.

Please report issues with (along with reproduction projects) at https://bitbucket.org/sirenix/odin-inspector/issues";
            }
        }

        public override string DependenciesDescription
        {
            get
            {
                return "com.unity.entities package v0.1.1+";
            }
        }


        public override bool CheckSupportsCurrentEnvironment()
        {
            return UnityPackageUtility.HasPackageInstalled("com.unity.entities", new Version(0, 1, 1));
        }
    }
}
#endif