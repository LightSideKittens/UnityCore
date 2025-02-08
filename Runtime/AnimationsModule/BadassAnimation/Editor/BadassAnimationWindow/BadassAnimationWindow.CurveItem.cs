#if UNITY_EDITOR
using DG.DemiEditor;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

public partial class BadassAnimationWindow
{
    public class CurveItem : MenuItem
    {
        public CurvesEditor curvesEditor;
        public BadassCurveEditor editor;
        public BadassCurve curve;
        private BadassAnimation.Handler handler;
        private BadassAnimationClip clip;
        public Color color;

        public bool TryGetCurve(out BadassCurve curve)
        {
            if (clip.namesToCurvesByHandlerGuids.TryGetValue(handler.guid, out var curves))
            {
                if (curves.TryGetValue(Name, out curve))
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
            curve = new BadassCurve();
            clip.Add(handler, Name, curve);
            EditorUtility.SetDirty(clip);
        }

        public CurveItem(OdinMenuTree tree, string name, Color color, BadassAnimationClip clip,
            BadassAnimation.Handler handler, CurvesEditor editor) : base(tree, name, null)
        {
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

            if (handler.TryGetEvaluator(Name, out var evaluator))
            {
                rect.TakeFromRight(5);
                rect.TakeFromRight(40);
                EditorGUI.FloatField(rect.TakeFromRight(40), evaluator.y);
            }
        }

        public void DeleteCurve()
        {
            window.RecordDeleteCurve();
            curvesEditor.curvesEditor.Remove(editor);
            curvesEditor.CurveItems.Remove(this);
            clip.Remove(handler, Name);
            handler.RemoveEvaluator(Name);
            window.TryUpdateAnimationMode();
            editor = null;
            curve = null;
        }

        public void TryCreateCurveEditor(Object context)
        {
            if (editor != null) return;

            if (TryGetCurve(out var c))
            {
                var bce = new BadassCurveEditor(c, context);
                bce.curveColor = color;
                bce.tangentLineColor = color;
                bce.IsLocked = IsLockedSelf;
                bce.IsVisible = IsVisibleSelf;

                LockedSelfChanged += state => bce.IsLocked = state;
                VisibleSelfChanged += state => bce.IsVisible = state;

                editor = bce;
                curvesEditor.Add(bce);
                curvesEditor.CurveItems.Add(this);
            }
        }
    }
}
#endif