using System;
using LSCore.Extensions.Unity;
using UnityEngine;

public class DestroyEvent : MonoBehaviour, DestroyEvent.I
{
    public static void AddOnDestroy(object obj, Action onDestroy)
    {
        switch (obj)
        {
            case I e:
                e.Destroyed += onDestroy;
                break;
            case Component component:
            {
                var de = component.GetOrAddComponent<DestroyEvent>();
                de.Destroyed += onDestroy;
                break;
            }
            case GameObject gameObject:
                var dee = gameObject.GetOrAddComponent<DestroyEvent>();
                dee.Destroyed += onDestroy;
                break;
        }
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