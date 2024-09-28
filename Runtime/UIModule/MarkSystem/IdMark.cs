using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LSCore
{
    public abstract class Mark<T> : MonoBehaviour
    {
        [SerializeReference] public MarkKeys provider = new MarkKeys.AsId();

        private void OnEnable()
        {
            HandleView();
        }

        protected abstract void HandleView();
        
        protected IEnumerable<T> Get()
        {
            if (provider.Group.Any())
            {
                foreach (var targetId in provider.Group)
                {
                    if (Do(targetId, out var value))
                    {
                        yield return value;
                    }
                }
            }

            if (Do(provider.Id, out var val))
            {
                yield return val;
            }
            
            bool Do(string targetId, out T newVal)
            {
                return Marker.TryGet(provider.MarkTypeId, targetId, out newVal);
            }
        }

        protected void DoMark(T value)
        {
            if (provider.Group.Any())
            {
                foreach (var targetId in provider.Group)
                {
                    Do(targetId);
                }
                    
                return;
            }

            Do(provider.Id);
                
            return;
            
            void Do(string targetId)
            {
                Marker.Mark(provider.MarkTypeId, targetId, value);
            }
        }
            
        protected void DoUnMark()
        {
            if (provider.Group.Any())
            {
                foreach (var targetId in provider.Group)
                {
                    Do(targetId);
                }
                    
                return;
            }

            Do(provider.Id);
            
            return;
            
            void Do(string targetId)
            {
                Marker.UnMark(provider.MarkTypeId, targetId);
            }
        }
    }
}