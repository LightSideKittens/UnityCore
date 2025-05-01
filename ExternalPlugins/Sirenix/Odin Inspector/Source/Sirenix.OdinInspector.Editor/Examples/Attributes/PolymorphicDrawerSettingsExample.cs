//-----------------------------------------------------------------------
// <copyright file="PolymorphicDrawerSettingsExample.cs" company="Sirenix ApS">
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
using Sirenix.Serialization;
using Sirenix.Utilities;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Examples
{
#pragma warning disable

	[AttributeExample(typeof(PolymorphicDrawerSettingsAttribute))]
	internal class PolymorphicDrawerSettingsExample
	{
		[ShowInInspector]
		public IDemo<int> Default;

		[Title("Show Base Type"), ShowInInspector, LabelText("On")]
		[PolymorphicDrawerSettings(ShowBaseType = true)]
		public IDemo<int> ShowBaseType_On;

		[ShowInInspector, LabelText("Off")]
		[PolymorphicDrawerSettings(ShowBaseType = false)]
		public IDemo<int> ShowBaseType_Off;

		[Title("Read Only If Not Null Reference"), ShowInInspector, LabelText("On")]
		[PolymorphicDrawerSettings(ReadOnlyIfNotNullReference = true)]
		public IDemo<int> ReadOnlyIfNotNullReference_On;

		[ShowInInspector, LabelText("Off")]
		[PolymorphicDrawerSettings(ReadOnlyIfNotNullReference = false)]
		public IDemo<int> ReadOnlyIfNotNullReference_Off;

		[Title("Non Default Constructor Preference"), ShowInInspector, LabelText("Exclude")]
		[PolymorphicDrawerSettings(NonDefaultConstructorPreference = NonDefaultConstructorPreference.Exclude)]
		public IVector2<int> NonDefaultConstructorPreference_Ignore;
		
		[ShowInInspector, LabelText("Construct Ideal")]
		[PolymorphicDrawerSettings(NonDefaultConstructorPreference = NonDefaultConstructorPreference.ConstructIdeal)]
		public IVector2<int> NonDefaultConstructorPreference_ConstructIdeal;

		[ShowInInspector, LabelText("Prefer Uninitialized")]
		[PolymorphicDrawerSettings(NonDefaultConstructorPreference = NonDefaultConstructorPreference.PreferUninitialized)]
		public IVector2<int> NonDefaultConstructorPreference_PreferUninit;

		[ShowInInspector, LabelText("Log Warning")]
		[PolymorphicDrawerSettings(NonDefaultConstructorPreference = NonDefaultConstructorPreference.LogWarning)]
		public IVector2<int> NonDefaultConstructorPreference_LogWarning;

		[Title("Create Custom Instance"), ShowInInspector]
		[PolymorphicDrawerSettings(CreateInstanceFunction = nameof(CreateInstance))]
		public IVector2<int> CreateCustomInstance;

		private IVector2<int> CreateInstance(Type type)
		{
			Debug.Log("Constructor called for " + type + '.');

			if (typeof(SomeNonDefaultCtorClass) == type)
			{
				return new SomeNonDefaultCtorClass(485);
			}

			return type.InstantiateDefault(false) as IVector2<int>;
		}

		public interface IVector2<T>
		{
			T X { get; set; }
			T Y { get; set; }
		}

		[Serializable]
		public class SomeNonDefaultCtorClass : IVector2<int>
		{
			[OdinSerialize]
			public int X { get; set; }

			[OdinSerialize]
			public int Y { get; set; }

			public SomeNonDefaultCtorClass(int x)
			{
				this.X = x;
				this.Y = (x + 1) * 4;
			}
		}

		public interface IDemo<T>
		{
			T Value { get; set; }
		}

		[Serializable]
		public class DemoSOInt32 : SerializedScriptableObject, IDemo<int>
		{
			[OdinSerialize]
			public int Value { get; set; }
		}

		[Serializable]
		public class DemoSOInt32Target : SerializedScriptableObject, IDemo<int>
		{
			[OdinSerialize]
			public int Value { get; set; }

			public int target;
		}

		[Serializable]
		public class DemoSOFloat32 : SerializedScriptableObject, IDemo<float>
		{
			[OdinSerialize]
			public float Value { get; set; }
		}

		[Serializable]
		public class Demo<T> : IDemo<T>
		{
			[OdinSerialize]
			public T Value { get; set; }
		}

		[Serializable]
		public class DemoInt32Interface : IDemo<int>
		{
			[OdinSerialize]
			public int Value { get; set; }
		}

		public class DemoInt32 : Demo<int> { }

		public struct DemoStructInt32 : IDemo<int>
		{
			[OdinSerialize]
			public int Value { get; set; }
		}
	}
}
#endif