using System;
using System.Collections.Generic;
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
        public BadassAnimationCurve curve;
        [NonSerialized] public float x;
        [NonSerialized] public float y;

        public void Evaluate()
        {
            y = curve.Evaluate(x);
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
        animatables.Add(animatable);
        evaluators.AddRange(animatable.Evaluators);
    }

    public static void Unregister(IAnimatable animatable)
    {
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