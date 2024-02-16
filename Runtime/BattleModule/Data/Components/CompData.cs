using System;
using UnityEngine;

namespace LSCore.BattleModule
{
    public class CompData
    {
        public Transform transform;
        public Action onInit;
        public Action reset;
        public Action enable;
        public Action disable;
        public Action destroy;
        public Action update;
        public Action fixedUpdate;

        public void Remove<T>() where T : BaseComp
        {
            TransformDict<T>.Remove(transform);
        }
    }
}