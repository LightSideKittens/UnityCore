using System;

namespace LSCore
{
    [Serializable]
    public class GoHome : DoIt
    {
        public override void Invoke() => UIViewBoss.GoHome();
    }
}