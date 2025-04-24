#if UNITY_EDITOR
using DG.DemiEditor;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;
using static MoveIt;
using Event = UnityEngine.Event;

public partial class MoveItWindow
{
    public class CurveItem : MenuItem
    {
        public CurvesEditor curvesEditor;
        public MoveItCurveEditor editor;
        private Handler handler;
        private MoveItClip clip;
        public Color color;
        private HandlerEvaluateData evaluator;

        public MoveItCurve Curve => evaluator.curve;
        public string property;

        public int ContainsInKeys(float y)
        {
            int cur = 0;
            foreach (var key in editor.Keys)
            {
                if (Mathf.Approximately(key.y, y))
                {
                    cur++;
                }
            }
        
            return cur;
        }
        
        public bool TryGetCurve(out HandlerEvaluateData evaluator)
        {
            if (clip.evaluatorsByHandlerGuids.TryGetValue(handler.guid, out var curves))
            {
                if (curves.TryGetValue(property, out evaluator))
                {
                    this.evaluator = evaluator;
                    return true;
                }
            }

            evaluator = null;
            this.evaluator = evaluator;
            return false;
        }

        public void CreateCurve(out HandlerEvaluateData evaluator)
        {
            window.RecordAddCurve();
            var propType = handler.FindProperty(property).propertyType;
            var curve = new MoveItCurve(propType is SerializedPropertyType.Boolean or SerializedPropertyType.Enum or SerializedPropertyType.ObjectReference);
            evaluator = new HandlerEvaluateData
            {
                property = property,
                curve = curve,
                isRef = propType == SerializedPropertyType.ObjectReference,
                isFloat = propType is SerializedPropertyType.Boolean or SerializedPropertyType.Float or SerializedPropertyType.Integer,
            };
            clip.Add(handler, evaluator);
            EditorUtility.SetDirty(clip);
        }

        public CurveItem(OdinMenuTree tree, string property, Color color, MoveItClip clip,
            Handler handler, CurvesEditor editor) : base(tree, property, null)
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
                        window.UpdateAnimationComponent();
                        window.TryUpdateAnimationMode();
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
            editor = null;
            evaluator = null;
        }

        public void TryCreateCurveEditor(Object context)
        {
            if (editor != null) return;

            if (TryGetCurve(out var evaluator))
            {
                var bce = new MoveItCurveEditor(evaluator.curve, context);
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