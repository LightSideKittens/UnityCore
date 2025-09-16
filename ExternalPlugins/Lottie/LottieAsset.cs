using LSCore;
using UnityEngine;

public abstract class BaseLottieAsset : ScriptableObject
{
    public abstract string Json { get; }
    public virtual LSImage.RotationMode Rotation => LSImage.RotationMode.None;
    public virtual (bool x, bool y) Flip => default;
    
    public void SetupImage(LottieImage image)
    {
        image.PreserveAspectRatio = true;
        image.asset = this;
        image.Rotation = Rotation;
        image.Flip = Flip;
    }
}

public class LottieAsset : BaseLottieAsset
{
    [TextArea(10, 30)]
    [SerializeField] [HideInInspector] internal string json;
    public override string Json => json;
}