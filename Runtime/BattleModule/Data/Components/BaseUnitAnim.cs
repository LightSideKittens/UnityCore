using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using LSCore.AnimationsModule;
using LSCore.Async;
using LSCore.Attributes;
using Sirenix.OdinInspector;
using UnityEngine;

namespace LSCore.BattleModule
{
    [Serializable]
    [HideReferenceObjectPicker]
    [TypeFrom]
    public abstract class BaseUnitAnim
    {
        public abstract Tween Animate();
        public abstract void Stop();
        public abstract void ResolveBinds<T>(string key, T target);
    }

    [Serializable]
    public class TweenUnitAnim : BaseUnitAnim
    {
        public AnimSequencer anim;
        private Tween tween;
        
        public override Tween Animate()
        {
            tween = anim.Animate();
            return tween;
        }

        public override void Stop()
        {
            tween?.Kill();
        }

        public override void ResolveBinds<T>(string key, T target) => anim.ResolveBinds(key, target);
    }
    
    [Serializable]
    public class AnimationUnitAnim : BaseUnitAnim
    {
        public BadassAnimation badassAnimation;
        
        [ValueDropdown("Clips")]
        public BadassAnimationClip clip;

        private IEnumerable<BadassAnimationClip> Clips => badassAnimation.Clips;
        
        public override Tween Animate()
        {
            var tween = Wait.Delay(clip.length).OnKill(Stop);
            Play();
            return tween;
        }

        public override void Stop()
        {
            badassAnimation.Stop(clip);
        }

        public override void ResolveBinds<T>(string key, T target)
        {
            
        }

        private void Play()
        {
            badassAnimation.Play(clip);
        }
    }
}