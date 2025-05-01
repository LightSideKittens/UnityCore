using System;

namespace LSCore
{
    [Serializable]
    public class GoBack : LSAction
    {
        public string id;
        
        public override void Invoke()
        {
            if (string.IsNullOrEmpty(id))
            {
                UIViewBoss.GoBack();
            }
            else
            {
                using (new UIViewBoss.UseId(id))
                {
                    UIViewBoss.GoBack();
                }
            }
        }
    }
}