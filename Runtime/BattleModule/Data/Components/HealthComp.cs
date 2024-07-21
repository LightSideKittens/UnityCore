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
        private Renderer[] renderers;
        private MaterialPropertyBlock block;
        private Animatable.HealthBar healthBar;
        private static readonly int exposure = Shader.PropertyToID("_Exposure");

        protected override void OnInit()
        {
            base.OnInit();
            renderers = visualRoot.GetComponentsInChildren<Renderer>();
            block = new();
            healthBar = Animatable.HealthBar.Create(health, transform, offset, scale, affiliation == AffiliationType.Enemy);
            data.update += healthBar.Update; 
            GetPropBlock();
        }

        protected override void Reset()
        {
            base.Reset();
            healthBar.Reset();
            healthBar.Active = false;
        }

        protected override void OnDamageTaken(float damage)
        {
            healthBar.Active = true;
            visualRoot.localPosition = Vector3.zero;
            visualRoot.DOShakePosition(0.15f, 0.2f, 25);
            block.SetFloat(exposure, 3f);
            block.DOFloat(0.3f, exposure, 1f).OnUpdate(SetPropBlock);
            healthBar.SetValue(realHealth);
            AnimText.Create($"{(int)damage}", transform.position);
        }

        private void GetPropBlock()
        {
            for (int i = 0; i < renderers.Length; i++)
            {
                renderers[i].GetPropertyBlock(block);
            }
        }

        private void SetPropBlock()
        {
            for (int i = 0; i < renderers.Length; i++)
            {
                renderers[i].SetPropertyBlock(block);
            }
        }

        protected override void OnKilled()
        {
            healthBar.Active = false;
        }
    }
}