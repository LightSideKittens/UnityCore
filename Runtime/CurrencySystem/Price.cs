using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace LSCore
{
    [Serializable]
    public class Price : BasePrice
    {
        [HideIf("isControls")] public Id id;
        
        public override void Earn()
        {
            Currencies.Earn(id, value);
        }

        public override bool Spend(out Action spend)
        {
            return Currencies.Spend(id, value, out spend);
        }

#if UNITY_EDITOR
        public override string ToString() => id == null ? "Null" : id;

        protected override void SetIcon(ref Texture2D icon)
        {
            if (id == null || !IconsById.Instance.ByKey.TryGetValue(id, out var sprite))
            {
                icon = EditorUtils.GetTextureByColor(Color.white);
                return;
            }
                
            icon = sprite.texture;
        }

        public override bool Equals(object obj)
        {
            if (obj is Price price)
            {
                return Equals(price);
            }

            return false;
        }
        
        public bool Equals(Price other) => id == other.id;

        public override int GetHashCode() => id.GetHashCode();
#endif
    }
}