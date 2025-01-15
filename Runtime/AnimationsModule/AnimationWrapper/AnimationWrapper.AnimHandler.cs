using System;
using System.Collections.Generic;
using LSCore;
using Sirenix.OdinInspector;
using UnityEngine;

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

        protected bool forceHandle;
        private bool isStarted;

        public IEnumerable<HandlerEvaluateData> Evaluators => evaluators.Values;

#if UNITY_EDITOR
        protected virtual bool CanUse => true;
#endif

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
                forceHandle = true;
                OnStart();
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

        protected virtual void OnStart()
        {
        }

        protected virtual void OnStop()
        {
        }

        public bool TryGetEvaluator(string key, out HandlerEvaluateData evaluator) => evaluators.TryGetValue(key, out evaluator);
        public void ClearEvaluators() => evaluators.Clear();

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
        protected bool isDiff;
        
#if UNITY_EDITOR
        protected virtual string Label => "Value";
        public override Type ValueType => typeof(T);
        public override string HandlerName => GetType().Name;
#endif

        public sealed override void Handle()
        {
#if UNITY_EDITOR
            if (!CanUse)
            {
                return;
            }
#endif
            if (forceHandle)
            {
                forceHandle = false;
                goto handle;
            }
            
            isDiff = false;
            
            applyEvaluationResult();
            
            if (!isDiff) return;
            
            handle:
            OnHandle();
        }

        protected abstract void OnHandle();
    }

    public abstract class Vector2Handler : Handler<Vector2>
    {
        private HandlerEvaluateData x;
        private HandlerEvaluateData y;

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
        private HandlerEvaluateData x;
        private HandlerEvaluateData y;
        private HandlerEvaluateData z;
        
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
    }
}
