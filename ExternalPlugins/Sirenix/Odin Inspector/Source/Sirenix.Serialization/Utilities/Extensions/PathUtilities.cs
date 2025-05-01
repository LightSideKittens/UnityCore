//-----------------------------------------------------------------------
// <copyright file="PathUtilities.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
namespace Sirenix.Serialization.Utilities
{
#pragma warning disable

    using System.IO;

    /// <summary>
    /// DirectoryInfo method extensions.
    /// </summary>
    internal static class PathUtilities
    {
        /// <summary>
        /// Determines whether the directory has a given directory in its hierarchy of children.
        /// </summary>
        /// <param name="parentDir">The parent directory.</param>
        /// <param name="subDir">The sub directory.</param>
        public static bool HasSubDirectory(this DirectoryInfo parentDir, DirectoryInfo subDir)
        {
            var parentDirName = parentDir.FullName.TrimEnd('\\', '/');

            while (subDir != null)
            {
                if (subDir.FullName.TrimEnd('\\', '/') == parentDirName)
                {
                    return true;
                }
                else
                {
                    subDir = subDir.Parent;
                }
            }

            return false;
        }
    }
}