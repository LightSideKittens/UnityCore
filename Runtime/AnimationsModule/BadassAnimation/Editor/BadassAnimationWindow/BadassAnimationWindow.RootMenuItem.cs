using Sirenix.OdinInspector.Editor;
using UnityEngine;

public partial class BadassAnimationWindow
{
    public class RootMenuItem : MenuItem
    {
        public RootMenuItem(OdinMenuTree tree, string name) : base(tree, name, null)
        {
        }

        protected override void OnDrawMenuItem(Rect rect, Rect labelRect)
        {
            Rect = ConvertRect(rect);
            BaseOnDrawMenuItem(rect, labelRect);
        }
    }
}