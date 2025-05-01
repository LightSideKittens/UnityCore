//-----------------------------------------------------------------------
// <copyright file="SirenixGlobalConfigAttribute.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
namespace Sirenix.Utilities
{
#pragma warning disable

    /// <summary>
    /// <para>This attribute is used by classes deriving from GlobalConfig and specifies the menu item path for the preference window and the asset path for the generated config file.</para>
    /// <para>The scriptable object created will be located at the OdinResourcesConigs path unless other is specified.</para>
    /// <para>Classes implementing this attribute will be part of the Odin Preferences window.</para>
    /// </summary>
    /// <seealso cref="SirenixEditorConfigAttribute"/>
    public class SirenixGlobalConfigAttribute : GlobalConfigAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SirenixGlobalConfigAttribute"/> class.
        /// </summary>
        public SirenixGlobalConfigAttribute()
         : base(SirenixAssetPaths.OdinResourcesConfigsPath)
        {
        }
    }
}