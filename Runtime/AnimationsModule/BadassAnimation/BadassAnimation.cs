using System;
using System.Collections.Generic;
using LSCore.Attributes;
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

        [SerializeReference]
        public List<Event> events;

        public void Add(Event eevent)
        {
            events ??= new List<Event>();
            var index = events.BinarySearch(eevent, eventComparer);
            if (index < 0) index = ~index;
            events.Insert(index, eevent);
        }

        private static readonly IComparer<Event> eventComparer = new EventXComparer();

        private class EventXComparer : IComparer<Event>
        {
            public int Compare(Event x, Event y)
            {
                return x.x.CompareTo(y.x);
            }
        }
    }
    
    public bool loop;
    public bool reverse;
    
    [HideInInspector] public List<Data> data;
    [HideInInspector] public BadassAnimationClip defaultClip;
    [SerializeField] private float startTime;
    
    private Dictionary<string, Data> dataByClip;
    private BadassAnimationClip currentClip;
    private List<Handler> currentHandlers = new();
    private List<Event> events = new();
    private List<HandlerEvaluateData> currentEvaluators = new();
    private float time;
    private float length;
    private Action eventsAction;
    
    public float StartTime
    {
        get => startTime;
        set
        {
            if(value >= length) return;
            
            startTime = value;
            var t = time;
            time -= 1;
            Time = t;
        }
    }
    
    public float RealTime { get; private set; }
    
    public float Time
    {
        get => time;
        set
        {
            var newTime = value;

            var oldRealTime = RealTime;
            RealTime = value;
            
            var max = length - startTime;

            if (loop)
            {
                if (newTime > length)
                {
                    newTime = startTime + (newTime - length) % max;
                }
                else if(newTime < startTime)
                {
                    newTime = length + (newTime - startTime) % max;
                }
            }
            else
            {
                newTime = Mathf.Clamp(newTime, 0, length);
            }
            
            time = newTime;
            eventsAction = null;
            
            foreach (var eevent in SelectEvents(events, oldRealTime, RealTime, new Vector2(startTime, length), reverse))
            {
                eventsAction += eevent.Invoke;
            }
        }
    }

    public BadassAnimationClip Clip
    {
        get => currentClip;
        set
        {
            Unregister(this);
            currentEvaluators.Clear();

            foreach (var handler in currentHandlers)
            {
                handler.Stop();
            }
            
            currentHandlers = ListSpan<Handler>.Empty;
            
            if (value == null || currentClip == value || !isActiveAndEnabled)
            {
                currentClip = value;
                return;
            }
            
            currentClip = value;
            var d = dataByClip[value.guid];
            events = d.events;
            length = value.length;
            var handlers = d.handlers;
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
            
            foreach (var handler in currentHandlers)
            {
                handler.Start();
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
        Time = 0;
    }

    private void Awake()
    {
        TryInit();
        Clip = defaultClip;
    }

    internal void TryInit()
    {
        if(dataByClip != null) return;
        
        dataByClip = new();
        data ??= new List<Data>();
        
        for (int i = 0; i < data.Count; i++)
        {
            dataByClip.Add(data[i].clip.guid, data[i]);
        }
    }

    public void BeforeEvaluate()
    {
        Time += UnityEngine.Time.deltaTime;
        
        for (int i = 0; i < currentEvaluators.Count; i++)
        {
            currentEvaluators[i].x = Time;
        }
    }

    public IEnumerable<IEvaluator> Evaluators => currentEvaluators;
    
    public void AfterEvaluate()
    {
        eventsAction?.Invoke();
        for (int i = 0; i < currentHandlers.Count; i++)
        {
            var handler = currentHandlers[i];
            handler.Handle();
        }
    }

    public void Add(BadassAnimationClip newClip, List<Handler> handlers, List<Event> events)
    {
        var d = new Data { clip = newClip, handlers = handlers, events = events };
        data.Add(d);
        dataByClip.Add(newClip.guid, d);
    }
    
    public static IEnumerable<Event> SelectEvents(List<Event> events, float startTime, float endTime, Vector2 clampRange, bool reverse)
    {
        float length = clampRange.y - clampRange.x;
        float offsetForEvents = startTime;
        offsetForEvents -= ((offsetForEvents - clampRange.x) % length + length) % length + clampRange.x;
        
        int startIndex = 0;
        int endIndex = events.Count - 1;
        
        while (startIndex < events.Count && events[startIndex].x < clampRange.x)
        {
            startIndex++;
        }

        while (endIndex >= 0 && events[endIndex].x > clampRange.y)
        {
            endIndex--;
        }
        endIndex++;

        var eventsSpan = events.AsSpan(startIndex..endIndex);
        
        if(eventsSpan.Count == 0) yield break;
        
        if (reverse)
        {
            startTime *= -1;
            endTime *= -1;
            
            while (true)
            {
                for (int i = eventsSpan.Count - 1; i >= 0; i--)
                {
                    var eevent = eventsSpan[i];
                    var x = eevent.x * -1 - offsetForEvents;
                    bool start = x > startTime;
                    bool end = x <= endTime;
                
                    if (start && end)
                    {
                        yield return eevent;
                    }
                    else if(!end)
                    {
                        yield break;
                    }
                }

                offsetForEvents -= length;
            }
        }

        while (true)
        {
            for (int i = 0; i < eventsSpan.Count; i++)
            {
                var eevent = eventsSpan[i];
                var x = eevent.x + offsetForEvents;
                bool start = x > startTime;
                bool end = x <= endTime;
                
                if (start && end)
                {
                    yield return eevent;
                }
                else if(!end)
                {
                    yield break;
                }
            }

            offsetForEvents += length;
        }
    }

    public bool TryGetData(BadassAnimationClip clip, out Data data) => TryGetData(clip.guid, out data);
    public bool TryGetData(string guid, out Data data) => dataByClip.TryGetValue(guid, out data);

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
    
    private void OnDrawGizmos()
    {
        foreach (var handler in currentHandlers)
        {
            handler.DrawGizmos();
        }
    }

    [SceneGUI]
    private void OnSceneGUI()
    {
        foreach (var handler in currentHandlers)
        {
            handler.OnSceneGUI();
        }
    }
#endif
}