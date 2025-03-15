using LSCore;
using Sirenix.OdinInspector;
using UnityEngine;

[ExecuteAlways]
public sealed class LottieImage : LSRawImage
{
    public Transform Transform { get; private set; }
    public LSRawImage RawImage => this;
    internal string AnimationJson => json;
    internal LottieAnimation LottieAnimation => lottieAnimation;
    internal float AnimationSpeed => animationSpeed;

    public bool Loop
    {
        get => loop;
        set
        {
            loop = value;
            if (value)
            {
                enabled = true;
            }
        }
    }

    [SerializeField] private float animationSpeed = 1f;
    [SerializeField] private bool loop = true;

    private string json;
    private LottieAnimation lottieAnimation;

    public static LottieImage Create(string json, RectTransform transform, float animationSpeed = 1f, bool loop = true)
    {
        json = json.Remove(1, "\"tgs\":1,".Length);
        LottieImage lottieImage = new GameObject().AddComponent<LottieImage>();
        lottieImage.PreserveAspectRatio = true;
        lottieImage.transform.SetParent(transform);
        lottieImage.json = json;
        lottieImage.animationSpeed = animationSpeed;
        lottieImage.loop = loop;
        lottieImage.enabled = false;
        return lottieImage;
    }

    protected override void Awake()
    {
        base.Awake();
        Transform = transform;
    }

    private void Update()
    {
        lottieAnimation.Update(animationSpeed);
        if (!loop && lottieAnimation.CurrentFrame == lottieAnimation.TotalFramesCount - 1)
        {
            Stop();
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        DisposeLottieAnimation();
    }

    [Button]
    public void Play()
    {
        CreateIfNeededAndReturnLottieAnimation();
        lottieAnimation.Play();
        enabled = true;
    }

    [Button]
    public void Stop()
    {
        enabled = false;
        lottieAnimation.Stop();
        lottieAnimation.DrawOneFrame(0);
    }

    internal LottieAnimation CreateIfNeededAndReturnLottieAnimation()
    {
        var size = rectTransform.rect.size;
        if (lottieAnimation == null)
        {
            lottieAnimation = LottieAnimation.LoadFromJsonData(
                json,
                string.Empty,
                (uint)size.x,
                (uint)size.y);
            texture = lottieAnimation.Texture;
        }

        return lottieAnimation;
    }

    internal void DisposeLottieAnimation()
    {
        if (lottieAnimation != null)
        {
            lottieAnimation.Dispose();
            lottieAnimation = null;
        }
    }
}

