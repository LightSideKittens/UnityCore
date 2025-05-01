//-----------------------------------------------------------------------
// <copyright file="TypeInclusionFilter.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
using System;

namespace Sirenix.OdinInspector
{
#pragma warning disable

	/// <summary> Specifies the types to include based on certain criteria. </summary>
	[Flags]
	public enum TypeInclusionFilter
	{
		None = 0,

		/// <summary> Represents types that are not interfaces, abstracts, or generics. </summary>
		IncludeConcreteTypes = 1 << 0,

		IncludeGenerics = 1 << 1,

		IncludeAbstracts = 1 << 2,

		IncludeInterfaces = 1 << 3,

		IncludeAll = IncludeConcreteTypes | IncludeGenerics | IncludeAbstracts | IncludeInterfaces,
	}
}