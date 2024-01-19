#if UNITY_EDITOR
using System.Reflection;
using UnityEditor;

namespace LSCore.LevelSystem
{
    [InitializeOnLoad]
    internal class Initializer
    {
        static Initializer()
        {
            BaseGameProperty.AddAllTypesFromAssembly(Assembly.GetExecutingAssembly());
        }
    }
}
#endif