using System.Collections.Generic;
using UnityEngine;

namespace LSCore.BattleModule
{
    public class Buffs
    {
        public class BuffData
        {
            public float value;
            public float time;
        }
        
        public static float operator*(float value, Buffs buffs)
        {
            var factor = 1f;
            foreach (var data in buffs.ByName.Values)
            {
                factor += data.value;
            }

            return value * factor;
        }

        public Dictionary<string, BuffData> ByName { get; } = new();
        private readonly List<string> toRemove = new();

        public void Set(string name, float value, float time)
        {
            if (!ByName.TryGetValue(name, out var data))
            {
                data = new BuffData();
                ByName.Add(name, data);
            }
            
            data.value = value;
            data.time = time;
        }

        public void Reset()
        {
            ByName.Clear();
            toRemove.Clear();
        }
        
        public void Update()
        {
            if(ByName.Count == 0) return;
            
            toRemove.Clear();
            
            foreach (var (name, data) in ByName)
            {
                data.time -= Time.deltaTime;

                if (data.time <= 0)
                {
                    toRemove.Add(name);
                }
            }

            for (int i = 0; i < toRemove.Count; i++)
            {
                ByName.Remove(toRemove[i]);
            }
        }
    }
}