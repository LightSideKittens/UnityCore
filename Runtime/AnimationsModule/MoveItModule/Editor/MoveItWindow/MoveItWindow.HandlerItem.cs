#if UNITY_EDITOR
using System.Linq;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEngine;

public partial class MoveItWindow
{
    public class HandlerItem : MenuItem
    {
        public MoveIt.Handler handler;
        private MoveIt animation;

        public HandlerItem(OdinMenuTree tree, string name, MoveIt animation, MoveIt.Handler handler) :
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
                        
                        if (window.objectByHandlerType.TryGetValue(handler.Target, out var handlers))
                        {
                            handlers.Remove(handler.GetType());
                            if (handlers.Count == 0)
                            {
                                window.objectByHandlerType.Remove(handler.Target);
                            }
                        }
                        
                        handler.Stop();
                        window.TryUpdateAnimationMode();
                    }
                };
                popup.Show(e.mousePosition);
            }
        }
    }
}
#endif