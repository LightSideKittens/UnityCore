using LSCore.Extensions.Unity;
using UnityEngine;
using UnityEngine.EventSystems;

public class Joystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public Vector2 Direction { get; private set; }
    public bool IsUsing { get; private set; }
    [SerializeField] private RectTransform area;
    [SerializeField] private RectTransform handleArea;
    [SerializeField] private RectTransform handle;
    [SerializeField] private CanvasGroup group;
    [SerializeField] private GameObject[] directions;

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

    public void OnPointerDown(PointerEventData eventData)
    {
        IsUsing = true;
        group.alpha = 1;
        startTouchPosition = eventData.position;
        handleArea.SetPositionByScreenPoint(area, eventData.position, canvas);
        handle.localPosition = Vector3.zero;
    }

    public void OnDrag(PointerEventData eventData)
    {
        Direction = (eventData.position - startTouchPosition).normalized;
        var position = handleArea.GetLocalPositionByScreenPoint(eventData.position, canvas);
        handle.localPosition = Vector2.ClampMagnitude(position, maxRadius);
        SetupDirections();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        IsUsing = false;
        group.alpha = 0.5f;
        handle.localPosition = Vector3.zero;
        Direction = Vector2.zero;
        handleArea.localPosition = handleAreaStartPosition;
        directions[lastFactor].SetActive(false);
    }

    private void SetupDirections()
    {
        var factor = Direction.DetermineQuadrant();
        if (lastFactor == factor) return;
        
        directions[factor].SetActive(true);
        directions[lastFactor].SetActive(false);
        lastFactor = factor;
    }
}