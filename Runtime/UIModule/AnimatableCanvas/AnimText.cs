using System;
using DG.Tweening;
using LSCore;
using TMPro;
using UnityEngine;
using static Animatable.AnimatableCanvas;

namespace Animatable
{
    [Serializable]
    public class AnimText
    {
        [SerializeField] private TMP_Text text;
        [SerializeField] private Vector2 animOffset = new Vector2(0, 100);
        [SerializeField] private float duration = 1;
        private OnOffPool<TMP_Text> pool;

        internal void Init() => pool = new OnOffPool<TMP_Text>(text, shouldStoreActive: true);
        internal void ReleaseAll() => pool.ReleaseAll();

        public static AnimText Create(string message, Vector2 pos = default, Vector2 offset = default, bool fromWorldSpace = false)
        {
            var template = AnimatableCanvas.AnimText;

            if (fromWorldSpace)
            {
                pos = GetLocalPosition(pos + offset);
            }

            var scale = template.text.transform.localScale;
            var text = template.pool.Get();
            var textTransform = text.transform;
            textTransform.SetParent(SpawnPoint, true);
            textTransform.position = pos;
            textTransform.localScale = scale;
            text.text = message;
            var rect = (RectTransform) text.transform;
            text.alpha = 1;
            rect.localScale = Vector3.zero;
            
            var sequence = DOTween.Sequence();
            sequence.Insert(0, rect.DOMove(rect.position + (Vector3)template.animOffset, template.duration * 1.5f));
            sequence.Insert(0,rect.DOScale(1, template.duration * 0.2f));
            sequence.Insert(0,text.DOFade(0, template.duration).SetEase(Ease.InExpo));
            sequence.OnComplete(() => template.pool.Release(text));

            return new AnimText
            {
                text = text
            };
        }
    }
}