using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using LSCore.Attributes;
using LSCore.DataStructs;
using Sirenix.OdinInspector;
using UnityEngine;
[assembly: InternalsVisibleTo("LSCore.BadassAnimation.Editor")]
public partial class BadassAnimation : MonoBehaviour, IAnimatable
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
    
    public enum UpdateModeType
    {
        Update,
        FixedUpdate,
        Manual,
    }
    
    [Serializable]
    public class Data
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

    [ValueDropdown("Clips")]
    public BadassAnimationClip defaultClip;
    public bool loop;
    public bool reverse;

    [SerializeField]
    private UpdateModeType updateMode = UpdateModeType.Update;

    [HideInInspector] public List<Data> data;

    private UpdateModeType updateModeAtRegister;
    private Dictionary<string, Data> dataByClip;
    private BadassAnimationClip currentClip;
    private List<Handler> currentHandlers = new();
    private List<Event> events = new();
    private List<HandlerEvaluateData> currentEvaluators = new();
    private float startTime;
    private float time;
    private float length;
    private Action eventsAction;
    
    public UpdateModeType UpdateMode
    {
        get => updateMode;
        set
        {
            updateMode = value;
            Unregister();
            Register();
        }
    }
    
    public float StartTime
    {
        get => startTime;
        set
        {
            startTime = value >= length ? length : value;
            Time = RealTime;
        }
    }

    public float Length
    {
        get => length;
        set
        {
            if (startTime >= value)
            {
                startTime = value;
            }
            
            length = value;
            Time = RealTime;
        }
    }

    public float RealTime { get; private set; }
    
    public float Time
    {
        get => time;
        set
        {
            var oldRealTime = RealTime;
            RealTime = value;
            
            var newTime = value;

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
            if (currentClip == value) return;
            
            Unregister();
            currentEvaluators.Clear();
            
            foreach (var handler in currentHandlers)
            {
                handler.Stop();
            }
            
            currentHandlers = new List<Handler>();
            currentClip = value;
            
            if (value == null) return;
            
            time = 0;
            RealTime = 0;
            var d = dataByClip[value.guid];
            events = d.events;
            Length = value.length;
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

#if UNITY_EDITOR
            foreach (var handler in currentHandlers)
            {
                handler.IsPreview = isPreview;
            }
#endif
            
            foreach (var handler in currentHandlers)
            {
                handler.Start();
            }
            
            BadassAnimationEvaluator.Register(this, updateMode);
            OnClipChanged();
        }
    }

    private void OnEnable()
    {
        if (currentClip != null)
        {
            Register();
        }
    }

    private void OnDisable()
    {
        Unregister();
    }

    private void Register()
    {
        updateModeAtRegister = updateMode;
        BadassAnimationEvaluator.Register(this, updateMode);
    }

    private void Unregister() => BadassAnimationEvaluator.Unregister(this, updateModeAtRegister);

#if UNITY_EDITOR
    [Button]
    private void Edit()
    { 
        NeedShowWindow?.Invoke(this);
    }
#endif

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

    void IAnimatable.BeforeEvaluate(float deltaTime)
    {
        if (reverse) deltaTime *= -1;
        Time += deltaTime;
        
        for (int i = 0; i < currentEvaluators.Count; i++)
        {
            currentEvaluators[i].x = Time;
        }
    }

    IEnumerable<IEvaluator> IAnimatable.Evaluators => currentEvaluators;
    public IEnumerable<BadassAnimationClip> Clips => data.Select(x => x.clip);

    void IAnimatable.AfterEvaluate() => AfterEvaluate();
    
    private void AfterEvaluate()
    {
        eventsAction?.Invoke();
        for (int i = 0; i < currentHandlers.Count; i++)
        {
            var handler = currentHandlers[i];
            handler.Handle();
        }
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
    public static event Action<BadassAnimation> NeedShowWindow;
    private bool isPreview = true;
    
    internal void Editor_SetClip(BadassAnimationClip clip, bool isPreview)
    {
        this.isPreview = isPreview;
        Clip = clip;
        this.isPreview = true;
    }
    
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
    
    internal bool Add(BadassAnimationClip newClip, List<Handler> handlers, List<Event> events)
    {
        if(dataByClip.ContainsKey(newClip.guid)) return false;
        var d = new Data { clip = newClip, handlers = handlers, events = events };
        data.Add(d);
        dataByClip.Add(newClip.guid, d);
        return true;
    }
    
    internal void Remove(BadassAnimationClip clip)
    {
        var d = data.Find(d => d.clip == clip);
        if (d != null)
        {
            dataByClip.Remove(d.clip.guid);
            if (Clip == clip)
            {
                Clip = null;
            }
        }
    }
    
    internal void Remove(Handler handler)
    {
        var d = data.Find(d => d.handlers.Contains(handler));
        d?.handlers.Remove(handler);
        currentHandlers.Remove(handler);
    }
#endif
    
    public void Play(BadassAnimationClip clip)
    {
        Clip = clip;
    }
    
    public void Stop(BadassAnimationClip clip)
    {
        Clip = null;
    }
}