using DG.Tweening;
using UnityEngine;

namespace LSCore.AnimationsModule
{
    public class AnimComp : MonoBehaviour
    {
        public AnimSequencer anim;

        public Tween Animate() => anim.Animate();
    }
}