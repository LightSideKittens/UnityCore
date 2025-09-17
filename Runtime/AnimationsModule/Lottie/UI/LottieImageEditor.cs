#if UNITY_EDITOR
using LSCore;
using Sirenix.OdinInspector.Editor;
using UnityEditor;

[CustomEditor(typeof(LottieImage), true)]
[CanEditMultipleObjects]
public class LottieImageEditor : LSRawImageEditor
{
    private PropertyTree tree;
    private InspectorProperty asset;
    private InspectorProperty loop;
    private InspectorProperty speed;
    private InspectorProperty isEnabled;
    
    protected override void OnEnable()
    {
        base.OnEnable();
        tree = PropertyTree.Create(serializedObject);
        var children = tree.RootProperty.Children;
        asset = children["Asset"];
        loop = children["Loop"];
        speed = children["Speed"];
        isEnabled = children["Enabled"];
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        tree.BeginDraw(true);
        isEnabled.Draw();
        asset.Draw();
        loop.Draw();
        speed.Draw();
        tree.EndDraw();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        tree.Dispose();
    }
}
#endif