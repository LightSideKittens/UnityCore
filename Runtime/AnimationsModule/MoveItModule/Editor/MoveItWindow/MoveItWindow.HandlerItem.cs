#if UNITY_EDITOR
using System;
using System.Linq;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

public partial class MoveItWindow
{
    public class HandlerItem : MenuItem
    {
        private PropertyTree propertyTree;
        public MoveIt.Handler handler;
        private MoveIt animation;

        public HandlerItem(OdinMenuTree tree, string name, MoveIt animation, MoveIt.Handler handler) :
            base(tree, name, null)
        {
            this.animation = animation;
            this.handler = handler;
            propertyTree = PropertyTree.Create(handler);
        }

        protected override void OnDrawMenuItem(Rect rect, Rect labelRect)
        {
            base.OnDrawMenuItem(rect, labelRect);

            var e = Event.current;
            if (e.OnContextClick(rect))
            {
                var popup = new Popup(default, new Vector2(600, 200));
                popup.onGui = () =>
                {
                    EditorUtils.DrawInBoxFoldout("Handler", Draw, this, true);
                    GUILayout.FlexibleSpace();
                    if (popup.DrawButton("Delete Handler", Array.Empty<GUILayoutOption>()))
                    {
                        window.RecordDeleteHandler();
                        foreach (var curveItem in GetChildMenuItemsRecursive(false).OfType<CurveItem>())
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

                        window.IsAnimationMode = false;
                        window.UpdateAnimationComponent();
                        window.IsAnimationMode = true;
                    }
                };
                popup.Show(e.mousePosition);
            }
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