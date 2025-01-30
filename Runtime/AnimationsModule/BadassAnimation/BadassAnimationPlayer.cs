using System;
using System.Collections.Generic;
using LSCore.Extensions.Unity;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace LSCore.AnimationsModule
{
    public class BadassAnimationPlayer : MonoBehaviour
    {
        [Serializable]
        private struct LSVector2
        {
            public float x;
            public float y;
        }
        
        [SerializeField] private Vector2 vector2;
        
        public new BadassAnimation animation;
        public Button first;
        public Button second;
        public Button nullButton;
        [ValueDropdown("Clips")] public BadassAnimationClip clip;
        [ValueDropdown("Clips")] public BadassAnimationClip clip2;
        
        private IEnumerable<BadassAnimationClip> Clips => animation.Clips;

        private void Awake()
        {
            first.AddListener(() => animation.Clip = clip);
            second.AddListener(() => animation.Clip = clip2);
            nullButton.AddListener(() => animation.Clip = null);
        }
    }
}