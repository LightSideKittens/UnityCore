#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor;
using UnityEditor;

public partial class MoveItWindow
{
    public class HandlerNoItem : MenuItem
    {
        public MoveIt.Handler handler;
        private MoveIt animation;
        private PropertyTree propertyTree;

        public HandlerNoItem(OdinMenuTree tree, string name, object value, MoveIt animation,
            MoveIt.Handler handler) : base(tree, name, value)
        {
            this.animation = animation;
            this.handler = handler;
            propertyTree = PropertyTree.Create(handler);
        }

        public override void DrawMenuItem(int indentLevel)
        {
            var last = EditorGUI.indentLevel;
            EditorGUI.indentLevel = indentLevel;
            EditorUtils.DrawInBoxFoldout("Handler", Draw, this, false);
            EditorGUI.indentLevel = last;
        }

        private void Draw()
        {
            EditorGUI.BeginChangeCheck();
            propertyTree.Draw(false);
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(animation);
            }
        }
    }
}
#endif