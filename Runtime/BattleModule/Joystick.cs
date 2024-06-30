using System;
using LSCore.Extensions.Unity;
using UnityEngine;
using UnityEngine.EventSystems;

public class Joystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [Serializable]
    private struct KeysData
    {
        public KeyCode Up;
        public KeyCode Right;
        public KeyCode Down;
        public KeyCode Left;
    }

    [SerializeField] private KeysData keysData;
    public Vector2 Direction { get; private set; }
    public ReactProp<float> Magnitude { get; private set; } = new();
    public ReactProp<bool> IsUsing { get; set; } = new();
    [SerializeField] private RectTransform area;
    [SerializeField] private RectTransform handleArea;
    [SerializeField] private RectTransform handle;
    [SerializeField] private CanvasGroup group;
    [SerializeField] private GameObject[] directions;

    private bool isUsingByTouch;
    private int lastFactor;
    private float maxRadius;
    private Canvas canvas;
    private Vector2 startTouchPosition;
    private Vector2 handleAreaStartPosition;
    
    protected virtual void Start()
    {
        var size = handleArea.rect;
        maxRadius = Mathf.Min(size.width / 2, size.height / 2);
        canvas = GetComponentInParent<Canvas>();
        handleAreaStartPosition = handleArea.localPosition;
        group.alpha = 0.5f;
    }

    private void Update()
    {
        if(isUsingByTouch) return;
        bool isUsing = false;
        
        if (Input.GetKey(keysData.Up))
        {
           Direction = Vector2.up;
           isUsing = true;
        }
        
        if (Input.GetKey(keysData.Left))
        {
            Direction += Vector2.left;
            isUsing = true;
        }
            
        if (Input.GetKey(keysData.Down))
        {
            Direction += Vector2.down;
            isUsing = true;
        }
            
        if (Input.GetKey(keysData.Right))
        {
            Direction += Vector2.right;
            isUsing = true;
        }

        if (isUsing)
        {
            Direction = Direction.normalized;
        }

        IsUsing.Value = isUsing;
        Magnitude.Value = isUsing ? 1 : 0;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isUsingByTouch = true;
        IsUsing.Value = true;
        group.alpha = 1;
        startTouchPosition = eventData.position;
        handleArea.SetPositionByScreenPoint(area, eventData.position, canvas);
        handle.localPosition = Vector3.zero;
    }

    public void OnDrag(PointerEventData eventData)
    {
        Direction = (eventData.position - startTouchPosition).normalized;
        var position = handleArea.GetLocalPositionByScreenPoint(eventData.position, canvas);
        Magnitude.Value = Mathf.InverseLerp(0, maxRadius, position.magnitude);
        handle.localPosition = Vector2.ClampMagnitude(position, maxRadius);
        SetupDirections();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isUsingByTouch = false;
        IsUsing.Value = false;
        group.alpha = 0.5f;
        handle.localPosition = Vector3.zero;
        handleArea.localPosition = handleAreaStartPosition;
        
        if (directions.Length > 3)
        {
            directions[lastFactor].SetActive(false);
        }
    }

    private void SetupDirections()
    {
        if (directions.Length < 4)
        {
            return;
        }
        
        var factor = Direction.DetermineQuadrant();
        if (lastFactor == factor) return;
        
        directions[factor].SetActive(true);
        directions[lastFactor].SetActive(false);
        lastFactor = factor;
    }
}