using System;

namespace LSCore
{
    [Serializable]
    public class GoHome : DoIt
    {
        public override void Do() => UIViewBoss.GoHome();
    }
}