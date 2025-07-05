using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace LSCore
{
    public interface IClickable : 
        IPointerDownHandler, IPointerUpHandler,
        IPointerClickHandler,
        ISelectHandler, IDeselectHandler,
        ISubmitHandler,
        IPointerEnterHandler, IPointerExitHandler
    {
        Transform Transform { get; }
        Action Submitted { get; set; }
        ClickableStates States { get; }
    }
    
    public interface IToggle : IClickable
    {
        bool IsOn { get; set; }
        void Set(bool value);
        void SetSilently(bool value);
        Action<bool> ValueChanged { get; set; }
    }
}