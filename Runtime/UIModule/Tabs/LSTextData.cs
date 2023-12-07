using LightSideCore.Runtime.UIModule;
using UnityEngine;

namespace LSCore
{
    public class LSTextData : Tab.BaseData
    {
        [SerializeField] private LSText openButton;

        public override IClickable Clickable => openButton;
    }
}