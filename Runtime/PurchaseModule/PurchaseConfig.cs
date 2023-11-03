using Sirenix.OdinInspector;
using Sirenix.Serialization;

namespace LSCore
{
    public abstract class PurchaseConfig : RewardConfig
    {
        [OdinSerialize] [HideReferenceObjectPicker] public Funds Funds { get; set; } = new();
    }
}