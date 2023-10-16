using System.Collections.Generic;

namespace LSCore.Firebase
{
    public class FirebaseData : Dictionary<string, object>
    {
        public static FirebaseData Create(string key, object value)
        {
            return new FirebaseData() { {key, value} };
        }
        
        public new FirebaseData Add(string key, object value)
        {
            base.Add(key, value);
            return this;
        }
    }
}