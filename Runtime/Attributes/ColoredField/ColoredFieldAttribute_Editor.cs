#if UNITY_EDITOR
using UnityEditor;
using static EditorUtils;

namespace GameCore.Attributes
{
    public partial class ColoredFieldAttribute
    {
        public static int index;
        public int myIndex;
        public bool isModified;
        public ColorData colorData;

        private static void GetGUID(ref string guid)
        {
            guid = GUID.Generate().ToString();
        }

        partial void Constructor()
        {
            var guid = string.Empty;
            GetGUID(ref guid);
            GroupID = guid;
            index %= Colors.Length;
            colorData = Colors[index];
            myIndex = index;
            index++;
        }
    }
}
#endif