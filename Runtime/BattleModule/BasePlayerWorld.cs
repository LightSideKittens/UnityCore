﻿using System;
using System.Collections.Generic;
using UnityEngine;
using UnitsByTransform = LSCore.BattleModule.TransformDict<LSCore.BattleModule.Unit>;

namespace LSCore.BattleModule
{
    public abstract class BasePlayerWorld<T> : SingleService<T> where T : BasePlayerWorld<T>
    {
        public static event System.Action AllUnitsDestroyed;

        public static List<Unit> activeUnits = new();
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
            Unit.DestroyAllPools();
        }

        protected OnOffPool<Unit> CreatePool(Unit prefab)
        {
            var pool = Unit.CreatePool(prefab);
            pool.Created += InitUnit;
            pool.Got += OnUnitGot;
            pool.Released += OnUnitReleased;
            pool.Destroyed += OnUnitReleased;
            return pool;
        }

        private void InitUnit(Unit unit)
        {
            unit.Init(UserId, TeamId);
            unit.OnInit();
        }

        private static void OnUnitGot(Unit unit) => activeUnits.Add(unit);
        private static void OnUnitReleased(Unit unit)
        {
            activeUnits.Remove(unit);

            if (activeUnits.Count == 0)
            {
                AllUnitsDestroyed?.Invoke();
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