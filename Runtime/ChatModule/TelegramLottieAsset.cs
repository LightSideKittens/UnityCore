using LSCore;
using UnityEngine;

public abstract class BaseTelegramLottieAsset : BaseLottieAsset
{
    public override LSImage.RotationMode Rotation => LSImage.RotationMode.D180;
    public override (bool x, bool y) Flip => (true, false);

    protected static string SanitizeJson(string src)
    {
        const string tag = "\"tgs\":1,";
        int i = src.IndexOf(tag, System.StringComparison.Ordinal);
        src = src.Remove(i, tag.Length);
        return src;
    }
}

public class TelegramLottieAsset : BaseTelegramLottieAsset
{
    [SerializeField] internal byte[] rawData;
    private string json;
    public override string Json => string.IsNullOrEmpty(json) ? json = SanitizeJson(TgsHelper.TgsToJsonString(rawData)) : json;

    public static TelegramLottieAsset Create(byte[] rawData)
    {
        var asset = CreateInstance<TelegramLottieAsset>();
        asset.rawData = rawData;
        return asset;
    }
}