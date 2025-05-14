﻿using System;
using System.Collections.Generic;
using UnityEngine;
using static StaticDict<UnityEngine.Transform,LSCore.BattleModule.Unit>;

namespace LSCore.BattleModule
{
    public partial class Unit : BaseUnit
    {
        public static event Action<Unit> Releasedd;
        public event Action Released;
        
        [SerializeReference] private List<BaseComp> comps = new();
        
        private readonly CompData compData = new();
        private bool isRegistered;
        
        public override void Init(string userId, string teamId)
        {
            base.Init(userId, teamId);
            
            compData.transform = transform;
            Add(transform, this);
            
            for (int i = 0; i < comps.Count; i++)
            {
                comps[i].Init(compData);
            }

            OnInit();
        }
        
#if UNITY_EDITOR
        private int lastWorldInstanceId;
#endif
        public void RegisterComps()
        {
#if UNITY_EDITOR
            var instanceId = World.InstanceId;
            if (lastWorldInstanceId != instanceId)
            {
                lastWorldInstanceId = instanceId;
                isRegistered = false;
            }
#endif
            
            if(isRegistered) return;
            
            transform = gameObject.GetComponent<Transform>();
            compData.transform = transform;
            
            for (int i = 0; i < comps.Count; i++)
            {
                comps[i].Register(compData);
            }

            isRegistered = true;
        }

        public T GetComp<T>() where T : BaseComp
        {
            return TransformDict<T>.Get(transform);
        }
        
        public override void OnInit()
        {
            compData.onInit?.Invoke();
        }
        
        private void Resett()
        {
            compData.reset?.Invoke();
        }

        private void Enable()
        {
            compData.enable?.Invoke();
        }

        private void Disable()
        {
            compData.disable?.Invoke();
        }
        
        public void Run()
        {
            compData.update?.Invoke();
        }

        public void FixedRun()
        {
            compData.fixedUpdate?.Invoke();
        }

        public void Release()
        {
            Release(this);
            Released?.Invoke();
            Releasedd?.Invoke(this);
        }
        
        public override void DeInit()
        {
            base.DeInit();
            Remove(transform);
            compData.destroy?.Invoke();
        }
    }
}