using LSCore;
using UnityEngine;

public abstract class BaseLottieAsset : ScriptableObject
{
    public abstract string Json { get; }
    public virtual LSImage.RotationMode Rotation => LSImage.RotationMode.None;
    public virtual (bool x, bool y) Flip => default;
    protected internal virtual bool IsCompressed => false;
    protected internal abstract string CompressedExtension { get; }
    protected internal abstract string DecompressedExtension { get; }
    
    public void SetupImage(LottieImage image)
    {
        image.PreserveAspectRatio = true;
        image.asset = this;
        image.Rotation = Rotation;
        image.Flip = Flip;
    }
    
    public void SetupRenderer(LottieRenderer renderer)
    {
        renderer.asset = this;
        renderer.Rotation = Rotation;
        renderer.Flip = Flip;
    }
}

public abstract class BaseRegularLottieAsset : BaseLottieAsset
{
    protected internal override string CompressedExtension => ".ziplottie";
    protected internal override string DecompressedExtension => ".lottie";
}

public class LottieAsset : BaseRegularLottieAsset
{
    [SerializeField] internal byte[] data;
    private string json;
    public override string Json => string.IsNullOrEmpty(json) ? json = LottieCompressor.Decompress(data) : json;
    protected internal override bool IsCompressed => true;

    public static LottieAsset Create(byte[] rawData)
    {
        var asset = CreateInstance<LottieAsset>();
        asset.data = rawData;
        return asset;
    }
}