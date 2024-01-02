using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace LSCore
{
    public class CurrencyPanel : MonoBehaviour
    {
        [Serializable]
        public class Data
        {
            [ValueDropdown("Ids")]
            [SerializeField] private Id id;

            [SerializeField] private GameObject gameObject;
            [SerializeField] private LSButton button;
            [SerializeField] private LSText text;

            public void Init()
            {
                Funds.OnChanged(id, OnChanged, true);
            }

            private void OnChanged(int value)
            {
                text.text = $"{value}";
            }
            
#if UNITY_EDITOR
            private IEnumerable<Id> Ids => instance.Ids;
#endif
        }

        [SerializeField] private Data[] data;
        
        private static CurrencyPanel instance;

        private void Awake()
        {
            for (int i = 0; i < data.Length; i++)
            {
                data[i].Init();
            }
        }

        private void OnEnable()
        {
            instance = this;
        }
#if UNITY_EDITOR
        protected virtual IEnumerable<CurrencyIdGroup> Groups => AssetDatabaseUtils.LoadAllAssets<CurrencyIdGroup>();
        protected virtual IEnumerable<Id> Ids => Groups.SelectMany(group => group);

        [OnInspectorInit]
        private void OnInit()
        {
            instance = this;
        }
#endif
    }
}