using System;
using Animatable;
using DG.Tweening;
using LSCore.Extensions;
using UnityEngine;

namespace LSCore.BattleModule
{
    [Serializable]
    public class HealthComp : BaseHealthComp
    {
        [SerializeField] private Vector2 scale = new Vector2(1, 1);
        [SerializeField] private Vector2 offset;
        [SerializeField] private Transform visualRoot;
        [SerializeField] private Renderer[] renderers;
        private MaterialPropertyBlock block;
        private Animatable.HealthBar healthBar;
        private static readonly int exposure = Shader.PropertyToID("_Exposure");

        protected override void OnInit()
        {
            base.OnInit();
            block = new();
            healthBar = Animatable.HealthBar.Create(health, transform, offset, scale, affiliation == AffiliationType.Enemy);
            data.update += healthBar.Update; 
        }

        protected override void Reset()
        {
            base.Reset();
            healthBar.Reset();
        }

        protected override void OnDamageTaken(float damage)
        {
            visualRoot.DOShakePosition(0.15f, 0.2f, 25);
            block.SetFloat(exposure, 1.6f);
            block.DOFloat(1, exposure, 0.5f).OnUpdate(() =>
            {
                for (int i = 0; i < renderers.Length; i++)
                {
                    renderers[i].SetPropertyBlock(block);
                }
            });
            
            healthBar.SetValue(health);
            AnimText.Create($"{(int)damage}", transform.position);
        }

        protected override void OnKilled()
        {
            healthBar.Disable();
        }
    }
}