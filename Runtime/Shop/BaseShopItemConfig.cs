namespace LSCore
{
    public abstract class BaseShopItemConfig : PurchaseConfig
    {
        public abstract void Create();
    }
    
    public abstract class BaseShopItemConfig<TConfig> : BaseShopItemConfig where TConfig : BaseShopItemConfig<TConfig>
    {
        public BaseShopItemView<TConfig> prefab { get; }
        
        protected abstract TConfig This { get; }

        public override void Create()
        {
            var view = Instantiate(prefab);
            view.Setup(This);
        }
    }
}