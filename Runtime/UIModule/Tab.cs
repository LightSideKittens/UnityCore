using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace LSCore
{
    [RequireComponent(typeof(CanvasGroup))]
    [Serializable]
    public class Tab : MonoBehaviour
    {
        public class Controller
        {
            private readonly Dictionary<Data, Tab> tabs = new();
            public RectTransform Parent { get; }
            private Tab prevTab;
            private Action<Tab> opened;
            public void OnOpen(Action<Tab> action) => opened += action;
            public Controller(RectTransform parent)
            {
                Parent = parent;
            }
            
            public void Register(Data data)
            {
                data.Init(this);
            }

            public void Add(Data data)
            {
                prevTab = data.tabPrefab;
                data.Init(this);
                prevTab.Init(data.reference, Parent);
                prevTab.OnOpen(OnOpen);
                tabs.Add(data, prevTab);
                prevTab = null;
                Open(data);
            }

            public Tab Open(Data data)
            {
                if (!tabs.TryGetValue(data, out var tab))
                {
                    tab = data.CreateTab(this);
                    tab.OnOpen(OnOpen);
                    tabs[data] = tab;
                }
                
                tab.Open();
                return tab;
            }

            private void OnOpen(Tab tab)
            {
                if (prevTab != null)
                {
                    prevTab.Close();
                }

                prevTab = tab;
                opened?.Invoke(tab);
            }
        }

        [Serializable]
        public struct Data
        {
            public Tab tabPrefab;
            public LSButton openButton;
            public RectTransform reference;
            private Controller controller;

            internal void Init(Controller controller)
            {
                this.controller = controller;
                if (openButton != null)
                {
                    openButton.Listen(Open);
                }
            }
            
            internal Tab CreateTab(Controller controller)
            {
                var parent = controller.Parent;
                var tab = Instantiate(tabPrefab, parent);
                tab.Init(reference, parent);
                
                return tab;
            }

            private void Open() => controller.Open(this);

            public override bool Equals(object obj)
            {
                if (obj is Data drawer)
                {
                    return Equals(drawer);
                }

                return false;
            }
        
            public bool Equals(Data other) => tabPrefab == other.tabPrefab;
            public override int GetHashCode() => tabPrefab.GetInstanceID();
        }

        [SerializeField] private float animDuration = 0.2f;
        private CanvasGroup group;
        private RectTransform parent;
        private RectTransform reference;
        private Action<Tab> opened;

        private void Init(RectTransform reference, RectTransform parent)
        {
            this.reference = reference;
            this.parent = parent;
            group = GetComponent<CanvasGroup>();
        }

        public void OnOpen(Action<Tab> action) => opened += action;

        public void Open()
        {
            group.interactable = true;
            group.blocksRaycasts = true;
            group.DOFade(1, animDuration);
            parent.DOSizeDelta(reference.rect.size, animDuration);
            opened?.Invoke(this);
        }

        private void Close()
        {
            group.interactable = false;
            group.blocksRaycasts = false;
            group.DOFade(0, animDuration);
        }
    }
}