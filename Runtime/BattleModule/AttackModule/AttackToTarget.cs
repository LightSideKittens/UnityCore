using System;
using DG.Tweening;
using LSCore.BattleModule;
using UnityEngine;

namespace LSCore
{
    public abstract class BaseJoystickGetter
    {
        public abstract IJoystick Get();
    }
    
    [Serializable]
    public class JoystickAttack : BaseAttack
    {
        [SerializeReference] public BaseUnitAnim anim;
        [SerializeReference] public BaseJoystickGetter joystickGetter;
        
        private IJoystick joystick;
        
        protected override void OnInit()
        {
            base.OnInit();
            joystick = joystickGetter.Get();
        }

        public override bool AttackCondition { get; }

        protected override Tween Attack() => anim.Animate();
        public override void Stop() => anim.Stop();
    }

    [Serializable]
    public class AttackToTarget : BaseAttack
    {
        [SerializeReference] public BaseUnitAnim anim;
        public float radius;
        [NonSerialized] public bool inRadius;
        [NonSerialized] public Transform target;
        
        public override bool AttackCondition
        {
            get
            {
                inRadius = findTargetComp.Find(radius, out target);
                return inRadius;
            }
        }

        protected override Tween Attack()
        {
            anim.ResolveBinds("target", target);
            return anim.Animate();
        }

        public override void Stop() => anim.Stop();
    }
}