using Sirenix.OdinInspector;
using UnityEngine;

namespace LSCore
{
    [CreateAssetMenu(fileName = nameof(ShopConfig), menuName = "Launcher/" + nameof(ShopConfig), order = 0)]
    public class ShopConfig : SerializedScriptableObject
    {
        public ShopCategoryConfig[] Categories;
    }
}