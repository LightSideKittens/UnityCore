using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace LSCore
{
    [Serializable]
    public abstract class BaseIntFund : BaseFund
    {
        public abstract override int Value { get; set; }

#if UNITY_EDITOR
        private static IconsById _icons;
        public override string ToString() => Id == null ? "Null" : Id;

        protected override void SetIcon(ref Texture2D icon)
        {
            _icons ??= AssetDatabaseUtils.LoadAny<IconsById>("FundIcons_Editor");
            
            if (Id != null && _icons.TryGetIcon(Id, out var sprite))
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
        
        public bool Equals(Fund other) => Id == other.Id;

        public override int GetHashCode() => Id.GetHashCode();
#endif
    }
    
    [Serializable]
    public class Fund : BaseIntFund
    {
        [ValueDropdown("Ids")] 
        [SerializeField] private Id id;

        public override Id Id => id;

        [field: SerializeField] 
        [field: CustomValueDrawer("Editor_Draw")]
        public override int Value { get; set; }

#if UNITY_EDITOR
        protected virtual IEnumerable<CurrencyIdGroup> Groups => AssetDatabaseUtils.LoadAllAssets<CurrencyIdGroup>();
        protected virtual IEnumerable<Id> Ids => Groups.SelectMany(group => group);
#endif
    }
}