using DG.Tweening;
using Sirenix.OdinInspector;

namespace LSCore.AnimationsModule.Animations
{
    public class TweenGetterAnim : BaseAnim
    {
        public bool useMultiple;

        public TweenGetter tweenGetter;
        [ShowIf("useMultiple")]
        public TweenGetter[] tweenGetters;
        [ShowIf("useMultiple")]
        public float timeOffsetPerTarget = 0.1f;

        public override bool NeedInit
        {
            get => true;
            set {} 
        }

        public override float Duration
        {
            get => float.MaxValue;
            set {}
        }

        protected override void Internal_Init()
        {
            tweenGetter.Init();
            for (int i = 0; i < tweenGetters.Length; i++)
            {
                tweenGetters[i].Init();
            }
        }

        protected override Tween Internal_Animate()
        {
            if (useMultiple)
            {
                var sequence = DOTween.Sequence();
                var pos = 0f;
                sequence.Insert(pos, tweenGetter.Invoke());
                for (int i = 0; i < tweenGetters.Length; i++)
                {
                    pos += timeOffsetPerTarget;
                    sequence.Insert(pos, tweenGetters[i].Invoke());
                }

                return sequence;
            }
            
            return tweenGetter.Invoke();
        }
    }
}