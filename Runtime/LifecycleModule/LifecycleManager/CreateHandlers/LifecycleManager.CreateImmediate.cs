using System;
using UnityEngine;

namespace LSCore.ObjectModule
{
    public partial class LifecycleManager<T>
    {
        [Serializable]
        public class CreateImmediate : CreateHandler
        {
            [SerializeReference] public ObjectsSelector selector;
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