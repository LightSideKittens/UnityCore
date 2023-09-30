#if UNITY_IOS
#if !UNITY_EDITOR
#define RUNTIME
#endif

using System;
using System.Diagnostics;

namespace LSCore.Runtime
{
    public static partial class Clipboard
    {
        [Conditional("RUNTIME")]
        static partial void IOS_Copy(string text)
        {

        }
        
        [Conditional("RUNTIME")]
        static partial void IOS_Paste(Action<string> action)
        {
            
        }
    }
}
#endif