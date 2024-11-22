using System;
using LSCore.Extensions.Unity;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace LSCore
{
    public class Tooltip : MonoBehaviour
    {
        public RectTransform tooltipContainer;
        public LSText tooltipText;
        public RectTransform pointer;
        public Camera mainCamera;
        public Canvas canvas;
        public float defaultPointOffset;
        public float defaultTooltipContainerOffset;
        private Direction direction;

        [Button]
        public void ShowTooltip(Transform worldPoint, string message)
        {
            ShowTooltip(worldPoint.position, message);
        }
        

        [Button]
        public void ShowTooltip(Vector3 worldPoint, string message)
        {
            tooltipText.text = message;
            LayoutRebuilder.ForceRebuildLayoutImmediate(tooltipContainer);
            
            Rect cameraBounds = GetRect();
            tooltipContainer.gameObject.SetActive(true);
            var plane = mainCamera.PlaneAt(canvas.planeDistance);
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
            
            Vector3 pos = mainCamera.transform.InverseTransformPoint(camPos + dir * e);
            var side = DetermineQuadrant(pos, cameraBounds.width, cameraBounds.height);
            pos = mainCamera.transform.TransformPoint(pos);
            
            var sideDir = GetDir(side);

            pos -= sideDir * defaultTooltipContainerOffset;
            tooltipContainer.pivot = GetPivot(side);
            tooltipContainer.position = pos;

            pos -= sideDir * defaultPointOffset;
            pointer.localEulerAngles = new Vector3(0, 0, GetRot(side));
            pointer.position = pos;
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
        
        private Vector3 GetProjectionPointOnCameraPlane(Vector3 objectPosition)
        {
            var cameraNormal = mainCamera.transform.forward;
            var cameraPosition = mainCamera.transform.position;
            var objectToCamera = objectPosition - cameraPosition;
            var projectedVector = Vector3.ProjectOnPlane(objectToCamera, cameraNormal);
            var projectedPoint = cameraPosition + projectedVector;
            return projectedPoint;
        }

        private Vector2 GetBestTooltipPosition(Rect cameraBounds, Vector3 worldPoint)
        {
            // Получить размеры экрана
            float screenWidth = Screen.width;
            float screenHeight = Screen.height;

            // Размеры тултипа
            float tooltipWidth = tooltipContainer.sizeDelta.x;
            float tooltipHeight = tooltipContainer.sizeDelta.y;

            // Изначально тултип показывается справа и чуть выше точки
            Vector2 tooltipPosition = worldPoint + new Vector3(tooltipWidth / 2, tooltipHeight / 2);

            // Проверка, чтобы тултип не вышел за правую и верхнюю границы экрана
            if (tooltipPosition.x + tooltipWidth > screenWidth)
                tooltipPosition.x = worldPoint.x - tooltipWidth / 2; // Перемещаем тултип влево

            if (tooltipPosition.y + tooltipHeight > screenHeight)
                tooltipPosition.y = worldPoint.y - tooltipHeight / 2; // Перемещаем тултип вниз

            // Проверка левой и нижней границы
            if (tooltipPosition.x < 0)
                tooltipPosition.x = worldPoint.x + tooltipWidth / 2;

            if (tooltipPosition.y < 0)
                tooltipPosition.y = worldPoint.y + tooltipHeight / 2;

            return tooltipPosition;
        }

        private void PositionPointer(Rect cameraBounds, Vector3 worldPoint, int side)
        {
            
        }

        public void HideTooltip()
        {
            tooltipContainer.gameObject.SetActive(false);
        }
    }
}