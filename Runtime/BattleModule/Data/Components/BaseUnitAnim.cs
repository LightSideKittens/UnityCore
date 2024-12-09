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
    }

    [Serializable]
    public class TweenUnitAnim : BaseUnitAnim
    {
        [SerializeField] private AnimSequencer anim;
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
    }
    
    [Serializable]
    public class AnimationUnitAnim : BaseUnitAnim
    {
        [HideIf("@animation != null")]
        [SerializeField] private AnimationWrapper wrapper;
        [HideIf("@wrapper != null")]
        [SerializeField] private UnityEngine.Animation animation;
        
        [ValueDropdown("Clips")]
        [SerializeField] private AnimationClip clip;

        private IEnumerable<AnimationClip> Clips => from AnimationState state in Anim select state.clip;

        private UnityEngine.Animation Anim
        {
            get
            {
                if (animation != null) return animation;
                return wrapper.Animation;
            }
        }
        
        public override Tween Animate()
        {
            var tween = Wait.Delay(clip.length);
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