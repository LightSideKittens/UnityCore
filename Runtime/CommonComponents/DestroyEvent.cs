using System;
using UnityEngine;

namespace LSCore.CommonComponents
{
    public class DestroyEvent : MonoBehaviour
    {
        public event Action Destroyed;

        private void OnDestroy()
        {
            Destroyed?.Invoke();
        }
    }
}

