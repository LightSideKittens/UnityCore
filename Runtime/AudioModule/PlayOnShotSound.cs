public class PlayOneShotSound : DoIt
{
    public LaLaLa.Settings settings;
    public override void Do()
    {
        LaLaLa.playOneShotSettings.Copy(settings);
        LaLaLa.PlayOneShot(settings.Clip);
    }
    
}