using System;

namespace LSCore
{
    [Serializable]
    public abstract class BaseSelectableHandler
    {
        public abstract void OnSelect();
        public abstract void OnDeselect();
    }
    
    [Serializable]
    public abstract class BaseSelectAnimHandler
    {
        
    }
    
    [Serializable]
    public abstract class BaseHandler
    {
        
    }
}