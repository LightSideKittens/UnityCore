using System;
using UnityEngine;

namespace LSCore
{
    [DisallowMultipleComponent]
    public abstract class BaseSingleService : MonoBehaviour
    {
        public abstract Type Type { get; }
    }
}