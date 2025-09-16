using System;
using System.Linq;
using LSCore;
using LSCore.Extensions.Unity;
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
        image.animationSpeed = animationSpeed;
        image.loop = loop;
        return image;
    }
    
    [SerializeField] private float animationSpeed = 1f;
    private LottieAnimation lottieAnimation;

    [SerializeField, HideInInspector] private bool loop = true;
    [ShowInInspector]
    public bool Loop
    {
        get => loop;
        set
        {
            loop = value;
            if (value)
            {
                IsPlaying = true;
            }
        }
    }

    [HideInInspector] public BaseLottieAsset asset;
    [ShowInInspector]
    public BaseLottieAsset Asset
    {
        get => asset;
        set
        {
            if (value == asset) return;
            asset = value;
            DestroyLottieAnimation();
            if (asset != null)
            {
                asset.SetupImage(this);
            }
            UpdatePlayingState();
        }
    }
    
    [SerializeField] private bool isPlaying = true;
    [ShowInInspector]
    public bool IsPlaying
    {
        get => isPlaying;
        set
        {
            if (value == isPlaying) return;
            isPlaying = value;
            UpdatePlayingState();
        }
    }
    
    public Transform Transform { get; private set; }
    public LSRawImage RawImage => this;
    internal LottieAnimation LottieAnimation => lottieAnimation;
    internal float AnimationSpeed => animationSpeed;
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

    private bool canPlay;
    private void UpdatePlayingState()
    {
        var lastCanPlay = canPlay;
        canPlay = IsVisible && isPlaying && asset != null;
        if(canPlay == lastCanPlay) return;
        
        if (canPlay)
        {
            if (lottieAnimation == null)
            {
                CreateLottieAnimation();
                lottieAnimation!.DrawOneFrame(0);
            }
            
            updated += LocalUpdate;
        }
        else
        {
            updated -= LocalUpdate;
        }
    }
    
    protected override void Awake()
    {
        base.Awake();
        Transform = transform;
        if (asset != null) asset.SetupImage(this);
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        canPlay = false;
        CanvasUpdateRegistry.Updated += Update;

        void Update()
        {
            CanvasUpdateRegistry.Updated -= Update;
#if UNITY_EDITOR
            if(World.IsEditMode && !this) return; 
#endif
            UpdatePlayingState();
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        UpdatePlayingState();
    }

    public override void OnCullingChanged()
    {
        base.OnCullingChanged();
        UpdatePlayingState();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        DestroyLottieAnimation();
    }

    protected override void OnRectTransformDimensionsChange()
    {
        base.OnRectTransformDimensionsChange();
        if(lottieAnimation == null || !IsVisible) return; 
        
        var prev = lastSize;
        var size = Size;
        if (prev == size) return;

        var lastFrame = lottieAnimation.CurrentFrame;
        
        DestroyLottieAnimation();
        CreateLottieAnimation();
        lottieAnimation.CurrentFrame = lastFrame;
    }

    [Button] public void Play()
    {
        IsPlaying = true;
    }

    [Button] public void Pause()
    {
        IsPlaying = false;
    }

    [Button] public void Resume()
    {
        IsPlaying = true;
    }

    [Button] public void Stop()
    {
        IsPlaying = false;
        lottieAnimation?.DrawOneFrame(0);
    }

    private void LocalUpdate()
    {
        lottieAnimation.LateUpdateFetch();
        lottieAnimation.UpdateAsync(animationSpeed);
        if (!loop && lottieAnimation.CurrentFrame == lottieAnimation.TotalFramesCount - 1)
        {
            IsPlaying = false;
        }
    }
    
    private void CreateLottieAnimation()
    {
        lottieAnimation = new LottieAnimation(asset.Json, string.Empty, Size);
        lottieAnimation.OnTextureSwapped += OnTextureSwapped;
    }

    private void OnTextureSwapped(Texture2D tex)
    {
        texture = tex;
        canvasRenderer.SetTexture(m_Texture);
    }

    internal void DestroyLottieAnimation()
    {
        if (lottieAnimation != null)
        {
            lottieAnimation.Destroy();
            lottieAnimation = null;
        }
    }
}
