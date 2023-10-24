using System;
using DG.Tweening;
using UnityEditor;
using UnityEngine;

namespace LSCore
{
    [Serializable]
    public struct ClickAnim
    {
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
        public void Editor_Draw()
        {
            isDisabled = !EditorGUILayout.Toggle("Is Animatable", !isDisabled);
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
        
        public void OnPointerClick()
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
        }
    }
}