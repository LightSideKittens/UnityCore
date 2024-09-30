using UnityEngine;

namespace LSCore
{
    public class WorldSpace : MonoBehaviour
    {
        public bool onlyOnEnable;
        private new Transform transform;
        
        private void OnEnable()
        {
            transform = base.transform;
            
            if (onlyOnEnable)
            {
                UpdateTransform();
            }
        }

        private void LateUpdate()
        {
            if (!onlyOnEnable)
            {
                UpdateTransform();
            }
        }

        private void UpdateTransform()
        {
            var parent = transform.parent;
            if (parent!= null)
            {
                Vector3 parentScale = parent.lossyScale;
                transform.localScale = new Vector3(
                    1 / parentScale.x,
                    1 / parentScale.y,
                    1 / parentScale.z
                );
            }
            else
            {
                transform.localScale = Vector3.one;
            }
        }
    }
}