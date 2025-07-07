using System;
using UnityEngine;
using UnityEngine.EventSystems;
namespace LSCore
{
    public class SubmittableRect : MonoBehaviour, ISubmittableElement
    {
        [SerializeReference] public ISubmittable submittable = new DefaultSubmittable();
        [SerializeField] public ClickActions clickActions;
        public object Submittable => submittable;
        public event Action Submitted
        {
            add => submittable.Submitted += value;
            remove => submittable.Submitted -= value;
        }

        protected void Awake()
        {
            submittable.Init(transform);
        }
        
        protected void OnDisable()
        {
            submittable.OnDisable();
        }
    }
}