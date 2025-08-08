using System;
using System.Collections.Generic;
using UnityEngine;

namespace LSCore
{
    [Serializable]
    public abstract class BaseSubmittableDoIter : BaseSubmittableHandler
    {
        [SerializeReference] public List<DoIt> onSubmit = new();
        public Action onSubmitAction;
        
        public void Subscribe()
        {
            Submittable.Submitted += OnSubmit;
        }

        public void Unsubscribe()
        {
            Submittable.Submitted -= OnSubmit;
        }

        private void OnSubmit()
        {
            onSubmit.Do();
            onSubmitAction?.Invoke();
        }
    }
    
    [Serializable]
    public class DefaultSubmittableDoIter : BaseSubmittableDoIter
    {
        protected override void Init()
        {
            Subscribe();
        }
    }
}