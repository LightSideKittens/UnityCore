using System;
using System.Collections.Generic;
using LSCore.Attributes;
using LSCore.Extensions;
using Sirenix.OdinInspector;
using UnityEngine;

namespace LSCore.LifecycleSystem
{
    [Serializable]
    [HideReferenceObjectPicker]
    [TypeFrom]
    public abstract class SingleSelector
    {
        public abstract LifecycleObject Select(List<LifecycleObject> objs);
    }

    [Serializable]
    public class RandomSingleSelector : SingleSelector
    {
        public override LifecycleObject Select(List<LifecycleObject> objs)
        {
            return objs.Random();
        }
    }
    

    [Serializable]
    [HideReferenceObjectPicker]
    [TypeFrom]
    public abstract class MultipleSelector
    {
        public abstract IEnumerable<LifecycleObject> Select(List<LifecycleObject> objs);
    }

    [Serializable]
    public class All : MultipleSelector
    {
        public override IEnumerable<LifecycleObject> Select(List<LifecycleObject> objs) => objs;
    }

    [Serializable]
    public class Several : MultipleSelector
    {
        public int count;
        [SerializeReference] public SingleSelector singleSelector;

        public override IEnumerable<LifecycleObject> Select(List<LifecycleObject> objs)
        {
            for (int i = 0; i < count; i++)
            {
                yield return singleSelector.Select(objs);
            }
        }
    }
    
    [Serializable]
    public class BySelectEx : MultipleSelector
    {
        [SelectEx] public string expression;

        public override IEnumerable<LifecycleObject> Select(List<LifecycleObject> objs)
        {
            return objs.BySelectEx(expression);
        }
    }
}