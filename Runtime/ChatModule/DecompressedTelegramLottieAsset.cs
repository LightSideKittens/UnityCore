using UnityEngine;

public class DecompressedTelegramLottieAsset : BaseTelegramLottieAsset
{
    [SerializeField] internal string json;
    public override string Json => json;
}