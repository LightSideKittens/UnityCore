//-----------------------------------------------------------------------
// <copyright file="TypeInclusionFilterExtensions.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#if UNITY_EDITOR
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
using System;

namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

	public static class TypeInclusionFilterExtensions
	{
		public static bool IsValidType(this TypeInclusionFilter filter, Type type)
		{
			switch (filter)
			{
				case TypeInclusionFilter.None:
					return false;
				
				case TypeInclusionFilter.IncludeAbstracts:
					return type.IsAbstract && !type.IsInterface && !type.IsGenericType;
				
				case TypeInclusionFilter.IncludeGenerics:
					return type.IsGenericType && !type.IsInterface && !type.IsAbstract;
				
				case TypeInclusionFilter.IncludeInterfaces:
					return type.IsInterface && !type.IsGenericType;
				
				case TypeInclusionFilter.IncludeConcreteTypes:
					return !type.IsGenericType && !type.IsInterface && !type.IsAbstract;
			}
			
			bool includeConcreteTypes = (filter & TypeInclusionFilter.IncludeConcreteTypes) != 0;
			bool includeAbstracts = (filter & TypeInclusionFilter.IncludeAbstracts) != 0;
			bool includeInterfaces = (filter & TypeInclusionFilter.IncludeInterfaces) != 0;
			bool includeGenerics = (filter & TypeInclusionFilter.IncludeGenerics) != 0;

			if (!includeAbstracts && type.IsAbstract && !type.IsInterface)
			{
				return false;
			}

			if (!includeInterfaces && type.IsInterface)
			{
				return false;
			}

			if (!includeGenerics && type.IsGenericType)
			{
				return false;
			}

			if (!includeConcreteTypes && !type.IsAbstract && !type.IsInterface)
			{
				return false;
			}

			return true;
		}
	}
}
#endif