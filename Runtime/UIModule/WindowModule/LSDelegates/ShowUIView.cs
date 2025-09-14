using System;
using UnityEngine;

namespace LSCore
{
    [Serializable]
    public class ShowUIView : DoIt
    {
        public UIView view;
        public ShowWindowOption option;
        public override void Do()
        {
            view.Show(option);
        }
    }
    
    [Serializable]
    public class ShowUIViewDynamic : DoIt
    {
        [SerializeReference] public Get<UIView> view;
        public ShowWindowOption option;
        public override void Do()
        {
            view.Data.Show(option);
        }
    }
}