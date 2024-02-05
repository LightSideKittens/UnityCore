#if UNITY_EDITOR
using System.Reflection;
using LSCore.LevelSystem;
using UnityEditor;

namespace LSCore.GameProperty
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