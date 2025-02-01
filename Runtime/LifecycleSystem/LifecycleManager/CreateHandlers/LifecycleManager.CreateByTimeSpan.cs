using System;
using System.Threading.Tasks;
using LSCore.Attributes;
using LSCore.Extensions;
using LSCore.Extensions.Time;
using UnityEngine;

namespace LSCore.LifecycleSystem
{
    public partial class LifecycleManager
    {
        [Serializable]
        public class CreateByTimeSpan : CreateHandler
        {
            [TimeSpan(0, 5, 0)] public long time;

            [Min(1)] public int limit = 1;
            public bool createImmediately;

            [SerializeReference] public MultipleSelector selector;
            private DateTime target;
            
            protected void Wait(Action onComplete)
            {
                target = Config[lastCreationDt]!.ToObject<long>().ToDateTime() + time.ToTimeSpan();
                var now = DateTime.UtcNow;

                if (target > now)
                {
                    Task.Delay(target - now).GetAwaiter().OnCompleted(onComplete);
                }
                else
                {
                    onComplete();
                }
            }

            protected override void StartCreating()
            {
                if (Config[lastCreationDt] == null)
                {
                    Config[lastCreationDt] = DateTime.UtcNow.Ticks;
                    if (createImmediately)
                    {
                        Create();
                        return;
                    }
                }

                Wait(Create);
            }

            private void Create()
            {
                var count = Config.Increase("createdCount", 1);
                CreateBySelector(selector);
                Config[lastCreationDt] = target.Ticks;

                if (limit > count)
                {
                    StartCreating();
                }
            }
        }
    }
}