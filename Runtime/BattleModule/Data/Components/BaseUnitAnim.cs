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
        [HideIf("@animation != null")]
        public AnimationWrapper wrapper;
        [HideIf("@wrapper != null")]
        public UnityEngine.Animation animation;
        
        [ValueDropdown("Clips")]
        public AnimationClip clip;

        private IEnumerable<AnimationClip> Clips => from AnimationState state in Anim select state.clip;

        public UnityEngine.Animation Anim
        {
            get
            {
                if (animation != null) return animation;
                return wrapper.Animation;
            }
        }
        
        public override Tween Animate()
        {
            var tween = Wait.Delay(clip.length).OnKill(Stop);
            Play();
            return tween;
        }

        public override void Stop()
        {
            if (animation != null)
            {
                animation.Stop(clip.name);
            }
            else
            {
                wrapper.Stop(clip.name);
            }
        }

        public override void ResolveBinds<T>(string key, T target)
        {
            
        }

        private void Play()
        {
            if (animation != null)
            {
                animation.Play(clip.name);
            }
            else
            {
                wrapper.Play(clip.name);
            }
        }
    }
}