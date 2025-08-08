using System;

public static class Feel
{
    [Serializable]
    public class SoundAndHaptic : DoIt
    {
        public LaLa.PlayOneShot sound;
        public BzBz.Preset haptic;

        public override void Do()
        {
            sound.Do();
            haptic.Play();
        }
    }
}