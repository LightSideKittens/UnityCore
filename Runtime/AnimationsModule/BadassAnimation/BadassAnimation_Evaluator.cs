using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using LSCore;
using LSCore.Attributes;
using Sirenix.Utilities;

public partial class BadassAnimation
{
    [Serializable]
    [Unwrap]
    public class EvaluateData : IEvaluator
    {
        public BadassCurve curve;
        [NonSerialized] public float x;
        [NonSerialized] public float y;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void Evaluate()
        {
            y = curve.Evaluate(x);
        }
    }

    public class HandlerEvaluateData : EvaluateData
    {
        public bool isDiff;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Evaluate()
        {
            var last = y;
            base.Evaluate();
            isDiff = Math.Abs(y - last) > 0.0001f;
        }
    }

    private static readonly HashSet<IAnimatable> animatables = new();
    private static readonly int threshold;
    
    static BadassAnimation()
    {
        threshold = Environment.ProcessorCount * 20;
        World.Updated += OnWorldUpdated;
        World.Created += ClearStatic;
        World.Destroyed += ClearStatic;
        
        void ClearStatic()
        {
            evaluators.Clear();
            animatables.Clear();
        }
    }

    public static void Register(IAnimatable animatable)
    {
#if UNITY_EDITOR
        if(World.IsEditMode) return;
#endif
        animatables.Add(animatable);
        evaluators.AddRange(animatable.Evaluators);
    }

    public static void Unregister(IAnimatable animatable)
    {
#if UNITY_EDITOR
        if(World.IsEditMode) return;
#endif
        animatables.Remove(animatable);
        foreach (var evaluator in animatable.Evaluators)
        {
            evaluators.Remove(evaluator);
        }
    }

    private static void OnWorldUpdated()
    {
        animatables.ForEach(x => x.BeforeEvaluate());
        
        if (evaluators.Count > threshold)
        {
            Parallel.ForEach(evaluators, x => x.Evaluate());
        }
        else
        {
            foreach (var animatable in evaluators)
            {
                animatable.Evaluate();
            }
        }
        
        animatables.ForEach(x => x.AfterEvaluate());
    }

    private static readonly HashSet<IEvaluator> evaluators = new (1024);
}