using System;
using UnityEngine;

namespace LSCore
{
    [Serializable]
    public class Fund : BaseFund
    {
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
            if (IconsById.TryGetMainIcon(id, out var sprite))
            {
                icon = sprite.texture;
                return;
            }
            
            icon = EditorUtils.GetTextureByColor(Color.white);
        }

        public override bool Equals(object obj)
        {
            if (obj is Fund fund)
            {
                return Equals(fund);
            }

            return false;
        }
        
        public bool Equals(Fund other) => id == other.id;

        public override int GetHashCode() => id.GetHashCode();
#endif
    }
}