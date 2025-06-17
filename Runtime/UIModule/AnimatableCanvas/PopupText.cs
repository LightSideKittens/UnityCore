﻿using System;
using DG.Tweening;
using LSCore;
using TMPro;
using UnityEngine;
using static Animatable.AnimatableCanvas;
using Object = UnityEngine.Object;

namespace Animatable
{
    [Serializable]
    public class PopupTextAction : DoIt
    {
        public TMP_Text messagePrefab;
            
        public override void Do()
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
            var groupTransform = (RectTransform)group.transform;
            groupTransform.SetParent(SpawnPoint, true);
            groupTransform.localPosition = pos;
            groupTransform.localScale = scale;
            groupTransform.localRotation = Quaternion.identity;
            
            group.alpha = 1;
            groupTransform.localScale = Vector3.zero;
            
            var sequence = DOTween.Sequence();
            sequence.Insert(0, groupTransform.DOLocalMove(groupTransform.localPosition + (Vector3)template.animOffset, template.duration * 1.5f));
            sequence.Insert(0,groupTransform.DOScale(1, template.duration * 0.2f));
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