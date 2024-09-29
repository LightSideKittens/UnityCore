using System;

namespace LSCore
{
    [Serializable]
    public class GoHome : LSAction
    {
        public override void Invoke() => WindowsData.GoHome();
    }
}