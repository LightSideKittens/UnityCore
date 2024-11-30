using System;
using DG.Tweening;
using LSCore.Extensions.Unity;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.UI.GridLayoutGroup;
using Vector3 = UnityEngine.Vector3;

namespace LSCore
{
    [Serializable]
    public class ShowTooltip : CreateSinglePrefab<Tooltip>
    {
        public Transform target;
        public string message;
        
        public override void Invoke()
        {
            base.Invoke();
            obj.Show(target, message);
        }
    }

    [ExecuteAlways]
    public class Tooltip : MonoBehaviour
    {
        [Serializable]
        public struct PointerData
        {
            public Sprite top;
            public Sprite bottom;
            public Sprite left;
            public Sprite right;

            public void SetupImage(Image target, Direction direction)
            {
                var sprite = direction switch
                {
                    Direction.Left => left,
                    Direction.Right => right,
                    Direction.Top => top,
                    Direction.Bottom => bottom,
                };
                target.sprite = sprite; 
            }
        }
        
        [SerializeField] private RectTransform tooltipContainer;
        [SerializeField] private LSText tooltipText;
        [SerializeField] private Image pointerImage;
        [SerializeField] private PointerData pointerData;
        [SerializeField] private RectTransform pointer;
        [SerializeField] private float defaultPointOffset;
        [SerializeField] private float defaultTooltipContainerOffset;
        [SerializeField] private float pointerSideOffset;
        [SerializeField] private float canvasSizeOffset;
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private CustomContentSizeFitter sizeFitter;
        [SerializeField] private float maxHeight = 500;
        [SerializeReference] public ShowHideAnim showHideAnim = new DefaultUIViewAnimation();
        
        private Direction direction;
        private Vector3[] tooltipCorners = new Vector3[4];
        private Vector3[] canvasCorners = new Vector3[4];
        private Camera mainCamera;
        private Canvas canvas;
        private LSButton backButton;
        private GraphicRaycaster raycaster;
        private Tween currentTween;
        
        private float OffsetFactor => canvas.transform.localScale.x;

        private void Awake()
        {
#if UNITY_EDITOR
            if (World.IsEditMode)
            {
                return;
            }
#endif
            sizeFitter.SizeChanged += OnSizeChanged;
            mainCamera = Camera.main;
            canvas = new GameObject(name).AddComponent<Canvas>();
            backButton = canvas.gameObject.AddComponent<LSButton>();
            backButton.color = Color.clear;
            backButton.Clicked += Hide;
            raycaster = canvas.gameObject.AddComponent<GraphicRaycaster>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.planeDistance = 10;
            canvas.worldCamera = mainCamera;
            canvas.sortingOrder = WindowsData.DefaultSortingOrder + 100;
            transform.SetParent(canvas.transform);
            transform.localScale = Vector3.one;
            transform.localRotation = Quaternion.identity;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (sizeFitter != null)
            {
                sizeFitter.SizeChanged -= OnSizeChanged;
                sizeFitter.SizeChanged += OnSizeChanged;
            }
        }
#endif
        
        private void OnSizeChanged(Vector2 size)
        {
            size.y = Mathf.Clamp(size.y, 0, maxHeight);
            tooltipContainer.sizeDelta = size;
        }
        
        public void Hide()
        {
            raycaster.enabled = false;
            currentTween?.Kill();
            currentTween = showHideAnim.Hide();
        }
        
        [Button]
        public void EditorShow(Transform worldPoint, string message)
        {
            mainCamera = Camera.main;
            canvas = ((RectTransform)transform).root.GetComponent<Canvas>();
            Show(worldPoint.position, message);
        }
        
        public void Show(Transform worldPoint, string message)
        {
            Show(worldPoint.position, message);
        }
        
        public void Show(Vector3 worldPoint, string message)
        {
            raycaster.enabled = true;
            transform.localScale = Vector3.one;
            transform.localRotation = Quaternion.identity;
            
            currentTween?.Kill();
            currentTween = showHideAnim.Show();
            scrollRect.verticalNormalizedPosition = 1;

            var contentRect = scrollRect.content.rect;
            var tooltipContainerRect = tooltipContainer.rect;
            var vertical = contentRect.height > scrollRect.viewport.rect.height + 0.1f;
            var verticalOffset = 0f;

            if (!vertical)
            {
                verticalOffset = (tooltipContainerRect.height - contentRect.height) / 2;
            }
            
            tooltipText.text = message;
            LayoutRebuilder.ForceRebuildLayoutImmediate(scrollRect.content);
            scrollRect.vertical = vertical;
            
            Rect cameraBounds = GetRect();
            
            var f = OffsetFactor;
            var defaultPointOffset = this.defaultPointOffset * f;
            var defaultTooltipContainerOffset = this.defaultTooltipContainerOffset * f;
            var pointerSideOffset = this.pointerSideOffset;
            
            tooltipContainer.gameObject.SetActive(true);
            var plane = mainCamera.PlaneAt(canvas.planeDistance);
            Vector3 point;
            
            if (mainCamera.orthographic)
            {
                point = plane.ClosestPointOnPlane(worldPoint);
            }
            else
            {
                var camPos = mainCamera.transform.position;
                var forward = mainCamera.transform.forward;
            
                var dir = worldPoint - camPos;
                var dot = Vector3.Dot(forward, dir);

                if (dot < 0)
                {
                    var forwardComponent = dot * forward;
                    dir -= 2 * forwardComponent;
                }

                var ray = new Ray(camPos, dir);
                plane.Raycast(ray, out float e);
                dir = dir.normalized;
                point = camPos + dir * e;
            }

            UpdateCanvasCorners();
            Vector3 canvasMin = canvasCorners[0];
            Vector3 canvasMax = canvasCorners[2];
            
            Vector3 pos = canvas.transform.InverseTransformPoint(point);

            var side = DetermineQuadrant(pos, cameraBounds.width, cameraBounds.height);
            pointerData.SetupImage(pointerImage, side);
            
            if (pos.x < canvasMin.x) pos.x = canvasMin.x;
            if (pos.x > canvasMax.x) pos.x = canvasMax.x;
            if (pos.y < canvasMin.y) pos.y = canvasMin.y;
            if (pos.y > canvasMax.y) pos.y = canvasMax.y;

            pos = canvas.transform.TransformPoint(pos);
            
            var sideDir = GetDir(side);
            var axis = GetSideAxis(side);
            
            pos -= sideDir * defaultTooltipContainerOffset;
            tooltipContainer.pivot = GetPivot(side);
            tooltipContainer.position = pos;
            Vector3 totalOffset = GetOffsetForTooltipContainer();
            if (side == Direction.Top) totalOffset.y += verticalOffset;
            if (side == Direction.Bottom) totalOffset.y -= verticalOffset;
            tooltipContainer.localPosition += totalOffset;
            
            tooltipContainer.GetWorldCorners(tooltipCorners);
            canvas.transform.InverseTransformPoints(tooltipCorners);

            Vector3 tooltipMin = tooltipCorners[0];
            Vector3 tooltipMax = tooltipCorners[2];
            
            pos = canvas.transform.InverseTransformPoint(pos);

            if (axis == Axis.Vertical)
            {
                if (pos.x < tooltipMin.x + pointerSideOffset) pos.x = tooltipMin.x + pointerSideOffset;
                if (pos.x > tooltipMax.x - pointerSideOffset) pos.x = tooltipMax.x - pointerSideOffset;
            }
            else if (axis == Axis.Horizontal)
            {
                if (pos.y < tooltipMin.y + pointerSideOffset) pos.y = tooltipMin.y + pointerSideOffset;
                if (pos.y > tooltipMax.y - pointerSideOffset) pos.y = tooltipMax.y - pointerSideOffset;
            }
            
            
            pos = canvas.transform.TransformPoint(pos);
            pos -= sideDir * defaultPointOffset;
            pointer.localEulerAngles = new Vector3(0, 0, GetRot(side));
            pointer.position = pos;
        }

        private void UpdateCanvasCorners()
        {
            var canvasRect = (RectTransform)canvas.transform;
            canvasRect.GetWorldCorners(canvasCorners);
            canvas.transform.InverseTransformPoints(canvasCorners);
            
            canvasCorners[0] += new Vector3(canvasSizeOffset, canvasSizeOffset);
            canvasCorners[1] += new Vector3(canvasSizeOffset, -canvasSizeOffset);
            canvasCorners[2] -= new Vector3(canvasSizeOffset, canvasSizeOffset);
            canvasCorners[3] += new Vector3(-canvasSizeOffset, canvasSizeOffset);
        }

        private Vector3 GetOffsetForTooltipContainer()
        {
            tooltipContainer.GetWorldCorners(tooltipCorners);
            canvas.transform.InverseTransformPoints(tooltipCorners);
            
            Vector3 canvasMin = canvasCorners[0];
            Vector3 canvasMax = canvasCorners[2];
            
            Vector3 tooltipMin = tooltipCorners[0];
            Vector3 tooltipMax = tooltipCorners[2];
            
            Vector3 totalOffset = Vector3.zero;
            
            if (tooltipMin.x < canvasMin.x) totalOffset.x += canvasMin.x - tooltipMin.x;
            if (tooltipMax.x > canvasMax.x) totalOffset.x += canvasMax.x - tooltipMax.x;
            if (tooltipMin.y < canvasMin.y) totalOffset.y += canvasMin.y - tooltipMin.y;
            if (tooltipMax.y > canvasMax.y) totalOffset.y += canvasMax.y - tooltipMax.y;
            
            return totalOffset;
        }

        public Axis GetSideAxis(Direction side)
        {
            if (side is Direction.Bottom or Direction.Top) return Axis.Vertical;
            if (side is Direction.Left or Direction.Right) return Axis.Horizontal;
            return default;
        }

        private Rect GetRect()
        {
            if (mainCamera.orthographic)
            {
                return mainCamera.GetRect();
            }
            
            return mainCamera.GetRect(canvas.planeDistance);
        }
        
        private Vector3 GetDir(Direction direction)
        {
            var tr = mainCamera.transform;
            return direction switch
            {
                Direction.Left => -tr.right,
                Direction.Right => tr.right,
                Direction.Top => tr.up,
                Direction.Bottom => -tr.up
            };
        }
        
        private static Vector2 GetPivot(Direction direction)
        {
            return direction switch
            {
                Direction.Left => new Vector2(0, 0.5f),
                Direction.Right => new Vector2(1, 0.5f),
                Direction.Top => new Vector2(0.5f, 1f),
                Direction.Bottom => new Vector2(0.5f, 0f),
            };
        }
        
        private static float GetRot(Direction direction)
        {
            return direction switch
            {
                Direction.Left => 90,
                Direction.Right => -90,
                Direction.Top => 0,
                Direction.Bottom => 180
            };
        }

        private static Direction DetermineQuadrant(Vector2 localPoint, float cameraWidth, float cameraHeight)
        {
            var xNorm = localPoint.x / (cameraWidth / 2);
            var yNorm = localPoint.y / (cameraHeight / 2);
            
            if (yNorm > xNorm && yNorm > -xNorm)
            {
                return Direction.Top;
            }

            if (yNorm > xNorm && yNorm < -xNorm)
            {
                return Direction.Left;
            }
            
            if (yNorm < xNorm && yNorm > -xNorm)
            {
                return Direction.Right;
            }
            
            return Direction.Bottom;
        }
    }
}