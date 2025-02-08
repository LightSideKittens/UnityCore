using System;
using UnityEngine;

namespace LSCore.BadassAnimationModule.Events
{
    [Serializable]
    public class SetParent : BadassAnimation.Event
    {
        public Transform target;
        public Transform parent;
        public bool saveWorldPosition = true;
        private Transform lastParent;

        public override void Start()
        {
            lastParent = target.parent;
        }

        public override void Invoke()
        {
            target.SetParent(parent, saveWorldPosition);
        }

        public override void End()
        {
            target.parent = lastParent;
        }
    }
}