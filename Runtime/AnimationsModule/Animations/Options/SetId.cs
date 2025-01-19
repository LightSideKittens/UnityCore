using System;
using DG.Tweening;
using Sirenix.OdinInspector;
using Object = UnityEngine.Object;

namespace LSCore.AnimationsModule.Animations.Options
{
    [Serializable]
    public struct SetId : IOption
    {
        [HideIf("HideKey")] public string key;
        [HideIf("HideObj")] public Object obj;
        
        private bool HideObj => !string.IsNullOrEmpty(key);
        private bool HideKey => obj != null;
        
        public void ApplyTo(Tween tween)
        {
            if (HideObj)
            {
                tween.SetId(key);
            }
            else if(HideKey)
            {
                tween.SetId(obj);
            }
        }
    }
}