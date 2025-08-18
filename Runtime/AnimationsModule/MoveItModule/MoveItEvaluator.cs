using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using LSCore;
using UnityEngine;

public interface IAnimatable<T> where T : IEvaluator
{
    void BeforeEvaluate(float deltaTime);
    IList<T> Evaluators { get; }
    void AfterEvaluate();
}

public interface IEvaluator
{
    void Evaluate();
}

public class MoveItEvaluator<T> where T : IEvaluator
{
    private static MoveItEvaluator<T> updateEvaluator = new(MoveIt.UpdateModeType.Update);
    private static MoveItEvaluator<T> fixedUpdateEvaluator = new(MoveIt.UpdateModeType.FixedUpdate);
    private static MoveItEvaluator<T> manualUpdateEvaluator = new(MoveIt.UpdateModeType.Manual);

    private static Dictionary<MoveIt.UpdateModeType, MoveItEvaluator<T>> updateEvaluators = new()
    {
        { MoveIt.UpdateModeType.Update, updateEvaluator },
        { MoveIt.UpdateModeType.FixedUpdate, fixedUpdateEvaluator },
        { MoveIt.UpdateModeType.Manual, manualUpdateEvaluator }
    };
    
    private HashSet<IEvaluator> evaluators = new (1024 * 32);
    private HashSet<IAnimatable<T>> animatables = new (1024 * 16);
    private static readonly int threshold;

    static MoveItEvaluator()
    {
        threshold = Environment.ProcessorCount * 20;
    }
    

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
        
#if UNITY_EDITOR
        World.Created += ClearStatic;
        World.Destroyed += ClearStatic;
        
        void ClearStatic()
        {
            evaluators.Clear();
            animatables.Clear();
        }
#endif
    }
    
    public static void Register(IAnimatable<T> animatable, MoveIt.UpdateModeType updateMode)
    {
        updateEvaluators[updateMode].Register(animatable);
    }

    public static void Unregister(IAnimatable<T> animatable, MoveIt.UpdateModeType updateMode)
    {
        updateEvaluators[updateMode].Unregister(animatable);
    }

    public void Register(IAnimatable<T> animatable)
    {
#if UNITY_EDITOR
        if(World.IsEditMode) return;
#endif
        animatables.Add(animatable);
        var evs = animatable.Evaluators;
        var count = evs.Count;
        for (int i = 0; i < count; i++)
        {
            evaluators.Add(evs[i]);
        }
    }

    public void Unregister(IAnimatable<T> animatable)
    {
#if UNITY_EDITOR
        if(World.IsEditMode) return;
#endif
        animatables.Remove(animatable);
        
        var evs = animatable.Evaluators;
        var count = evs.Count;
        for (int i = 0; i < count; i++)
        {
            evaluators.Remove(evs[i]);
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
            Parallel.ForEach(evaluators, InvokeEvaluate);
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