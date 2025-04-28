#if UNITY_EDITOR
using System;
using System.Text.RegularExpressions;
using DG.DemiEditor;
using LSCore.Extensions.Unity;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;
using static MoveIt;
using Event = UnityEngine.Event;
using Object = UnityEngine.Object;

public partial class MoveItWindow
{
    public class CurveItem : MenuItem
    {
        public CurvesEditor curvesEditor;
        public MoveItCurveEditor editor;
        private Handler handler;
        private MoveItClip clip;
        public Color color;
        private HandlerEvaluator evaluator;

        public MoveItCurve Curve => evaluator.curve;
        public string property;
        public string rawProperty;
        public SerializedProperty sProperty;

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

        public bool TryGetCurve(out HandlerEvaluator evaluator)
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

        public void CreateCurve(out HandlerEvaluator evaluator)
        {
            window.RecordAddCurve();
            var propType = sProperty.propertyType;
            var curve = new MoveItCurve(propType is SerializedPropertyType.Boolean or SerializedPropertyType.Enum or SerializedPropertyType.ObjectReference);
            evaluator = new HandlerEvaluator
            {
                rawProperty = rawProperty,
                property = property,
                curve = curve,
                propertyType = propType switch
                {
                    SerializedPropertyType.Float => MoveIt.PropertyType.Float,
                    SerializedPropertyType.Integer => MoveIt.PropertyType.Int,
                    SerializedPropertyType.Boolean => MoveIt.PropertyType.Bool,
                    SerializedPropertyType.Enum => MoveIt.PropertyType.Enum,
                    SerializedPropertyType.ObjectReference => MoveIt.PropertyType.Ref,
                },
            };
            clip.Add(handler, evaluator);
            EditorUtility.SetDirty(clip);
        }

        private static readonly Regex kUnityArray = new(@"\.Array\.data\[(\d+)\]");
        private static string Canonicalize(string propPath) => kUnityArray.Replace(propPath, "[$1]");
        
        public CurveItem(OdinMenuTree tree, string property, Color color, MoveItClip clip,
            Handler handler, CurvesEditor editor) : base(tree, property, null)
        {
            this.rawProperty = property;
            this.property = property;
            curvesEditor = editor;
            this.clip = clip;
            this.handler = handler;
            this.color = color;
            Color = color.SetAlpha(0.5f);
            Style = Style.Clone();
            Style.SelectedColorDarkSkin = Color;
            Style.SelectedColorLightSkin = Color;
            
            sProperty = handler.FindProperty(rawProperty);
            this.property = Canonicalize(sProperty.propertyPath);
            Name = this.property.Split('.')[^1];
            
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
                    }
                };
                popup.Show(e.mousePosition);
            }

            if (handler.TryGetEvaluator(property, out var evaluator))
            {
                rect.TakeFromRight(5);
                rect.TakeFromRight(40);
                
                float value;
                var prop = handler.FindProperty(rawProperty);
                var prevValue = prop.propertyType switch
                {
                    SerializedPropertyType.Boolean => prop.boolValue ? 1 : 0,
                    SerializedPropertyType.Integer => prop.intValue,
                    SerializedPropertyType.Float => prop.floatValue,
                    SerializedPropertyType.Enum => prop.enumValueFlag,
                    SerializedPropertyType.ObjectReference => prop.objectReferenceInstanceIDValue,
                    _ => 0f
                };
                
                EditorGUI.BeginChangeCheck();

                if (evaluator.propertyType == MoveIt.PropertyType.Ref)
                {
                    editor.isYBlocked = true;
                    var id = (int)evaluator.y;
                    Object obj = null;
                    
                    if (id != 0)
                    {
                        handler.Objects.TryGetValue(id, out obj);
                    }
                    
                    obj = SirenixEditorFields.UnityObjectField(rect.TakeFromRight(labelRect.width / 2), obj, prop.GetFieldType(), true);
                    value = obj != null ? obj.GetInstanceID() : 0;
                }
                else
                {
                    var last = EditorGUIUtility.labelWidth;
                    EditorGUIUtility.labelWidth = 40;
                    value = EditorGUI.FloatField(rect.TakeFromRight(labelRect.width / 2), " ", evaluator.y);
                    EditorGUIUtility.labelWidth = last;
                }
                
                if (EditorGUI.EndChangeCheck())
                {
                    window.ModifyCurve(this, handler, value, prevValue);
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

        public void SetColor(Color color)
        {
            this.color = color;
            Color = color.SetAlpha(0.5f);
            if (editor != null)
            {
                editor.curveColor = color;
                editor.tangentLineColor = color;
            }
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
            }
        }
    }
}
#endif