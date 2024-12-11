using UnityEngine;

namespace LSCore.AnimationsModule
{
    public class AnimationTestMode : MonoBehaviour
    {
        public static bool Is { get; private set; }

        private void Awake()
        {
            Is = true;
        }

        private void OnDestroy()
        {
            Is = false;
        }
    }
}