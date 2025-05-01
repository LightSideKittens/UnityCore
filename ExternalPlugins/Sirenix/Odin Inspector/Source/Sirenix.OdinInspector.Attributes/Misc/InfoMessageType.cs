//-----------------------------------------------------------------------
// <copyright file="InfoMessageType.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
    
namespace Sirenix.OdinInspector
{
#pragma warning disable

    /// <summary>
    /// Type of info message box. This enum matches Unity's MessageType enum which could not be used since it is located in the UnityEditor assembly.
    /// </summary>
    public enum InfoMessageType
    {
		/// <summary>
		/// Generic message box with no type.
		/// </summary>
		None,

		/// <summary>
		/// Information message box.
		/// </summary>
		Info,

		/// <summary>
		/// Warning message box.
		/// </summary>
		Warning,

		/// <summary>
		/// Error message box.
		/// </summary>
		Error
	}
}