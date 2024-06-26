using DG.Tweening;
using UnityEngine;

namespace LSCore
{
    public class DefaultUIViewAnimationComp : MonoBehaviour
    {
        public DefaultUIViewAnimation anim;

        public Tween Show() => anim.Show;
        public Tween Hide() => anim.Hide;
    }
}