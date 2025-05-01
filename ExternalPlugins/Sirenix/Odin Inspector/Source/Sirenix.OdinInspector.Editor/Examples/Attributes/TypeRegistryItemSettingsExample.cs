//-----------------------------------------------------------------------
// <copyright file="TypeRegistryItemSettingsExample.cs" company="Sirenix ApS">
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
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Examples
{
#pragma warning disable

	[AttributeExample(typeof(TypeRegistryItemAttribute))]
	internal class TypeRegistryItemSettingsExample
	{
		private const string CATEGORY_PATH = "Sirenix.TypeSelector.Demo";
		private const string BASE_ITEM_NAME = "Painting Tools";
		private const string PATH = CATEGORY_PATH + "/" + BASE_ITEM_NAME;

		[TypeRegistryItem(Name = BASE_ITEM_NAME, Icon = SdfIconType.Tools, CategoryPath = CATEGORY_PATH, Priority = Int32.MinValue)]
		public abstract class Base { }

		[TypeRegistryItem(darkIconColorR: 0.8f, darkIconColorG: 0.3f,
								lightIconColorR: 0.3f, lightIconColorG: 0.1f,
								Name = "Brush", CategoryPath = PATH, Icon = SdfIconType.BrushFill, Priority = Int32.MinValue)]
		public class InheritorA : Base
		{
			public Color Color = Color.red;
			public float PaintRemaining = 0.4f;
		}

		[TypeRegistryItem(darkIconColorG: 0.8f, darkIconColorB: 0.3f,
								lightIconColorG: 0.3f, lightIconColorB: 0.1f,
								Name = "Paint Bucket", CategoryPath = PATH, Icon = SdfIconType.PaintBucket, Priority = Int32.MinValue)]
		public class InheritorB : Base
		{
			public Color Color = Color.green;
			public float PaintRemaining = 0.8f;
		}

		[TypeRegistryItem(darkIconColorB: 0.8f, darkIconColorG: 0.3f,
								lightIconColorB: 0.3f, lightIconColorG: 0.1f,
								Name = "Palette", CategoryPath = PATH, Icon = SdfIconType.PaletteFill, Priority = Int32.MinValue)]
		public class InheritorC : Base
		{
			public ColorPaletteItem[] Colors =
			{
				new ColorPaletteItem(Color.blue, 0.8f),
				new ColorPaletteItem(Color.red, 0.5f),
				new ColorPaletteItem(Color.green, 1.0f),
				new ColorPaletteItem(Color.white, 0.6f),
			};
		}

		[ShowInInspector]
		[PolymorphicDrawerSettings(ShowBaseType = false)]
		[InlineProperty]
		public Base PaintingItem;

		public struct ColorPaletteItem
		{
			public Color Color;
			public float Remaining;

			public ColorPaletteItem(Color color, float remaining)
			{
				this.Color = color;
				this.Remaining = remaining;
			}
		}
	}
}
#endif