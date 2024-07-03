public class PlaySound : LSAction
{
    public LaLaLa.Settings settings;
    public override void Invoke()
    {
        LaLaLa.playSettings.Copy(settings);
        LaLaLa.Play(settings.Clip);
    }
}