using System;

[Serializable]
public class PlaySound : DoIt
{
    public LaLaLa.Settings settings;
    public override void Do()
    {
        LaLaLa.playSettings.Copy(settings);
        LaLaLa.Play(settings.Clip);
    }
}