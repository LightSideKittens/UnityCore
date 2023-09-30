using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LSCore.UIModule
{
    public class ScrollViewSwiper : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler, IPointerClickHandler
    {
        [Serializable]
        public enum ScrollType
        {
            Vertical,
            Horizontal
        }

        [Serializable]
        public struct ObjectActivateOnDragData
        {
            public bool needSetActiveObjectOnDrag;
            public GameObject obj;
        }

        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private Transform parent;
        [SerializeField] private ScrollType type;
        [SerializeField] private float velocityThreshold;
        [SerializeField] private ObjectActivateOnDragData objectActivateData;

        private InFunc<Vector2, float> valueGetter;
        private float prevValue;
        private float currentValue;
        private Tween currentTween;
        private bool isDragging;
        private bool isActivated;
        private float lastYDelta;

        
        private ScrollRect[] childsScrollRects;
        private int currentChildScrollRectIndex;

        private bool isChildScrollVertical;

        private void Awake()
        {
            scrollRect.onValueChanged.AddListener(OnValueChanged);
            childsScrollRects = parent.GetComponentsInChildren<ScrollRect>();

            SetNewChildRect();
        
            if (type == ScrollType.Horizontal)
            {
                valueGetter = GetXValue;
            }
            else
            {
                valueGetter = GetYValue;
            }
        }

        private void Start()
        {
            for (int i = 0; i < childsScrollRects.Length; i++)
            {
                childsScrollRects[i].GetComponent<LayoutElement>().preferredWidth = scrollRect.viewport.rect.width;
                childsScrollRects[i].GetComponent<LayoutElement>().preferredHeight = scrollRect.viewport.rect.height;
            }
        }

        [SerializeField] private  GraphicRaycaster raycaster;
        PointerEventData m_PointerEventData;

        private float GetXValue(in Vector2 value) => value.x;
        private float GetYValue(in Vector2 value) => value.y;

        private void OnValueChanged(Vector2 value)
        {
            currentValue = valueGetter(value);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            isDragging = true;
            if (objectActivateData.needSetActiveObjectOnDrag)
            {
                isActivated = false;
            }
        
            if (currentTween != null)
            {
                currentTween.Kill();
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (isActivated)
            {

                if (isChildScrollVertical)
                {
                    childsScrollRects[currentChildScrollRectIndex].verticalNormalizedPosition -=
                        eventData.delta.y / (childsScrollRects[currentChildScrollRectIndex].content.rect.height);
                }
                else
                {
                    childsScrollRects[currentChildScrollRectIndex].horizontalNormalizedPosition -=
                        eventData.delta.x / (childsScrollRects[currentChildScrollRectIndex].content.rect.width);
                }

                switch (type)
                {
                    case ScrollType.Horizontal:
                        scrollRect.horizontalNormalizedPosition = prevValue;
                        break;
                    case ScrollType.Vertical:
                        scrollRect.verticalNormalizedPosition = prevValue;
                        break;
                }
                return;
            }
        
            if (objectActivateData.needSetActiveObjectOnDrag)
            {
                switch (type)
                {
                    case ScrollType.Horizontal:
                        if (Mathf.Abs(eventData.delta.y) >= Mathf.Abs(eventData.delta.x))
                        {
                            ChangeSelectedObj();
                            isActivated = true;
                        }
                        break;
                    case ScrollType.Vertical:
                        if (Mathf.Abs(eventData.delta.x) >= Mathf.Abs(eventData.delta.y))
                        {
                            ChangeSelectedObj();
                            isActivated = true;
                        }
                        break;
                }
            }

            lastYDelta = eventData.delta.y;
        }

        private void ChangeSelectedObj()
        {
            SwipePage(false);
            isActivated = true;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            isDragging = false;
            if (isActivated)
            {
                if (isChildScrollVertical)
                {
                    childsScrollRects[currentChildScrollRectIndex].velocity = Vector2.up * 20 * lastYDelta;
                }
                else
                {
                
                    childsScrollRects[currentChildScrollRectIndex].velocity = Vector2.right * 20 * lastYDelta;
                }
                return;
            }

            SwipePage();
        }

        private void SwipePage(bool withCurrentValue = true)
        {
            var k = (float) 1 / (parent.childCount - 1);
            if (withCurrentValue && (Mathf.Abs(valueGetter(scrollRect.velocity)) > velocityThreshold ||
                Mathf.Abs(currentValue - prevValue) > k / 2))
            {
                var newValue = currentValue - prevValue > 0 ? 1 : -1;
            
                currentChildScrollRectIndex += newValue;
                SetNewChildRect();
            
                prevValue += k * newValue;
                newValue = Mathf.Clamp(newValue, 0, 1);
                prevValue = newValue;   
            }

            switch (type)
            {
                case ScrollType.Horizontal:
                    currentTween = scrollRect.DOVerticalNormalizedPos(prevValue, 0.5f);
                    break;
                case ScrollType.Vertical:
                    currentTween = scrollRect.DOVerticalNormalizedPos(prevValue, 0.5f);
                    break;
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (isDragging)
            {
                return;
            }
        
            objectActivateData.obj.SetActive(false);
        
            m_PointerEventData = new PointerEventData(EventSystem.current);
            m_PointerEventData.position = Input.mousePosition;

            List<RaycastResult> results = new List<RaycastResult>();

            raycaster.Raycast(m_PointerEventData, results);

            foreach (RaycastResult result in results)
            {
                ExecuteEvents.Execute (result.gameObject , m_PointerEventData , ExecuteEvents.pointerClickHandler);
            }
        
            objectActivateData.obj.SetActive(true);
        }

        private void SetNewChildRect()
        {
            currentChildScrollRectIndex = Mathf.Clamp(currentChildScrollRectIndex, 0, childsScrollRects.Length - 1);
            isChildScrollVertical = childsScrollRects[currentChildScrollRectIndex].vertical;
        }
    }
}
