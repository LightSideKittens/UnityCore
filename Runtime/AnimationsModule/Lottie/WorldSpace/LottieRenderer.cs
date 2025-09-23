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
        EditorApplication.update += Update;

        void Update()
        {
            EditorApplication.update -= Update;
            Init();
            mr.hideFlags = HideFlags.HideAndDontSave | HideFlags.HideInInspector;
            mf.hideFlags = HideFlags.HideAndDontSave | HideFlags.HideInInspector;
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
            if (ret == null) ret = gameObject.AddComponent<T>();
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

    internal void OnSpriteChanged(Lottie.Sprite sprite)
    {
#if UNITY_EDITOR
        if (sprite.Texture == null) return;
#endif
        var v = quad.uv;
        v[0] = sprite.UvMin;
        v[1] = new Vector2(sprite.UvMin.x, sprite.UvMax.y);
        v[2] = sprite.UvMax;
        v[3] = new Vector2(sprite.UvMax.x, sprite.UvMin.y);
        quad.uv = v;
        OnTextureChanged(sprite.Texture);
    }

    private void OnTextureChanged(Texture texture)
    {
        mr.GetPropertyBlock(mpb);
        mpb.SetTexture(mainTexId, texture);
        mr.SetPropertyBlock(mpb);
    }
}
