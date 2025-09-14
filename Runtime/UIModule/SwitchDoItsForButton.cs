using System;
using System.Collections.Generic;

namespace LSCore.UIModule
{
    [Serializable]
    public class SwitchDoItsForButton : DoIt
    {
        public Get<LSButton> button;
        public List<DoIt> doIts;
        private List<DoIt> lastDoIts;
        
        public override void Do()
        {
            var doiter = button.Data.uiControl.doIter;
            if (lastDoIts == null)
            { 
                lastDoIts = new(doIts);
                lastDoIts.Add(this);
            }

            List<DoIt> list = new(doiter.onActivate);
            doiter.onActivate.Clear();
            doiter.onActivate.AddRange(lastDoIts);
            doiter.onActivate.Add(this);
            lastDoIts = list;
        }
    }
}