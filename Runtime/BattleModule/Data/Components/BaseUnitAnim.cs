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
    }
    
    [Serializable]
    public class AnimationUnitAnim : BaseUnitAnim
    {
        public MoveIt moveIt;
        
        [ValueDropdown("Clips")]
        public MoveItClip clip;

        private IEnumerable<MoveItClip> Clips => moveIt.Clips;
        
        public override Tween Animate()
        {
            var tween = Wait.Delay(clip.length).OnKill(Stop);
            Play();
            return tween;
        }

        public override void Stop()
        {
            moveIt.Stop(clip);
        }

        private void Play()
        {
            moveIt.Play(clip);
        }
    }
}