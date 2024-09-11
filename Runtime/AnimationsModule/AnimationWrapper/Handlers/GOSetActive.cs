using UnityEngine;

namespace LSCore.AnimationsModule
{
    public class GOSetActive : AnimationWrapper.Handler<bool>
    {
        [SerializeField] private GameObject gameObject;
        protected override string Label => "Active";
        
        protected override void OnHandle()
        {
            gameObject.SetActive(value);
        }
    }
}