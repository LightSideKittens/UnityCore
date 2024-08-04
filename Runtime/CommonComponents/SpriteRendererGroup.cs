using System;
using LSCore.Extensions.Unity;  
using UnityEngine;

namespace LSCore
{
    [ExecuteAlways]
    public class SpriteRendererGroup : MonoBehaviour
    {
        [SerializeField] [Range(0, 1)] private float alpha;
        private SpriteRenderer[] renderers;
        
        public float Alpha
        {
            get => alpha;
            set
            {
                if (Math.Abs(alpha - value) < 0.0001f) return;
                UpdateAlpha();
            }
        }

        private void Awake()
        {
            renderers = GetComponentsInChildren<SpriteRenderer>();
        }

        private void UpdateAlpha()
        {
            for (int i = 0; i < renderers.Length; i++)
            {
                renderers[i].Alpha(alpha);
            }
        }

        private void OnDidApplyAnimationProperties()
        {
            UpdateAlpha();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            renderers ??= GetComponentsInChildren<SpriteRenderer>();
            UpdateAlpha();
        }
#endif
    }
}