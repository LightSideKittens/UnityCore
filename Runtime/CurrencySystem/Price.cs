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

        public override void Spend(Func<bool> confirmation)
        {
            Currencies.Spend(id, value, confirmation);
        }

#if UNITY_EDITOR
        public override string ToString() => id == null ? "Null" : id;

        protected override Texture2D Icon
        {
            get
            {
                if (id == null || !IconsById.Instance.ByKey.TryGetValue(id, out var sprite))
                {
                    return EditorUtils.GetTextureByColor(Color.white);
                }
                
                
                return sprite.texture;
            }
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