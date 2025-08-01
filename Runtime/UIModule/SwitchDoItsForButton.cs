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
            var doiter = button.Data.submittable.doIter;
            if (lastDoIts == null)
            { 
                lastDoIts = new(doIts);
                lastDoIts.Add(this);
            }

            List<DoIt> list = new(doiter.onSubmit);
            doiter.onSubmit.Clear();
            doiter.onSubmit.AddRange(lastDoIts);
            doiter.onSubmit.Add(this);
            lastDoIts = list;
        }
    }
}