using System;

namespace LSCore
{
    [Serializable]
    public class GoBack : DoIt
    {
        public string id;
        
        public override void Do()
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