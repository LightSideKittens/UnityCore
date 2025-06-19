﻿using UnityEngine;

namespace LSCore.Extensions.Unity
{
    public static class Vector3Extensions
    {
        public static void X(ref this Vector3 a, float x)
        {
            a.x = x;
        }
        
        public static void Y(ref this Vector3 a, float y)
        {
            a.y = y;
        }
        
        public static Vector3 SetByIndex(this Vector3 a, float value, int index)
        {
            a[index] = value;
            return a;
        }
        
        public static Vector3 Divide(this Vector3 a, Vector3 b)
        {
            a.x /= b.x;
            a.y /= b.y;
            a.z /= b.z;
            return a;
        }
        
        public static Vector2 Divide(this Vector2 a, Vector2 b)
        {
            a.x /= b.x;
            a.y /= b.y;
            return a;
        }
        
        public static Vector3 RotateAroundPivot(this Vector3 point, Vector3 pivot, Vector3 eulerAngles)
        {
            Vector3 direction = point - pivot;
            Quaternion rotation = Quaternion.Euler(eulerAngles);
            direction = rotation * direction;
            return pivot + direction;
        }
        
        public static void ClampX(this ref Vector3 target, in float min, in float max)
        {
            target.x = Mathf.Clamp(target.x, min, max);
        }
        
        public static void ClampY(this ref Vector3 target, in float min, in float max)
        {
            target.y = Mathf.Clamp(target.y, min, max);
        }
        
        public static void ClampZ(this ref Vector3 target, in float min, in float max)
        {
            target.z = Mathf.Clamp(target.z, min, max);
        }
        
        public static float UnclampedInverseLerp(this in Vector3 value, Vector3 a, Vector3 b)
        {
            b.x -= a.x;
            b.y -= a.y;
            b.z -= a.z;
            a.x = value.x - a.x;
            a.y = value.y - a.y;
            a.z = value.z - a.z;
            
            //Vector3.Project
            float num1 = b.x * b.x + b.y * b.y + b.z * b.z; //Vector3.Dot
            if (num1 == 0f) return 0f;
            float num2 = a.x * b.x + a.y * b.y + a.z * b.z; //Vector3.Dot
            a.x = b.x * num2 / num1;
            a.y = b.y * num2 / num1;
            a.z = b.z * num2 / num1;
            //Vector3.Project
            
            float t = a.magnitude / b.magnitude;
            
            if (b.x * a.x + b.y * a.y + b.z * a.z < 0f) //Vector3.Dot
                t *= -1;
            
            return t;
        }
        
        public static float InverseLerp(this in Vector3 value, Vector3 a, Vector3 b)
        {
            b.x -= a.x;
            b.y -= a.y;
            b.z -= a.z;
            a.x = value.x - a.x;
            a.y = value.y - a.y;
            a.z = value.z - a.z;
            
            //Vector3.Project
            float num1 = b.x * b.x + b.y * b.y + b.z * b.z; //Vector3.Dot
            if (num1 == 0f) return 0f;
            float num2 = a.x * b.x + a.y * b.y + a.z * b.z; //Vector3.Dot
            a.x = b.x * num2 / num1;
            a.y = b.y * num2 / num1;
            a.z = b.z * num2 / num1;
            //Vector3.Project

            float bMagnitude = b.magnitude;
            float t = a.magnitude / bMagnitude;
            
            if (b.x * a.x + b.y * a.y + b.z * a.z < 0) //Vector3.Dot
                t *= -1;
            
            return Mathf.InverseLerp(0f, bMagnitude, t * bMagnitude);
        }

        public static Vector3 Project(this in Vector3 vector, Vector3 direction)
        {
            float d = direction.x * direction.x + direction.y * direction.y + direction.z * direction.z;
            if (d == 0f) return Vector3.zero;
            float v = vector.x * direction.x + vector.y * direction.y + vector.z * direction.z;
            v /= d;
            direction.x *= v;
            direction.y *= v;
            direction.z *= v;
            return direction;
        }

        public static float Dot(this in Vector3 a, in Vector3 b) => a.x * b.x + a.y * b.y + a.z * b.z;
        
        public static Vector3 Snap(this Vector3 pos, float gridStep)
        {
            pos.x = Mathf.Round(pos.x / gridStep) * gridStep;
            pos.y = Mathf.Round(pos.y / gridStep) * gridStep;
            pos.z = Mathf.Round(pos.z / gridStep) * gridStep;
            return pos;
        }
        
        public static void SnapX(this ref Vector3 pos, float step) => pos.x = Mathf.Round(pos.x / step) * step;
        public static void SnapY(this ref Vector3 pos, float step) => pos.y = Mathf.Round(pos.y / step) * step;
        public static void SnapZ(this ref Vector3 pos, float step) => pos.z = Mathf.Round(pos.z / step) * step;
    }
}