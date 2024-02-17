using System;
using System.Collections.Generic;
using LSCore.Attributes;
using LSCore.Extensions.Unity;
using Sirenix.OdinInspector;
using UnityEngine;

namespace LSCore
{
    [RequireComponent(typeof(ParticleSystem))]
    public class ImpactObject : MonoBehaviour
    {
        [UniqueTypeFilter]
        [SerializeReference] public List<Impact> impacts;
        [UniqueTypeFilter(typeof(TriggerHandler))]
        [SerializeReference] public List<ParticlesHandler> handlers;
        
        public TriggerHandler triggerHandler;
        public Func<Collider2D, bool> canImpactChecker;
        protected ParticleSystem mainPs;
        protected ParticleSystem ps;
        private Kill kill;
        
        private void Awake()
        {
            ps = GetComponent<ParticleSystem>();
            triggerHandler.Init(ps);
            triggerHandler.Triggered += OnTriggered;
            
            mainPs = ps;
            
            ParticleSystem[] parentSystem = GetComponentsInParent<ParticleSystem>(true);
            
            if (parentSystem.Length > 0)
            {
                mainPs = parentSystem[^1];
            }
        }

        private void Update()
        {
            if (!mainPs.isPaused && !ps.isPaused)
            {
                HandleParticles();
            }
        }
        
        private void HandleParticles()
        {
            var particles = ps.GetParticles();
            
            triggerHandler.Handle(particles);
            for (int i = 0; i < handlers.Count; i++)
            {
                handlers[i].Handle(particles);
            }
            
            ps.SetParticles(particles);
        }

        public void Emit() => ps.Play();

        private void OnTriggered(ref ParticleSystem.Particle particle, Collider2D[] colliders)
        {
            for (int i = 0; i < colliders.Length; i++)
            {
                OnTriggered(colliders[i]);
            }
        }
        
        private void OnTriggered(Collider2D target)
        {
            if(!canImpactChecker(target)) return;
            
            for (int i = 0; i < impacts.Count; i++)
            {
                impacts[i].Apply(target.transform);
            }
        }

#if UNITY_EDITOR
        [Button] private void PauseMain() => mainPs.Pause();
        [Button] private void Pause() => ps.Pause();
        [Button] private void PlayMain() => mainPs.Play();
        [Button] private void Play() => ps.Play();
#endif
    }
}