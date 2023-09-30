using System;
using System.Collections.Generic;
using DG.Tweening;
using LSCore.AnimationsModule.Animations;
using LSCore.AnimationsModule.Animations.Options;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

namespace LSCore.AnimationsModule
{
    [Serializable]
    public class AnimSequencer : ISerializationCallbackReceiver
    {
        [Serializable]
        private struct AnimData
        {
            [TableColumnWidth(60, false)]
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
        
        [SerializeReference] private IOptions[] options;
        
        [TableList]
        [SerializeField] private AnimData[] animsData;

        private Dictionary<string, BaseAnim> animsById = new();

        public BaseAnim this[string id] => animsById[id];

#if UNITY_EDITOR
        [OnInspectorGUI]
        private void OnGUI()
        {
            if (animsData is {Length: > 0})
            {
                var currentTime = 0f;
                
                for (int i = 0; i < animsData.Length; i++)
                {
                    var data = animsData[i];
                    currentTime += data.timeOffset;
                    data.time = currentTime;
                    animsData[i] = data;
                }

                totalTime = currentTime;
            }
        }

        [Button("Animate")]
        public void Editor_Animate()
        {
            if (Application.isPlaying)
            {
                Animate();
            }
        }
#endif

        public void InitAllAnims()
        {
            for (int i = 0; i < animsData.Length; i++)
            {
                animsData[i].anim.TryInit();
            }
        }

        public Sequence Animate()
        {
            var currentTime = 0f;
            var sequence = DOTween.Sequence().SetId(this);

            for (int i = 0; i < animsData.Length; i++)
            {
                var data = animsData[i];
                var anim = data.anim;
                anim.TryInit();
                
                if (!anim.IsDurationZero)
                {
                    currentTime += data.timeOffset;
                    sequence.Insert(currentTime, anim.Animate());
                }
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
            if (animsData != null && World.IsPlaying)
            {
                for (int i = 0; i < animsData.Length; i++)
                {
                    var data = animsData[i];
                    var id = data.anim.id;
                    id = string.IsNullOrEmpty(id) ? Random.Range(0, int.MaxValue).ToString() : id;
                    animsById.Add(id, data.anim);
                }
            }
        }
    }
}