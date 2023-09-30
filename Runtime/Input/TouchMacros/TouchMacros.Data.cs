using System;
using Sirenix.OdinInspector;
using UnityEngine;

public partial class TouchMacros
{
    [Serializable]
    private struct Data
    {
        public Key key;
        [Range(0, 1)] public float xPosition;
        [Range(0, 1)] public float yPosition;
        public Vector2 cursorPosition;
        public bool isTouching;
        
        public string KeyLabel
        {
            get
            {
                if (labelByKey.TryGetValue(key, out var label))
                {
                    return label;
                }

                return key.ToString();
            }
        }
        
        [Button]
        private void SetCursor()
        { 
            SetCursorPos(cursorPosition);
        }
    }
}