using Sirenix.OdinInspector.Editor;
using UnityEngine;

public partial class BadassAnimationWindow
{
    public class ToolbarDummy : OdinMenuItem
    {
        public ToolbarDummy(OdinMenuTree tree) : base(tree, string.Empty, null)
        {
            Style = Style.Clone();
            Style.Height = 50;
        }

        protected override void OnDrawMenuItem(Rect rect, Rect labelRect)
        {
            GUI.DrawTexture(rect, EditorUtils.GetTextureByColor(new Color(0.15f, 0.14f, 0.23f)));
        }
    }
}