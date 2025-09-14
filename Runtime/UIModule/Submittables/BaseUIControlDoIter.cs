using System;
using System.Collections.Generic;
using UnityEngine;

namespace LSCore
{
    [Serializable]
    public abstract class BaseUIControlDoIter : BaseUIControlHandler
    {
        [SerializeReference] public List<DoIt> onActivate = new();
        public Action onActivateAction;
        
        public void Subscribe()
        {
            UIControl.Activated += Do;
        }

        public void Unsubscribe()
        {
            UIControl.Activated -= Do;
        }

        public void Do()
        {
            onActivate.Do();
            onActivateAction?.Invoke();
        }
    }
    
    [Serializable]
    public class DefaultUIControlDoIter : BaseUIControlDoIter
    {
        protected override void Init()
        {
            Subscribe();
        }
    }
}