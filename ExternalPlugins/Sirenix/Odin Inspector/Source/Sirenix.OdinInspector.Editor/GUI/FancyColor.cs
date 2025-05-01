//-----------------------------------------------------------------------
// <copyright file="FancyColor.cs" company="Sirenix ApS">
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
using System.Collections.Generic;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Internal
{
#pragma warning disable

	public struct FancyColor : IFormattable, IEquatable<FancyColor>
	{
		public enum BlendMode
		{
			Normal,
			Multiply,
			Screen,
			Overlay,
		}

		public struct BlendLayer
		{
			public FancyColor Top;
			public BlendMode BlendMode;
		}

		// TODO: this is a temporary solution until we need more complex blends
		public static readonly Stack<BlendLayer> BlendStack = new Stack<BlendLayer>();

		public static void PushBlend(FancyColor color, BlendMode blendMode) => BlendStack.Push(new BlendLayer {Top = color, BlendMode = blendMode});

		public static void PopBlend()
		{
			if (BlendStack.Count > 0)
			{
				BlendStack.Pop();
			}
		}

		public const float EQUATABLE_THRESHOLD = 0.5f / MAX_BYTE_F;
		private const float MAX_BYTE_F = byte.MaxValue;

		public static FancyColor White => new FancyColor(1.0f);

		public static FancyColor Gray => new FancyColor(0.5f);

		public static FancyColor Black => new FancyColor(0.0f);

		public static FancyColor Red => new FancyColor(1.0f, 0.0f, 0.0f);

		public static FancyColor Blue => new FancyColor(0.0f, 0.0f, 1.0f);

		public static FancyColor Green => new FancyColor(0.0f, 1.0f, 0.0f);

		public static FancyColor Yellow => new FancyColor(1.0f, 1.0f, 0.0f);

		public static FancyColor Cyan => new FancyColor(0.0f, 1.0f, 1.0f);

		public static FancyColor Magenta => new FancyColor(1.0f, 0.0f, 1.0f);

		public static FancyColor Clear => new FancyColor(0.0f, 0.0f);
		
		public FancyColor Inverse => new FancyColor(this.InverseR, this.InverseG, this.InverseB, this.InverseA);
		public float InverseR => 1.0f - this.R;
		public float InverseG => 1.0f - this.G;
		public float InverseB => 1.0f - this.B;
		public float InverseA => 1.0f - this.A;

		public byte ByteR => (byte) Mathf.Round(Mathf.Clamp01(this.R) * MAX_BYTE_F);
		public byte ByteG => (byte) Mathf.Round(Mathf.Clamp01(this.G) * MAX_BYTE_F);
		public byte ByteB => (byte) Mathf.Round(Mathf.Clamp01(this.B) * MAX_BYTE_F);
		public byte ByteA => (byte) Mathf.Round(Mathf.Clamp01(this.A) * MAX_BYTE_F);

		public float R, G, B, A;

		public FancyColor(float r, float g, float b, float a = 1.0f)
		{
			this.R = r;
			this.G = g;
			this.B = b;
			this.A = a;
		}

		public FancyColor(float value, float a = 1.0f)
		{
			this.R = value;
			this.G = value;
			this.B = value;
			this.A = a;
		}

		public static FancyColor Create32(byte r, byte g, byte b, byte a = 255)
		{
			return new FancyColor
			{
				R = r / MAX_BYTE_F,
				G = g / MAX_BYTE_F,
				B = b / MAX_BYTE_F,
				A = a / MAX_BYTE_F
			};
		}

		public static FancyColor Create32(byte value, byte a = 255)
		{
			float valueFloat = value / MAX_BYTE_F;

			return new FancyColor
			{
				R = valueFloat,
				G = valueFloat,
				B = valueFloat,
				A = a / MAX_BYTE_F
			};
		}

		public static FancyColor CreateHex(int rgb, float a = 1.0f)
		{
			return new FancyColor
			{
				R = (rgb >> 16 & 0xFF) / MAX_BYTE_F,
				G = (rgb >> 8 & 0xFF) / MAX_BYTE_F,
				B = (rgb & 0xFF) / MAX_BYTE_F,
				A = a
			};
		}

		public static FancyColor CreateHtmlString(string htmlHex)
		{
			if (ColorUtility.TryParseHtmlString(htmlHex, out Color color))
			{
				return new FancyColor
				{
					R = color.r,
					G = color.g,
					B = color.b,
					A = color.a
				};
			}

			Debug.LogError($"Failed to parse html-hex-string: {htmlHex}");

			return Magenta;
		}

		public static implicit operator FancyColor(Color color) => new FancyColor(color.r, color.g, color.b, color.a);

		public static implicit operator Color(FancyColor fancyColor)
		{
			float r, g, b, a;

			if (BlendStack.Count > 0)
			{
				BlendLayer currentBlendItem = BlendStack.Peek();

				FancyColor blendResult = fancyColor.Blend(currentBlendItem.Top, currentBlendItem.BlendMode);

				r = blendResult.R;
				g = blendResult.G;
				b = blendResult.B;
				a = blendResult.A;
			}
			else
			{
				r = fancyColor.R;
				g = fancyColor.G;
				b = fancyColor.B;
				a = fancyColor.A;
			}

			return new Color(r, g, b, a);
		}

		public static implicit operator FancyColor(Color32 color32) => new FancyColor(color32.r, color32.g, color32.b, color32.a);

		public static implicit operator Color32(FancyColor fancyColor) => new Color32(fancyColor.ByteR, fancyColor.ByteG, fancyColor.ByteB, fancyColor.ByteA);

		public void Deconstruct(out float r, out float g, out float b)
		{
			r = this.R;
			g = this.G;
			b = this.B;
		}

		public void Deconstruct(out float r, out float g, out float b, out float a)
		{
			r = this.R;
			g = this.G;
			b = this.B;
			a = this.A;
		}

		public float this[int index]
		{
			get
			{
				switch (index)
				{
					case 0: return this.R;
					case 1: return this.G;
					case 2: return this.B;
					case 3: return this.A;

					default:
						throw new ArgumentOutOfRangeException($"Expected index within the range of 0..3, the actual index was: {index}");
				}
			}

			set
			{
				switch (index)
				{
					case 0: this.R = value; break;
					case 1: this.G = value; break;
					case 2: this.B = value; break;
					case 3: this.A = value; break;

					default:
						throw new ArgumentOutOfRangeException($"Expected index within the range of 0..3, the actual index was: {index}");
				}
			}
		}

		public static bool operator ==(FancyColor self, FancyColor other) => self.Equals(other);
		public static bool operator !=(FancyColor self, FancyColor other) => !self.Equals(other);
		public static FancyColor operator +(FancyColor self, FancyColor other) => new FancyColor(self.R + other.R, self.G + other.G, self.B + other.B, self.A + other.A);
		public static FancyColor operator -(FancyColor self, FancyColor other) => new FancyColor(self.R - other.R, self.G - other.G, self.B - other.B, self.A - other.A);

		public static FancyColor operator *(FancyColor self, FancyColor other)
		{
			float r = self.R > 1.0f ? 1.0f * other.R + self.R - 1.0f : self.R * other.R;
			float g = self.G > 1.0f ? 1.0f * other.G + self.G - 1.0f : self.G * other.G;
			float b = self.B > 1.0f ? 1.0f * other.B + self.B - 1.0f : self.B * other.B;
			float a = self.A > 1.0f ? 1.0f * other.A + self.A - 1.0f : self.A * other.A;

			return new FancyColor(r, g, b, a);
		}

		public static FancyColor operator *(FancyColor self, float value)
		{
			float r = self.R > 1.0f ? 1.0f * value + self.R - 1.0f : self.R * value;
			float g = self.G > 1.0f ? 1.0f * value + self.G - 1.0f : self.G * value;
			float b = self.B > 1.0f ? 1.0f * value + self.B - 1.0f : self.B * value;
			float a = self.A > 1.0f ? 1.0f * value + self.A - 1.0f : self.A * value;

			return new FancyColor(r, g, b, a);
		}

		public static FancyColor operator *(float value, FancyColor self)
		{
			float r = self.R > 1.0f ? value * 1.0f + self.R - 1.0f : value * self.R;
			float g = self.G > 1.0f ? value * 1.0f + self.G - 1.0f : value * self.G;
			float b = self.B > 1.0f ? value * 1.0f + self.B - 1.0f : value * self.B;
			float a = self.A > 1.0f ? value * 1.0f + self.A - 1.0f : value * self.A;

			return new FancyColor(r, g, b, a);
		}

		public static bool operator <(FancyColor self, float value) => self.R < value || self.G < value || self.B < value;
		public static bool operator >(FancyColor self, float value) => self.R > value || self.G > value || self.B > value;

		public void Clamp()
		{
			this.R = Mathf.Clamp01(this.R);
			this.G = Mathf.Clamp01(this.G);
			this.B = Mathf.Clamp01(this.B);
			this.A = Mathf.Clamp01(this.A);
		}

		public FancyColor BakeBlends()
		{
			BlendLayer currentBlendItem = BlendStack.Peek();

			FancyColor result = this.Blend(currentBlendItem.Top, currentBlendItem.BlendMode);

			return result;
		}

		public string ToString(string format, IFormatProvider formatProvider)
		{
			return $"(R: {this.R}, G: {this.G}, B: {this.B}, A: {this.A}) [{this.ToHexCode()}]";
		}

		public string ToHexCode() => $"#{this.ByteR:X2}{this.ByteG:X2}{this.ByteB:X2}{this.ByteA:X2}";

		public bool Equals(float value, int index) => Math.Abs(this[index] - value) < EQUATABLE_THRESHOLD;

		public bool EqualsR(float value) => Math.Abs(this.R - value) < EQUATABLE_THRESHOLD;

		public bool EqualsG(float value) => Math.Abs(this.G - value) < EQUATABLE_THRESHOLD;

		public bool EqualsB(float value) => Math.Abs(this.B - value) < EQUATABLE_THRESHOLD;

		public bool EqualsA(float value) => Math.Abs(this.A - value) < EQUATABLE_THRESHOLD;

		public bool Equals(FancyColor other) =>
			Math.Abs(this.R - other.R) < EQUATABLE_THRESHOLD &&
			Math.Abs(this.G - other.G) < EQUATABLE_THRESHOLD &&
			Math.Abs(this.B - other.B) < EQUATABLE_THRESHOLD &&
			Math.Abs(this.A - other.A) < EQUATABLE_THRESHOLD;

		public override bool Equals(object obj) => obj is FancyColor other && this.Equals(other);

		public override int GetHashCode()
		{
			unchecked
			{
				int hashCode = this.R.GetHashCode();
				hashCode = (hashCode * 397) ^ this.G.GetHashCode();
				hashCode = (hashCode * 397) ^ this.B.GetHashCode();
				hashCode = (hashCode * 397) ^ this.A.GetHashCode();
				return hashCode;
			}
		}

		public FancyColor Lerp(FancyColor target, float t)
		{
			FancyColor result = this;

			result.R = Mathf.Lerp(result.R, target.R, t);
			result.G = Mathf.Lerp(result.G, target.G, t);
			result.B = Mathf.Lerp(result.B, target.B, t);
			result.A = Mathf.Lerp(result.A, target.A, t);

			return result;
		}

		public float Luminosity(bool includeAlpha = true)
		{
			if (includeAlpha && this.A <= 0.0f)
			{
				return 0f;
			}

			float luminosity = 0.3f * this.R + 0.59f * this.G + 0.11f * this.B;

			if (includeAlpha && this.A < 1.0f)
			{
				luminosity *= this.A;
			}

			return luminosity;
		}

		public FancyColor InvertLuminosity()
		{
			float luminosity = this.Luminosity();
			float invertedLuminosity = 1.0f - luminosity;

			float luminosityMultiplier = invertedLuminosity / luminosity;

			return new FancyColor(this.R * luminosityMultiplier, this.G * luminosityMultiplier, this.B * luminosityMultiplier, this.A);
		}

		public FancyColor Blend(FancyColor top, BlendMode blendMode)
		{
			FancyColor result;

			switch (blendMode)
			{
				case BlendMode.Normal:
					result = this;
					break;

				case BlendMode.Multiply:
					result = this * top;
					break;

				case BlendMode.Screen:
					result = (this.Inverse * top.Inverse).Inverse;
					break;

				case BlendMode.Overlay:
					if (this < 0.5f)
					{
						result = 2.0f * this * top;
						break;
					}

					result = (2.0f * this.Inverse * top.Inverse).Inverse;
					break;

				default:
					throw new ArgumentOutOfRangeException(nameof(blendMode), blendMode, null);
			}

			result.Clamp();

			if (this.A < 1.0f || top.A < 1.0f)
			{
				// TODO: this won't look correct if the bottom layer has an alpha value
				result.R = Mathf.Lerp(result.R, this.R, top.InverseA);
				result.G = Mathf.Lerp(result.G, this.G, top.InverseA);
				result.B = Mathf.Lerp(result.B, this.B, top.InverseA);
				result.A = this.A;
			}

			return result;
		}
	}
}
#endif