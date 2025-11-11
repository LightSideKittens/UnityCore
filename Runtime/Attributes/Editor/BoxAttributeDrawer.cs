#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEngine;

[DrawerPriority(0, 1, 0)]
public class BoxAttributeDrawer : OdinAttributeDrawer<BoxAttribute>
{
    protected override void DrawPropertyLayout(GUIContent label)
    {
        SirenixEditorGUI.BeginBox();
        CallNextDrawer(label);
        SirenixEditorGUI.EndBox();
    }
}
#endif