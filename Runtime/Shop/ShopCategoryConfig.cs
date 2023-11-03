using UnityEngine;

namespace LSCore
{
    [CreateAssetMenu(fileName = nameof(ShopCategoryConfig), menuName = "Launcher/" + nameof(ShopCategoryConfig), order = 0)]
    public class ShopCategoryConfig : ScriptableObject
    {
        public string title; // TODO: Localization
        public BaseShopItemConfig[] items;

        public void Init()
        {
            for (int i = 0; i < items.Length; i++)
            {
                items[i].Create();
            }
        }
    }
}