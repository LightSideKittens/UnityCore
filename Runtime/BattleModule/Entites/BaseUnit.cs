using System;
using System.Collections.Generic;
using LSCore.LevelSystem;
using Sirenix.OdinInspector;
using UnityEngine;
using static LSCore.BattleModule.ObjectsByTransforms<LSCore.BattleModule.BaseUnit>;

namespace LSCore.BattleModule
{
    public class BaseUnit : MonoBehaviour
    {
#if UNITY_EDITOR
        protected IEnumerable<Id> Ids => manager.Group;
        private bool HideManager => name.Contains("_Base");
#endif
        
        [SerializeField, ValueDropdown("Ids")] private Id id;
        public Dictionary<Type, Prop> Props { get; private set; }
        public string UserId { get; private set; }
        public string TeamId { get; private set; }
        public AffiliationType Affiliation { get; private set; }
        public new Transform transform { get; private set; }
        
        [ShowIf("$HideManager")]
        [SerializeField] protected LevelsManager manager;

        public Id Id => id;

        public float GetValue<T>() where T : FloatGameProp
        {
            return FloatGameProp.GetValue<T>(Props);
        }

        public virtual void Init(string userId, string teamId)
        {
            transform = base.transform;
            UserId = userId;
            TeamId = teamId;
            
            if (userId == BattlePlayerData.UserId)
            {
                Affiliation = AffiliationType.Self;
            }
            else if(teamId == BattlePlayerData.TeamId)
            {
                Affiliation = AffiliationType.Ally;
            }
            else
            {
                Affiliation = AffiliationType.Enemy;
            }
            
            Add(transform, this);
            Props = manager.GetProps(id);
        }

        public virtual void OnInit(){}

        public virtual void Destroy()
        {
            Remove(transform);
        }
    }
}