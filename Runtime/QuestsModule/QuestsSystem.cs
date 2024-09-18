using System;
using System.Collections.Generic;
using UnityEngine;

namespace LSCore.QuestModule
{
    public class QuestsSystem : SingleService<QuestsSystem>
    {
        [Serializable]
        public class CreateQuests : LSAction
        {
            public string placementId;
            [SerializeReference] public List<TransformAction> transformActions;
            private bool isCreated;

            public override void Invoke()
            {
                if(isCreated) return;
                isCreated = true;
                
                foreach (var quest in Create(placementId))
                {
                    transformActions.Invoke(quest.transform);
                }
            }
        }
        
        [SerializeField] private QuestsManager[] managers;
        
        protected override void Init()
        {
            base.Init();
            
            for (int i = 0; i < managers.Length; i++)
            {
                managers[i].Init();
            }
        }

        public static IEnumerable<Quest> Create(string placementId)
        {
            foreach (var manager in Instance.managers)
            {
                foreach (var quest in manager.Create(placementId))
                {
                    yield return quest;
                }
            }
        }
    }
}