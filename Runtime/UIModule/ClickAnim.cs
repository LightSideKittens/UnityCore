using System;
using DG.Tweening;
using LSCore.Attributes;
using Sirenix.OdinInspector;

#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;
using Object = UnityEngine.Object;

namespace LSCore
{
    [Unwrap]
    [Serializable]
    public struct ClickAnim
    {
        [CustomValueDrawer("DrawClickAnim")]
        [SerializeField] private bool isDisabled;
        private Transform transform;
        private Tween current;
        private Vector3 defaultScale;
        
        public bool IsDisabled => isDisabled;

        public void Init(Transform transform)
        {
            this.transform = transform;
        }

#if UNITY_EDITOR
        
        public bool DrawClickAnim(bool value, GUIContent _)
        {
            return !EditorGUILayout.Toggle("Is Animatable", !value);
        }
        
        public void Editor_Draw(Object target)
        {
            EditorGUI.BeginChangeCheck();
            isDisabled = !EditorGUILayout.Toggle("Is Animatable", !isDisabled);
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(target);
            }
        }
#endif
       
        public void SetActive(bool active) => isDisabled = active;
        
        public void OnPointerDown()
        {
            if(isDisabled) return;
            current.Complete();
            defaultScale = transform.localScale;
            current = transform.DOScale(defaultScale * 0.8f, 0.3f);
        }
        
        public void OnClick()
        {
            if(isDisabled) return;
            current.Kill();
            current = transform.DOScale(defaultScale, 0.5f).SetEase(Ease.OutElastic);
        }
        
        public void OnPointerUp()
        {
            if(isDisabled) return;
            current.Kill();
            current = transform.DOScale(defaultScale, 0.15f);
        }

        public void OnDisable()
        {
            current.Kill();
            if (current == null) return;
            transform.localScale = defaultScale;
            current = null;
        }
    }
}