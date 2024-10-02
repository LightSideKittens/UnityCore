using System;
using System.Threading.Tasks;
using Cronos;
using LSCore.Extensions;
using UnityEngine;

namespace LSCore.QuestModule
{
    public partial class QuestsManager
    {
        [Serializable]
        public class CreateByCron : CreateHandler
        {
            [CronEx] public string cron;
            
            [SerializeReference] public QuestsSelector selector;
            private DateTime target;

            protected void Wait()
            {
                var dt = Config[lastCreationDt]!.ToObject<long>().ToDateTime();
                target = CronExpression.Parse(cron).GetNextOccurrence(dt) ?? DateTime.MaxValue;
                var now = DateTime.UtcNow;

                if (target > now)
                {
                    Task.Delay(target - now).GetAwaiter().OnCompleted(Create);
                }
                else
                {
                    Create();
                }
            }

            protected override void StartCreating()
            {
                Config[lastCreationDt] ??= DateTime.UtcNow.Ticks;
                Wait();
            }

            private void Create()
            {
                CreateBySelector(selector);
                Config[lastCreationDt] = target;
            }
        }
    }
}