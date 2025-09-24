using LSCore;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

[ExecuteAlways]
[DisallowMultipleComponent]
public sealed partial class LottieRenderer : MonoBehaviour
{
    private const int MinSize = 64;
    private const int MaxSize = 2048;

    public LottieRendererManager manager;
    
    private bool isVisible;

    public bool IsVisible
    {
        get => enabled && gameObject.activeInHierarchy && isVisible;
        private set
        {
            if(isVisible == value) return;
            isVisible = value;
            manager.UpdatePlayState();
        }
    }

    internal uint Size
    {
        get
        {
            var s = transform.localScale;
            var maxAxis = Mathf.Max(Mathf.Abs(s.x), Mathf.Abs(s.y));
            var px = Mathf.RoundToInt(maxAxis * pixelsPerUnit);
            var clamped = Mathf.Clamp(px, MinSize, MaxSize);
            var newSize = (uint)Mathf.ClosestPowerOfTwo(clamped);
            return newSize;
        }
    }

    private void Awake()
    {
        Init();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if(World.IsBuilding) return;
        EditorApplication.update += Update;

        void Update()
        {
            EditorApplication.update -= Update;
            Init();
        }
    }
#endif

    private void Init()
    {
        manager.renderer = this;
        mr ??= GetComp<MeshRenderer>();
        mr.allowOcclusionWhenDynamic = false;
        mr.lightProbeUsage = LightProbeUsage.Off;
        mr.reflectionProbeUsage = ReflectionProbeUsage.Off;
        mr.shadowCastingMode = ShadowCastingMode.Off;
        mr.receiveShadows = false;
        mf ??= GetComp<MeshFilter>();
        mpb ??= new MaterialPropertyBlock();

        if (quad == null) BuildUnitQuad();
        if (unlitMat == null) unlitMat = new Material(Shader.Find("Sprites/Default"));
        if (!Material) Material = unlitMat;
        if(mr.sharedMaterial == null) mr.sharedMaterial = Material;

        T GetComp<T>() where T : Component
        {
            T ret;
#if UNITY_EDITOR
            ret = GetComponent<T>();
            if (ret == null)
            {
                ret = gameObject.AddComponent<T>();
            }
            ret.hideFlags = HideFlags.HideAndDontSave | HideFlags.HideInInspector;
#else
            ret = gameObject.AddComponent<T>();
#endif
            return ret;
        }
    }

    private void OnEnable()
    {
#if UNITY_EDITOR
        if (mpb == null) Init();
#endif
        manager.UpdatePlayState();
    }
    

    private void OnDisable() => manager.UpdatePlayState();
    private void OnDestroy() => manager.DestroyLottie();
    private void OnBecameVisible() => IsVisible = true;
    private void OnBecameInvisible() => IsVisible = false;

    internal void TryRefresh()
    {
        if (verticesIsDirty)
        {
            BuildUnitQuad();
            verticesIsDirty = false;
        }

        if (colorIsDirty)
        {
            UpdateColor();
            colorIsDirty = false;
        }
        
        manager.ResizeIfNeeded();
    }

    internal void OnTextureSwapped(Texture tex)
    {
#if UNITY_EDITOR
        if (tex == null) return;
#endif
        mr.GetPropertyBlock(mpb);
        mpb.SetTexture(mainTexId, tex);
        mr.SetPropertyBlock(mpb);
    }

#if UNITY_EDITOR

#endif
}
