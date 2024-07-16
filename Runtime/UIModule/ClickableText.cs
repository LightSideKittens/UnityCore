using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.EventSystems;

#if UNITY_EDITOR
using UnityEditor;
using Sirenix.OdinInspector.Editor;
#endif


namespace LSCore
{
    public class ClickableText : LSText, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler, IClickable, ISerializationCallbackReceiver
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

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
        }
#endif
        
        public void OnBeforeSerialize()
        {
#if UNITY_EDITOR
            if (!World.IsPlaying)
            {
                isClickSoundOverride = clickActions?.Any(x => x is PlayOneShotSound) ?? false;
            }
#endif
        }

        public void OnAfterDeserialize()
        {
            if (World.IsPlaying)
            {
#if UNITY_EDITOR
                isClickSoundOverride = clickActions?.Any(x => x is PlayOneShotSound) ?? false;
#endif
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
        
        public void OnPointerClick(PointerEventData eventData)
        {
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
        
        public Transform Transform => transform;
        public Action Clicked { get; set; }
    }
    
#if UNITY_EDITOR
    [CustomEditor(typeof(ClickableText), true), CanEditMultipleObjects]
    public class ClickableTextEditor : LSTextEditor
    {
        ClickableText text;
        private InspectorProperty clickActions;
        
        protected override void OnEnable()
        {
            base.OnEnable();
            text = (ClickableText)target;
            clickActions = propertyTree.RootProperty.Children["clickActions"];
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            propertyTree.BeginDraw(true);
            clickActions.Draw();
            propertyTree.EndDraw();
        }

        protected override void DrawExtraSettings()
        {
            base.DrawExtraSettings();
            text.Anim.Editor_Draw();
        }
    }
#endif
}