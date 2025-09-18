#if UNITY_EDITOR
using LSCore;
using Sirenix.OdinInspector.Editor;
using UnityEditor;

[CustomEditor(typeof(LottieImage), true)]
[CanEditMultipleObjects]
public class LottieImageEditor : LSRawImageEditor
{
    private PropertyTree tree;
    private InspectorProperty manager;
    
    protected override void OnEnable()
    {
        base.OnEnable();
        tree = PropertyTree.Create(serializedObject);
        var children = tree.RootProperty.Children;
        manager = children["manager"];
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        tree.BeginDraw(true);
        manager.Draw();
        tree.EndDraw();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        tree.Dispose();
    }
}
#endif