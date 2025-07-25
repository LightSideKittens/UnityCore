using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using LSCore.AnimationsModule;
using LSCore.Attributes;
using LSCore.DataStructs;
using LSCore.Extensions.Unity;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Animations;
using Debug = UnityEngine.Debug;
#if UNITY_EDITOR
using UnityEditor;
#endif

[assembly: InternalsVisibleTo("LSCore.MoveIt.Editor")]
public partial class MoveIt : MonoBehaviour, IAnimatable<MoveIt.HandlerEvaluator>
{
    public enum UpdateModeType
    {
        Update,
        FixedUpdate,
        Manual,
    }
    
    [Serializable]
    public class Data
    {
        public MoveItClip clip;

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
    
    internal static List<Handler> handlersBuffer = new(1024 * 16);

    [ValueDropdown("Clips")]
    public MoveItClip defaultClip;
    public bool loop;
    public bool reverse;

    [SerializeField]
    private UpdateModeType updateMode = UpdateModeType.Update;

    [HideInInspector] public List<Data> data;

    private UpdateModeType updateModeAtRegister;
    private Dictionary<string, Data> dataByClip;
    private MoveItClip currentClip;
    private List<Handler> currentHandlers = new();
    private List<Event> events = new();
    private List<HandlerEvaluator> currentEvaluators = new();
    private List<Event> selectedEvents = new();
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
            bool looped = false;
            
            if (loop)
            {
                if (newTime > length)
                {
                    looped = true;
                    newTime = startTime + (newTime - length) % max;
                }
                else if(newTime < startTime)
                {
                    looped = true;
                    newTime = length + (newTime - startTime) % max;
                }
            }
            else
            {
                newTime = Mathf.Clamp(newTime, 0, length);
            }

            if (looped)
            {
                OnLooped();
            }
            
            time = newTime;
            eventsAction = null;
            SelectEvents(events, oldRealTime, RealTime, new Vector2(startTime, length), reverse, selectedEvents);

            for (int i = 0; i < selectedEvents.Count; i++)
            {
                eventsAction += selectedEvents[i].Invoke;
            }
        }
    }
    
    public MoveItClip Clip
    {
        get => currentClip;
        set
        {
            if (currentClip == value) return;
            
            Unregister();
            currentEvaluators.Clear();

            handlersBuffer.Clear();

            for (int i = 0; i < currentHandlers.Count; i++)
            {
                currentHandlers[i].Stop();
            }

            ResetProps();
            UnBindCurrent();
            
            currentHandlers.Clear();
            currentClip = value;
            
            if (value == null) return;
            
            time = 0;
            RealTime = 0;
            var d = dataByClip[value.guid];
            for (int i = 0; i < events.Count; i++)
            {
                events[i].End();
            }
            events = d.events;
            for (int i = 0; i < events.Count; i++)
            {
                events[i].Start();
            }
            Length = value.length;
            
            var handlers = d.handlers;
            for (int i = 0; i < handlers.Count; i++)
            {
                var handler = handlers[i];
                
                if (currentClip.evaluatorsByHandlerGuids.TryGetValue(handler.guid, out var curves))
                {
                    var evaluators = handler.evaluators;
                    if (evaluators.Count == 0)
                    {
                        foreach (var evaluator in curves.Values)
                        {
                            var newEvaluator = new HandlerEvaluator
                            {
#if UNITY_EDITOR
                                rawProperty = evaluator.rawProperty,
#endif
                                property = evaluator.property,
                                curve = evaluator.curve,
                                propertyType = evaluator.propertyType,
                            };
                            handler.AddEvaluator(newEvaluator);
                            currentEvaluators.Add(newEvaluator);
                        }
                    }
                    else
                    {
                        int index = 0;
                        foreach (var evaluator in curves.Values)
                        {
                            var ev = evaluators[index];
#if UNITY_EDITOR
                            ev.rawProperty = evaluator.rawProperty;
#endif
                            ev.property = evaluator.property;
                            ev.curve = evaluator.curve;
                            ev.propertyType = evaluator.propertyType;
                            currentEvaluators.Add(ev);
                            index++;
                        }
                    }
                    
                    currentHandlers.Add(handler);
                }
            }

#if UNITY_EDITOR
            for (int i = 0; i < currentHandlers.Count; i++)
            {
                currentHandlers[i].isPreview = isPreview;
            }
#endif
            BindCurrent(); //TODO: Parallel.For

            handlersBuffer.Clear();
            for (int i = 0; i < currentHandlers.Count; i++)
            {
                currentHandlers[i].Start();
            }

            GetAndUpdateProps();
            
            MoveItEvaluator<HandlerEvaluator>.Register(this, updateMode);
            OnClipChanged();
        }
    }
    
    private void Register()
    {
        updateModeAtRegister = updateMode;
        MoveItEvaluator<HandlerEvaluator>.Register(this, updateMode);
    }

    private void Unregister() => MoveItEvaluator<HandlerEvaluator>.Unregister(this, updateModeAtRegister);

#if UNITY_EDITOR
    [Button(DirtyOnClick = false)]
    private void Edit()
    { 
        NeedShowWindow?.Invoke(this);
    }
#endif

    private void OnClipChanged()
    {
        Time = 0;
    }

    internal void OnLooped()
    {
        foreach (var handler in currentHandlers)
        {
            handler.OnLooped();
        }
    }

    private void Awake()
    {
        TryInit();
        Clip = defaultClip; //TODO: Parallel.For
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


    private void OnDestroy()
    {
        Clip = null;
    }

    internal void TryInit()
    {
        if(dataByClip != null) return;
        
        dataByClip = new();
        data ??= new List<Data>();
        
        for (int i = 0; i < data.Count; i++)
        {
            var d = data[i];
            if (d == null || d.clip == null)
            {
                data.RemoveAt(i);
                i--;
                continue;
            }
            dataByClip.Add(d.clip.guid, d);
        }
    }

    void IAnimatable<HandlerEvaluator>.BeforeEvaluate(float deltaTime)
    {
        if (reverse) deltaTime *= -1;
        Time += deltaTime;
        
        for (int i = 0; i < currentEvaluators.Count; i++)
        {
            currentEvaluators[i].x = Time;
        }
    }

    IList<HandlerEvaluator> IAnimatable<HandlerEvaluator>.Evaluators => currentEvaluators;
    public IEnumerable<MoveItClip> Clips => data != null ? data.Select(x => x.clip) : Array.Empty<MoveItClip>();

    void IAnimatable<HandlerEvaluator>.AfterEvaluate() => AfterEvaluate();
    
    private void AfterEvaluate()
    {
        eventsAction?.Invoke();
        handlersBuffer.Clear();
        
        for (int i = 0; i < currentHandlers.Count; i++)
        {
            var handler = currentHandlers[i];
            handler.Handle();
        }
        
        UpdateProps();
    }

    private void GetAndUpdateProps()
    {
        if(!isBound) return;
        
        var floatIndex = 0;
        var intIndex = 0;
            
        for (int i = 0; i < handlersBuffer.Count; i++)
        {
            var handler = handlersBuffer[i];
            var objects = handler.Objects;
            var obj = handler.Target;
            var propertyHandler = obj as IPropertyHandler;
            
            var evaluators = handler.evaluators;
            for (int j = 0; j < evaluators.Count; j++)
            {
                evaluators[j].getUpdate(ref floatIndex, ref intIndex, objects, handler, obj, propertyHandler);
            }
        }
    }

    private void UpdateProps()
    {
        if(!isBound) return;
        
        var floatIndex = 0;
        var intIndex = 0;
            
        for (int i = 0; i < handlersBuffer.Count; i++)
        {
            var handler = handlersBuffer[i];
            var objects = handler.Objects;
            var obj = handler.Target;
            var propertyHandler = obj as IPropertyHandler;

            var evaluators = handler.evaluators;
            for (int j = 0; j < evaluators.Count; j++)
            {
                evaluators[j].update(ref floatIndex, ref intIndex, objects, handler, obj, propertyHandler);
            }
        }
    }
    
    private void ResetProps()
    {
        if(!isBound) return;
        
        var floatIndex = 0;
        var intIndex = 0;
            
        for (int i = 0; i < handlersBuffer.Count; i++)
        {
            var handler = handlersBuffer[i];
            var obj = handler.Target;
            var propertyHandler = obj as IPropertyHandler;
            var objects = handler.Objects;
            
            var evaluators = handler.evaluators;
            for (int j = 0; j < evaluators.Count; j++)
            {
                evaluators[j].reset(ref floatIndex, ref intIndex, objects, handler, obj, propertyHandler);
            }
        }
    }
    
    
    public void Play(MoveItClip clip)
    {
        Clip = clip;
    }
    
    public void Stop(MoveItClip clip)
    {
        Clip = null;
    }

    public static void SelectEvents(
        List<Event> events,
        float startTime,
        float endTime,
        Vector2 clampRange,
        bool reverse,
        List<Event> result)
    {
        result.Clear();
        if (events == null || events.Count == 0) return;

        var length = clampRange.y - clampRange.x;

        float offsetForEvents = startTime;
        offsetForEvents -= ((offsetForEvents - clampRange.x) % length + length) % length
                           + clampRange.x;

        int startIndex = 0;
        int endIndex = events.Count - 1;

        while (startIndex < events.Count && events[startIndex].x < clampRange.x)
            startIndex++;

        while (endIndex >= 0 && events[endIndex].x > clampRange.y)
            endIndex--;

        endIndex++;

        var span = events.Slice(startIndex..endIndex);
        if (span.Count == 0) return;

        if (reverse)
        {
            startTime *= -1f;
            endTime *= -1f;

            while (true)
            {
                for (int i = span.Count - 1; i >= 0; --i)
                {
                    var e = span[i];
                    float x = -e.x - offsetForEvents;

                    if (x > startTime && x <= endTime)
                        result.Add(e);
                    else if (x > endTime)
                        return;
                }

                offsetForEvents -= length;
            }
        }

        while (true)
        {
            for (int i = 0; i < span.Count; ++i)
            {
                var e = span[i];
                float x = e.x + offsetForEvents;

                if (x > startTime && x <= endTime)
                    result.Add(e);
                else if (x > endTime)
                    return;
            }

            offsetForEvents += length;
        }
    }

    public bool TryGetData(MoveItClip clip, out Data data) => TryGetData(clip.guid, out data);
    public bool TryGetData(string guid, out Data data) => dataByClip.TryGetValue(guid, out data);

#if UNITY_EDITOR
    public static event Action<MoveIt> NeedShowWindow;
    private bool isPreview = true;
    
    internal void Editor_SetClip(MoveItClip clip, bool isPreview)
    {
        if (clip != null)
        {
            var d = dataByClip[clip.guid];
            
            var handlers = d.handlers;
            for (int i = 0; i < handlers.Count; i++)
            {
                handlers[i].ClearEvaluators();
            }
        }
        
        this.isPreview = isPreview;
        Clip = clip;
        this.isPreview = true;
    }
    
    internal void Editor_Evaluate(float time, bool needApply)
    {
        foreach (var evaluator in currentEvaluators)
        {
            evaluator.x = time;
            evaluator.Evaluate();
        }

        if (needApply)
        {
            AfterEvaluate();
        }
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
    
    internal bool Add(MoveItClip newClip, List<Handler> handlers, List<Event> events)
    {
        if(dataByClip.ContainsKey(newClip.guid)) return false;
        var d = new Data { clip = newClip, handlers = handlers, events = events };
        data.Add(d);
        dataByClip.Add(newClip.guid, d);
        return true;
    }
    
    internal void Remove(MoveItClip clip)
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
    }
    
    public void TrimModifications(List<UndoPropertyModification> modifications)
    {
        foreach (var handler in currentHandlers)
        {
            handler.TrimModifications(modifications);
        }
    }
    
    public void StartAnimationMode()
    {
        foreach (var handler in currentHandlers)
        {
            handler.StartAnimationMode();
        }
    }
    
#endif
}