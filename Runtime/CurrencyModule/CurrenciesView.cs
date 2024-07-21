using System;
using LSCore.Extensions.Unity;
using UnityEngine;

namespace LSCore
{
    public class CurrenciesView : MonoBehaviour
    {
        [Serializable]
        public class Data
        {
            [Id(typeof(CurrencyIdGroup))]
            [SerializeField] private Id id;

            [SerializeField] private GameObject gameObject;
            [SerializeField] private LSButton button;
            [SerializeField] private LSText text;

            public void Init()
            {
                Funds.AddOnChanged(id, OnChanged, true);
            }

            private void OnChanged(int value)
            {
                text.text = $"{value}";
            }
        }

        [SerializeField] private Data[] data;

        private void Start()
        {
            for (int i = 0; i < data.Length; i++)
            {
                data[i].Init();
            }
        }
    }
}