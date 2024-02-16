using System.Collections.Generic;
namespace LSCore.BattleModule
{
    public partial class Unit
    {
        private static readonly Dictionary<Id, OnOffPool<Unit>> pools = new();
        
        static Unit() => World.Destroyed += pools.Clear;

        public static Unit Create(Unit prefab)
        {
            if (pools.TryGetValue(prefab.Id, out var pool)) return pool.Get();
            
            pool = CreatePool(prefab);
            pools.Add(prefab.Id, pool);

            return pool.Get();
        }

        public static void Release(Unit unit) => pools[unit.Id].Release(unit);
        
        public static OnOffPool<Unit> CreatePool(Unit prefab, int capacity = 10)
        {
            if (pools.TryGetValue(prefab.Id, out var pool)) return pool;
            
            pool = new OnOffPool<Unit>(prefab, capacity);
            pool.Got += OnGot;
            pool.Released += OnReleased;
            pool.Destroyed += OnDestroyed;
            pools.Add(prefab.Id, pool);

            return pool;
        }

        public static void DestroyPool(Id id)
        {
            pools[id].Destroy();
            pools.Remove(id);
        }
        
        public static void DestroyAllPools()
        {
            foreach (var pool in pools.Values)
            {
                pool.Destroy();
            }
            
            pools.Clear();
        }

        private static void OnGot(Unit unit)
        {
            unit.Resett();
            unit.Enable();
        }
        
        private static void OnReleased(Unit unit) => unit.Disable();
        private static void OnDestroyed(Unit unit) => unit.DeInit();
    }
}