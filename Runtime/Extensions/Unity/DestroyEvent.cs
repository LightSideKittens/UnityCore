using System;
using DG.Tweening;
using LSCore;
using LSCore.Extensions;
using LSCore.Extensions.Unity;
using UnityEngine;

public static class Extensions
{
    public static Tween KillOnDestroy(this Tween tween)
    {
        DestroyEvent.AddOnDestroy(tween.target, tween.KillVoid);
        return tween;
    }
        
    public static Tween KillOnDestroy(this Tween tween, object obj)
    {
        tween.target = obj;
        return tween.KillOnDestroy();
    }
    
    public static void KillImmediate(this Tween tween)
    {
        tween.onKill?.Invoke();
        tween.onKill = null;
        tween.Kill();
    }

    public static Tween KillOnEverySecondLoop(this Tween tween)
    {
        tween.OnStepComplete(() =>
        {
            var loops = (int)(tween.fullPosition / tween.Duration(false));
            if(loops % 2 == 0) tween.Kill();
        });
        return tween;
    }
}

[DisallowMultipleComponent]
public class DestroyEvent : MonoBehaviour, DestroyEvent.I
{
    public static void AddOnDestroy(object obj, Action onDestroy)
    {
        DestroyEvent de = null;
        switch (obj)
        {
            case I e:
                e.Subscribe(onDestroy);
                break;
            case Component component:
            {
                de = component.GetOrAddComponent<DestroyEvent>();
                de.Subscribe(onDestroy);
                break;
            }
            case GameObject gameObject:
                de = gameObject.GetOrAddComponent<DestroyEvent>();
                de.Subscribe(onDestroy);
                break;
        }
        
#if UNITY_EDITOR
        if (de != null && World.IsEditMode)
        {
            de.hideFlags = HideFlags.HideAndDontSave;
        }
#endif
    }
    
    public interface I
    {
        event Action Destroyed;
        void Subscribe(Action onDestroy);
    }
    
    public event Action Destroyed;
    private static Action updated;

    static DestroyEvent()
    {
        World.Updated += () => updated?.Invoke();
    }

    void I.Subscribe(Action onDestroy) => Subscribe(onDestroy);
    
    private void Subscribe(Action onDestroy)
    {
        if (!didAwake)
        { 
            updated += CheckDestroy;
        }
        
        Destroyed += onDestroy;
    }

    private void Awake()
    {
        updated -= CheckDestroy;
    }

    private void CheckDestroy()
    {
        if (this == null)
        {
            updated -= CheckDestroy;
            OnDestroy();
        }
    }

    private void OnDestroy()
    {
        Destroyed?.Invoke();
        Destroyed = null;
    }
}