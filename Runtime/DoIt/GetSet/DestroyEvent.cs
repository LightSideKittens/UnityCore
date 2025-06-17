using System;
using UnityEngine;

public class DestroyEvent : MonoBehaviour, DestroyEvent.I
{
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