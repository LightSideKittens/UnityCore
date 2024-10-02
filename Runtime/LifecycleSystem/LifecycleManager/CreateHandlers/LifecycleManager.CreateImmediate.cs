using System;
using UnityEngine;

namespace LSCore.LifecycleSystem
{
    public partial class LifecycleManager
    {
        [Serializable]
        public class CreateImmediate : CreateHandler
        {
            [SerializeReference] public MultipleSelector selector;
            [Range(1, 10)] public int count = 1;

            protected override void StartCreating()
            {
                if (Config["wasCreated"] != null) return;
                for (int i = 0; i < count; i++)
                {
                    CreateBySelector(selector);
                }
                Config["wasCreated"] = true;
            }
        }
    }
}