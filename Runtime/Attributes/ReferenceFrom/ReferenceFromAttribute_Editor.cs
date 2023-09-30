#if UNITY_EDITOR
using static EditorUtils;

public partial class ReferenceFromAttribute
{
    public string PrefabName { get; private set;}
    public static int Index { get; private set; }
    public int MyIndex { get; private set;}
    public ColorData ColorData { get; private set;}

    partial void Constructor(string prefabName)
    {
        PrefabName = prefabName;
        Index %= Colors.Length;
        ColorData = Colors[Index];
        MyIndex = Index;
        Index++;
    }
}
#endif