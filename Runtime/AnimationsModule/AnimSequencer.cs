using System;
using System.Collections.Generic;
using DG.Tweening;
using LSCore.AnimationsModule.Animations;
using LSCore.AnimationsModule.Animations.Options;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace LSCore.AnimationsModule
{
    [Serializable]
    public class AnimSequencer : ISerializationCallbackReceiver
    {
        [Serializable]
        public struct AnimData
        {
            [TableColumnWidth(80, false)]
            public float timeOffset;
            
            [TableColumnWidth(60, false)]
            [NonSerialized]
            [ShowInInspector]
            [ReadOnly]
            public float time;

            [TableColumnWidth(400)]
            [SerializeReference] public BaseAnim anim;
        }

        [NonSerialized]
        [ShowInInspector]
        [ReadOnly]
        public float totalTime;
        
        [SerializeReference] private IOption[] options;
        
        [TableList]
        [SerializeField] private List<AnimData> animsData = new();

        private Dictionary<string, BaseAnim> animsById = new();
        private Dictionary<Type, List<BaseAnim>> animsByType = new();
        private Sequence sequence;

        public int Count => animsData.Count;
        
        public T GetAnim<T>(string id) where T : BaseAnim => (T)animsById[id]; 
        public T GetAnim<T>() where T : BaseAnim
        {
            return (T)animsByType[typeof(T)][0];
        }
        
        public bool TryGetAnim<T>(out T anim) where T : BaseAnim
        {
            if (animsByType.TryGetValue(typeof(T), out var anim1))
            {
                anim = (T)anim1[0];
                return true;
            }

            anim = null;
            return false;
        }

        public bool Contains<T>()
        {
            return animsByType.ContainsKey(typeof(T));
        }

#if UNITY_EDITOR
        [OnInspectorGUI]
        private void OnGUI()
        {
            if (animsData is {Count: > 0})
            {
                var currentTime = 0f;
                
                for (int i = 0; i < animsData.Count; i++)
                {
                    var data = animsData[i];
                    currentTime += data.timeOffset;
                    data.time = currentTime;
                    animsData[i] = data;
                }

                totalTime = currentTime;
            }
        }

        private bool isPlaying;
        private double lastTime;
        private Sequence editor_sequence;
        
        [Button("Play")]
        [HideIf("isPlaying")]
        public void Editor_Play()
        {
            editor_sequence = Animate().SetUpdate(UpdateType.Manual).SetAutoKill(false);
            isPlaying = true;
            lastTime = EditorApplication.timeSinceStartup;
            EditorApplication.update += Editor_Update;
        }
        
        [Button("Stop")]
        [ShowIf("isPlaying")]
        public void Editor_Stop()
        {
            isPlaying = false;
            EditorApplication.update -= Editor_Update;
            editor_sequence.Rewind(); 
        }

        private void Editor_Update()
        {
            var time = EditorApplication.timeSinceStartup;
            var dt = (float)(time - lastTime);
            editor_sequence.ManualUpdate(dt, dt);
            lastTime = time;
        }
#endif

        public void Init()
        {
            for (int i = 0; i < animsData.Count; i++)
            {
                animsData[i].anim.TryInit();
            }
        }

        public Sequence Animate()
        {
            var currentTime = 0f;
            if (sequence is { active: false })
            {
                sequence.Kill();
            }
            
            sequence = DOTween.Sequence().SetId(this);

            for (int i = 0; i < animsData.Count; i++)
            {
                var data = animsData[i];
                var anim = data.anim;
                currentTime += data.timeOffset;
                sequence.Insert(currentTime, anim.Animate());
            }

            if (options != null)
            {
                for (int i = 0; i < options.Length; i++)
                {
                    options[i].ApplyTo(sequence);
                }
            }

            return sequence;
        }

        public void Kill()
        {
            DOTween.Kill(this);
        }
        
        public void OnBeforeSerialize() { }

        public void OnAfterDeserialize()
        {
            if (animsData != null)
            {
                for (int i = 0; i < animsData.Count; i++)
                {
#if UNITY_EDITOR
                    if(animsData[i].anim == null) continue;    
#endif
                    Add(animsData[i].anim);
                }
            }
        }
        
        public void Add(AnimData data)
        {
            animsData.Add(data);
            Add(data.anim);
        }
        
        private void Add(BaseAnim anim)
        {
            var id = anim.id;
            if (!string.IsNullOrEmpty(id))
            {
                animsById.TryAdd(id, anim);
            }
            var type = anim.GetType();
            if (!animsByType.TryGetValue(type, out var list))
            {
                list = new List<BaseAnim>();
                animsByType.Add(type, list);
            }

            list.Add(anim);
        }
    }
}