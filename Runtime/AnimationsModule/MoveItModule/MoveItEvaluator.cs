using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
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

public class MoveItEvaluator
{
    private static MoveItEvaluator updateEvaluator = new(MoveIt.UpdateModeType.Update);
    private static MoveItEvaluator fixedUpdateEvaluator = new(MoveIt.UpdateModeType.FixedUpdate);
    private static MoveItEvaluator manualUpdateEvaluator = new(MoveIt.UpdateModeType.Manual);

    private static Dictionary<MoveIt.UpdateModeType, MoveItEvaluator> updateEvaluators = new()
    {
        { MoveIt.UpdateModeType.Update, updateEvaluator },
        { MoveIt.UpdateModeType.FixedUpdate, fixedUpdateEvaluator },
        { MoveIt.UpdateModeType.Manual, manualUpdateEvaluator }
    };
    
    private HashSet<IEvaluator> evaluators = new (1024);
    private HashSet<IAnimatable> animatables = new();
    private static readonly int threshold;

    static MoveItEvaluator()
    {
        threshold = Environment.ProcessorCount * 20;
    }
    
    private static readonly Action<IEvaluator> evaluateAction = InvokeEvaluate;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void InvokeEvaluate(IEvaluator ev)
    {
        ev.Evaluate();
    }

    
    public MoveItEvaluator(MoveIt.UpdateModeType updateMode)
    {
        switch (updateMode)
        {
            case MoveIt.UpdateModeType.Update:
                World.Updated += OnWorldUpdated;
                break;
            case MoveIt.UpdateModeType.FixedUpdate:
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
    
    public static void Register(IAnimatable animatable, MoveIt.UpdateModeType updateMode)
    {
        updateEvaluators[updateMode].Register(animatable);
    }

    public static void Unregister(IAnimatable animatable, MoveIt.UpdateModeType updateMode)
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
            Parallel.ForEach(evaluators, evaluateAction);
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