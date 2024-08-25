using System.Collections.Generic;
using LSCore.LevelSystem;
using Sirenix.OdinInspector;
using UnityEngine;
using static LSCore.BattleModule.StaticDict<UnityEngine.Transform,LSCore.BattleModule.BaseUnit>;

namespace LSCore.BattleModule
{
    public class BaseUnit : MonoBehaviour
    {
#if UNITY_EDITOR
        protected IEnumerable<Id> Ids1 => manager.Ids;
        private bool HideManager => name.Contains("_Base");
#endif
        
        [SerializeField, ValueDropdown("Ids1")] private Id id;
        public string UserId { get; private set; }
        public string TeamId { get; private set; }
        public AffiliationType Affiliation { get; private set; }
        public new Transform transform { get; protected set; }
        
        [ShowIf("$HideManager")]
        [SerializeField] protected LevelsManager manager;

        public Id Id => id;

        private void Awake()
        {
            transform = GetComponent<Transform>();
        }

        public virtual void Init(string userId, string teamId)
        {
            UserId = userId;
            TeamId = teamId;
#if UNITY_EDITOR
            name = $"{name}_{GetInstanceID()}";
#endif
            
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
        }

        public virtual void OnInit(){}

        public virtual void DeInit()
        {
            Remove(transform);
        }
    }
}