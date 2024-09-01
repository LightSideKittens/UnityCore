using System.Collections;
using LSCore.Extensions;
using UnityEngine;

namespace LSCore.AnimationsModule.Examples
{
    public class AnimationBridgeTest : MonoBehaviour
    {
        [SerializeField] private AnimationBridge animation;

        public IEnumerator Start()
        {
            while (enabled)
            {
                yield return new WaitForSeconds(Random.Range(0.1f, 0.6f));
                var state = animation.Animation.RandomElement<AnimationState>();
                animation.Play(state.name);
            }
        }
    }
}