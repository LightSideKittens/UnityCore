using System;
using LSCore;

namespace LightSideCore.Runtime.UIModule
{
    [Serializable]
    public class ResetButton : DoIt
    {
        public LSButton button;
        
        public override void Do()
        {
            button.Clicked = null;
            button.clickActions.actions.Clear();
        }
    }
}