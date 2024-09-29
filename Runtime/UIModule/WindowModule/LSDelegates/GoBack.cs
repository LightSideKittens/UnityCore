using System;

namespace LSCore
{
    [Serializable]
    public class GoBack : LSAction
    {
        public override void Invoke() => WindowsData.GoBack();
    }
}