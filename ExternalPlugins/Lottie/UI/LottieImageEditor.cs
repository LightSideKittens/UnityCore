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
    private InspectorProperty animationSpeed;
    
    protected override void OnEnable()
    {
        base.OnEnable();
        tree = PropertyTree.Create(serializedObject);
        var children = tree.RootProperty.Children;
        asset = children["Asset"];
        loop = children["Loop"];
        animationSpeed = children["animationSpeed"];
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        tree.BeginDraw(true);
        asset.Draw();
        loop.Draw();
        animationSpeed.Draw();
        tree.EndDraw();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        tree.Dispose();
    }
}
#endif