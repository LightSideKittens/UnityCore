using System;
using System.Collections.Generic;
using DG.Tweening;
using LSCore.Async;
using LSCore.DataStructs;
using LSCore.Extensions.Unity;
using UnityEngine;
using static UnityEngine.ParticleSystem;

[Serializable]
public class PlayUIParticleAttractor : DoIt
{
    [SerializeReference] public Get<UIParticleAttractor> attractor;
    public UIParticleAttractor.Data data;
    
    public override void Do()
    {
        ((UIParticleAttractor)attractor).Play(data);
    }
}

public class UIParticleAttractor : MonoBehaviour
{
    [Serializable]
    public struct Data
    {
        [SerializeReference] public Get<Transform> target;
        [SerializeReference] public Get<ParticleSystem> ps;
        [SerializeReference] public DoIt onAttracted;
        [SerializeReference] public DoIt onComplete;
    }
    
    public float delayBeforeAttract = 0.5f;
    public float duration = 0.3f;
    public float delayPerParticle = 0.05f;
    public Ease ease = Ease.InOutCubic;
    [SerializeReference] public DoIt[] onParticleAttracted;
    [SerializeReference] public DoIt[] onComplete;
    
    public void Play(Data data)
    {
        ParticleSystem ps = data.ps;
        ps.Play();
        Wait.Delay(delayBeforeAttract, () =>
        {
            Attract(data.ps, data.target, data.onAttracted, data.onComplete);
        }).KillOnDestroy(this);
    }
    
    public void Attract(ParticleSystem ps, Transform target, Action onAttracted, Action onComplete)
    {
        currentId = Vector4.zero;
        
        int count = ps.particleCount;
        ArraySlice<Particle> arr = ps.GetParticles();
        TryInitParticles(ps);
        var tweens = new Tween[count];
            
        int i = 0;
        for (; i < count; i++)
        {
            int idx = i;
            tweens[i] = DOTween.To(
                    () => arr[idx].position,
                    x =>
                    {
                        arr[idx].position = x;
                    },
                    ps.transform.InverseTransformPoint(target.position),
                    duration)
                .SetEase(ease)
                .KillOnDestroy(ps)
                .SetDelay(i * delayPerParticle)
                .OnComplete(() => Kill(ref arr[idx], onAttracted))
                .SetUpdate(UpdateType.Manual);
        }

        Wait.Run(delayPerParticle * count + duration, () =>
        {
            var a = ps.GetParticles();
            ps.GetCustomParticleData(customData, ParticleSystemCustomData.Custom2);
            for (var i1 = 0; i1 < customData.Count; i1++)
            {
                var id = (int)customData[i1].x - 1;
                arr[id] = a[i1];
                tweens[id].ManualUpdate(Time.deltaTime, Time.unscaledDeltaTime);
                a[i1] = arr[id];
            }
                
            ps.SetParticles(a);
        }).KillOnDestroy(ps).OnComplete(() => onComplete?.Invoke());
    }
    
    private void Kill(ref Particle particle, Action onAttracted)
    {                     
        onAttracted?.Invoke();
        particle.startLifetime = 0;
        particle.remainingLifetime = 0;
    }
    
    private void TryInitParticles(ParticleSystem ps)
    {
        ps.GetCustomParticleData(customData, ParticleSystemCustomData.Custom2);
        for (int i = 0; i < customData.Count; i++)
        {
            if (customData[i].x == 0f)
            {
                currentId.x++;
                customData[i] = currentId;
            }
        }
        ps.SetCustomParticleData(customData, ParticleSystemCustomData.Custom2);
    }
    
    private List<Vector4> customData = new();
    private static Vector4 currentId;
}