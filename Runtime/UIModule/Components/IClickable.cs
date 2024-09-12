using System;
using UnityEngine;

namespace LSCore
{
    public interface IClickable
    {
        Transform Transform { get; }
        Action Clicked { get; set; }
    }
    
    public interface IToggle : IClickable
    {
        bool IsOn { get; set; }
        void Set(bool value);
        void SetSilently(bool value);
        Action<bool> ValueChanged { get; set; }
    }
}