//-----------------------------------------------------------------------
// <copyright file="SirenixAnimationUtility.cs" company="Sirenix ApS">
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
using Microsoft.Win32.SafeHandles;
using Sirenix.Utilities.Editor;
using UnityEngine;
using UnityEngine.Serialization;

namespace Sirenix.OdinInspector.Editor.Internal
{
#pragma warning disable

	public enum Easing
	{
		None,
		Linear,
		InSine,
		OutSine,
		InOutSine,
		InQuad,
		OutQuad,
		InOutQuad,
		InCubic,
		OutCubic,
		InOutCubic,
		InQuart,
		OutQuart,
		InOutQuart,
		InQuint,
		OutQuint,
		InOutQuint,
		InExpo,
		OutExpo,
		InOutExpo,
		InCirc,
		OutCirc,
		InOutCirc,
		InBack,
		OutBack,
		InOutBack,
		InElastic,
		OutElastic,
		InOutElastic,
		InBounce,
		OutBounce,
		InOutBounce,
	}

	public static class SirenixAnimationUtility
	{
		private static readonly object AnimationTempRefKey = new object();
		
		public struct InterpolatedFloat
		{
			public bool IsDone => this.Time >= 1.0f;
			public bool HasBegun => this.Time > 0.0f;

			public float Start;
			public float Destination;
			public float Time;
			public Easing Easing;

			public float GetValue()
			{
				if (this.Time <= 0.0f)
				{
					return this.Start;
				}

				if (this.IsDone)
				{
					return this.Destination;
				}

				return Interpolate(this.Start, this.Destination, this.Time, this.Easing);
			}

			public void Move(float speed, Easing easing = Easing.Linear)
			{
				if (this.IsDone)
				{
					return;
				}

				this.Easing = easing;

				this.Time += GUITimeHelper.LayoutDeltaTime * speed;
				this.Time = Mathf.Clamp01(this.Time);

				GUIHelper.RequestRepaint();
			}

			public void Reverse(float speed, Easing easing = Easing.Linear)
			{
				this.Easing = easing;

				this.Time -= GUITimeHelper.LayoutDeltaTime * speed;
				this.Time = Mathf.Clamp01(this.Time);

				GUIHelper.RequestRepaint();
			}

			public void ChangeDestination(float destination)
			{
				if (this.IsDone)
				{
					this.Start = this.Destination;
				}
				else if (this.HasBegun)
				{
					this.Start = this.GetValue();
				}

				this.Time = 0.0f;
				this.Destination = destination;
			}

			public void Reset() => this.Reset(this.Start);

			public void Reset(float start)
			{
				this.Start = start;
				this.Time = 0.0f;
			}

			public static implicit operator InterpolatedFloat(float value) => new InterpolatedFloat {Start = value, Time = 0.0f};
			public static implicit operator float(InterpolatedFloat interpolatedFloat) => interpolatedFloat.GetValue();
		}

		public struct InterpolatedVector2
		{
			public bool IsDone => this.Time >= 1.0f;
			public bool HasBegun => this.Time > 0.0f;
			
			public Vector2 Start;
			public Vector2 Destination;
			public float Time;
			public Easing Easing;

			public Vector2 GetValue() => new Vector2(Interpolate(this.Start.x, this.Destination.x, this.Time, this.Easing),
																  Interpolate(this.Start.y, this.Destination.y, this.Time, this.Easing));

			public void Move(float speed, Easing easing = Easing.Linear)
			{
				if (this.IsDone)
				{
					return;
				}

				this.Easing = easing;

				this.Time += GUITimeHelper.LayoutDeltaTime * speed;
				this.Time = Mathf.Clamp01(this.Time);

				GUIHelper.RequestRepaint();
			}

			public void ChangeDestination(Vector2 destination)
			{
				if (this.IsDone)
				{
					this.Start = this.Destination;
				}
				else if (this.HasBegun)
				{
					this.Start = this.GetValue();
				}

				this.Time = 0.0f;
				this.Destination = destination;
			}

			public void Reset() => this.Reset(this.Start);

			public void Reset(Vector2 start)
			{
				this.Start = start;
				this.Time = 0.0f;
			}

			public static implicit operator InterpolatedVector2(Vector2 value) => new InterpolatedVector2 {Start = value, Time = 0.0f};
			public static implicit operator Vector2(InterpolatedVector2 interpolatedVector2) => interpolatedVector2.GetValue();
		}

		public static float Interpolate(float start, float end, float time, Easing easing = Easing.Linear)
		{
			return Mathf.Lerp(start, end, ApplyEasingFunction(time, easing));
		}

		public static Vector2 Interpolate(Vector2 start, Vector2 end, float time, Easing easing = Easing.Linear)
		{
			float easeT = ApplyEasingFunction(time, easing);

			return new Vector2(Mathf.Lerp(start.x, end.x, easeT), Mathf.Lerp(start.y, end.y, easeT));
		}

		public static ref InterpolatedFloat GetTemporaryFloat(object key, float defaultValue)
		{
			GUIContext<InterpolatedFloat> result = GUIHelper.GetTemporaryContext(AnimationTempRefKey, key, (InterpolatedFloat) defaultValue);

			return ref result.Value;
		}

		private static float EaseOutBounce(float value)
		{
			const float CONST_0 = 7.5625f;
			const float CONST_1 = 2.75f;

			if (value < 1.0f / CONST_1)
			{
				return CONST_0 * value * value;
			}

			if (value < 2.0f / CONST_1)
			{
				return CONST_0 * (value -= 1.5f / CONST_1) * value + 0.75f;
			}

			if (value < 2.5f / CONST_1)
			{
				return CONST_0 * (value -= 2.25f / CONST_1) * value + 0.9375f;
			}

			return CONST_0 * (value -= 2.625f / CONST_1) * value + 0.984375f;
		}

		public static float ApplyEasingFunction(float t, Easing function)
		{
			const float S = 1.70158f;
			const float S_MULT = S * 1.525f;
			const float S_PLUS = S + 1;

			switch (function)
			{
				case Easing.None:
					return 0.0f;

				case Easing.Linear:
					return t;

				case Easing.InSine:
					return 1.0f - Mathf.Cos(t * Mathf.PI * 0.5f);

				case Easing.OutSine:
					return Mathf.Sin(t * Mathf.PI * 0.5f);

				case Easing.InOutSine:
					return -(Mathf.Cos(Mathf.PI * t) - 1.0f) * 0.5f;

				case Easing.InQuad:
					return t * t;

				case Easing.OutQuad:
					return 1.0f - (1.0f - t) * (1.0f - t);

				case Easing.InOutQuad:
					return t < 0.5f ? 2.0f * t * t : 1.0f - Mathf.Pow(-2.0f * t + 2.0f, 2.0f) * 0.5f;

				case Easing.InCubic:
					return t * t * t;

				case Easing.OutCubic:
					return 1.0f - Mathf.Pow(1.0f - t, 3.0f);

				case Easing.InOutCubic:
					return t < 0.5f ? 4.0f * t * t * t : 1.0f - Mathf.Pow(-2.0f * t + 2.0f, 3.0f) * 0.5f;

				case Easing.InQuart:
					return t * t * t * t;

				case Easing.OutQuart:
					return 1.0f - Mathf.Pow(1.0f - t, 4.0f);

				case Easing.InOutQuart:
					return t < 0.5f ? 8.0f * t * t * t * t : 1.0f - Mathf.Pow(-2.0f * t + 2.0f, 4.0f) * 0.5f;

				case Easing.InQuint:
					return t * t * t * t * t;

				case Easing.OutQuint:
					return 1.0f - Mathf.Pow(1.0f - t, 5.0f);

				case Easing.InOutQuint:
					return t < 0.5f ? 16.0f * t * t * t * t * t : 1.0f - Mathf.Pow(-2.0f * t + 2.0f, 5.0f) * 0.5f;

				case Easing.InExpo:
					return t <= 0.0f ? 0.0f : Mathf.Pow(2.0f, 10.0f * t - 10.0f);

				case Easing.OutExpo:
					return t >= 1.0f ? 1.0f : 1.0f - Mathf.Pow(2.0f, -10.0f * t);

				case Easing.InOutExpo:
					if (t <= 0.0f)
					{
						return 0.0f;
					}

					if (t >= 1.0f)
					{
						return 1.0f;
					}

					return t < 0.5f ? Mathf.Pow(2.0f, 20.0f * t - 10.0f) * 0.5f : (2.0f - Mathf.Pow(2.0f, -20.0f * t + 10.0f)) * 0.5f;

				case Easing.InCirc:
					return 1.0f - Mathf.Sqrt(1.0f - Mathf.Pow(t, 2.0f));

				case Easing.OutCirc:
					return Mathf.Sqrt(1.0f - Mathf.Pow(t - 1.0f, 2.0f));

				case Easing.InOutCirc:
					return t < 0.5f ? (1.0f - Mathf.Sqrt(1.0f - Mathf.Pow(2.0f * t, 2.0f))) * 0.5f : (Mathf.Sqrt(1.0f - Mathf.Pow(-2.0f * t + 2.0f, 2.0f)) + 1.0f) * 0.5f;

				case Easing.InBack:
					return S_PLUS * t * t * t - S * t * t;

				case Easing.OutBack:
					return 1.0f + S_PLUS * Mathf.Pow(t - 1.0f, 3.0f) + S * Mathf.Pow(t - 1.0f, 2.0f);

				case Easing.InOutBack:
					if (t < 0.5f)
					{
						return (Mathf.Pow(2.0f * t, 2.0f) * ((S_MULT + 1.0f) * 2.0f * t - S_MULT)) * 0.5f;
					}

					return (Mathf.Pow(2.0f * t - 2.0f, 2.0f) * ((S_MULT + 1.0f) * (t * 2.0f - 2.0f) + S_MULT) + 2.0f) * 0.5f;

				case Easing.InElastic:
					if (t <= 0.0f)
					{
						return 0.0f;
					}

					if (t >= 1.0f)
					{
						return 1.0f;
					}

					return -Mathf.Pow(2.0f, 10.0f * t - 10.0f) * Mathf.Sin((t * 10.0f - 10.75f) * (2.0f * Mathf.PI / 3.0f));

				case Easing.OutElastic:
					if (t <= 0.0f)
					{
						return 0.0f;
					}

					if (t >= 1.0f)
					{
						return 1.0f;
					}

					return Mathf.Pow(2.0f, -10.0f * t) * Mathf.Sin((t * 10.0f - 0.75f) * (2.0f * Mathf.PI / 3.0f)) + 1.0f;

				case Easing.InOutElastic:
					if (t <= 0.0f)
					{
						return 0.0f;
					}

					if (t >= 1.0f)
					{
						return 1.0f;
					}

					const float E_C = 2.0f * Mathf.PI / 4.5f;

					if (t < 0.5f)
					{
						return -(Mathf.Pow(2.0f, 20.0f * t - 10.0f) * Mathf.Sin((20.0f * t - 11.125f) * E_C)) * 0.5f;
					}

					return (Mathf.Pow(2.0f, -20.0f * t + 10.0f) * Mathf.Sin((20.0f * t - 11.125f) * E_C)) * 0.5f + 1.0f;

				case Easing.InBounce:
					return 1.0f - EaseOutBounce(1.0f - t);

				case Easing.OutBounce:
					return EaseOutBounce(t);

				case Easing.InOutBounce:
					return t < 0.5f ? (1.0f - EaseOutBounce(1.0f - 2.0f * t)) * 0.5f : (1.0f + EaseOutBounce(2.0f * t - 1.0f)) * 0.5f;

				default:
					throw new ArgumentOutOfRangeException(nameof(function), function, null);
			}
		}
	}
}
#endif