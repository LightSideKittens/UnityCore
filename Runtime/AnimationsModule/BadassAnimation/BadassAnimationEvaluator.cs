using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LSCore;
using Sirenix.Utilities;
using UnityEngine;

public interface IAnimatable
{
    void BeforeEvaluate(float deltaTime);
    IEnumerable<IEvaluator> Evaluators { get; }
    void AfterEvaluate();
}

public interface IEvaluator
{
    void Evaluate();
}

public class BadassAnimationEvaluator
{
    private static BadassAnimationEvaluator updateEvaluator = new(BadassAnimation.UpdateModeType.Update);
    private static BadassAnimationEvaluator fixedUpdateEvaluator = new(BadassAnimation.UpdateModeType.FixedUpdate);
    private static BadassAnimationEvaluator manualUpdateEvaluator = new(BadassAnimation.UpdateModeType.Manual);

    private static Dictionary<BadassAnimation.UpdateModeType, BadassAnimationEvaluator> updateEvaluators = new()
    {
        { BadassAnimation.UpdateModeType.Update, updateEvaluator },
        { BadassAnimation.UpdateModeType.FixedUpdate, fixedUpdateEvaluator },
        { BadassAnimation.UpdateModeType.Manual, manualUpdateEvaluator }
    };
    
    private HashSet<IEvaluator> evaluators = new (1024);
    private HashSet<IAnimatable> animatables = new();
    private static readonly int threshold;

    static BadassAnimationEvaluator()
    {
        threshold = Environment.ProcessorCount * 20;
    }
    
    public BadassAnimationEvaluator(BadassAnimation.UpdateModeType updateMode)
    {
        switch (updateMode)
        {
            case BadassAnimation.UpdateModeType.Update:
                World.Updated += OnWorldUpdated;
                break;
            case BadassAnimation.UpdateModeType.FixedUpdate:
                World.FixedUpdated += OnWorldFixedUpdate;
                break;
        }
        
        World.Created += ClearStatic;
        World.Destroyed += ClearStatic;
        
        void ClearStatic()
        {
            evaluators.Clear();
            animatables.Clear();
        }
    }
    
    public static void Register(IAnimatable animatable, BadassAnimation.UpdateModeType updateMode)
    {
        updateEvaluators[updateMode].Register(animatable);
    }

    public static void Unregister(IAnimatable animatable, BadassAnimation.UpdateModeType updateMode)
    {
        updateEvaluators[updateMode].Unregister(animatable);
    }

    public void Register(IAnimatable animatable)
    {
#if UNITY_EDITOR
        if(World.IsEditMode) return;
#endif
        animatables.Add(animatable);
        evaluators.AddRange(animatable.Evaluators);
    }

    public void Unregister(IAnimatable animatable)
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

    private void OnWorldUpdated() => OnWorldUpdated(Time.deltaTime);
    private void OnWorldFixedUpdate() => OnWorldUpdated(Time.fixedDeltaTime);

    private void OnWorldUpdated(float deltaTime)
    {
        foreach (var animatable in animatables)
        {
            animatable.BeforeEvaluate(deltaTime);
        }
        
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
        
        foreach (var animatable in animatables)
        {
            animatable.AfterEvaluate();
        }
    }
}