using System;
using Animatable;
using DG.Tweening;
using UnityEngine;

namespace LSCore.BattleModule
{
    [Serializable]
    public class HealthComp : BaseHealthComp
    {
        [SerializeField] private Vector2 scale = new Vector2(1, 1);
        [SerializeField] private Vector2 offset;
        [SerializeField] private Transform visualRoot;
        [SerializeField] private Renderer renderer;
        private Material expose;
        private Animatable.HealthBar healthBar;
        private static readonly int exposure = Shader.PropertyToID("_Exposure");

        protected override void OnInit()
        {
            base.OnInit();
            healthBar = Animatable.HealthBar.Create(health, transform, offset, scale, affiliation == AffiliationType.Enemy);
            data.update += healthBar.Update;
            expose = renderer.material;
        }

        protected override void Reset()
        {
            base.Reset();
            healthBar.Reset();
        }

        protected override void OnDamageTaken(float damage)
        {
            visualRoot.DOShakePosition(0.15f, 0.2f, 25);
            expose.SetFloat(exposure, 1.6f);
            expose.DOFloat(1, exposure, 0.5f);
            healthBar.SetValue(health);
            AnimText.Create($"{(int)damage}", transform.position);
        }

        protected override void OnKilled()
        {
            healthBar.Disable();
        }
    }
}