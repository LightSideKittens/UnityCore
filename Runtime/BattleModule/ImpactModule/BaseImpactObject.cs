using System;
using UnityEngine;

namespace LSCore
{
    public abstract class BaseImpactObject : MonoBehaviour
    {
        protected Func<Collider2D, bool> canImpactChecker;
        protected Collider2D ignoredCollider;
        protected Transform initiator;
        
        public virtual Func<Collider2D, bool> CanImpactChecker
        {
            set => canImpactChecker = value;
        }
        
        public virtual Collider2D IgnoredCollider
        {
            set => ignoredCollider = value;
        }
        
        protected bool IgnoreCollider
        {
            set
            {
                if (ignoredCollider is not null) ignoredCollider.enabled = !value;
            }
        }
        
        public virtual Transform Initiator
        {
            set => initiator = value;
        }
        
        
    }
}