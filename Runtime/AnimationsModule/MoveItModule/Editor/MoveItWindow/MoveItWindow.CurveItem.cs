#if UNITY_EDITOR
using DG.DemiEditor;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

public partial class MoveItWindow
{
    public class CurveItem : MenuItem
    {
        public CurvesEditor curvesEditor;
        public MoveItCurveEditor editor;
        public MoveItCurve curve;
        private MoveIt.Handler handler;
        private MoveItClip clip;
        public Color color;
        public string property;

        public bool TryGetCurve(out MoveItCurve curve)
        {
            if (clip.namesToCurvesByHandlerGuids.TryGetValue(handler.guid, out var curves))
            {
                if (curves.TryGetValue(property, out curve))
                {
                    this.curve = curve;
                    return true;
                }
            }

            curve = null;
            this.curve = curve;
            return false;
        }

        public void CreateCurve()
        {
            window.RecordAddCurve();
            curve = new MoveItCurve();
            clip.Add(handler, property, curve);
            EditorUtility.SetDirty(clip);
        }

        public CurveItem(OdinMenuTree tree, string property, Color color, MoveItClip clip,
            MoveIt.Handler handler, CurvesEditor editor) : base(tree, property, null)
        {
            this.property = property;
            curvesEditor = editor;
            this.color = color;
            this.clip = clip;
            this.handler = handler;
            Color = color.SetAlpha(0.5f);
            Style = Style.Clone();
            Style.SelectedColorDarkSkin = Color;
            Style.SelectedColorLightSkin = Color;
            TryGetCurve(out _);
        }

        protected override void OnDrawMenuItem(Rect rect, Rect labelRect)
        {
            EditorGUI.DrawRect(rect, Color.SetAlpha(0.2f));
            base.OnDrawMenuItem(rect, labelRect);

            var e = Event.current;
            if (e.OnContextClick(rect))
            {
                var popup = new Popup();
                popup.onGui = () =>
                {
                    if (popup.DrawButton("Delete Curve"))
                    {
                        DeleteCurve();
                    }
                };
                popup.Show(e.mousePosition);
            }

            if (handler.TryGetEvaluator(property, out var evaluator))
            {
                rect.TakeFromRight(5);
                rect.TakeFromRight(40);
                EditorGUI.BeginChangeCheck();
                var value = EditorGUI.FloatField(rect.TakeFromRight(40), evaluator.y);
                if (EditorGUI.EndChangeCheck())
                {
                    window.ModifyCurve((HandlerItem)Parent, property, value);
                }
            }
        }

        public void DeleteCurve()
        {
            window.RecordDeleteCurve();
            curvesEditor.curvesEditor.Remove(editor);
            curvesEditor.CurveItems.Remove(this);
            clip.Remove(handler, property);
            handler.RemoveEvaluator(property);
            window.TryUpdateAnimationMode();
            editor = null;
            curve = null;
        }

        public void TryCreateCurveEditor(Object context)
        {
            if (editor != null) return;

            if (TryGetCurve(out var c))
            {
                var bce = new MoveItCurveEditor(c, context);
                bce.curveColor = color;
                bce.tangentLineColor = color;
                bce.IsLocked = IsLockedSelf;
                bce.IsVisible = IsVisibleSelf;

                LockedSelfChanged += state => bce.IsLocked = state;
                VisibleSelfChanged += state => bce.IsVisible = state;

                editor = bce;
                curvesEditor.Add(bce);
                curvesEditor.CurveItems.Add(this);
                window.TryUpdateAnimationMode();
            }
        }
    }
}
#endif