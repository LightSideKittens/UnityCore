using System;
using LSCore;

public class ErrorScreen : BaseWindow<ErrorScreen>
{
    [Serializable]
    public class LoaderError : LSCore.LoaderError
    {
#if LOCALIZATION_PRESENT
        public override void SetText(LocalizationData data) => ErrorScreen.SetText(data);
#endif
        public override void Show() => ErrorScreen.Show();
        public override void Show(Action retry) => ErrorScreen.Show(retry);
        public override void Hide() => ErrorScreen.Hide();
    }
    
#if LOCALIZATION_PRESENT
    public LocalizationText text;
#endif
    public LSButton button;
    public override WindowManager Manager { get; } = new NotRecordableWindowManager();

#if LOCALIZATION_PRESENT
    public static void SetText(LocalizationData data)
    {
        Instance.text.SetLocalizationData(data);
    }
#endif

    public new static void Show()
    {
        Instance.Manager.OnlyShow();
        Instance.Internal_Show();
    }

    public static void Show(Action retry)
    {
        Instance.Manager.OnlyShow();
        Instance.Internal_Show(retry);
    }

    public static void Hide()
    {
        Instance.Manager.OnlyHide();
    }

    private void Internal_Show()
    {
        button.gameObject.SetActive(false);
    }

    private void Internal_Show(Action retry)
    {
        button.gameObject.SetActive(true);
        retry += Hide;
        button.Did += OnSubmit;

        void OnSubmit()
        {
            button.Did -= OnSubmit;
            retry();
        }
    }
}