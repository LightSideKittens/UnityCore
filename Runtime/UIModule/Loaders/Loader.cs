using System;

namespace LSCore
{
    [Serializable]
    public abstract class Loader
    {
        public abstract void ShowLoop();
        public abstract void ShowPercentProgress(out Action<float> progress);
        public abstract void ShowValueProgress(float maxValue, out Action<float> progress);
        public abstract void Hide();
    }
}