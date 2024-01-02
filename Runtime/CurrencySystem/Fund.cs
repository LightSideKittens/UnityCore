using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace LSCore
{
    [Serializable]
    public abstract class BaseIntFund : BaseFund
    {
        public abstract override int Value { get; set; }

#if UNITY_EDITOR
        public override string ToString() => id == null ? "Null" : id;

        protected override void SetIcon(ref Texture2D icon)
        {
            if (id != null && IconsById.TryGetMainIcon(id, out var sprite))
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
    
    [Serializable]
    public class Fund : BaseIntFund
    {
        [field: SerializeField] 
        [field: CustomValueDrawer("Editor_Draw")]
        public override int Value { get; set; }
    }
}