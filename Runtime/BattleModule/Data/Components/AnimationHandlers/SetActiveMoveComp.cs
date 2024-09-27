﻿using System;
using LSCore.AnimationsModule;
using UnityEngine;

namespace LSCore.BattleModule.Animation
{
    [Serializable]
    public class SetActiveMoveComp : AnimationWrapper.Handler<bool>
    {
        public Transform transform;
        public BaseMoveComp moveComp;
        
        protected override string Label => "Active";
        protected override bool IsRuntimeOnly => true;
        private bool startValue;

        protected override void OnStart()
        {
            base.OnStart();
            moveComp = transform.Get<BaseMoveComp>();
            startValue = moveComp.IsRunning;
        }

        protected override void OnHandle()
        {
            moveComp.IsRunning = value;
        }

        protected override void OnStop()
        {
            base.OnStop();
            moveComp.IsRunning = startValue;
        }
    }
}