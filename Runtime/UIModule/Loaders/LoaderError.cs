using System;

namespace LSCore
{
    [Serializable]
    public abstract class LoaderError
    {
        public abstract void SetText(LocalizationData data);
        public abstract void Show();
        public abstract void Show(Action retry);
        public abstract void Hide();
    }
}