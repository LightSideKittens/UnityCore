using UnityEngine;

namespace LSCore.Extensions.Unity
{
    public static class RigidbodyExtensions
    {
        public static void SetPosition(this Rigidbody rb, Vector3 targetPosition)
        {
            Vector3 delta = targetPosition - rb.position;
            float distance = delta.magnitude;
            Vector3 direction = delta.normalized;
            
            if (!rb.SweepTest(direction, out _, distance, QueryTriggerInteraction.Ignore))
            {
                rb.position = targetPosition;
            }
        }
    }
}