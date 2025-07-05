using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace LSCore
{
    public interface ISubmittable : 
        IPointerDownHandler, IPointerUpHandler,
        IPointerClickHandler,
        ISelectHandler, IDeselectHandler,
        ISubmitHandler,
        IPointerEnterHandler, IPointerExitHandler
    {
        Transform Transform { get; }
        event Action Submitted;
        ClickableStates States { get; }
    }
    
    public interface IToggle : ISubmittable
    {
        bool IsOn { get; set; }
        void Set(bool value);
        void SetSilently(bool value);
        Action<bool> ValueChanged { get; set; }
    }
}