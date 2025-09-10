using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using LSCore.Attributes;
using LSCore.DataStructs;
using LSCore.Extensions.Unity;
using Unity.Collections;
using UnityEditor;
using Object = UnityEngine.Object;

public partial class MoveIt
{
    public enum PropertyType : byte
    {
        Unknown = 0,
        Int8 = 1,
        UInt8 = 2,
        Int16 = 3,
        UInt16 = 4,
        Int32 = 5,
        UInt32 = 6,
        Int64 = 7,
        UInt64 = 8,
        Float = 100,
        Double = 101,
        Enum = 200,
        Ref = 201,
        Bool = 202
    }
    
    [Serializable]
    [Unwrap]
    public class Evaluator : IEvaluator
    {
        public MoveItCurve curve;
        [NonSerialized] public float x;
        [NonSerialized] public float y;
        [NonSerialized] public float startY = float.NaN;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void Evaluate()
        {
            y = curve.Evaluate(x);
        }
    }
    
    [Serializable]
    public class HandlerEvaluator : Evaluator, IEquatable<HandlerEvaluator>
    {
        public Func<object, object> get;
        public Action<object, object> set;
        
        public Func<object, float> numGet;
        public Action<object, float> numSet;
        
#if UNITY_EDITOR
        public string rawProperty;
#endif
        
        public string property;
        public PropertyType propertyType;
        [NonSerialized] public bool isDiff;
        
        private TypedPathAccessor<float> floatAccessor;
        private TypedPathAccessor<byte> byteAccessor;
        private TypedPathAccessor<sbyte> sbyteAccessor;
        private TypedPathAccessor<short> shortAccessor;
        private TypedPathAccessor<ushort> ushortAccessor;
        private TypedPathAccessor<int> intAccessor;
        private TypedPathAccessor<uint> uintAccessor;
        private TypedPathAccessor<long> longAccessor;
        private TypedPathAccessor<ulong> ulongAccessor;
        private TypedPathAccessor<double> doubleAccessor;
        private TypedPathAccessor<bool> boolAccessor;
        
        public bool IsNumber => propertyType != PropertyType.Ref && propertyType != PropertyType.Enum;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Evaluate()
        {
            var last = y;
            base.Evaluate();
            isDiff = Math.Abs(y - last) > 0.0001f;
        }
        
        public bool Equals(HandlerEvaluator other)
        {
            return property == other.property;
        }

        public override int GetHashCode()
        {
            return property.GetHashCode();
        }

        public delegate void PropAction(ref int floatIndex, ref int discreteIndex, UniDict<int, Object> objects, Handler handler, Object obj, IPropertyHandler propertyHandler);
        
        public PropAction reset;
        public PropAction update;
        public PropAction getUpdate;

        private void Reset(ref int floatIndex, ref int discreteIndex, UniDict<int, Object> objects, Handler handler, Object obj, IPropertyHandler propertyHandler)
        {
            numSet(obj, startY);
        }
        
        private void Reset3(ref int floatIndex, ref int discreteIndex, UniDict<int, Object> objects, Handler handler, Object obj, IPropertyHandler propertyHandler)
        {
            var intY = (int)startY;
            if (!objects.TryGetValue(intY, out var value))
            {
                objects.Remove(intY);
            }
                        
            set(obj, value);
            propertyHandler?.HandleAnimatedProperty(handler, this);
        }
        
        private void Update(ref int floatIndex, ref int discreteIndex, UniDict<int, Object> objects, Handler handler, Object obj, IPropertyHandler propertyHandler)
        {
            numSet(obj, y);
        }
        
        private void Update3(ref int floatIndex, ref int discreteIndex, UniDict<int, Object> objects, Handler handler, Object obj, IPropertyHandler propertyHandler)
        {
            var intY = (int)y;
            if (!objects.TryGetValue(intY, out var value))
            {
                objects.Remove(intY);
            }
                        
            set(obj, value);
            propertyHandler?.HandleAnimatedProperty(handler, this);
        }
        
        private void GetUpdate(ref int floatIndex, ref int discreteIndex, UniDict<int, Object> objects, Handler handler, Object obj, IPropertyHandler propertyHandler)
        {
            startY = numGet(obj);
            numSet(obj, y);
        }
        
        private void GetUpdate3(ref int floatIndex, ref int discreteIndex, UniDict<int, Object> objects, Handler handler, Object obj, IPropertyHandler propertyHandler)
        {
            var value = get(obj);
            startY = !value.Equals(null) ? value.GetHashCode() : 0;
            
            var intY = (int)y;
            if (!objects.TryGetValue(intY, out var value1))
            {
                objects.Remove(intY);
            }
                        
            set(obj, value1);
            propertyHandler?.HandleAnimatedProperty(handler, this);
        }
        
        public void InitAccessor(object obj)
        {
            switch (propertyType)
            {
                case PropertyType.Float:
                    floatAccessor = PathAccessor.Get<float>(obj, property);
                    numGet = GetFloat;
                    numSet = SetFloat;
                    break;
                case PropertyType.Int8:
                    sbyteAccessor = PathAccessor.Get<sbyte>(obj, property);
                    numGet = GetInt8;
                    numSet = SetInt8;
                    break;
                case PropertyType.UInt8:
                    byteAccessor = PathAccessor.Get<byte>(obj, property);
                    numGet = GetUInt8;
                    numSet = SetUInt8;
                    break;
                case PropertyType.Int16:
                    shortAccessor = PathAccessor.Get<short>(obj, property);
                    numGet = GetInt16;
                    numSet = SetInt16;
                    break;
                case PropertyType.UInt16:
                    ushortAccessor = PathAccessor.Get<ushort>(obj, property);
                    numGet = GetUInt16;
                    numSet = SetUInt16;
                    break;
                case PropertyType.Int32:
                    intAccessor = PathAccessor.Get<int>(obj, property);
                    numGet = GetInt32;
                    numSet = SetInt32;
                    break;
                case PropertyType.UInt32:
                    uintAccessor = PathAccessor.Get<uint>(obj, property);
                    numGet = GetUInt32;
                    numSet = SetUInt32;
                    break;
                case PropertyType.Int64:
                    longAccessor = PathAccessor.Get<long>(obj, property);
                    numGet = GetInt64;
                    numSet = SetInt64;
                    break;
                case PropertyType.UInt64:
                    ulongAccessor = PathAccessor.Get<ulong>(obj, property);
                    numGet = GetUInt64;
                    numSet = SetUInt64;
                    break;
                case PropertyType.Double:
                    doubleAccessor = PathAccessor.Get<double>(obj, property);
                    numGet = GetDouble;
                    numSet = SetDouble;
                    break;
                case PropertyType.Bool:
                    boolAccessor = PathAccessor.Get<bool>(obj, property);
                    numGet = GetBool;
                    numSet = SetBool;
                    break;
                case PropertyType.Enum:
                    var accessor = PathAccessor.GetRef(obj, property);
                    get = accessor.Get;
                    set = accessor.Set;
                    break;
                case PropertyType.Ref:
                    var refAccessor = PathAccessor.GetRef(obj, property);
                    get = refAccessor.Get;
                    set = refAccessor.Set;
                    break;
            }
        }

        public void InitDelegates()
        {
            if (IsNumber)
            {
                if (numSet != null)
                {
                    reset = Reset;
                    update = Update;
                    getUpdate = GetUpdate;
                }
            }
            else
            {
                if (propertyType is PropertyType.Ref or PropertyType.Enum)
                {
                    if (set != null)
                    {
                        reset = Reset3;
                        update = Update3;
                        getUpdate = GetUpdate3;
                    }
                }
            }
        }

        private float GetFloat(object target) => floatAccessor.Get(target);
        private void SetFloat(object target, float value) => floatAccessor.Set(target, value);
        
        private float GetInt8(object target) => sbyteAccessor.Get(target);
        private void SetInt8(object target, float value) => sbyteAccessor.Set(target, (sbyte)value);
        
        private float GetUInt8(object target) => byteAccessor.Get(target);
        private void SetUInt8(object target, float value) => byteAccessor.Set(target, (byte)value);
        
        private float GetInt16(object target) => shortAccessor.Get(target);
        private void SetInt16(object target, float value) => shortAccessor.Set(target, (short)value);
        
        private float GetUInt16(object target) => ushortAccessor.Get(target);
        private void SetUInt16(object target, float value) => ushortAccessor.Set(target, (ushort)value);
        
        private float GetInt32(object target) => intAccessor.Get(target);
        private void SetInt32(object target, float value) => intAccessor.Set(target, (int)value);
        
        private float GetUInt32(object target) => uintAccessor.Get(target);
        private void SetUInt32(object target, float value) => uintAccessor.Set(target, (uint)value);
        
        private float GetInt64(object target) => longAccessor.Get(target);
        private void SetInt64(object target, float value) => longAccessor.Set(target, (long)value);
        
        private float GetUInt64(object target) => ulongAccessor.Get(target);
        private void SetUInt64(object target, float value) => ulongAccessor.Set(target, (ulong)value);
        
        private float GetDouble(object target) => (float)doubleAccessor.Get(target);
        private void SetDouble(object target, float value) => doubleAccessor.Set(target, value);
        
        private float GetBool(object target) => boolAccessor.Get(target) ? 1 : 0;
        private void SetBool(object target, float value) => boolAccessor.Set(target, value >= 1);
        
        
#if UNITY_EDITOR
        public static void TrimModifications(UnityEngine.Object target, List<UndoPropertyModification> modifications, HandlerEvaluator evaluator, string propPath, string propertyName)
        {
            TrimModifications(target, modifications, evaluator, $"{propPath}.{propertyName}");
        }
        
        public static void TrimModifications(UnityEngine.Object target, List<UndoPropertyModification> modifications, HandlerEvaluator evaluator, string propPath)
        {
            if(string.IsNullOrEmpty(propPath)) return;
            if(evaluator == null) return;
            if(target == null) return;
            
            for (int i = 0; i < modifications.Count; i++)
            {
                var mod = modifications[i].currentValue;
                if (mod.target == target && mod.propertyPath == propPath)
                {
                    modifications.RemoveAt(i);
                    i--;
                }
            }
        }
        
        public static void StartAnimationMode(UnityEngine.Object target, HandlerEvaluator evaluator, string propPath, string propertyName)
        {
            StartAnimationMode(target, evaluator, $"{propPath}.{propertyName}");
        }
        
        public static void StartAnimationMode(Object target, HandlerEvaluator evaluator, string property)
        {
            if(string.IsNullOrEmpty(property)) return;
            if(evaluator == null) return;
            if(target == null) return;
            
            if (AnimationMode.IsPropertyAnimated(target, property))
            {
                return;
            }
            var binding = EditorCurveBinding.FloatCurve(property, target.GetType(), "");
            AnimationMode.AddPropertyModification(binding, new PropertyModification()
            {
                target = target,
                propertyPath = property
            }, true);
        }
#endif
    }
    
    /*public class Vector2HandlerEvaluateData
    {
        public Vector2 value;
        public HandlerEvaluateData x;
        public HandlerEvaluateData y;
        public bool isDiff;
        public string prefix = string.Empty;
        
#if UNITY_EDITOR
        public static Vector2HandlerEvaluateData Empty { get; } = new()
        {
            x = new HandlerEvaluateData(),
            y = new HandlerEvaluateData(),
        };
        
        public void TrimModifications(UnityEngine.Object target, List<UndoPropertyModification> modifications, string propPath)
        {
            HandlerEvaluateData.TrimModifications(target, modifications, x, propPath, "x");
            HandlerEvaluateData.TrimModifications(target, modifications, y, propPath, "y");
        }

        public void StartAnimationMode(UnityEngine.Object target, string propPath, Vector2 val)
        {
            HandlerEvaluateData.StartAnimationMode(target, x, propPath, "x", val.x);
            HandlerEvaluateData.StartAnimationMode(target, y, propPath, "y", val.y);
        }
#endif
        
        public Action GetApplyEvaluationResultAction(string key, HandlerEvaluateData evaluator)
        {
            if (!string.IsNullOrEmpty(prefix))
            {
                key = key.Replace(prefix, string.Empty);
            }
            
            switch (key)
            {
                case "x": x = evaluator; return X;
                case "y": y = evaluator; return Y;
                default: return null;
            }
        }

        private void X() {isDiff |= x.isDiff; value.x = x.y;}
        private void Y() {isDiff |= y.isDiff; value.y = y.y;}
    }
    
    public class Vector3HandlerEvaluateData
    {
        public Vector3 value;
        public HandlerEvaluateData x;
        public HandlerEvaluateData y;
        public HandlerEvaluateData z;
        public bool isDiff;
        public string prefix = string.Empty;
        
#if UNITY_EDITOR
        public static Vector3HandlerEvaluateData Empty { get; } = new()
        {
            x = new HandlerEvaluateData(),
            y = new HandlerEvaluateData(),
            z = new HandlerEvaluateData(),
        };

        public void TrimModifications(UnityEngine.Object target, List<UndoPropertyModification> modifications, string propPath)
        {
            HandlerEvaluateData.TrimModifications(target, modifications, x, propPath, "x");
            HandlerEvaluateData.TrimModifications(target, modifications, y, propPath, "y");
            HandlerEvaluateData.TrimModifications(target, modifications, y, propPath, "z");
        }

        public void StartAnimationMode(UnityEngine.Object target, string propPath, Vector3 val)
        {
            HandlerEvaluateData.StartAnimationMode(target, x, propPath, "x", val.x);
            HandlerEvaluateData.StartAnimationMode(target, y, propPath, "y", val.y);
            HandlerEvaluateData.StartAnimationMode(target, y, propPath, "z", val.z);
        }
#endif
        
        public Action GetApplyEvaluationResultAction(string key, HandlerEvaluateData evaluator)
        {
            if (!string.IsNullOrEmpty(prefix))
            {
                key = key.Replace(prefix, string.Empty);
            }
            
            switch (key)
            {
                case "x": x = evaluator; return X;
                case "y": y = evaluator; return Y;
                case "z": z = evaluator; return Z;
                default: return null;
            }
        }

        private void X() {isDiff |= x.isDiff; value.x = x.y;}
        private void Y() {isDiff |= y.isDiff; value.y = y.y;}
        private void Z() {isDiff |= z.isDiff; value.z = z.y;}
    }
    
    public class QuaternionHandlerEvaluateData
    {
        public Quaternion value;
        public HandlerEvaluateData x;
        public HandlerEvaluateData y;
        public HandlerEvaluateData z;
        public HandlerEvaluateData w;
        public bool isDiff;
        public string prefix = string.Empty;
        
#if UNITY_EDITOR
        public static QuaternionHandlerEvaluateData Empty { get; } = new()
        {
            x = new HandlerEvaluateData(),
            y = new HandlerEvaluateData(),
            z = new HandlerEvaluateData(),
            w = new HandlerEvaluateData(),
        };

        public void TrimModifications(UnityEngine.Object target, List<UndoPropertyModification> modifications, string propPath)
        {
            HandlerEvaluateData.TrimModifications(target, modifications, x, propPath, "x");
            HandlerEvaluateData.TrimModifications(target, modifications, y, propPath, "y");
            HandlerEvaluateData.TrimModifications(target, modifications, y, propPath, "z");
            HandlerEvaluateData.TrimModifications(target, modifications, w, propPath, "w");
        }

        public void StartAnimationMode(UnityEngine.Object target, string propPath, Quaternion val)
        {
            HandlerEvaluateData.StartAnimationMode(target, x, propPath, "x", val.x);
            HandlerEvaluateData.StartAnimationMode(target, y, propPath, "y", val.y);
            HandlerEvaluateData.StartAnimationMode(target, y, propPath, "z", val.z);
            HandlerEvaluateData.StartAnimationMode(target, w, propPath, "w", val.w);
        }
#endif
        
        public Action GetApplyEvaluationResultAction(string key, HandlerEvaluateData evaluator)
        {
            if (!string.IsNullOrEmpty(prefix))
            {
                key = key.Replace(prefix, string.Empty);
            }
            
            switch (key)
            {
                case "x": x = evaluator; return X;
                case "y": y = evaluator; return Y;
                case "z": z = evaluator; return Z;
                case "w": w = evaluator; return W;
                default: return null;
            }
        }

        private void X() {isDiff |= x.isDiff; value.x = x.y;}
        private void Y() {isDiff |= y.isDiff; value.y = y.y;}
        private void Z() {isDiff |= z.isDiff; value.z = z.y;}
        private void W() {isDiff |= w.isDiff; value.w = z.y;}
    }

    public class ColorHandlerEvaluateData
    {
        public static ColorHandlerEvaluateData Empty { get; } = new()
        {
            r = new HandlerEvaluateData(),
            g = new HandlerEvaluateData(),
            b = new HandlerEvaluateData(),
            a = new HandlerEvaluateData(),
        };
        
        public Color value;
        public HandlerEvaluateData r;
        public HandlerEvaluateData g;
        public HandlerEvaluateData b;
        public HandlerEvaluateData a;

#if UNITY_EDITOR
        public void TrimModifications(UnityEngine.Object target, List<UndoPropertyModification> modifications, string propPath)
        {
            HandlerEvaluateData.TrimModifications(target, modifications, r, propPath, "r");
            HandlerEvaluateData.TrimModifications(target, modifications, g, propPath, "g");
            HandlerEvaluateData.TrimModifications(target, modifications, b, propPath, "b");
            HandlerEvaluateData.TrimModifications(target, modifications, a, propPath, "a");
        }

        public void StartAnimationMode(UnityEngine.Object target, string propPath, Color val)
        {
            HandlerEvaluateData.StartAnimationMode(target, r, propPath, "r", val.r);
            HandlerEvaluateData.StartAnimationMode(target, g, propPath, "g", val.g);
            HandlerEvaluateData.StartAnimationMode(target, b, propPath, "b", val.b);
            HandlerEvaluateData.StartAnimationMode(target, a, propPath, "a", val.a);
        }
#endif
    }*/
}