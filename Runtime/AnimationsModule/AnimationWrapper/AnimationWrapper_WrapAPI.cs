using UnityEngine;

namespace LSCore.AnimationsModule
{
    public partial class AnimationWrapper
    {
        private bool isPlayCalled;
        
        public void Play(string clipName = null, PlayMode playMode = PlayMode.StopSameLayer)
        {
            bool needSet = !animation.isPlaying;
            
            if (clipName == null)
            {
                animation.Play(playMode);
            }
            else
            {
                animation.Play(clipName, playMode);
            }

            if (animation.isPlaying && needSet)
            {
                isPlayCalled = true;
            }
        }
    }
}