﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace LSCore.BattleModule
{
    public abstract class BaseTeamWorld<T> : SingleService<T> where T : BaseTeamWorld<T>
    {
        public static event Action AllUnitsReleased;

        public static HashSet<Unit> activeUnits = new();
        public static int UnitCount => activeUnits.Count;
        public abstract string UserId { get; }
        public abstract string TeamId { get; }

        protected override void Init()
        {
            enabled = false;
        }

        public static void Begin()
        {
#if UNITY_EDITOR
            if(!Instance.gameObject.activeSelf) return;
#endif
            Instance.enabled = true;
            Instance.OnBegin();
        }
        
        public static void Stop()
        {
            Debug.Log($"Stopped {typeof(T)}");
            Instance.enabled = false;
            Instance.OnStop();
        }
        
        protected virtual void OnBegin(){}
        protected virtual void OnStop(){}

        protected override void DeInit()
        {
            foreach (var unit in activeUnits)
            {
                unit.DeInit();
            }
            
            activeUnits.Clear();
            Unit.ClearAllPools();
        }

        protected OnOffPool<Unit> CreatePool(Unit prefab)
        {
            var pool = Unit.CreatePool(prefab);
            pool.Created += InitUnit;
            pool.CreatedOrGot += OnUnitGot;
            pool.Released += OnUnitReleased;
            pool.Removed += OnUnitReleased;
            return pool;
        }

        protected virtual void InitUnit(Unit unit)
        {
            unit.Init(UserId, TeamId);
        }

        private static void OnUnitGot(Unit unit)
        {
            activeUnits.Add(unit);
        }

        private static void OnUnitReleased(Unit unit)
        {
            activeUnits.Remove(unit);

            if (activeUnits.Count == 0)
            {
                AllUnitsReleased?.Invoke();
            }
        }

        private void Update()
        {
            foreach (var unit in activeUnits)
            {
                unit.Run();
            }
        }
        
        private void FixedUpdate()
        {
            foreach (var unit in activeUnits)
            {
                unit.FixedRun();
            }
        }
    }
}