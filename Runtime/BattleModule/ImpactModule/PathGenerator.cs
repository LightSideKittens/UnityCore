#if UNITY_EDITOR

using System.Collections.Generic;
using LSCore.Attributes;
using LSCore.Extensions.Unity;
using UnityEngine;
using static UnityEngine.ParticleSystem;

namespace LSCore
{
    [ExecuteAlways]
    public class PathGenerator : MonoBehaviour
    {
        [UniqueTypeFilter]
        [SerializeReference] public List<ParticleHandler> particleIniters;
        [UniqueTypeFilter(typeof(TriggerHandler))]
        [SerializeReference] public List<ParticlesHandler> particleHandlers;
        [UniqueTypeFilter]
        [SerializeReference] public List<ParticleHandler> particleDeathHandlers;
        
        private Vector4 currentId;
        private static Material lineMaterial;
        private ParticleSystem ps;
        private ImpactObject[] childPs;
        private List<Vector4> customData = new();
        private List<LineRenderer> lineRenderers = new();
        private List<List<float>> widths = new();
        private Transform targetParent;
        private float currentTime;
        private float radiusScale;
        
        private void Awake()
        {
            lineMaterial = LSDefaultAssets.LineMaterial;
            ps = GetComponent<ParticleSystem>();
            radiusScale = ps.trigger.radiusScale;
            childPs = GetComponentsInChildren<ImpactObject>();
        }

        public static void Generate(ImpactObject impactObject, float simulateTime = 0.02f)
        {
            simulateTime = Mathf.Clamp(simulateTime, 0.01f, 100);
            var targetParent = new GameObject("Preview").transform;
            targetParent.SetParent(impactObject.transform, false);
            impactObject = Instantiate(impactObject);
            var pathGenerator = impactObject.gameObject.AddComponent<PathGenerator>();
            var pathTransform = pathGenerator.transform;
            pathTransform.position = Vector3.zero;
            pathTransform.rotation = Quaternion.identity;
            pathTransform.localScale = Vector3.one;
            pathGenerator.targetParent = targetParent;
            pathGenerator.particleIniters = impactObject.particleIniters;
            pathGenerator.particleHandlers = impactObject.particleHandlers;
            pathGenerator.particleDeathHandlers = impactObject.particleDeathHandlers;
            pathGenerator.ps.Play();
            pathGenerator.SimulateParticles(simulateTime);
        }

        private void SimulateParticles(float simulateTime)
        {
            ps.Simulate(simulateTime, true, false, false);
            var particles = ps.GetParticles();
            int numParticlesAlive = particles.Length;
            
            if (numParticlesAlive == 0 && currentTime >= ps.main.duration)
            {
                ps.Stop();
                DestroyImmediate(gameObject);
                for (int i = 0; i < lineRenderers.Count; i++)
                {
                    var width = widths[i];
                    var step = 1f / widths[i].Count;
                    var line = lineRenderers[i];
                    var curve = line.widthCurve;
                    curve.ClearKeys();

                    for (int j = 0; j < width.Count; j++)
                    {
                        curve.AddKey(new Keyframe(j * step, width[j]));
                    }

                    line.widthCurve = curve;
                }
                return;
            }
            currentTime += simulateTime;

            TryInitParticles(particles);
            for (int i = 0; i < particleHandlers.Count; i++)
            {
                particleHandlers[i].Handle(particles);
            }
            ps.SetParticles(particles);
            
            for (int i = 0; i < numParticlesAlive; i++)
            {
                var lineIndex = (int)customData[i].x;
                lineIndex--;
                ref var particle = ref particles[i];
                var count = lineRenderers[lineIndex].positionCount;
                var radius = particle.GetCurrentSize(ps) * radiusScale;
                lineRenderers[lineIndex].positionCount++;
                lineRenderers[lineIndex].SetPosition(count, particle.position);
                widths[lineIndex].Add(radius);
            }
            
            SimulateParticles(simulateTime);
        }
        
        private void TryInitParticles(Particle[] particles)
        {
            ps.GetCustomParticleData(customData, ParticleSystemCustomData.Custom2);
            for (int i = 0; i < customData.Count; i++)
            {
                if (customData[i].x == 0f)
                {
                    for (int j = 0; j < particleIniters.Count; j++)
                    {
                        particleIniters[j].Handle(ref particles[i]);
                    }
                    currentId.x++;
                    customData[i] = currentId;
                    lineRenderers.Add(GetLine());
                    widths.Add(new List<float>());
                }
            }
            ps.SetCustomParticleData(customData, ParticleSystemCustomData.Custom2);
        }

        private LineRenderer GetLine()
        {
            var line = new GameObject($"Line_{(int)currentId.x}").AddComponent<LineRenderer>();
            line.transform.SetParent(targetParent, false);
            line.material = lineMaterial;
            line.useWorldSpace = false;
            line.positionCount = 0;
            line.numCapVertices = 90;
            return line;
        }
    }
}

#endif