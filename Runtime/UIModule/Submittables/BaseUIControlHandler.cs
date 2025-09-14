using System;

namespace LSCore
{
    [Serializable]
    public abstract class BaseUIControlHandler
    {
        public IUIControl UIControl { get; private set; }

        public void Init(IUIControl uiControl)
        {
            UIControl = uiControl;
            Init();
        }
        
        protected virtual void Init(){}
        public virtual void OnEnable(){}
        public virtual void OnDisable(){}
    }
}