using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
[RequireComponent(typeof(CanvasRenderer))]
[ExecuteAlways]
public class UIParticleRenderer : MonoBehaviour
{
    [SerializeField] private Material material;

    private ParticleSystem ps;
    private ParticleSystemRenderer psr;
    private CanvasRenderer cr;
    private Mesh mesh;

    public ParticleSystem ParticleSystem => ps;

    private void Awake()
    {
        InitComponents();
        mesh = new Mesh(){name = "UIParticleMesh"};
        mesh.MarkDynamic();
        InitMaterial();
    }

    private void InitComponents()
    {
        ps = GetComponent<ParticleSystem>();
        psr = GetComponent<ParticleSystemRenderer>();
        cr = GetComponent<CanvasRenderer>();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        InitComponents();
        InitMaterial();
    }
#endif

    private void LateUpdate()
    {
        if (!ps.IsAlive(true))
        {
            return;
        }

        psr.BakeMesh(mesh, ParticleSystemBakeMeshOptions.Default | ParticleSystemBakeMeshOptions.BakePosition);
        cr.SetMesh(mesh);
    }

    private void InitMaterial()
    {
        if (material == null && psr != null)
        {
            material = psr.sharedMaterial;
        }

        if (material == null)
        {
            return;
        }
        
        cr.materialCount = 1;
        cr.SetMaterial(material, 0);
        cr.SetTexture(material.mainTexture);
    }

    private void OnDestroy()
    {
        if (Application.isPlaying)
        {
            Destroy(mesh);
        }
        else
        {
            DestroyImmediate(mesh);
        }
    }
}