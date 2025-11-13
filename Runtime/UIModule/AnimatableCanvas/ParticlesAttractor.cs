using System;
using System.Collections.Generic;
using Animatable;
using LSCore;
using LSCore.Async;
using UnityEngine;

[Serializable]
public class ParticlesAttractor
{
    [SerializeReference] public Get<IEnumerable<KeyValuePair<string, UIParticleRenderer>>> particles;
    public UIParticleAttractor attractor;
    private Dictionary<string, OnOffPool<UIParticleRenderer>> particlesPools = new();
    private OnOffPool<UIParticleAttractor> attractorPool;
    
    internal void Init()
    {
        attractorPool = new OnOffPool<UIParticleAttractor>(attractor, parent: AnimatableCanvas.SpawnPoint, shouldStoreActive: true);
        foreach (var (key, value) in particles.Data)
        { 
            var pool = new OnOffPool<UIParticleRenderer>(value, shouldStoreActive: true);
            particlesPools[key] = pool;
        }
    }

    private (UIParticleAttractor attractor, UIParticleRenderer particles) Get(string id)
    {
        return (attractorPool.Get(), particlesPools[id].Get());
    }
    
    private void Release(string id, (UIParticleAttractor attractor, UIParticleRenderer particles) data)
    {
        attractorPool.Release(data.attractor);
        particlesPools[id].Release(data.particles);
    }
    
    internal void ReleaseAll()
    {
        attractorPool.ReleaseAll();
        foreach (var (_, pool) in particlesPools)
        {
            pool.ReleaseAll();
        }
    }
    
    public static ParticlesAttractor Create(string id, Transform from, Transform to, int count, Action onAttracted, Action onComplete)
    {
        var template = AnimatableCanvas.ParticlesAttractor;
        
        var (attractor, particles) = template.Get(id);
        var particlesTransform = particles.transform;
        particlesTransform.SetParent(AnimatableCanvas.SpawnPoint, false);
        particlesTransform.position = from.position;

        var ps = particles.ParticleSystem;
        var module = ps.emission;
        var burst = module.GetBurst(0);
        burst.count = count;
        module.SetBurst(0, burst);
        
        ps.Play();
        Wait.Delay(attractor.delayBeforeAttract, () =>
        {
            attractor.Attract(ps, to, onAttracted, OnComplete);
        }).KillOnDestroy(attractor);
        
        var result = new ParticlesAttractor()
        {
            attractor = attractor,
        };
        
        return result;

        void OnComplete()
        {
            onComplete?.Invoke();
            template.Release(id, (attractor, particles));
        }
    }
}