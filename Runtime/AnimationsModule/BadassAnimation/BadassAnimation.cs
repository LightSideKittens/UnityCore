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
    public Dictionary<string, List<Handler>> handlersByClip;
    private BadassAnimationClip currentClip;
    private List<Handler> currentHandlers = new();
    private List<EvaluateData> currentEvaluators = new();
    private float time;

    public BadassAnimationClip Clip
    {
        get => currentClip;
        set
        {
            if (value == null || currentClip == value || !isActiveAndEnabled)
            {
                Unregister(this);
                return;
            }
            
            Unregister(this);
            currentClip = value;
            currentHandlers = handlersByClip[value.name];
            currentEvaluators.Clear();
            for (int i = 0; i < currentHandlers.Count; i++)
            {
                var handler = currentHandlers[i];
                var evaluators = handler.evaluators;
                evaluators.Clear();
                foreach (var evaluator in currentClip.namesToCurvesByHandlerGuids[handler.guid])
                {
                    evaluators.Add(evaluator.Key, new EvaluateData(){curve = evaluator.Value});
                }
                currentEvaluators.AddRange(currentHandlers[i].evaluators.Values);
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
    }

    public void TryInit()
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
}