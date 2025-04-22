/*using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LSCore.AnimationsModule
{
    [Serializable]
    public abstract class BaseTransformHandler : MoveIt.Vector3Handler, ITransformHandler
    {
        public Transform transform;
        
        public sealed override Object Target => transform;
        public MoveIt.Handler Handler => this;
        public Transform Transform
        {
            get => transform;
            set => transform = value;
        }
    }

    [Serializable]
    public class TransformPosition : BaseTransformHandler
    {
        [SerializeField] private bool add;
        private Vector3 startPosition;
        
#if UNITY_EDITOR
        protected override string Label => "Position";
        protected override string PropertyPath => "m_LocalPosition";
#endif

        protected override Vector3 GetStartValue() => transform.localPosition;

        protected override void OnStart()
        {
            startPosition = GetStartValue();
        }

        public override void OnLooped()
        {
#if UNITY_EDITOR
            if (World.IsEditMode) return;
#endif

            startPosition = GetStartValue();
        }

        protected override void OnHandle()
        {
            var target = value;
            if(add) target += startPosition;
            transform.localPosition = target;
        }
        
        protected override void OnStop()
        {
#if UNITY_EDITOR
            if (World.IsEditMode)
            {
                transform.localPosition = StartValue;
                return;
            }
#endif
            if (!add)
            {
                transform.localPosition = StartValue;
            }
        }
    }
}*/