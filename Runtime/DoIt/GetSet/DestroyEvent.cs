using System;
using LSCore;
using LSCore.Extensions.Unity;
using UnityEngine;

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
    }
}