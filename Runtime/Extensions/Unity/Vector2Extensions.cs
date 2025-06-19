﻿using UnityEngine;

namespace LSCore.Extensions.Unity
{
    public static class LSVector2
    {
        public static readonly float oneMagnitude = Vector2.one.magnitude;
        public static readonly Vector2 half = Vector2.one / 2;
        public static readonly Vector2 one = Vector2.one;
        public static readonly Vector2 oneDir = Vector2.one.normalized;
        
        public static Vector2 Get(float f)
        {
            return new Vector2(f, f);
        }
    }
    
    public static class LSVector3
    {
        public static readonly Vector3 one = Vector3.one;
        public static readonly Vector3 oneDir = Vector3.one.normalized;
        public static readonly Vector3 half = Vector3.one / 2;

        public static Vector3 Get(float f)
        {
            return new Vector3(f, f, f);
        }
    }
    
    public static class Vector2Extensions
    {
        public static int DetermineQuadrant(this in Vector2 vector)
        {
            int signX = vector.x < 0 ? 1 : 0;
            int signY = vector.y < 0 ? 1 : 0;
            int quadrant = (signY << 1) | signX;
            
            return quadrant;
        }

        public static Vector2 Project(this in Vector2 a, Vector2 b)
        {
            float num1 = b.x * b.x + b.y * b.y; //Vector2.Dot
            if (num1 == 0f) return Vector2.zero;
            float num2 = a.x * b.x + a.y * b.y; //Vector2.Dot
            num2 /= num1;
            b.x *= num2;
            b.y *= num2;
            return b;
        }
        
        public static Vector2 Scalee(this in Vector2 a, Vector2 b)
        {
            b.x = a.x * b.x;
            b.y = a.y * b.y;
            return b;
        }
        
        public static Vector2 Lerp(this in Vector2 a, Vector2 b, float t)
        {
            t = Mathf.Clamp01(t);
            b.x = a.x + (b.x - a.x) * t;
            b.y = a.y + (b.y - a.y) * t;
            return b;
        }
        
        public static float UnclampedInverseLerp(this in Vector2 value, Vector2 a, Vector2 b)
        {
            b.x -= a.x;
            b.y -= a.y;
            a.x = value.x - a.x;
            a.y = value.y - a.y;
            
            //Vector3.Project
            float num1 = b.x * b.x + b.y * b.y; //Vector3.Dot
            if (num1 == 0f) return 0f;
            float num2 = a.x * b.x + a.y * b.y; //Vector3.Dot
            a.x = b.x * num2 / num1;
            a.y = b.y * num2 / num1;
            //Vector3.Project
            
            float t = a.magnitude / b.magnitude;
            
            if (b.x * a.x + b.y * a.y < 0f) //Vector3.Dot
                t *= -1;
            
            return t;
        }
        
        public static float InverseLerp(this in Vector2 value, Vector2 a, Vector2 b)
        {
            b.x -= a.x;
            b.y -= a.y;
            a.x = value.x - a.x;
            a.y = value.y - a.y;
            
            //Vector3.Project
            float num1 = b.x * b.x + b.y * b.y; //Vector3.Dot
            if (num1 == 0f) return 0f;
            float num2 = a.x * b.x + a.y * b.y; //Vector3.Dot
            a.x = b.x * num2 / num1;
            a.y = b.y * num2 / num1;
            //Vector3.Project

            float bMagnitude = b.magnitude;
            float t = a.magnitude / bMagnitude;
            
            if (b.x * a.x + b.y * a.y < 0) //Vector3.Dot
                t *= -1;
            
            return Mathf.InverseLerp(0f, bMagnitude, t * bMagnitude);
        }
        
        
        public static Vector2 Rotate(this Vector2 v, in float degrees)
        {
            float radianAngle = degrees * Mathf.Deg2Rad;
            float sin = Mathf.Sin(radianAngle);
            float cos = Mathf.Cos(radianAngle);
            
            float x = v.x;
            float y = v.y;
            v.x = cos * x - sin * y;
            v.y = sin * x + cos * y;
            return v;
        }
        
        public static Vector2 RotateAroundPoint(this Vector2 point, in Vector2 pivot, in float degrees)
        {
            point.x -= pivot.x;
            point.y -= pivot.y;
            
            //Rotate
            float radianAngle = degrees * Mathf.Deg2Rad;
            float sin = Mathf.Sin(radianAngle);
            float cos = Mathf.Cos(radianAngle);

            float x = point.x;
            float y = point.y;
            point.x = cos * x - sin * y;
            point.y = sin * x + cos * y;
            //Rotate
            
            point.x += pivot.x;
            point.y += pivot.y;

            return point;
        }

        public static float GetScreenAspect(this in Vector2 target)
        {
            if (LSScreen.IsPortrait)
            {
                return target.x / target.y;
            }
            
            return target.y / target.x;
        }
        
        public static float GetScreenAspect(this in Vector2 target, ScreenOrientation orientation)
        {
            if (orientation == ScreenOrientation.Portrait)
            {
                return target.x / target.y;
            }
            
            return target.y / target.x;
        }
        
        public static Vector2 Snap(this Vector2 pos, float gridStep)
        {
            pos.x = Mathf.Round(pos.x / gridStep) * gridStep;
            pos.y = Mathf.Round(pos.y / gridStep) * gridStep;
            return pos;
        }
        
        public static void SnapX(this ref Vector2 pos, float step) => pos.x = Mathf.Round(pos.x / step) * step;
        public static void SnapY(this ref Vector2 pos, float step) => pos.y = Mathf.Round(pos.y / step) * step;
    }
}