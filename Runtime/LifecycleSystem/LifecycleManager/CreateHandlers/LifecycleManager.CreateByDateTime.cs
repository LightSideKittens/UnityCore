using System;
using System.Threading.Tasks;
using LSCore.Attributes;
using LSCore.Extensions.Time;
using UnityEngine;

namespace LSCore.LifecycleSystem
{
    public partial class LifecycleManager
    {
        [Serializable]
        public class CreateByDateTime : CreateHandler
        {
            [DateTime] public long time;
            [SerializeReference] public MultipleSelector selector;

            protected void Wait(Action onComplete)
            {
                var targetDt = time.ToDateTime();
                var now = DateTime.UtcNow;

                if (targetDt > now)
                {
                    Task.Delay(targetDt - now).GetAwaiter().OnCompleted(onComplete);
                }
                else
                {
                    onComplete();
                }
            }

            protected override void StartCreating()
            {
                Wait(Create);
            }

            private void Create()
            {
                CreateBySelector(selector);
                Config[lastCreationDt] = time;
            }
        }
    }
}