using UnityEngine;

namespace LSCore
{
    public abstract class Mark<T> : MonoBehaviour
    {
        [Id(typeof(MarkIdGroup), "AllMarks")] public Id id;
        [IdGroup] public IdGroup group;
        [Id(typeof(MarkIdGroup), "AllMarkTypes")] public Id markTypeId;

        private void OnEnable()
        {
            HandleView();
        }

        protected abstract void HandleView();

        protected bool TryGet(out T value)
        {
            if (group != null)
            {
                foreach (var targetId in group)
                {
                    if (Do(targetId, out value))
                    {
                        return true;
                    }
                }

                value = default;
                return false;
            }

            return Do(id, out value);
            
            bool Do(Id targetId, out T newVal)
            {
                return Marker.TryGet(markTypeId, targetId, out newVal);
            }
        }

        public void DoMark(T value)
        {
            if (group != null)
            {
                foreach (var targetId in group)
                {
                    Do(targetId);
                }
                    
                return;
            }

            Do(id);
                
            return;
            
            void Do(Id targetId)
            {
                Marker.Mark(markTypeId, targetId, value);
            }
        }
            
        public void DoUnMark()
        {
            if (group != null)
            {
                foreach (var targetId in group)
                {
                    Do(targetId);
                }
                    
                return;
            }

            Do(id);
            
            return;
            
            void Do(Id targetId)
            {
                Marker.UnMark(markTypeId, targetId);
            }
        }
    }
}