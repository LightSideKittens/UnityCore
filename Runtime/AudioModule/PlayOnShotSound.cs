public class PlayOneShotSound : LSAction
{
    public LaLaLa.Settings settings;
    public override void Invoke()
    {
        LaLaLa.playOneShotSettings.Copy(settings);
        LaLaLa.PlayOneShot(settings.Clip);
    }
    
}