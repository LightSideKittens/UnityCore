using System;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor;
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.EventSystems;

namespace LSCore
{
    public class LSButton : LSImage,  IPointerDownHandler, IPointerUpHandler, IPointerClickHandler, IClickable
    {
        [SerializeField] private ClickAnim anim;
        [SerializeReference] public List<LSAction> clickActions;
        [HideInInspector] [SerializeField] private bool isClickSoundOverride;
        
        public ref ClickAnim Anim => ref anim;

        protected override void Awake()
        {
            base.Awake();
            anim.Init(transform);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            clickActions.Invoke();
            anim.OnPointerClick();
            Clicked?.Invoke();
        }

        public void OnPointerDown(PointerEventData eventData) => anim.OnPointerDown();
        public void OnPointerUp(PointerEventData eventData) => anim.OnPointerUp();

        protected override void OnDisable()
        {
            base.OnDisable();
            anim.OnDisable();
        }

        public override void OnBeforeSerialize()
        {
            base.OnBeforeSerialize();
#if UNITY_EDITOR
            if (!World.IsPlaying)
            {
                isClickSoundOverride = clickActions?.Any(x => x is PlayOneShotSound) ?? false;
            }
#endif
        }

        public override void OnAfterDeserialize()
        {
            base.OnAfterDeserialize();
            if (World.IsPlaying)
            {
                if (!isClickSoundOverride)
                {
                    var action = new PlayOneShotSound();
                    var settings = new LaLaLa.Settings();
                    action.settings = settings;
                    settings.Clip = SingleAsset<AudioClip>.Get("ButtonClick");
                    settings.Group = SingleAsset<AudioMixerGroup>.Get("AudioMixer[UI]");
                    
                    clickActions.Add(action);
                }
            }
        }

        public Transform Transform => transform;
        public Action Clicked { get; set; }
    }
    
#if UNITY_EDITOR
    [CustomEditor(typeof(LSButton), true)]
    [CanEditMultipleObjects]
    public class LSButtonEditor : LSImageEditor
    {
        private LSButton button;
        private PropertyTree propertyTree;
        private InspectorProperty clickActions;
        protected override void OnEnable()
        {
            base.OnEnable();
            button = (LSButton)target;
            propertyTree = PropertyTree.Create(serializedObject);
            clickActions = propertyTree.RootProperty.Children["clickActions"];
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            propertyTree?.Dispose();
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            propertyTree.BeginDraw(true);
            clickActions.Draw();
            propertyTree.EndDraw();
            serializedObject.ApplyModifiedProperties();
        }

        protected override void DrawRotateButton()
        {
            EditorGUI.BeginChangeCheck();
            button.Anim.Editor_Draw();
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(target);
            }
            base.DrawRotateButton();
        }
    }
#endif
}