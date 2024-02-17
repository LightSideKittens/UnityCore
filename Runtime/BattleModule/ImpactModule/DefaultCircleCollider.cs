using System;
using UnityEngine;

namespace LSCore
{
    public class DefaultCircleCollider : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D other)
        {
            UnityEngine.Debug.Log($"Enter {other.name}");
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            UnityEngine.Debug.Log($"Exit {other.name}");
        }
    }
}