using System;
using System.Collections.Generic;
using DG.Tweening;
using LightSideCore.Runtime.UIModule;
using LightSideCore.Runtime.UIModule.TabAnimations;
using UnityEngine;
using UnityEngine.Serialization;

namespace LSCore
{
    [RequireComponent(typeof(CanvasGroup))]
    [Serializable]
    public class Tab : MonoBehaviour
    {
        public class Controller
        {
            private readonly Dictionary<BaseData, Tab> tabs = new();
            public RectTransform Parent { get; }
            public BaseData CurrentData { get; private set; }
            private Tab currentTab;
            private Action<Tab> opened;
            public void OnOpen(Action<Tab> action) => opened += action;
            public Controller(RectTransform parent)
            {
                Parent = parent;
            }
            
            public void Register(BaseData data)
            {
                data.Init(this);
            }
            
            public void Register(BaseData[] data)
            {
                for (int i = 0; i < data.Length; i++)
                {
                    data[i].Init(this);
                }
            }

            public void Add(BaseData data)
            {
                currentTab = data.tabPrefab;
                data.Init(this);
                currentTab.Init(data.reference, Parent);
                currentTab.OnOpen(OnOpen);
                tabs.Add(data, currentTab);
                currentTab = null;
                Open(data);
            }

            public Tab Open(BaseData data)
            {
                if (!tabs.TryGetValue(data, out var tab))
                {
                    tab = data.CreateTab(this);
                    tab.OnOpen(OnOpen);
                    tabs[data] = tab;
                }

                if (currentTab != tab)
                {
                    CurrentData = data;
                    tab.Open();
                }
                
                return tab;
            }

            private void OnOpen(Tab tab)
            {
                if (currentTab != null)
                {
                    currentTab.Close();
                }

                currentTab = tab;
                opened?.Invoke(tab);
            }
        }

        [Serializable]
        public abstract class BaseData
        {
            public Tab tabPrefab;
            public RectTransform reference;
            private Controller controller;

            public abstract IClickable Clickable { get; }
            internal void Init(Controller controller)
            {
                this.controller = controller;
                if (Clickable != null)
                {
                    Clickable.Clicked += Open;
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
        }

        [SerializeReference] private BaseTabAnim anim;
        private CanvasGroup group;
        private Action<Tab> opened;

        private void Init(RectTransform reference, RectTransform parent)
        {
            group = GetComponent<CanvasGroup>();
            
            anim.group = group;
            anim.reference = reference;
            anim.parent = parent;
        }

        public void OnOpen(Action<Tab> action) => opened += action;

        public void Open()
        {
            group.interactable = true;
            group.blocksRaycasts = true;
            anim.ShowAnim();
            opened?.Invoke(this);
        }

        private void Close()
        {
            group.interactable = false;
            group.blocksRaycasts = false;
            anim.HideAnim();
        }
    }
}