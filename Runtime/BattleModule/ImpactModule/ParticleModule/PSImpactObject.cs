﻿using System;
using System.Collections.Generic;
using Attributes;
using LSCore.Async;
using LSCore.Attributes;
using LSCore.DataStructs;
using LSCore.Extensions.Unity;
using Sirenix.OdinInspector;
using UnityEngine;
using static UnityEngine.ParticleSystem;

namespace LSCore
{
    [RequireComponent(typeof(ParticleSystem))]
    [ExecuteAlways]
    public class PSImpactObject : BaseImpactObject
    {
        private static Vector4 currentId;

        [UniqueTypeFilter]
        [SerializeReference] public List<ParticleHandler> particleIniters;
        [UniqueTypeFilter(typeof(TriggerHandler))]
        [SerializeReference] public List<ParticlesHandler> particleHandlers;
        [UniqueTypeFilter]
        [SerializeReference] public List<ParticleHandler> particleDeathHandlers;
        
        [PropertySpace(20)]
        [SerializeReference] public List<ParticleImpact> impacts;

        [ColoredField]
        public TriggerHandler triggerHandler;

        [SerializeField] private GameObject preview;
        
        [NonSerialized] public ParticleSystem ps;
        private List<Vector4> customData = new();
        private List<PSImpactObject> childImpactObjects;
        private new Transform transform;
        
        public override Func<Collider2D, bool> CanImpactChecker
        {
            set
            {
                canImpactChecker = value;
                for (int i = 0; i < childImpactObjects.Count; i++)
                {
                    childImpactObjects[i].canImpactChecker = value;
                }
            }
        }
        
        public override Collider2D IgnoredCollider
        {
            set
            {
                ignoredCollider = value;
                for (int i = 0; i < childImpactObjects.Count; i++)
                {
                    childImpactObjects[i].IgnoredCollider = value;
                }
            }
        }
        
        public override Transform Initiator
        {
            set
            {
                initiator = value;
                for (int i = 0; i < childImpactObjects.Count; i++)
                {
                    childImpactObjects[i].Initiator = value;
                }
            }
        }

        private List<PSImpactObject> GetChildren()
        {
            var components = new List<PSImpactObject>();
            foreach (Transform child in transform)
            {
                var component = child.GetComponent<PSImpactObject>();
                if (component != null)
                {
                    components.Add(component);
                }
            }

            return components;
        }

        private void Awake()
        {
            transform = base.transform;
            ps = GetComponent<ParticleSystem>();
            childImpactObjects = GetChildren();
            triggerHandler.Init(ps, customData);
            SubscribeOnTriggered();
            
            void SubscribeOnTriggered()
            {
#if UNITY_EDITOR
                if (!Application.isPlaying) return;
#endif
                triggerHandler.Triggered += OnTriggered;
            }
        }

        private void Update()
        {
            if (lockRotation)
            {
                transform.up = up;
            }
            
            if (ps.isPlaying)
            {
                HandleParticles();
            }
        }

        private void HandleParticles()
        {
            var particles = ps.GetParticles();
            if(particles.Length == 0) return;
            
            TryInitParticles(particles);
            IgnoreCollider = true;
            triggerHandler.Handle(particles);
            IgnoreCollider = false;

            for (int i = 0; i < particleHandlers.Count; i++)
            {
                particleHandlers[i].Handle(particles);
            }

            TryHandleDeath(particles);
            ps.SetParticles(particles);
        }
        
#if UNITY_EDITOR
        private HashSet<int> aliveParticlesIds = new();
#endif
        private void TryHandleDeath(ArraySpan<Particle> particles)
        {
            for (int i = 0; i < particles.Length; i++)
            {
#if UNITY_EDITOR
                var id = (int)customData[i].x;
                if(!aliveParticlesIds.Contains(id)) continue;
#endif
                ref var particle = ref particles[i];
                if (particle.remainingLifetime - Time.deltaTime <= 0)
                {
#if UNITY_EDITOR
                    aliveParticlesIds.Remove(id);
#endif     
                    for (int j = 0; j < particleDeathHandlers.Count; j++)
                    {
                        particleDeathHandlers[j].Handle(ref particles[i]);
                    }
                }
            }
        }

        private void TryInitParticles(ArraySpan<Particle> particles)
        {
            ps.GetCustomParticleData(customData, ParticleSystemCustomData.Custom2);
            for (int i = 0; i < customData.Count; i++)
            {
                if (customData[i].x == 0f)
                {
                    currentId.x++;
                    customData[i] = currentId;
#if UNITY_EDITOR
                    aliveParticlesIds.Add((int)currentId.x);
#endif
                    for (int j = 0; j < particleIniters.Count; j++)
                    {
                        particleIniters[j].Handle(ref particles[i]);
                    }
                }
            }
            ps.SetCustomParticleData(customData, ParticleSystemCustomData.Custom2);
        }

        public void LookAt(Transform target)
        {
            LookAt(target.position - transform.position);
        }
        
        public void LookAt(in Vector2 direction)
        { 
            transform.up = direction;
        }
        
        public void SetActivePreview(bool active)
        {
            if (preview != null)
            {
                preview.SetActive(active);
            }
        }

        private bool lockRotation;
        private Vector3 up;
        
        public void Emit()
        {
            lockRotation = true;
            up = transform.up;
            Wait.Frames(1, () => lockRotation = false);
            ps.Stop();
            ps.Play();
        }

        private void OnTriggered(ref Particle particle, Collider2D target)
        {
            if(!canImpactChecker(target)) return;
            
            for (int i = 0; i < impacts.Count; i++)
            {
                impacts[i].Apply(initiator, ref particle, target);
            }
        }

#if UNITY_EDITOR

        private void OnValidate()
        {
            if (ps == null)
            {
                Awake();
            }
        }

        [PropertySpace(20)]
        [OnValueChanged("OnSimulationTimeChanged")] 
        [ShowInInspector] private float simulationTime;

        private void OnSimulationTimeChanged()
        {
            ps.Simulate(simulationTime);
            HandleParticles();
            for (int i = 0; i < childImpactObjects.Count; i++)
            {
                childImpactObjects[i].HandleParticles();
            }
        }

        [Button] private void GeneratePath(float simulateTime = 0.02f) => PathGenerator.Generate(this, simulateTime);
        [Button] private void Play() => Emit();
#endif
    }
}