using System;

namespace LSCore.UIModule
{
    [Serializable]
    public class ResetButton : DoIt
    {
        public LSButton button;
        
        public override void Do()
        {
            button.uiControl.doIter.Unsubscribe();
        }
    }
}