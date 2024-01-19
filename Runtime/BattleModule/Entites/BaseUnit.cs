using System;
using System.Collections.Generic;
using LSCore;
using LSCore.LevelSystem;
using Sirenix.OdinInspector;
using UnityEngine;
using static Battle.ObjectsByTransfroms<Battle.Data.BaseUnit>;

namespace Battle.Data
{
    public class BaseUnit : MonoBehaviour
    {
#if UNITY_EDITOR
        protected IEnumerable<Id> Ids => manager.Group;
        private bool HideManager => name.Contains("_Base");
#endif
        
        
        [SerializeField, ValueDropdown("Ids")] private Id id;
        public Dictionary<Type, Prop> Props { get; private set; }
        
        public bool IsOpponent { get; private set; }
        public string UserId { get; private set; }
        public new Transform transform { get; private set; }
        
        [ShowIf("$HideManager")]
        [SerializeField] protected LevelsManager manager;

        public Id Id => id;

        public float GetValue<T>() where T : FloatGameProp
        {
            return FloatGameProp.GetValue<T>(Props);
        }

        public virtual void Init(string userId)
        {
            transform = base.transform;
            UserId = userId;
            IsOpponent = UserId == "Opponent";
            Add(transform, this);
            Props = manager.GetProps(id);
        }

        public virtual void Destroy()
        {
            Remove(transform);
        }
    }
}