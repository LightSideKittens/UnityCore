using System;
using UnityEngine;
using UnityEngine.Animations;

public class PointsTransformer
{
    public event Action HandlingStopped;
    public event Action<bool> NeedSetupHandler;
    public Action<Event> eventHandler;
    public Action<Event> applyEventHandler;
    public Action<Event> discardEventHandler;
    public Func<bool> canSetupHandler;

    private Vector2 lastMousePosition;
    private TransformationMode transformationMode;
    public Axis axis;
    public string expression = string.Empty;

    public bool IsHandling => applyEventHandler != null;

    public bool TryHandleEvent(Event e)
    {
        bool isMouseMove = lastMousePosition != e.mousePosition;

        if (isMouseMove)
        {
            lastMousePosition = e.mousePosition;
        }

        if (IsHandling)
        {
            GUI.changed = true;
            isMouseMove &= string.IsNullOrEmpty(expression);
            var expressionChanged = TryChangeExpression(e);
            
            if (isMouseMove || expressionChanged)
            {
                eventHandler(e);
            }

            switch (e.type)
            {
                case EventType.MouseDown:
                    applyEventHandler(e);
                    StopEventHandling();
                    break;
                case EventType.KeyDown:
                    if (e.modifiers != EventModifiers.None) break;
                    TrySetupEventHandler(e);
                    switch (e.keyCode)
                    {
                        case KeyCode.Escape:
                            discardEventHandler(e);
                            StopEventHandling();
                            break;
                        case KeyCode.X:
                            SetAxis(Axis.X);
                            SetupEventHandler(e, true);
                            break;
                        case KeyCode.Y:
                            SetAxis(Axis.Y);
                            SetupEventHandler(e, true);
                            break;
                    }

                    break;
            }

            return true;
        }

        return false;

        void SetAxis(Axis axis)
        {
            this.axis = this.axis == axis ? Axis.None : axis;
        }
    }

    private bool TryChangeExpression(Event e)
    {
        var prevExpression = expression;
        
        if (CanChangeExpression)
        {
            if (e.type == EventType.KeyDown)
            {
                var ch = e.character;
                if (char.IsDigit(ch))
                {
                    expression += ch;
                }
                else if (ch is '-' or '+')
                {
                    if (expression.Length == 0)
                    {
                        expression += ch;
                    }
                }
                else if (ch is '.')
                {
                    if (!expression.Contains('.'))
                    {
                        expression += ch;
                    }
                }
                else if (e.keyCode is KeyCode.Backspace)
                {
                    if (expression.Length is 0 or 1)
                    {
                        expression = string.Empty;
                    }
                    else
                    { 
                        expression = expression[..^1];
                    }
                }

                Debug.Log(expression);
            }
        }
        else
        {
            expression = string.Empty;
        }
        
        return prevExpression != expression;
    }
    
    private void StopEventHandling()
    {
        HandlingStopped?.Invoke();
        axis = Axis.None;
        eventHandler = null;
        discardEventHandler = null;
        applyEventHandler = null;
        transformationMode = TransformationMode.None;
        expression = string.Empty;
    }

    public void TrySetupEventHandler(Event e)
    {
        if (canSetupHandler())
        {
            if (e.keyCode == KeyCode.G)
            {
                SetupEventHandlerWithMode(TransformationMode.Translate);
            }
            else if (e.keyCode == KeyCode.S)
            {
                SetupEventHandlerWithMode(TransformationMode.Scale);
            }
            else if (e.keyCode == KeyCode.R)
            {
                SetupEventHandlerWithMode(TransformationMode.Rotate);
            }
        }


        void SetupEventHandlerWithMode(TransformationMode mode)
        {
            if (mode == transformationMode)
            {
                discardEventHandler(e);
                StopEventHandling();
                return;
            }

            transformationMode = mode;
            SetupEventHandler(e, false);
        }
    }

    public void SetupEventHandler(Event e, bool isResetup)
    {
        discardEventHandler?.Invoke(e);
        NeedSetupHandler?.Invoke(isResetup);
    }

    private bool CanUseExpression => !string.IsNullOrEmpty(expression) && CanChangeExpression;
    private bool CanChangeExpression => axis != Axis.None || transformationMode == TransformationMode.Rotate;
    
    public Func<Vector3, Vector3> BuildTransformer(Rect bounds, Vector2 startMousePos, Vector2 curMousePos)
    {
        if (CanUseExpression)
        {
            return GetPoint(bounds, expression);
        }
        
        var matrix = GetMatrix(bounds, startMousePos, curMousePos);
        return p => matrix.MultiplyPoint(p);
    }
    
    private Matrix4x4 GetMatrix(Rect bounds, Vector2 startPos, Vector2 curPos)
    {
        return GetMatrix(bounds, startPos, curPos, transformationMode, axis);
    }

    private Func<Vector3, Vector3> GetPoint(Rect bounds, string axisValue)
    {
        var value = float.Parse(axisValue);
        Func<Vector3, Vector3> getter = p => p;
        
        switch (transformationMode)
        {
            case TransformationMode.Translate:
            {
                if (char.IsDigit(axisValue[0]) || axisValue[0] == '.')
                {
                    getter = axis switch
                    {
                        Axis.X => p => { p.x = value; return p; },
                        Axis.Y => p => { p.y = value; return p; },
                    };
                }
                else
                {
                    getter = axis switch
                    {
                        Axis.X => p => { p.x += value; return p; },
                        Axis.Y => p => { p.y += value; return p; },
                    };
                }

                break;
            }
            case TransformationMode.Rotate:
            {
                Vector2 center = bounds.center;
                Quaternion rotation = Quaternion.Euler(0, 0, value);
                var transformation =
                    Matrix4x4.Translate(new Vector3(center.x, center.y, 0)) *
                    Matrix4x4.Rotate(rotation) *
                    Matrix4x4.Translate(new Vector3(-center.x, -center.y, 0));
                getter = point => transformation.MultiplyPoint(point);

                break;
            }
            case TransformationMode.Scale:
            {
                Vector2 center = bounds.center;
                float scaleFactor = value;
                scaleFactor = Mathf.Clamp(scaleFactor, 0.01f, 100f);

                Vector3 scale = axis switch
                {
                    Axis.X => new Vector3(scaleFactor, 1, 1),
                    Axis.Y => new Vector3(1, scaleFactor, 1),
                    _ => Vector3.one
                };

                var transformation =
                    Matrix4x4.Translate(new Vector3(center.x, center.y, 0)) *
                    Matrix4x4.Scale(scale) *
                    Matrix4x4.Translate(new Vector3(-center.x, -center.y, 0));
                getter = point => transformation.MultiplyPoint(point);

                break;
            }
            default:
                throw new ArgumentOutOfRangeException(nameof(transformationMode), transformationMode, null);
        }

        return getter;
    }

    public static Matrix4x4 GetMatrix(
        Rect bounds,
        Vector2 startPos,
        Vector2 currentPos,
        TransformationMode transformationMode,
        Axis axis = Axis.None)
    {
        Vector2 delta = currentPos - startPos;
        Vector2 center = bounds.center;

        Matrix4x4 transformation;

        switch (transformationMode)
        {
            case TransformationMode.Translate:
            {
                Vector2 translation = delta;

                _ = axis switch
                {
                    Axis.X => translation.y = 0,
                    Axis.Y => translation.x = 0,
                    _ => 0
                };

                transformation = Matrix4x4.Translate(new Vector3(translation.x, translation.y, 0));
                break;
            }
            case TransformationMode.Rotate:
            {
                float rotationAngle = Vector2.SignedAngle(startPos - center, currentPos - center);
                Quaternion rotation = Quaternion.Euler(0, 0, rotationAngle);
                transformation =
                    Matrix4x4.Translate(new Vector3(center.x, center.y, 0)) *
                    Matrix4x4.Rotate(rotation) *
                    Matrix4x4.Translate(new Vector3(-center.x, -center.y, 0));
                break;
            }
            case TransformationMode.Scale:
            {
                float scaleFactor = Vector2.Distance(center, currentPos) / Vector2.Distance(center, startPos);
                scaleFactor = Mathf.Clamp(scaleFactor, 0.01f, 100f);

                Vector3 scale = axis switch
                {
                    Axis.None => Vector3.one * scaleFactor,
                    Axis.X => new Vector3(scaleFactor, 1, 1),
                    Axis.Y => new Vector3(1, scaleFactor, 1),
                    _ => Vector3.one
                };

                transformation =
                    Matrix4x4.Translate(new Vector3(center.x, center.y, 0)) *
                    Matrix4x4.Scale(scale) *
                    Matrix4x4.Translate(new Vector3(-center.x, -center.y, 0));
                break;
            }
            default:
                throw new ArgumentOutOfRangeException(nameof(transformationMode), transformationMode, null);
        }

        return transformation;
    }

    public enum TransformationMode
    {
        None,
        Translate,
        Rotate,
        Scale,
    }
}