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
                e.Destroyed += onDestroy;
                break;
            case Component component:
            {
                de = component.GetOrAddComponent<DestroyEvent>();
                de.Destroyed += onDestroy;
                break;
            }
            case GameObject gameObject:
                de = gameObject.GetOrAddComponent<DestroyEvent>();
                de.Destroyed += onDestroy;
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
    }
    
    public event Action Destroyed;

    private void OnDestroy()
    {
        Destroyed?.Invoke();
        Destroyed = null;
    }
}