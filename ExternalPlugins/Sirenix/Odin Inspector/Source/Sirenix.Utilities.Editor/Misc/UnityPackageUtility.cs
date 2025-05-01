//-----------------------------------------------------------------------
// <copyright file="UnityPackageUtility.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#if UNITY_EDITOR
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
namespace Sirenix.Utilities.Editor
{
#pragma warning disable

    using System;
    using UnityEditor;
    using UnityEngine;

    public static class UnityPackageUtility
    {
        [Serializable]
        private struct PackageInfo
        {
            public string name;
            public string version;
        }

        public static bool HasPackageInstalled(string requiredPackage, Version minimumVersion)
        {
            var path = "Packages/" + requiredPackage + "/package.json";

            var asset = AssetDatabase.LoadAssetAtPath<TextAsset>(path);

            if (asset == null) return false;

            var info = JsonUtility.FromJson<PackageInfo>(asset.text);

            if (info.name != requiredPackage) return false;

            int dashIndex = info.version.IndexOf('-');

            if (dashIndex > -1)
            {
                info.version = info.version.Substring(0, dashIndex);
            }

            Version parsedVersion;

            try
            {
                parsedVersion = new Version(info.version);
            }
            catch
            {
                Debug.LogError("Failed to parse package version string '" + info.version + "' into System.Version");
                return false;
            }

            return parsedVersion >= minimumVersion;
        }
    }
}
#endif