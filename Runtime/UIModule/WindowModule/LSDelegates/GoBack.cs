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
                WindowsData.GoBack();
            }
            else
            {
                using (new WindowsData.UseId(id))
                {
                    WindowsData.GoBack();
                }
            }
        }
    }
}