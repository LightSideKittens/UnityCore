using UnityEngine;
using UnityEngine.UI;

public class ProgressJoystick : Joystick
{
    [SerializeField] private Image progressImage;

    public float Progress
    {
        get => progressImage.fillAmount;
        set => progressImage.fillAmount = value;
    }
}
