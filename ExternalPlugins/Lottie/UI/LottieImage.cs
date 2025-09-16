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
    public Transform Transform { get; private set; }
    public LSRawImage RawImage => this;
    internal LottieAnimation LottieAnimation => lottieAnimation;
    internal float AnimationSpeed => animationSpeed;

    [ShowInInspector]
    public bool Loop
    {
        get => loop;
        set
        {
            loop = value;
            if (value) Play();
        }
    }

    [ShowInInspector]
    public BaseLottieAsset Asset
    {
        get => asset;
        set
        {
            if (value == asset) return;
            asset = value;
            DisposeLottieAnimation();
            if (asset != null)
            {
                asset.SetupImage(this);
            }
            if (Enabled) Play();
        }
    }

    private bool localEnabled;
    public bool Enabled
    {
        get => localEnabled;
        set
        {
            if (value == localEnabled) return;
            localEnabled = value;

            // Управляем подпиской на наш статический “тик” только когда реально нужно
            if (localEnabled && !canvasRenderer.cull && enabled)
            {
                updated += LocalUpdate;
            }
            else
            {
                updated -= LocalUpdate;
            }

            // Сообщаем анимации об изменении видимости/активности
            if (lottieAnimation != null)
            {
                lottieAnimation.SetVisible(localEnabled && !canvasRenderer.cull);
            }
        }
    }

    [HideInInspector] public BaseLottieAsset asset;
    [SerializeField] private float animationSpeed = 1f;
    [SerializeField, HideInInspector] private bool loop = true;

    private LottieAnimation lottieAnimation;

    public static LottieImage Create(BaseLottieAsset asset, RectTransform transform, float animationSpeed = 1f, bool loop = true)
    {
        var image = new GameObject(nameof(LottieImage)).AddComponent<LottieImage>();
        image.Asset = asset;
        image.transform.SetParent(transform, false);
        image.animationSpeed = animationSpeed;
        image.loop = loop;
        return image;
    }

    protected override void Awake()
    {
        base.Awake();
        Transform = transform;
        if (asset != null) asset.SetupImage(this);
    }

    // -------------------- Размер текстуры (pow2 + clamp) --------------------
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

    // -------------------- Глобальный тик (Editor/Play) --------------------
    static LottieImage()
    {
#if UNITY_EDITOR
        Selection.selectionChanged += OnSelectionChanged;
        void OnSelectionChanged()
        {
            EditorWorld.Updated -= OnUpdate;
            if (Selection.gameObjects.Any(x => x.TryGetComponent<LottieImage>(out _)))
            {
                EditorWorld.Updated += OnUpdate;
            }
        }
#endif
        World.Updated += OnUpdate;
    }

    private static Action updated;
    private static void OnUpdate() => updated?.Invoke();

    // -------------------- Жизненный цикл UI --------------------
    protected override void OnEnable()
    {
        base.OnEnable();
        Play();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        Enabled = false; // отпишемся от тика и поставим на паузу
    }

    public override void OnCullingChanged()
    {
        base.OnCullingChanged();
        Enabled = !canvasRenderer.cull;
        if (lottieAnimation != null)
            lottieAnimation.SetVisible(enabled && !canvasRenderer.cull);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        DisposeLottieAnimation();
    }

    protected override void OnRectTransformDimensionsChange()
    {
        base.OnRectTransformDimensionsChange();
        if (asset == null) return;

        var prev = lastSize;
        var size = Size;
        if (prev == size) return;

        // Пересоздаём анимацию под новый размер, стараясь сохранить кадр
        var lastFrame = lottieAnimation?.CurrentFrame ?? 0;
        RecreateAnimation(size, lastFrame);
    }

    // -------------------- Управление --------------------
    [Button] public void Play()
    {
        CreateLottieAnimationIfNeeded();
        lottieAnimation?.Play();
        if (lottieAnimation != null)
            lottieAnimation.SetVisible(!canvasRenderer.cull);
        Enabled = true;
    }

    [Button] public void Pause()
    {
        Enabled = false;
        lottieAnimation?.Pause();
    }

    [Button] public void Resume()
    {
        Enabled = true;
        lottieAnimation?.Resume();
    }

    [Button] public void Stop()
    {
        Enabled = false;
        lottieAnimation?.Stop();
        lottieAnimation?.DrawOneFrame(0);
    }

    // -------------------- Обновление по кадрам --------------------
    private void LocalUpdate()
    {
#if UNITY_EDITOR
        if (World.IsEditMode && lottieAnimation == null)
        {
            Play();
            if (lottieAnimation == null) return;
        }
#endif
        if (lottieAnimation == null) return;

        // 1) Забираем готовый кадр N-1 и переключаем текстуры (GPU без Apply mipmaps)
        lottieAnimation.LateUpdateFetch();

        // 2) Планируем следующий кадр N (асинхронно в плагине)
        lottieAnimation.UpdateAsync(animationSpeed);

        // 3) Если не луп — останавливаемся на последнем кадре
        if (!loop && lottieAnimation.CurrentFrame == lottieAnimation.TotalFramesCount - 1)
        {
            Stop();
        }
    }

    // -------------------- Создание/уничтожение анимации --------------------
    internal void CreateLottieAnimationIfNeeded()
    {
        if (lottieAnimation != null || asset == null) return;

        lottieAnimation = LottieAnimation.LoadFromJsonData(asset.Json, string.Empty, Size);
        BindTextureEvents();
        lottieAnimation.DrawOneFrame(0);
        
    }

    private void RecreateAnimation(uint newSize, int frameToRestore)
    {
        // Отписываем события со старого экземпляра
        if (lottieAnimation != null)
            lottieAnimation.OnTextureSwapped -= OnTextureSwapped;

        lottieAnimation?.Dispose();
        lottieAnimation = LottieAnimation.LoadFromJsonData(asset.Json, string.Empty, newSize);

        // Восстанавливаем кадр синхронно (один раз)
        lottieAnimation.DrawOneFrame(Mathf.Clamp(frameToRestore, 0, (int)lottieAnimation.TotalFramesCount - 1));

        // Делayed-assign через Canvas апдейт, чтобы избежать лишних перерисовок лэйаута
        CanvasUpdateRegistry.Updated += OnCanvasUpdatedAssignTexture;
        void OnCanvasUpdatedAssignTexture()
        {
            CanvasUpdateRegistry.Updated -= OnCanvasUpdatedAssignTexture;
            BindTextureEvents();
            texture = lottieAnimation.Texture;
        }
    }

    private void BindTextureEvents()
    {
        if (lottieAnimation == null) return;
        // каждый раз при свопе буфера просто подставляем новую Texture2D
        lottieAnimation.OnTextureSwapped -= OnTextureSwapped;
        lottieAnimation.OnTextureSwapped += OnTextureSwapped;
        // Прокинем текущую видимость
        lottieAnimation.SetVisible(enabled && !canvasRenderer.cull);
    }

    private void OnTextureSwapped(Texture2D tex)
    {
        texture = tex;
    }

    internal void DisposeLottieAnimation()
    {
        if (lottieAnimation != null)
        {
            lottieAnimation.OnTextureSwapped -= OnTextureSwapped;
            lottieAnimation.Dispose();
            lottieAnimation = null;
        }
    }
}
