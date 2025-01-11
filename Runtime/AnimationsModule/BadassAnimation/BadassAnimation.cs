using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public partial class BadassAnimation : MonoBehaviour, IAnimatable
{
    [Serializable]
    public struct Data
    {
        public BadassAnimationClip clip;
        [SerializeReference] 
        public List<Handler> handlers;
    }

    [HideInInspector] public List<Data> data;
    [HideInInspector] public BadassAnimationClip defaultClip;
    
    public Dictionary<string, List<Handler>> handlersByClip;
    private BadassAnimationClip currentClip;
    private List<Handler> currentHandlers = new();
    private List<HandlerEvaluateData> currentEvaluators = new();
    private float time;

    public BadassAnimationClip Clip
    {
        get => currentClip;
        set
        {
            Unregister(this);
            currentEvaluators.Clear();
            currentHandlers = ListSpan<Handler>.Empty;
            
            if (value == null || currentClip == value || !isActiveAndEnabled)
            {
                currentClip = value;
                return;
            }
            
            currentClip = value;
            var handlers = handlersByClip[value.name];
            for (int i = 0; i < handlers.Count; i++)
            {
                var handler = handlers[i];
                handler.ClearEvaluators();
                
                if (currentClip.namesToCurvesByHandlerGuids.TryGetValue(handler.guid, out var curves))
                {
                    foreach (var (propertyName, curve) in curves)
                    {
                        handler.AddEvaluator(propertyName, curve);
                    }
                    
                    currentHandlers.Add(handler);
                    currentEvaluators.AddRange(handler.Evaluators);
                }
            }
            
            Register(this);
            OnClipChanged();
        }
    }

    private void OnEnable()
    {
        if (currentClip != null)
        {
            Register(this);
        }
    }

    private void OnDisable()
    {
        Unregister(this);
    }

    [Button]
    private void Edit()
    {
        BadassAnimationWindow.ShowWindow(this);
    }

    private void OnClipChanged()
    {
        time = 0;
    }

    private void Awake()
    {
        TryInit();
        Clip = defaultClip;
    }

    internal void TryInit()
    {
        if(handlersByClip != null) return;
        
        handlersByClip = new();
        data ??= new List<Data>();
        
        for (int i = 0; i < data.Count; i++)
        {
            handlersByClip.Add(data[i].clip.name, data[i].handlers);
        }
    }

    public void BeforeEvaluate()
    {
        time += Time.deltaTime;
        
        for (int i = 0; i < currentEvaluators.Count; i++)
        {
            currentEvaluators[i].x = time;
        }
    }

    public IEnumerable<IEvaluator> Evaluators => currentEvaluators;
    
    public void AfterEvaluate()
    {
        for (int i = 0; i < currentHandlers.Count; i++)
        {
            var handler = currentHandlers[i];
            handler.Handle();
        }
    }

    public void Add(BadassAnimationClip newClip, List<Handler> handlers)
    {
        data.Add(new Data { clip = newClip, handlers = handlers });
        handlersByClip.Add(newClip.name, handlers);
    }

#if UNITY_EDITOR
    internal void Editor_Evaluate(float time)
    {
        foreach (var evaluator in currentEvaluators)
        {
            evaluator.x = time;
            evaluator.Evaluate();
        }
        AfterEvaluate();
    }
#endif
}