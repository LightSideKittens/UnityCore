using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

namespace LSCore.Editor
{
    public static partial class LSHandles
    {
        public static float SnapX(float x, float step)
        {
            if (step > 0)
            {
                var rem = x % step;
                if (rem > step / 2)
                {
                    x += step - rem;
                }
                else
                {
                    x -= rem;
                }
            }
            
            return x;
        }
        
        public static Rect CalculateBounds(IEnumerable<Vector2> points)
        {
            float minX = float.MaxValue;
            float minY = float.MaxValue;
            float maxX = float.MinValue;
            float maxY = float.MinValue;

            foreach (var point in points)
            {
                if (point.x < minX) minX = point.x;
                if (point.y < minY) minY = point.y;
                if (point.x > maxX) maxX = point.x;
                if (point.y > maxY) maxY = point.y;
            }

            return new Rect(minX, minY, maxX - minX, maxY - minY);
        }

        public class PointsTransformer
        {
            public event Action HandlingStopped;
            public event Action<bool> NeedSetupHandler;
            public Action<Event> eventHandler;
            public Action<Event> applyEventHandler;
            public Action<Event> discardEventHandler;
            public Func<bool> canSetupHandler;

            private Vector2 lastMousePosition;
            private TransformationMode currentTransformationMode;
            public Axis currentAxis;

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
                    if (isMouseMove)
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
                    currentAxis = currentAxis == axis ? Axis.None : axis;
                }

                void StopEventHandling()
                {
                    HandlingStopped?.Invoke();
                    currentAxis = Axis.None;
                    eventHandler = null;
                    discardEventHandler = null;
                    applyEventHandler = null;
                }
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
                    currentTransformationMode = mode;
                    SetupEventHandler(e, false);
                }
            }

            public void SetupEventHandler(Event e, bool isResetup)
            {
                discardEventHandler?.Invoke(e);
                NeedSetupHandler?.Invoke(isResetup);
            }

            public Matrix4x4 GetMatrix(Rect bounds, Vector2 startPos, Vector2 curPos)
            {
                return GetMatrix(bounds, startPos, curPos, currentTransformationMode, currentAxis);
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

                        var scale = axis switch
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
                Translate,
                Rotate,
                Scale,
            }
        }
    }
}