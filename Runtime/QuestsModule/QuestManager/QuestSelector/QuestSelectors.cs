using System;
using System.Collections.Generic;
using LSCore.Extensions;
using UnityEngine;

namespace LSCore.QuestModule
{
    [Serializable]
    public abstract class QuestSelector
    {
        public abstract Quest Select(List<Quest> quests);
    }

    [Serializable]
    public class RandomQuestSelector : QuestSelector
    {
        public override Quest Select(List<Quest> quests)
        {
            return quests.Random();
        }
    }
    

    [Serializable]
    public abstract class QuestsSelector
    {
        public abstract IEnumerable<Quest> Select(List<Quest> quests);
    }

    [Serializable]
    public class All : QuestsSelector
    {
        public override IEnumerable<Quest> Select(List<Quest> quests) => quests;
    }

    [Serializable]
    public class Several : QuestsSelector
    {
        public int count;
        [SerializeReference] public QuestSelector selector;

        public override IEnumerable<Quest> Select(List<Quest> quests)
        {
            for (int i = 0; i < count; i++)
            {
                yield return selector.Select(quests);
            }
        }
    }
    
    [Serializable]
    public class BySelectEx : QuestsSelector
    {
        [SelectEx] public string expression;

        public override IEnumerable<Quest> Select(List<Quest> quests)
        {
            return quests.BySelectEx(expression);
        }
    }
}