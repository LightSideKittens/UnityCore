using System;
using System.Collections.Generic;
using LSCore.Attributes;
using Sirenix.OdinInspector;
using UnityEngine;
using static UnityEngine.ParticleSystem;

namespace LSCore
{
    [ExecuteAlways]
    [RequireComponent(typeof(ParticleSystem))]
    public class ImpactObject : MonoBehaviour
    {
        private const int Infinity = 65535;

        [UniqueTypeFilter]
        [SerializeReference] public List<Impact> impacts;
        private readonly Collider2D[] hitColliders = new Collider2D[1];
        private readonly List<Particle> enterParticles = new();
        [SerializeField] private AnimationCurve curve;
        [SerializeField] private bool useInfinityLifetime;
        protected ParticleSystem mainPs;
        protected ParticleSystem ps;
        private static readonly Particle[] particles = new Particle[1000];
        
        private void Awake()
        {
            ps = GetComponent<ParticleSystem>();
            mainPs = ps;
            
            ParticleSystem[] parentSystem = GetComponentsInParent<ParticleSystem>(true);
            
            if (parentSystem.Length > 0)
            {
                mainPs = parentSystem[^1];
            }
        }

        [Button]
        private void PauseMain()
        {
            mainPs.Pause();
        }
        
        [Button]
        private void Pause()
        {
            ps.Pause();
        }
        
        [Button]
        private void PlayMain()
        {
            mainPs.Play();
        }
        
        [Button]
        private void Play()
        {
            ps.Play();
        }

        /*private void Update()
        {
            if (!mainPs.isPaused && !ps.isPaused)
            {
                ChangeParticlesColor();
            }
        }*/
        
        void ChangeParticlesColor()
        {
            int numParticlesAlive = ps.GetParticles(particles);

            for (int i = 0; i < numParticlesAlive; i++)
            {
                if (useInfinityLifetime && Math.Abs(particles[i].startLifetime - Infinity) > 0.0001f)
                {
                    particles[i].startLifetime = Infinity;
                    particles[i].remainingLifetime = Infinity;
                }

                var currentLifetime = particles[i].startLifetime - particles[i].remainingLifetime;
                particles[i].startColor = Color.Lerp(Color.white, Color.blue, curve.Evaluate(currentLifetime));
                particles[i].startSize *= 1 + 1.2f * Time.deltaTime;

                if (currentLifetime > 1)
                {
                    particles[i].velocity = Vector3.zero;
                }
            }

            /*if (particles.Length > 4)
            {
                particles[0].startLifetime = 0;
            }*/
            
            ps.SetParticles(particles, numParticlesAlive);
        }

        private void OnParticleCollision(GameObject other)
        {
            Debug.Log(other.name);
        }

        private void OnParticleTrigger()
        {
            int numEnter = ps.GetTriggerParticles(ParticleSystemTriggerEventType.Enter, enterParticles);
            float radiusScale = ps.trigger.radiusScale;
            
            for (int i = 0; i < numEnter; i++)
            {
                var particle = enterParticles[i];
                float radius = particle.GetCurrentSize(ps) * radiusScale / 2;
                var numColliders = Physics2D.OverlapCircleNonAlloc(particle.position, radius, hitColliders);

                for (int j = 0; j < numColliders; j++)
                {
                    particle.remainingLifetime = 0;
                    particle.startLifetime = 0;
                    enterParticles[i] = particle;
                    ps.SetTriggerParticles(ParticleSystemTriggerEventType.Enter, enterParticles);
                    OnParticleTriggerEnter(hitColliders[j]);
                }
            }
        }

        public void Emit(IEnumerable<Collider2D> colliders)
        {
            for (int i = 0; i < ps.trigger.colliderCount; i++)
            {
                ps.trigger.RemoveCollider(0);
            }
            
            foreach (var target in colliders)
            {
                ps.trigger.AddCollider(target);
            }
            
            ps.Play();
        }

        protected virtual void OnParticleTriggerEnter(Collider2D target)
        {
            for (int i = 0; i < impacts.Count; i++)
            {
                impacts[i].Apply(target.transform);
            }
        }
    }
}