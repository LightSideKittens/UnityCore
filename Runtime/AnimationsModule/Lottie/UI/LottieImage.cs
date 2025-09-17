using System;
using System.Linq;
using LSCore;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public sealed class LottieImage : LSRawImage
{
    static LottieImage()
    {
#if UNITY_EDITOR
        Selection.selectionChanged += OnSelectionChanged;
        EditorApplication.update += OnEditorUpdate;
        
        void OnSelectionChanged()
        {

            EditorWorld.Updated -= OnUpdate;
            if (Selection.gameObjects.Any(x => x.TryGetComponent<LottieImage>(out _)))
            {
                EditorWorld.Updated += OnUpdate;
            }
        }

        void OnEditorUpdate()
        {
            EditorApplication.update -= OnEditorUpdate;
            OnSelectionChanged();
        }
#endif
        World.Updated += OnUpdate;
    }
    
    private static Action updated;
    private static void OnUpdate() => updated?.Invoke();
    
    public static LottieImage Create(BaseLottieAsset asset, RectTransform transform, float animationSpeed = 1f, bool loop = true)
    {
        var image = new GameObject(nameof(LottieImage)).AddComponent<LottieImage>();
        image.Asset = asset;
        image.transform.SetParent(transform, false);
        image.speed = animationSpeed;
        image.loop = loop;
        return image;
    }
    
    private LottieAnimation lottie;

    [HideInInspector] public BaseLottieAsset asset;
    [ShowInInspector]
    public BaseLottieAsset Asset
    {
        get => asset;
        set
        {
            if (value == asset) return;
            asset = value;
            DestroyLottie();
            CreateLottie();
            if (asset != null)
            {
                asset.SetupImage(this);
            }
            UpdatePlayState();
        }
    }
    
    [SerializeField] [HideInInspector] private bool loop = true;
    [ShowInInspector]
    public bool Loop
    {
        get => loop;
        set
        {
            if(loop == value) return;
            if(lottie != null) lottie.loop = value;
            loop = value;
            if (value)
            {
                UpdatePlayState();
            }
        }
    }

    [SerializeField] private float speed = 1f;
    [ShowInInspector]
    public float Speed
    {
        get => speed;
        set
        {
            if(Mathf.Abs(speed - value) < 0.01f) return;
            speed = Mathf.Max(0f, value);
            UpdatePlayState();
        }
    }
    
    [SerializeField] private bool isEnabled = true;
    [ShowInInspector]
    public bool Enabled
    {
        get => isEnabled;
        set
        {
            if (value == isEnabled) return;
            isEnabled = value;
            UpdatePlayState();
        }
    }
    
    public bool IsVisible => !canvasRenderer.cull && IsActive();
    
    private uint lastSize;
    
    private uint Size
    {
        get
        {
            var size = rectTransform.rect.size;
            var min = (int)(Mathf.Min(size.x, size.y) * 0.5f);
            min = Mathf.ClosestPowerOfTwo(min);
            var newSize = (uint)Mathf.Clamp(min, 64, 2048);
            lastSize = newSize;
            return newSize;
        }
    }
    
    public bool IsEnded => !loop && lottie.currentFrame >= lottie.TotalFramesCount - 1;
    public bool IsPlaying { get; private set; }
    
    private void UpdatePlayState()
    {
        var lastIsPlaying = IsPlaying;
        IsPlaying = IsVisible && isEnabled && asset != null && !IsEnded && speed > 0;
        if (IsPlaying == lastIsPlaying) return;

        if (IsPlaying)
        {
            if (lottie == null)
            {
                CreateLottie();
                lottie!.DrawOneFrame(0);
            }

            updated += Tick;
        }
        else
        {
            updated -= Tick;
        }
    }
    
    protected override void Awake()
    {
        base.Awake();
        if (asset != null) asset.SetupImage(this);
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        IsPlaying = false;
        CanvasUpdateRegistry.Updated += Update;

        void Update()
        {
            CanvasUpdateRegistry.Updated -= Update;
#if UNITY_EDITOR
            if(World.IsEditMode && !this) return; 
#endif
            UpdatePlayState();
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        UpdatePlayState();
    }

    public override void OnCullingChanged()
    {
        base.OnCullingChanged();
        UpdatePlayState();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        DestroyLottie();
    }

    protected override void OnRectTransformDimensionsChange()
    {
        base.OnRectTransformDimensionsChange();
        if(lottie == null || !IsVisible) return; 
        if (lastSize == Size) return;

        var lastFrame = lottie.currentFrame;
        DestroyLottie();
        CreateLottie();
        lottie.currentFrame = lastFrame;
    }

    private void Tick()
    {
        lottie.LateUpdateFetch();
        lottie.UpdateAsync(speed);
        if (IsEnded) UpdatePlayState();
    }
    
    private void CreateLottie()
    {
        lottie = new LottieAnimation(asset.Json, string.Empty, Size);
        lottie.OnTextureSwapped += OnTextureSwapped;
        lottie.loop = Loop;
    }

    private void OnTextureSwapped(Texture2D tex)
    {
        m_Texture = tex;
        canvasRenderer.SetTexture(m_Texture);
    }

    internal void DestroyLottie()
    {
        if (lottie != null)
        {
            lottie.Destroy();
            lottie = null;
        }
    }
}
