namespace LSCore.AnimationsModule
{
    public class AnimAction : LSAction
    {
        public AnimSequencer anim;
        
        public override void Invoke()
        {
            anim.Animate();
        }
    }
}