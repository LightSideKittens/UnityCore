//-----------------------------------------------------------------------
// <copyright file="OdinInspectorVersion.cs" company="Sirenix ApS">
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

    /// <summary>
    /// Installed Odin Inspector Version Info.
    /// </summary>
    public static class OdinInspectorVersion
    {
        private static string version;
        private static string buildName;
        private static string licensee;

        /// <summary>
        /// Gets the name of the current running version of Odin Inspector.
        /// </summary>
        public static string BuildName
        {
            get
            {
                if (buildName == null)
                {
                    var attribute = typeof(InspectorConfig).Assembly.GetAttribute<SirenixBuildNameAttribute>(true);
                    buildName = attribute != null ? attribute.BuildName : "Source Code";
                }

                return buildName;
            }
        }

        public static bool HasLicensee { get { return !string.IsNullOrEmpty(Licensee); } }

        public static string Licensee
        {
            get
            {
#if SIRENIX_INTERNAL
                return "Some Licensee Whatever";
#endif

#pragma warning disable CS0162 // Unreachable code detected
                if (licensee == null)
                {
                    if (!BakedValues.TryGetBakedValue<string>("Licensee", out licensee))
                    {
                        licensee = "";
                    }
                }
#pragma warning restore CS0162 // Unreachable code detected

                return licensee;
            }
        }

        /// <summary>
        /// Gets the current running version of Odin Inspector.
        /// </summary>
        public static string Version
        {
            get
            {
                if (version == null)
                {
                    var attribute = typeof(InspectorConfig).Assembly.GetAttribute<SirenixBuildVersionAttribute>(true);
                    version = attribute != null ? attribute.Version : "Source Code Mode";
                }

                return version;
            }
        }

        /// <summary>
        /// Whether the current version of Odin is an enterprise version.
        /// </summary>
        public static bool IsEnterprise
        {
            get
            {
                return false;
            }
        }
    }
}
#endif