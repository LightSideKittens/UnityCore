using System;

[Serializable]
public class PlaySound : DoIt
{
    public LaLa.Settings settings;
    public override void Do()
    {
        settings.Play();
    }
}