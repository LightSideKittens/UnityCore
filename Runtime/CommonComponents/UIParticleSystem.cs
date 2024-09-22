using UnityEngine;
using UnityEngine.UI;

namespace LSCore
{
#if UNITY_5_3_OR_NEWER
    [ExecuteInEditMode]
    [RequireComponent(typeof(CanvasRenderer), typeof(ParticleSystem))]
    [AddComponentMenu("UI/Effects/Extensions/UIParticleSystem")]
    public class UIParticleSystem : MaskableGraphic
    {
        [Tooltip("Having this enabled run the system in LateUpdate rather than in Update making it faster but less precise (more clunky)")]
        public bool fixedTime = true;

        [Tooltip("Enables 3d rotation for the particles")]
        public bool use3dRotation;

        private readonly UIVertex[] quad = new UIVertex[4];
        private new Transform transform;
        private ParticleSystem ps;
        private ParticleSystem.Particle[] particles;
        private Vector4 imageUV = Vector4.zero;
        private ParticleSystem.TextureSheetAnimationModule textureSheetAnimation;
        private int textureSheetAnimationFrames;
        private Vector2 textureSheetAnimationFrameSize;
        private ParticleSystemRenderer pRenderer;
        private bool isInitialised;

        private Material currentMaterial;

        private Texture currentTexture;

        private ParticleSystem.MainModule mainModule;

        public override Texture mainTexture => currentTexture;

        private bool Initialize()
        {
            if (transform == null)
            {
                transform = base.transform;
            }
            if (ps == null)
            {
                ps = GetComponent<ParticleSystem>();

                if (ps == null)
                {
                    return false;
                }

                mainModule = ps.main;
                if (ps.main.maxParticles > 14000)
                {
                    mainModule.maxParticles = 14000;
                }

                pRenderer = ps.GetComponent<ParticleSystemRenderer>();
                if (pRenderer != null)
                {
                    pRenderer.enabled = false;
                }

                if (material == null)
                {
                    var foundShader = Shader.Find("UI Extensions/Particles/Additive");
                    if (foundShader)
                    {
                        material = new Material(foundShader);
                    }
                }

                currentMaterial = material;
                if (currentMaterial && currentMaterial.HasProperty("_MainTex"))
                {
                    currentTexture = currentMaterial.mainTexture;
                    if (currentTexture == null)
                    {
                        currentTexture = Texture2D.whiteTexture;
                    }
                }
                material = currentMaterial;
                mainModule.scalingMode = ParticleSystemScalingMode.Hierarchy;

                particles = null;
            }
            
            particles ??= new ParticleSystem.Particle[ps.main.maxParticles];

            imageUV = new Vector4(0, 0, 1, 1);

            textureSheetAnimation = ps.textureSheetAnimation;
            textureSheetAnimationFrames = 0;
            textureSheetAnimationFrameSize = Vector2.zero;
            if (textureSheetAnimation.enabled)
            {
                textureSheetAnimationFrames = textureSheetAnimation.numTilesX * textureSheetAnimation.numTilesY;
                textureSheetAnimationFrameSize = new Vector2(1f / textureSheetAnimation.numTilesX, 1f / textureSheetAnimation.numTilesY);
            }

            return true;
        }

        protected override void Awake()
        {
            base.Awake();
            if (!Initialize())
            {
                enabled = false;
            }
        }


        protected override void OnPopulateMesh(VertexHelper vh)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                if (!Initialize())
                {
                    return;
                }
            }
#endif
            vh.Clear();

            if (!gameObject.activeInHierarchy)
            {
                return;
            }

            if (!isInitialised && !ps.main.playOnAwake)
            {
                ps.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
                isInitialised = true;
            }

            var temp = Vector2.zero;
            var corner1 = Vector2.zero;
            var corner2 = Vector2.zero;
            var count = ps.GetParticles(particles);

            for (var i = 0; i < count; ++i)
            {
                var particle = particles[i];

                Vector2 position = (mainModule.simulationSpace == ParticleSystemSimulationSpace.Local ? particle.position : transform.InverseTransformPoint(particle.position));
                var rotation = -particle.rotation * Mathf.Deg2Rad;
                var rotation90 = rotation + Mathf.PI / 2;
                var color = particle.GetCurrentColor(ps);
                var size = particle.GetCurrentSize(ps) * 0.5f;
                
                if (mainModule.scalingMode == ParticleSystemScalingMode.Shape)
                {
                    position /= canvas.scaleFactor;
                }

                var particleUV = imageUV;
                if (textureSheetAnimation.enabled)
                {
                    var frameProgress = 1 - (particle.remainingLifetime / particle.startLifetime);

                    if (textureSheetAnimation.frameOverTime.curveMin != null)
                    {
                        frameProgress = textureSheetAnimation.frameOverTime.curveMin.Evaluate(1 - (particle.remainingLifetime / particle.startLifetime));
                    }
                    else if (textureSheetAnimation.frameOverTime.curve != null)
                    {
                        frameProgress = textureSheetAnimation.frameOverTime.curve.Evaluate(1 - (particle.remainingLifetime / particle.startLifetime));
                    }
                    else if (textureSheetAnimation.frameOverTime.constant > 0)
                    {
                        frameProgress = textureSheetAnimation.frameOverTime.constant - (particle.remainingLifetime / particle.startLifetime);
                    }

                    frameProgress = Mathf.Repeat(frameProgress * textureSheetAnimation.cycleCount, 1);
                    var frame = 0;

                    switch (textureSheetAnimation.animation)
                    {

                        case ParticleSystemAnimationType.WholeSheet:
                            frame = Mathf.FloorToInt(frameProgress * textureSheetAnimationFrames);
                            break;

                        case ParticleSystemAnimationType.SingleRow:
                            frame = Mathf.FloorToInt(frameProgress * textureSheetAnimation.numTilesX);

                            var row = textureSheetAnimation.rowIndex;
                            frame += row * textureSheetAnimation.numTilesX;
                            break;

                    }

                    frame %= textureSheetAnimationFrames;

                    particleUV.x = (frame % textureSheetAnimation.numTilesX) * textureSheetAnimationFrameSize.x;
                    particleUV.y = 1.0f - Mathf.FloorToInt(frame / textureSheetAnimation.numTilesX) * textureSheetAnimationFrameSize.y;
                    particleUV.z = particleUV.x + textureSheetAnimationFrameSize.x;
                    particleUV.w = particleUV.y + textureSheetAnimationFrameSize.y;
                }

                temp.x = particleUV.x;
                temp.y = particleUV.y;

                quad[0] = UIVertex.simpleVert;
                quad[0].color = color;
                quad[0].uv0 = temp;

                temp.x = particleUV.x;
                temp.y = particleUV.w;
                quad[1] = UIVertex.simpleVert;
                quad[1].color = color;
                quad[1].uv0 = temp;

                temp.x = particleUV.z;
                temp.y = particleUV.w;
                quad[2] = UIVertex.simpleVert;
                quad[2].color = color;
                quad[2].uv0 = temp;

                temp.x = particleUV.z;
                temp.y = particleUV.y;
                quad[3] = UIVertex.simpleVert;
                quad[3].color = color;
                quad[3].uv0 = temp;

                if (rotation == 0)
                {
                    corner1.x = position.x - size;
                    corner1.y = position.y - size;
                    corner2.x = position.x + size;
                    corner2.y = position.y + size;

                    temp.x = corner1.x;
                    temp.y = corner1.y;
                    quad[0].position = temp;
                    temp.x = corner1.x;
                    temp.y = corner2.y;
                    quad[1].position = temp;
                    temp.x = corner2.x;
                    temp.y = corner2.y;
                    quad[2].position = temp;
                    temp.x = corner2.x;
                    temp.y = corner1.y;
                    quad[3].position = temp;
                }
                else
                {
                    if (use3dRotation)
                    {
                        var pos3d = (mainModule.simulationSpace == ParticleSystemSimulationSpace.Local ? particle.position : transform.InverseTransformPoint(particle.position));

                        var verts = new Vector3[]
                        {
                            new(-size, -size, 0),
                            new(-size, size, 0),
                            new(size, size, 0),
                            new(size, -size, 0)
                        };

                        var particleRotation = Quaternion.Euler(particle.rotation3D);

                        quad[0].position = pos3d + particleRotation * verts[0];
                        quad[1].position = pos3d + particleRotation * verts[1];
                        quad[2].position = pos3d + particleRotation * verts[2];
                        quad[3].position = pos3d + particleRotation * verts[3];
                    }
                    else
                    {
                        var right = new Vector2(Mathf.Cos(rotation), Mathf.Sin(rotation)) * size;
                        var up = new Vector2(Mathf.Cos(rotation90), Mathf.Sin(rotation90)) * size;

                        quad[0].position = position - right - up;
                        quad[1].position = position - right + up;
                        quad[2].position = position + right + up;
                        quad[3].position = position + right - up;
                    }
                }

                vh.AddUIVertexQuad(quad);
            }
        }

        private void Update()
        {
            if (!fixedTime && Application.isPlaying)
            {
                ps.Simulate(Time.unscaledDeltaTime, false, false, true);
                SetAllDirty();

                if ((currentMaterial != null && currentTexture != currentMaterial.mainTexture) ||
                    (material != null && currentMaterial != null && material.shader != currentMaterial.shader))
                {
                    ps = null;
                    Initialize();
                }
            }
        }

        private void LateUpdate()
        {
            if (!Application.isPlaying)
            {
                SetAllDirty();
            }
            else
            {
                if (fixedTime)
                {
                    ps.Simulate(Time.unscaledDeltaTime, false, false, true);
                    SetAllDirty();
                    if ((currentMaterial != null && currentTexture != currentMaterial.mainTexture) ||
                        (material != null && currentMaterial != null && material.shader != currentMaterial.shader))
                    {
                        ps = null;
                        Initialize();
                    }
                }
            }
            if (material == currentMaterial)
            {
                return;
            }

            ps = null;
            Initialize();
        }

        protected override void OnDestroy()
        {
            currentMaterial = null;
            currentTexture = null;
        }

        public void StartParticleEmission()
        {
            ps.Play();
        }

        public void StopParticleEmission()
        {
            ps.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        public void PauseParticleEmission()
        {
            ps.Stop(false, ParticleSystemStopBehavior.StopEmitting);
        }
    }
#endif
}