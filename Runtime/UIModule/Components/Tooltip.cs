using LSCore.Extensions.Unity;
using Sirenix.OdinInspector;
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
        public float defaultPointOffset;

        [Button]
        public void ShowTooltip(Vector3 worldPoint, string message)
        {
            tooltipText.text = message;
            LayoutRebuilder.ForceRebuildLayoutImmediate(tooltipContainer);
            Rect cameraBounds = mainCamera.GetRect();
            tooltipContainer.gameObject.SetActive(true);
            Vector2 pos = mainCamera.transform.InverseTransformPoint(GetProjectionPointOnCameraPlane(worldPoint));
            
            if (pos.y >= 0)
            {
                
            }
            else if (pos.y < 0)
            {
                
            }
            else if (pos.x > 0)
            {
                
            }
            else
            {
                
            }
            
            Vector2 tooltipPosition = GetBestTooltipPosition(cameraBounds, worldPoint);
            tooltipContainer.position = tooltipPosition;
            
            PositionPointer(worldPoint, tooltipPosition);
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

        private void PositionPointer(Vector2 screenPoint, Vector2 tooltipPosition)
        {
            // Определить направление поинтера
            Vector2 direction = screenPoint - tooltipPosition;

            // Задать вращение и позицию поинтера относительно тултипа
            pointer.anchoredPosition = direction.normalized * (pointer.sizeDelta.x / 2); // Сдвиг указателя
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            pointer.rotation = Quaternion.Euler(0, 0, angle);
        }

        public void HideTooltip()
        {
            tooltipContainer.gameObject.SetActive(false);
        }
    }
}