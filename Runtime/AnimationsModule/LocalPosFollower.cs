using System;
using DG.Tweening;
using LSCore.Extensions.Unity;
using Sirenix.OdinInspector;
using UnityEngine;

namespace LSCore.AnimationsModule
{
    [Serializable]
    public class LocalPosFollower : MonoBehaviour
    {
        public float duration = 0.2f;
        public Transform target;
        public Vector3 offset;
        private Vector3 localPos;
        private Tween tween;

        private void Update()
        {
            if(target == null)return;
            var lp = target.localPosition;
            if (localPos != lp)
            {
                tween?.Kill();
                tween = transform.DOLocalMove(target.localPosition + offset, duration);
            }

            localPos = lp;
        }
    }

    [Serializable]
    public class SetTargetToFollower : LSAction
    {
        public bool useValuePath;
        [ShowIf("useValuePath")] public Transform root;
        [ShowIf("useValuePath")] public string compPath;
        [ShowIf("useValuePath")] public string targetPath;
        
        [HideIf("useValuePath")] public LocalPosFollower comp;
        [HideIf("useValuePath")] public Transform target;
        
        
        public override void Invoke()
        {
            var comp = this.comp;
            var target = this.target;
            
            if (useValuePath)
            {
                comp = root.FindComponent<LocalPosFollower>(compPath);
                target = root.FindComponent<Transform>(targetPath);
            }

            comp.target = target;
        }
    }
}