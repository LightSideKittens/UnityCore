using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using InternalBindings;
using LSCore.Attributes;
using LSCore.DataStructs;
using LSCore.Extensions.Unity;
using Sirenix.OdinInspector;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Profiling;
#if UNITY_EDITOR
using Sirenix.Utilities.Editor;
using UnityEditor;
#endif

[assembly: InternalsVisibleTo("LSCore.MoveIt.Editor")]
public partial class MoveIt : MonoBehaviour, IAnimatable
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
    
    internal static List<Handler> handlersBuffer = new();

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
    private List<HandlerEvaluateData> currentEvaluators = new();
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
            foreach (var handler in currentHandlers)
            {
                handler.Stop();
            }

            ResetProps();
            UnBindCurrent();
            
            currentHandlers = new List<Handler>();
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
                handler.ClearEvaluators();
                
                if (currentClip.namesToCurvesByHandlerGuids.TryGetValue(handler.guid, out var curves))
                {
                    foreach (var (propertyName, curve) in curves)
                    {
                        handler.AddEvaluator(propertyName, curve, out _);
                    }
                    
                    currentHandlers.Add(handler);
                    
                    var evaluators = handler.evaluators;
                    for (int j = 0; j < evaluators.Count; j++)
                    {
                        currentEvaluators.Add(evaluators[j].evaluator);
                    }
                }
            }

#if UNITY_EDITOR
            foreach (var handler in currentHandlers)
            {
                handler.isPreview = isPreview;
            }
#endif
            
            BindCurrent();
            
            handlersBuffer.Clear();
            foreach (var handler in currentHandlers)
            {
                handler.Start();
            }

            GetProps();
            UpdateProps();
            
            MoveItEvaluator.Register(this, updateMode);
            OnClipChanged();
        }
    }
    
    private void Register()
    {
        updateModeAtRegister = updateMode;
        MoveItEvaluator.Register(this, updateMode);
    }

    private void Unregister() => MoveItEvaluator.Unregister(this, updateModeAtRegister);

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
        Clip = defaultClip;
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
    public IEnumerable<MoveItClip> Clips => data != null ? data.Select(x => x.clip) : Array.Empty<MoveItClip>();

    void IAnimatable.AfterEvaluate() => AfterEvaluate();
    
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

    private void GetProps()
    {
        if(floatProps.Length == 0 || !wasBinded) return;
        
        GenericBindingUtility.GetValues(floatProps, floatValues);
        
        var index = 0;
            
        for (int i = 0; i < handlersBuffer.Count; i++)
        {
            var handler = handlersBuffer[i];
                
            var bindableProps = handler.evaluators;
            for (int j = 0; j < bindableProps.Count; j++)
            {
                bindableProps[j].evaluator.startY = floatValues[index];
                index++;
            }
        }
    }

    private void UpdateProps()
    {
        if(floatProps.Length == 0 || !wasBinded) return;
        
        var index = 0;
            
        for (int i = 0; i < handlersBuffer.Count; i++)
        {
            var handler = handlersBuffer[i];
                
            var bindableProps = handler.evaluators;
            for (int j = 0; j < bindableProps.Count; j++)
            {
                floatValues.Write(index, bindableProps[j].evaluator.y);
                index++;
            }
        }
        
        SetValues(floatProps, floatValues);
    }

    public static unsafe void SetValues(
        NativeArray<BoundProperty> boundProperties,
        NativeArray<float> values)
    {
        var unsafePtr = NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(boundProperties);
        var pointerWithoutChecks = NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(values);
        GenericBindingUtilityProxy.SetFloatValues(unsafePtr, boundProperties.Length, pointerWithoutChecks, values.Length);
    }
    
    private void ResetProps()
    {
        if(floatProps.Length == 0 || !wasBinded) return;
        
        var index = 0;
            
        for (int i = 0; i < handlersBuffer.Count; i++)
        {
            var handler = handlersBuffer[i];
                
            var bindableProps = handler.evaluators;
            for (int j = 0; j < bindableProps.Count; j++)
            {
                floatValues.Write(index, bindableProps[j].evaluator.startY);
                index++;
            }
        }
        
        SetValues(floatProps, floatValues);
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
        List<Event>      events,
        float            startTime,
        float            endTime,
        Vector2          clampRange,
        bool             reverse,
        List<Event>      result)
    {
        result.Clear();
        if (events == null || events.Count == 0) return;

        /* 1. Предварительно вычисляем нужный "срез" списка ------------- */
        var length          = clampRange.y - clampRange.x;

        float offsetForEvents = startTime;
        offsetForEvents      -= ((offsetForEvents - clampRange.x) % length + length) % length
                              +  clampRange.x;

        int startIndex = 0;
        int endIndex   = events.Count - 1;

        while (startIndex < events.Count && events[startIndex].x < clampRange.x)
            startIndex++;

        while (endIndex >= 0 && events[endIndex].x > clampRange.y)
            endIndex--;

        endIndex++;                       // сделать endIndex «не включительно»

        var span = events.AsSpan(startIndex..endIndex);
        if (span.Count == 0) return;

        /* 2. Основной проход без alloc‑ов ------------------------------ */
        if (reverse)
        {
            startTime *= -1f;
            endTime   *= -1f;

            while (true)
            {
                for (int i = span.Count - 1; i >= 0; --i)
                {
                    var e = span[i];
                    float x =  -e.x - offsetForEvents;     // зеркалим

                    if (x > startTime && x <= endTime)
                        result.Add(e);
                    else if (x > endTime)                  // ушли дальше интервала
                        return;
                }
                offsetForEvents -= length;                 // прыжок на предыдущий «период»
            }
        }
        else
        {
            while (true)
            {
                for (int i = 0; i < span.Count; ++i)
                {
                    var e = span[i];
                    float x =  e.x + offsetForEvents;

                    if (x > startTime && x <= endTime)
                        result.Add(e);
                    else if (x > endTime)
                        return;
                }
                offsetForEvents += length;                 // следующий «период»
            }
        }
    }

    public bool TryGetData(MoveItClip clip, out Data data) => TryGetData(clip.guid, out data);
    public bool TryGetData(string guid, out Data data) => dataByClip.TryGetValue(guid, out data);

#if UNITY_EDITOR
    public static event Action<MoveIt> NeedShowWindow;
    private bool isPreview = true;
    
    internal void Editor_SetClip(MoveItClip clip, bool isPreview)
    {
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