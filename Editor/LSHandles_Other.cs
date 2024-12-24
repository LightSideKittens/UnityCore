using System.Reflection;
using UnityEngine.SceneManagement;

namespace LSCore.Editor
{
    public static partial class LSHandles
    {
        public static Scene CreateScene(int handle)
        {
            var s = new Scene();
            FieldInfo handleField = typeof(Scene).GetField("m_Handle", BindingFlags.NonPublic | BindingFlags.Instance);
            handleField?.SetValue(s, handle);
            return s;
        }
    }
}