using UnityEngine;

namespace LSCore
{
    public abstract class BaseShopItemView<TConfig> : MonoBehaviour where TConfig : BaseShopItemConfig<TConfig>
    {
        public abstract void Setup(TConfig config);
    }
}