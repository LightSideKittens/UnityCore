using System;
using System.Collections.Generic;
using LSCore.Attributes;
using LSCore.Extensions;
using Sirenix.OdinInspector;
using UnityEngine;

namespace LSCore.ObjectModule
{
    public partial class LifecycleManager<T>
    {
        [Serializable]
        [HideReferenceObjectPicker]
        [TypeFrom]
        public abstract class ObjectSelector
        {
            public abstract T Select(List<T> objs);
        }

        [Serializable]
        public class RandomObjectSelector : ObjectSelector
        {
            public override T Select(List<T> objs)
            {
                return objs.Random();
            }
        }


        [Serializable]
        [HideReferenceObjectPicker]
        [TypeFrom]
        public abstract class ObjectsSelector
        {
            public abstract IEnumerable<T> Select(List<T> objs);
        }

        [Serializable]
        public class All : ObjectsSelector
        {
            public override IEnumerable<T> Select(List<T> objs) => objs;
        }

        [Serializable]
        public class Several : ObjectsSelector
        {
            public int count;
            [SerializeReference] public ObjectSelector selector;

            public override IEnumerable<T> Select(List<T> objs)
            {
                for (int i = 0; i < count; i++)
                {
                    yield return selector.Select(objs);
                }
            }
        }

        [Serializable]
        public class BySelectEx : ObjectsSelector
        {
            [SelectEx] public string expression;

            public override IEnumerable<T> Select(List<T> objs)
            {
                return objs.BySelectEx(expression);
            }
        }
    }
}