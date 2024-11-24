using System;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UI;
#endif

public class CustomContentSizeFitter : ContentSizeFitter
{
    public event Action<Vector2> SizeChanged; 
    [SerializeField] private Vector2 _maxSize = new Vector2(100, 100);
    
    private DrivenRectTransformTracker _tracker;
    [NonSerialized] private RectTransform m_Rect;
    public RectTransform rectTransform
    {
        get
        {
            if (m_Rect == null)
                m_Rect = GetComponent<RectTransform>();
            return m_Rect;
        }
    }
    
    protected override void OnDisable()
    {
        _tracker.Clear();
        LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
        base.OnDisable();
    }
    
    private void HandleSelfFittingAlongAxis(int axis)
    {
        FitMode fitting = (axis == 0 ? horizontalFit : verticalFit);
        if (fitting == FitMode.Unconstrained)
        {
            // Keep a reference to the tracked transform, but don't control its properties:
            _tracker.Add(this, rectTransform, DrivenTransformProperties.None);
            return;
        }

        _tracker.Add(this, rectTransform, (axis == 0 ? DrivenTransformProperties.SizeDeltaX : DrivenTransformProperties.SizeDeltaY));

        float size = 0;
        // Set size to min or preferred size
        if (fitting == FitMode.MinSize)
        {
            size = Mathf.Clamp(LayoutUtility.GetMinSize(m_Rect, axis), 0, _maxSize[axis]);
            rectTransform.SetSizeWithCurrentAnchors((RectTransform.Axis)axis, size);
        }
        else
        {
            size = Mathf.Clamp(LayoutUtility.GetPreferredSize(m_Rect, axis), 0, _maxSize[axis]);
            rectTransform.SetSizeWithCurrentAnchors((RectTransform.Axis)axis, size);
        }
        
        SizeChanged?.Invoke(rectTransform.rect.size);
    }
    
    public override void SetLayoutHorizontal()
    {
        _tracker.Clear();
        HandleSelfFittingAlongAxis(0);
    }

    public override void SetLayoutVertical()
    {
        HandleSelfFittingAlongAxis(1);
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(CustomContentSizeFitter), true)]
[CanEditMultipleObjects]
public class CustomContentSizeFitterEditor : ContentSizeFitterEditor
{
    SerializedProperty _maxSize;
    
    protected override void OnEnable()
    {
        base.OnEnable();
        _maxSize = serializedObject.FindProperty("_maxSize");
    }
    
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(_maxSize, true);
        serializedObject.ApplyModifiedProperties();

        base.OnInspectorGUI();
    }
}
#endif
