using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using LSCore.Attributes;
using UnityEditor;
using Object = UnityEngine.Object;

public partial class MoveIt
{
    [Serializable]
    [Unwrap]
    public class EvaluateData : IEvaluator
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
        
        public void Reset()
        {
            x = 0;
            y = 0;
            startY = float.NaN;
        }
    }
    
    [Serializable]
    public class HandlerEvaluateData : EvaluateData, IEquatable<HandlerEvaluateData>
    {
        public string property;
        public bool isRef;
        public bool isFloat;
        [NonSerialized] public bool isDiff;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Evaluate()
        {
            var last = y;
            base.Evaluate();
            isDiff = Math.Abs(y - last) > 0.0001f;
        }
        
        public bool Equals(HandlerEvaluateData other)
        {
            return property == other.property;
        }

        public override int GetHashCode()
        {
            return property.GetHashCode();
        }

#if UNITY_EDITOR
        public static void TrimModifications(UnityEngine.Object target, List<UndoPropertyModification> modifications, HandlerEvaluateData evaluateData, string propPath, string propertyName)
        {
            TrimModifications(target, modifications, evaluateData, $"{propPath}.{propertyName}");
        }
        
        public static void TrimModifications(UnityEngine.Object target, List<UndoPropertyModification> modifications, HandlerEvaluateData evaluateData, string propPath)
        {
            if(string.IsNullOrEmpty(propPath)) return;
            if(evaluateData == null) return;
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
        
        public static void StartAnimationMode(UnityEngine.Object target, HandlerEvaluateData evaluateData, string propPath, string propertyName)
        {
            StartAnimationMode(target, evaluateData, $"{propPath}.{propertyName}");
        }
        
        public static void StartAnimationMode(Object target, HandlerEvaluateData evaluateData, string property)
        {
            if(string.IsNullOrEmpty(property)) return;
            if(evaluateData == null) return;
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