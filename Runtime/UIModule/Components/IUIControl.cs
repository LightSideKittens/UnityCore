using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace LSCore
{
    public interface IUIControl : 
        IPointerDownHandler, IPointerUpHandler,
        IPointerClickHandler,
        ISelectHandler, IDeselectHandler,
        ISubmitHandler,
        IPointerEnterHandler, IPointerExitHandler,
        IMoveHandler
    {
        Transform Transform { get; }
        event Action Activated;
        UIControlStates States { get; }
        void Init(Transform transform);
        void OnEnable();
        void OnDisable();
    }
    
    public interface IToggle : IUIControlElement
    {
        bool IsOn { get; set; }
        void Set(bool value);
        Action<bool> ValueChanged { get; set; }
    }
}