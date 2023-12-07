using LightSideCore.Runtime.UIModule;
using UnityEngine;

namespace LSCore
{
    public class LSButtonData : Tab.BaseData
    {
        [SerializeField] private LSButton openButton;

        public override IClickable Clickable => openButton;
    }
}