using System;
using UnityEngine;

namespace LSCore.BattleModule
{
    public class CompData
    {
        public Transform transform;
        public System.Action onInit;
        public System.Action reset;
        public System.Action enable;
        public System.Action disable;
        public System.Action destroy;
        public System.Action update;
        public System.Action fixedUpdate;

        public void Remove<T>() where T : BaseComp
        {
            TransformDict<T>.Remove(transform);
        }
    }
}