using System;
using System.Collections.Generic;
using LSCore;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public partial class BadassAnimation
{
    [Serializable]
    public class Event
    {
        [HideInInspector] public float x;
        [SerializeReference] 
        [HideLabel]
        public LSAction action;

        public virtual void Invoke()
        {
            action.Invoke();
        }
    }
    
    [Serializable]
    public class RuntimeOnlyEvent : Event
    {
        public override void Invoke()
        {
            if (World.IsPlaying)
            {
                base.Invoke();
            }
        }
    }

    [Serializable]
    public abstract class Handler
    {
        public static class PropNames
        {
            public const string x = nameof(x);
            public const string y = nameof(y);
            public const string z = nameof(z);

            public const string value = nameof(value);

            public const string r = nameof(r);
            public const string g = nameof(g);
            public const string b = nameof(b);
            public const string a = nameof(a);
        }

        [GenerateGuid(Hide = true)] public string guid;
        protected Dictionary<string, HandlerEvaluateData> evaluators = new();
        
        protected bool isStarted;

        public IEnumerable<HandlerEvaluateData> Evaluators => evaluators.Values;

#if UNITY_EDITOR
        protected bool CanUse => IsPreview && Target != null && applyEvaluationResult != null;
        public bool IsPreview { get; set; } = true;
#endif

        public abstract Object Target { get; }
        
        
        public void Start()
        {
#if UNITY_EDITOR
            if (!CanUse)
            {
                return;
            }
#endif
            if (!isStarted)
            {
                isStarted = true;
                
                foreach (var evaluator in Evaluators)
                {
                    evaluator.Evaluate();
                    evaluator.isDiff = true;
                }
                
                OnStart();
                SetStartValue();
                Handle();
            }
        }

        public void Stop()
        {
#if UNITY_EDITOR
            if (!CanUse)
            {
                return;
            }
#endif

            if (isStarted)
            {
                isStarted = false;
                OnStop();
            }
        }

        public abstract void Handle();

        protected virtual void OnStart(){}
        protected abstract void SetStartValue();
        protected abstract void OnStop();

        public bool TryGetEvaluator(string key, out HandlerEvaluateData evaluator) => evaluators.TryGetValue(key, out evaluator);
        public bool RemoveEvaluator(string key)
        {
            applyEvaluationResult -= GetApplyEvaluationResultAction(key, null);
            var isRemoved = evaluators.Remove(key);
            if (isStarted)
            {
                Stop();
                Start();
            }
            return isRemoved;
        }

        public void ClearEvaluators()
        {
            applyEvaluationResult = null;
            evaluators.Clear();
            if (isStarted)
            {
                Stop();
                Start();
            }
        }

        public void AddEvaluator(string key, BadassCurve curve)
        {
            var evaluator = new HandlerEvaluateData{curve = curve};
            applyEvaluationResult += GetApplyEvaluationResultAction(key, evaluator);
            evaluators.Add(key, evaluator);
        }
        
        protected Action applyEvaluationResult;
        protected abstract Action GetApplyEvaluationResultAction(string key, HandlerEvaluateData evaluator);

#if UNITY_EDITOR
        public abstract Type ValueType { get; }
        public abstract string HandlerName { get; }

        [GUIColor(1f, 0.54f, 0.16f)]
        [ShowInInspector] private bool gizmos;

        public void DrawGizmos()
        {
            if (gizmos)
            {
                OnDrawGizmos();
            }
        }

        protected virtual void OnDrawGizmos()
        {
        }

        public virtual void OnSceneGUI()
        {
        }
        
        public abstract void TrimModifications(List<UndoPropertyModification> modifications);
        public abstract void StartAnimationMode();
#endif
    }

    [Serializable]
    public abstract class Handler<T> : Handler
    {
        [ShowInInspector]
        [LabelText("$Label")]
        [NonSerialized]
        [GUIColor(1f, 0.54f, 0.16f)]
        [PropertySpace(SpaceBefore = 0, SpaceAfter = 10)]
        public T value;
        
        public T StartValue { get; set; }
        protected abstract T GetStartValue();
        protected bool isDiff;

        protected sealed override void SetStartValue()
        {
            var v = GetStartValue();
            StartValue = v;
            value = v;
        }
        
#if UNITY_EDITOR
        protected virtual string Label => "Value";
        public override Type ValueType => typeof(T);
        public override string HandlerName => GetType().Name;
        protected abstract string PropertyPath { get; }
        protected void TrimModifications(List<UndoPropertyModification> modifications, HandlerEvaluateData evaluateData, string propertyName)
        {
            for (int i = 0; i < modifications.Count; i++)
            {
                var mod = modifications[i];
                if (evaluateData != null && mod.currentValue.propertyPath == $"{PropertyPath}.{propertyName}")
                {
                    modifications.RemoveAt(i);
                    i--;
                }
            }
        }

        protected void StartAnimationMode(HandlerEvaluateData evaluateData, string propertyName, float value)
        {
            if(Target == null || evaluateData == null) return;
            var binding = EditorCurveBinding.FloatCurve($"{PropertyPath}.{propertyName}", Target.GetType(), "");
            AnimationMode.AddPropertyModification(binding, new PropertyModification()
            {
                target = Target,
                propertyPath = $"{PropertyPath}.{propertyName}",
                value = value.ToString("g7"),
            }, true);
        }
#endif
        
        public sealed override void Handle()
        {
#if UNITY_EDITOR
            if (!CanUse)
            {
                return;
            }

            if (!isStarted)
            {
                Start();
                applyEvaluationResult();
                return;
            }
#endif
            
            isDiff = false;
            
            applyEvaluationResult();
            
            if (!isDiff) return;
            
            OnHandle();
        }

        protected abstract void OnHandle();
    }

    public abstract class Vector2Handler : Handler<Vector2>
    {
        private HandlerEvaluateData x;
        private HandlerEvaluateData y;

#if UNITY_EDITOR
        public override void TrimModifications(List<UndoPropertyModification> modifications)
        {
            TrimModifications(modifications, x, "x");
            TrimModifications(modifications, y, "y");
        }

        public override void StartAnimationMode()
        {
            StartAnimationMode(x, "x", StartValue.x);
            StartAnimationMode(y, "y", StartValue.y);
        }
#endif
        protected override Action GetApplyEvaluationResultAction(string key, HandlerEvaluateData evaluator)
        {
            switch (key)
            {
                case PropNames.x: x = evaluator; return X;
                case PropNames.y: y = evaluator; return Y;
                default: return null;
            }
        }

        private void X() {isDiff |= x.isDiff; value.x = x.y;}
        private void Y() {isDiff |= y.isDiff; value.y = y.y;}
    }

    public abstract class Vector3Handler : Handler<Vector3>
    {
        protected HandlerEvaluateData x;
        protected HandlerEvaluateData y;
        protected HandlerEvaluateData z;
        
#if UNITY_EDITOR
        public override void TrimModifications(List<UndoPropertyModification> modifications)
        {
            TrimModifications(modifications, x, "x");
            TrimModifications(modifications, y, "y");
            TrimModifications(modifications, z, "z");
        }
        
        public override void StartAnimationMode()
        {
            StartAnimationMode(x, "x", StartValue.x);
            StartAnimationMode(y, "y", StartValue.y);
            StartAnimationMode(z, "z", StartValue.z);
        }
#endif
        
        protected override Action GetApplyEvaluationResultAction(string key, HandlerEvaluateData evaluator)
        {
            switch (key)
            {
                case PropNames.x: x = evaluator; return X;
                case PropNames.y: y = evaluator; return Y;
                case PropNames.z: z = evaluator; return Z;
                default: return null;
            }
        }

        private void X() {isDiff |= x.isDiff; value.x = x.y;}
        private void Y() {isDiff |= y.isDiff; value.y = y.y;}
        private void Z() {isDiff |= z.isDiff; value.z = z.y;}
    }

    public abstract class ColorHandler : Handler<Color>
    {
        private HandlerEvaluateData r;
        private HandlerEvaluateData g;
        private HandlerEvaluateData b;
        private HandlerEvaluateData a;
        
#if UNITY_EDITOR
        public override void TrimModifications(List<UndoPropertyModification> modifications)
        {
            TrimModifications(modifications, r, "r");
            TrimModifications(modifications, g, "g");
            TrimModifications(modifications, b, "b");
            TrimModifications(modifications, a, "a");
        }
        
        public override void StartAnimationMode()
        {
            StartAnimationMode(r, "r", StartValue.r);
            StartAnimationMode(g, "g", StartValue.g);
            StartAnimationMode(b, "b", StartValue.b);
            StartAnimationMode(a, "a", StartValue.a);
        }
#endif
        
        protected override Action GetApplyEvaluationResultAction(string key, HandlerEvaluateData evaluator)
        {
            switch (key)
            {
                case PropNames.r: r = evaluator; return R;
                case PropNames.g: g = evaluator; return G;
                case PropNames.b: b = evaluator; return B;
                case PropNames.a: a = evaluator; return A;
                default: return null;
            }
        }

        private void R() {isDiff |= r.isDiff; value.r = r.y;}
        private void G() {isDiff |= g.isDiff; value.g = g.y;}
        private void B() {isDiff |= b.isDiff; value.b = b.y;}
        private void A() {isDiff |= a.isDiff; value.a = a.y;}
    }

    public abstract class FloatHandler : Handler<float>
    {
        private HandlerEvaluateData valueEvaluator;

        protected override Action GetApplyEvaluationResultAction(string key, HandlerEvaluateData evaluator)
        {
            if (key != PropNames.value) return null;
            valueEvaluator = evaluator; return Value;
        }

        private void Value() {isDiff |= valueEvaluator.isDiff; value = valueEvaluator.y;}

        protected override float GetStartValue()
        {
            throw new NotImplementedException();
        }
    }
}
