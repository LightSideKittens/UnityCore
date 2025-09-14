using System;
using UnityEngine;
using UnityEngine.EventSystems;
namespace LSCore
{
    public class UIControlRect : MonoBehaviour, IUIControlElement
    {
        [SerializeReference] public DefaultUIControl uiControl = new();
        object IUIControlElement.UIControl => uiControl;
        
        public event Action Did
        {
            add => uiControl.Did += value;
            remove => uiControl.Did -= value;
        }
        
        public void Do() => uiControl.doIter.Do();
        public void Activate() => uiControl.Activate();
        
        protected void Awake()
        {
            uiControl.Init(transform);
        }
        
        protected void OnDisable()
        {
            uiControl.OnDisable();
        }
    }
}