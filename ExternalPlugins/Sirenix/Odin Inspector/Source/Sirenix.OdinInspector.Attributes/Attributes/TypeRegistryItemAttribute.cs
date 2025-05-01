//-----------------------------------------------------------------------
// <copyright file="TypeRegistryItemAttribute.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
using System;
using UnityEngine;

namespace Sirenix.OdinInspector
{
#pragma warning disable

	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface | AttributeTargets.Enum)]
	public class TypeRegistryItemAttribute : Attribute
	{
		public string Name;
		public string CategoryPath;
		public SdfIconType Icon;
		public Color? LightIconColor;
		public Color? DarkIconColor;
		public int Priority;

		public TypeRegistryItemAttribute(string name = null,
													string categoryPath = null,
													SdfIconType icon = SdfIconType.None,
													float lightIconColorR = 0.0f,
													float lightIconColorG = 0.0f,
													float lightIconColorB = 0.0f,
													float lightIconColorA = 0.0f,
													float darkIconColorR = 0.0f,
													float darkIconColorG = 0.0f,
													float darkIconColorB = 0.0f,
													float darkIconColorA = 0.0f,
													int priority = 0)
		{
			this.Name = name;
			this.CategoryPath = categoryPath;
			this.Icon = icon;

			bool hasLightColor = lightIconColorR != 0.0f || lightIconColorG != 0.0f || lightIconColorB != 0.0f || lightIconColorA > 0.0f;

			if (hasLightColor)
			{
				float alpha = lightIconColorA > 0.0f ? lightIconColorA : 1.0f;

				this.LightIconColor = new Color(lightIconColorR, lightIconColorG, lightIconColorB, alpha);
			}
			else
			{
				this.LightIconColor = null;
			}

			bool hasDarkColor = darkIconColorR != 0.0f || darkIconColorG != 0.0f || darkIconColorB != 0.0f || darkIconColorA > 0.0f;

			if (hasDarkColor)
			{
				float alpha = darkIconColorA > 0.0f ? darkIconColorA : 1.0f;

				this.DarkIconColor = new Color(darkIconColorR, darkIconColorG, darkIconColorB, alpha);
			}
			else
			{
				this.DarkIconColor = null;
			}

			this.Priority = priority;
		}
	}
}