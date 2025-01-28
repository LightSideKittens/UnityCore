using System.Linq;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEngine;

public partial class BadassAnimationWindow
{
    public class HandlerItem : MenuItem
    {
        public BadassAnimation.Handler handler;
        private BadassAnimation animation;

        public HandlerItem(OdinMenuTree tree, string name, BadassAnimation animation, BadassAnimation.Handler handler) :
            base(tree, name, null)
        {
            this.animation = animation;
            this.handler = handler;
        }

        protected override void OnDrawMenuItem(Rect rect, Rect labelRect)
        {
            base.OnDrawMenuItem(rect, labelRect);

            var e = Event.current;
            if (e.OnContextClick(rect))
            {
                var popup = new Popup();
                popup.onGui = () =>
                {
                    if (popup.DrawButton("Delete Handler"))
                    {
                        window.RecordDeleteHandler();
                        foreach (var curveItem in ChildMenuItems.OfType<CurveItem>())
                        {
                            curveItem.DeleteCurve();
                        }

                        Remove();
                        animation.Remove(handler);
                        handler.Stop();
                    }
                };
                popup.Show(e.mousePosition);
            }
        }
    }
}