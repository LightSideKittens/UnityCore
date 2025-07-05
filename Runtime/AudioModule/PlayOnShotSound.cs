public class PlayOneShotSound : DoIt
{
    public LaLa.Settings settings;
    public override void Do()
    {
        settings.PlayOneShot();
    }
}