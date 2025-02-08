using System;
using LSCore;
using LSCore.Attributes;
using LSCore.ConditionModule;
using Sirenix.OdinInspector;
using UnityEngine;

public partial class BadassAnimation
{
    [Serializable]
    public abstract class Event
    {
        [HideInInspector] public float x;

        public virtual void Start()
        {
        }

        public abstract void Invoke();

        public virtual void End()
        {
        }
    }

    [Serializable]
    public abstract class BaseActionEvent : Event
    {
        public abstract LSAction Action { get; }

        public override void Invoke()
        {
            Action.Invoke();
        }
    }

    [Serializable]
    public class ActionEvent : BaseActionEvent
    {
        [SerializeReference] [HideLabel]
        public LSAction action;

        public override LSAction Action => action;
    }

    [Serializable]
    public class ConditionEvent : Event
    {
        [SerializeReference] public Conditions condition;

        [SerializeReference] [HideLabel] [ExcludeType(typeof(ConditionEvent))]
        public Event eventt;

        public override void Start()
        {
            if (condition)
            {
                eventt.Start();
            }
        }

        public override void Invoke()
        {
            if (condition)
            {
                eventt.Invoke();
            }
        }

        public override void End()
        {
            if (condition)
            {
                eventt.End();
            }
        }
    }

    [Serializable]
    public class RuntimeOnlyEvent : Event
    {
        [SerializeReference] [HideLabel] [ExcludeType(typeof(RuntimeOnlyEvent))]
        public Event eventt;

        public override void Start()
        {
            if (World.IsPlaying)
            {
                eventt.Start();
            }
        }

        public override void Invoke()
        {
            if (World.IsPlaying)
            {
                eventt.Invoke();
            }
        }

        public override void End()
        {
            if (World.IsPlaying)
            {
                eventt.End();
            }
        }
    }
}