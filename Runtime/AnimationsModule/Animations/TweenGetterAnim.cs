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
                tweenGetter.Invoke();
                sequence.Insert(pos, tweenGetter.value);
                for (int i = 0; i < tweenGetters.Length; i++)
                {
                    pos += timeOffsetPerTarget;
                    tweenGetters[i].Invoke();
                    sequence.Insert(pos, tweenGetters[i].value);
                }

                return sequence;
            }
            
            tweenGetter.Invoke();
            return tweenGetter.value;
        }
    }
}