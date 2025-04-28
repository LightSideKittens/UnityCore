using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using LSCore.Extensions.Unity;
using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UI;
#endif

namespace LSCore
{
    public class ExternalRectMask2D : RectMask2D
    {
        public List<ExternalRectMask2D> externalMasks = new();
        public string pathToObject;
        private Vector3 position;
        private ChildrenTracker tracker;


        private static readonly Func<RectMask2D, List<RectMask2D>> s_getClippers =
            CreateFieldGetter<List<RectMask2D>>("m_Clippers");

        private static readonly Func<RectMask2D, bool> getShouldRecalc =
            CreateFieldGetter<bool>("m_ShouldRecalculateClipRects");

        private static readonly Action<RectMask2D, bool> setShouldRecalc =
            CreateFieldSetter<bool>("m_ShouldRecalculateClipRects");

        private List<RectMask2D> Clippers => s_getClippers(this);

        private bool ShouldRecalculateClipRects
        {
            get => getShouldRecalc(this);
            set => setShouldRecalc(this, value);
        }

        private static Func<RectMask2D, T> CreateFieldGetter<T>(string fieldName)
        {
            var field = typeof(RectMask2D).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            if (field == null) throw new MissingFieldException(typeof(RectMask2D).FullName, fieldName);

            var target = Expression.Parameter(typeof(RectMask2D), "target");
            var fieldAcc = Expression.Field(target, field);
            var cast = Expression.Convert(fieldAcc, typeof(T));
            return Expression.Lambda<Func<RectMask2D, T>>(cast, target).Compile();
        }

        private static Action<RectMask2D, T> CreateFieldSetter<T>(string fieldName)
        {
            var field = typeof(RectMask2D).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            if (field == null) throw new MissingFieldException(typeof(RectMask2D).FullName, fieldName);

            var target = Expression.Parameter(typeof(RectMask2D), "target");
            var value = Expression.Parameter(typeof(T), "value");
            var assign = Expression.Assign(Expression.Field(target, field), Expression.Convert(value, field.FieldType));
            var lambda = Expression.Lambda<Action<RectMask2D, T>>(assign, target, value);
            return lambda.Compile();
        }

        protected override void Start()
        {
            tracker = transform.FindComponent<ChildrenTracker>(pathToObject);
            if (tracker != null)
            {
                tracker.Added += t =>
                {
                    t.GetComponentsInChildren<ExternalRectMask2D>().ForEach(x => x.externalMasks.Add(this));
                };

                tracker.Removed += t =>
                {
                    t.GetComponentsInChildren<ExternalRectMask2D>().ForEach(x => x.externalMasks.Remove(this));
                };
            }
        }

        public override void PerformClipping()
        {
            if (ShouldRecalculateClipRects)
            {
                GetRectMasksForClip(this, Clippers);
                ShouldRecalculateClipRects = false;
            }

            base.PerformClipping();
        }

        private void GetRectMasksForClip(RectMask2D clipper, List<RectMask2D> masks)
        {
            masks.Clear();
            masks.AddRange(externalMasks);
            List<Canvas> canvasComponents = ListPool<Canvas>.Get();
            List<RectMask2D> rectMaskComponents = ListPool<RectMask2D>.Get();
            clipper.transform.GetComponentsInParent(false, rectMaskComponents);

            if (rectMaskComponents.Count > 0)
            {
                clipper.transform.GetComponentsInParent(false, canvasComponents);
                for (int i = rectMaskComponents.Count - 1; i >= 0; i--)
                {
                    if (!rectMaskComponents[i].IsActive())
                        continue;
                    bool shouldAdd = true;
                    for (int j = canvasComponents.Count - 1; j >= 0; j--)
                    {
                        if (!IsDescendantOrSelf(canvasComponents[j].transform, rectMaskComponents[i].transform) &&
                            canvasComponents[j].overrideSorting)
                        {
                            shouldAdd = false;
                            break;
                        }
                    }

                    if (shouldAdd)
                        masks.Add(rectMaskComponents[i]);
                }
            }

            ListPool<RectMask2D>.Release(rectMaskComponents);
            ListPool<Canvas>.Release(canvasComponents);
        }

        private bool IsDescendantOrSelf(Transform father, Transform child)
        {
            if (father == null || child == null)
                return false;

            if (father == child)
                return true;

            while (child.parent != null)
            {
                if (child.parent == father)
                    return true;

                child = child.parent;
            }

            return false;
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(ExternalRectMask2D), true)]
    [CanEditMultipleObjects]
    public class ExternalRectMask2DEditor : RectMask2DEditor
    {
        SerializedProperty pathToObject;
        SerializedProperty externalMasks;
        protected override void OnEnable()
        {
            base.OnEnable();
            pathToObject = serializedObject.FindProperty("pathToObject");
            externalMasks = serializedObject.FindProperty("externalMasks");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUILayout.PropertyField(pathToObject);
            EditorGUILayout.PropertyField(externalMasks);
            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}