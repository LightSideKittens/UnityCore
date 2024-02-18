using System;
using System.Collections.Generic;
using LSCore.Attributes;
using LSCore.Extensions.Unity;
using Sirenix.OdinInspector;
using UnityEngine;
using static UnityEngine.ParticleSystem;

namespace LSCore
{
    [RequireComponent(typeof(ParticleSystem))]
    public class ImpactObject : MonoBehaviour
    {
        private static Vector4 currentId;
        
        [SerializeReference] public List<ParticleImpact> impacts;
        
        [UniqueTypeFilter]
        [SerializeReference] public List<ParticlesIniter> paticleIniters;
        
        [UniqueTypeFilter(typeof(TriggerHandler))]
        [SerializeReference] public List<ParticlesHandler> particleHandlers;
        
        public TriggerHandler triggerHandler;
        public Func<Collider2D, bool> canImpactChecker;
        protected ParticleSystem mainPs;
        protected ParticleSystem ps;
        private List<Vector4> customData = new();
        
        private void Awake()
        {
            ps = GetComponent<ParticleSystem>();
            triggerHandler.Init(ps, customData);
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

            TryInitParticles(particles);
            triggerHandler.Handle(particles);
            for (int i = 0; i < particleHandlers.Count; i++)
            {
                particleHandlers[i].Handle(particles);
            }
            
            ps.SetParticles(particles);
        }

        private void TryInitParticles(Particle[] particles)
        {
            ps.GetCustomParticleData(customData, ParticleSystemCustomData.Custom2);
            for (int i = 0; i < customData.Count; i++)
            {
                if (customData[i].x == 0f)
                {
                    currentId.x++;
                    customData[i] = currentId;
                    for (int j = 0; j < paticleIniters.Count; j++)
                    {
                        paticleIniters[j].Init(ref particles[i]);
                    }
                }
            }
            ps.SetCustomParticleData(customData, ParticleSystemCustomData.Custom2);
        }

        public void Emit() => ps.Play();

        private void OnTriggered(ref Particle particle, Collider2D target)
        {
            if(!canImpactChecker(target)) return;
            
            for (int i = 0; i < impacts.Count; i++)
            {
                impacts[i].Apply(ref particle, target);
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