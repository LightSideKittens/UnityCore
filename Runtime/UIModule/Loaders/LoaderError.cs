using System;

namespace LSCore
{
    [Serializable]
    public abstract class LoaderError
    {
#if LOCALIZATION_PRESENT
        public abstract void SetText(LocalizationData data);
#endif
        public abstract void Show();
        public abstract void Show(Action retry);
        public abstract void Hide();
    }
}