using System;
using DG.Tweening;
using LSCore;
using TMPro;
using UnityEngine;
using static Animatable.AnimatableCanvas;
using Object = UnityEngine.Object;

namespace Animatable
{
    [Serializable]
    public class PopupTextAction : LSAction
    {
        public TMP_Text messagePrefab;
            
        public override void Invoke()
        {
            PopupText.Create(messagePrefab);
        }
    }

    [Serializable]
    public class PopupText
    {
        [SerializeField] private CanvasGroup group;
        [SerializeField] private Vector2 animOffset = new Vector2(0, 100);
        [SerializeField] private float duration = 1;
        private OnOffPool<CanvasGroup> pool;

        internal void Init()
        {
            pool = new OnOffPool<CanvasGroup>(group, shouldStoreActive: true);
        }

        internal void ReleaseAll()
        {
            pool.ReleaseAll();
        }

        public static PopupText Create(TMP_Text message, Vector2 pos = default, Vector2 offset = default, bool fromWorldSpace = false)
        {
            var template = AnimatableCanvas.PopupText;

            if (fromWorldSpace)
            {
                pos = GetLocalPosition(pos + offset);
            }

            var scale = template.group.transform.localScale;
            var group = template.pool.Get();
            var text = Object.Instantiate(message, group.transform);
            var groupTransform = group.transform;
            groupTransform.SetParent(SpawnPoint, true);
            groupTransform.position = pos;
            groupTransform.localScale = scale;
            var rect = (RectTransform) group.transform;
            group.alpha = 1;
            rect.localScale = Vector3.zero;
            
            var sequence = DOTween.Sequence();
            sequence.Insert(0, rect.DOMove(rect.position + (Vector3)template.animOffset, template.duration * 1.5f));
            sequence.Insert(0,rect.DOScale(1, template.duration * 0.2f));
            sequence.Insert(0,group.DOFade(0, template.duration).SetEase(Ease.InExpo));
            sequence.OnComplete(() =>
            {
                template.pool.Release(group);
                Object.Destroy(text.gameObject);
            });

            return new PopupText
            {
                group = group
            };
        }
    }
}