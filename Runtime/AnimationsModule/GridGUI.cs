using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Sirenix.Utilities;

[Serializable]
public class GridGUI
{
    private List<Vector2> points = new();
    private Vector2 zoomRange = new(1, 10_000);
    private Vector2 gridScale = new (1, 1);
    private Vector2 dragCurrent;
    private Vector2 scaledOffset;
    private Vector2 pan;
    private Rect area;
    private float scalesWidth = 30f;
    private float zoomRef = 1000f;
    private float zoom = 1000f;
    private Vector2 gridSpacingRef = new (100f, 100f);
    private int cellDivides = 4;
    private bool isDragging;
    public bool NeedRepaint { get; private set; }
    private Texture2D scaleTexture;

    public GridGUI()
    {
        gridSpacingRef /= zoomRef;
    }
    
    public void OnGUI(Rect rectArea)
    {
        if (scaleTexture == null)
        {
            scaleTexture = EditorUtils.GetTextureByColor(Color.black);
        }
        rectArea.position += Vector2.one * scalesWidth;
        NeedRepaint = false;
        area = rectArea;
        HandleInput();
        Vector2 pivotPoint = new Vector2(0, 21) + rectArea.position;
        scaledOffset = new Vector2(-pivotPoint.x * (zoom - 1), -pivotPoint.y * (zoom - 1));
        var clip = rectArea;
        clip.size /= zoom;
        var offset = pan / zoom;
        clip.position -= pan / zoom;
        
        DrawVScale();
        DrawHScale();
        
        GUI.matrix = TransformMatrix();
        GUI.BeginClip(clip);
        {
            rectArea.position = offset;
            GUI.BeginGroup(rectArea);
            {
                var box = rectArea;
                box.position = Vector2.zero;
                GUI.Box(box, "");
                DrawGrid(gridSpacingRef, 0.5f, Color.white);
                DrawPoints();
            }
            GUI.EndGroup();
        }
        GUI.EndClip();
        GUI.matrix = Matrix4x4.identity;
    }

    private void HandleInput()
    {
        Event e = Event.current;
        if (e.type == EventType.MouseDrag && e.button == 2)
        {
            pan += e.delta;
            ClampPan();
            NeedRepaint = true;
        }
        else if (e.type == EventType.ScrollWheel)
        {
            Vector2 mousePos = GetOffseted(Event.current.mousePosition);
            Vector2 delta = e.delta;
            var wdt = e.delta * -Mathf.InverseLerp(0, 32, gridScale.sqrMagnitude) * 0.1f;
            
            if (e.control)
            {
                gridScale.x -= wdt.y;
            }
            else if(e.alt)
            {
                gridScale.y -= wdt.y;
            }
            else
            {
                float zoomDelta = -delta.y * Mathf.InverseLerp(0, zoomRange.y, zoom) * 400;
                float oldZoom = zoom;
                zoom += zoomDelta;
                zoom = Mathf.Clamp(zoom, zoomRange.x, zoomRange.y);
                float zoomFactor = zoom / oldZoom;
                pan = (pan - mousePos) * zoomFactor + mousePos;
            }
            
            
            ClampPan();
            e.Use();
            NeedRepaint = true;
        }

        if (e.type == EventType.MouseDown && e.button == 0)
        {
            Vector2 mousePos = ConvertScreenCoordsToZoomCoords(e.mousePosition);
            if (!isDragging)
            {
                isDragging = true;
            }

            points.Add(mousePos);
            NeedRepaint = true;
        }

        if (e.type == EventType.MouseDrag && e.button == 0 && isDragging)
        {
            dragCurrent = ConvertScreenCoordsToZoomCoords(e.mousePosition);
            points[^1] = dragCurrent;
            NeedRepaint = true;
        }

        if (e.type == EventType.MouseUp && e.button == 0)
        {
            isDragging = false;
        }
    }

    private void ClampPan()
    {
        pan = pan.Clamp(-(area.size * zoom - area.size), Vector2.zero);
    }

    private Vector2 ConvertScreenCoordsToZoomCoords(Vector2 screenCoords)
    {
        return (GetOffseted(screenCoords) - pan) / zoom;
    }
    
    private Vector2 GetOffseted(Vector2 screenCoords)
    {
        return screenCoords - area.position;
    }

    private Matrix4x4 TransformMatrix()
    {
        return Matrix4x4.TRS(scaledOffset + pan, Quaternion.identity, new Vector3(zoom, zoom, 1));
    }

    private void DrawGrid(Vector2 gridSpacing, float gridOpacity, Color gridColor, int depth = 1)
    {
        Handles.BeginGUI();
        {
            var c = new Color(gridColor.r, gridColor.g, gridColor.b, gridOpacity);
            Handles.color = c;
        
            var w = area.width / zoom;
            var h = area.height / zoom;
            float scaleXRatio = zoomRef / XZoom;
            float scaleYRatio = zoomRef / YZoom;
            int doublingXCount = (int)Mathf.Log(scaleXRatio, cellDivides);
            int doublingYCount = (int)Mathf.Log(scaleYRatio, cellDivides);
            gridSpacing.x = gridSpacingRef.x * Mathf.Pow(cellDivides, doublingXCount) * gridScale.x;
            gridSpacing.y = gridSpacingRef.y * Mathf.Pow(cellDivides, doublingYCount) * gridScale.y;
        
            var alphaX = CalculateXProgressToNextDoubling(depth);
            alphaX = Mathf.Lerp(gridOpacity / 2, 0, alphaX);
            
            var alphaY = CalculateYProgressToNextDoubling(depth);
            alphaY = Mathf.Lerp(gridOpacity / 2, 0, alphaY);
            
            var panXZoom = -pan.x / zoom;
            var panYZoom = -pan.y / zoom;
            var diffX = panXZoom % gridSpacing.x;
            var diffY = panYZoom % gridSpacing.y;
            w += panXZoom;
            h += panYZoom;

            float lw = 2;
            float x = panXZoom - diffX;
            int index = Mathf.RoundToInt(x / gridSpacing.x);
            for (;x < w; x += gridSpacing.x)
            {
                HandleOpacityX();
                Handles.DrawAAPolyLine(lw / zoom, new Vector3(x, panYZoom, 0), new Vector3(x, h, 0f));
            }

            float y = panYZoom - diffY;
            index = Mathf.RoundToInt(y / gridSpacing.y);
            for (;y < h; y += gridSpacing.y)
            {
                HandleOpacityY();
                Handles.DrawAAPolyLine(lw / zoom, new Vector3(panXZoom, y, 0), new Vector3(w, y, 0f));
            }

            Handles.color = Color.white;
            
            void HandleOpacityX()
            {
                if (index % 4 != 0)
                {
                    c.a = alphaX;
                    Handles.color = c;
                }
                else
                {
                    c.a = gridOpacity;
                    Handles.color = c;
                }

                index++;
            }
            
            void HandleOpacityY()
            {
                if (index % 4 != 0)
                {
                    c.a = alphaY;
                    Handles.color = c;
                }
                else
                {
                    c.a = gridOpacity;
                    Handles.color = c;
                }

                index++;
            }
        }
        Handles.EndGUI();
    }

    private void DrawVScale()
    {
        GUI.DrawTexture(new Rect(new Vector2(-scalesWidth, 0), new Vector2(scalesWidth, area.height)), scaleTexture);
    }

    private void DrawHScale()
    {
        GUI.DrawTexture(new Rect(new Vector2(0, -scalesWidth), new Vector2(area.width, scalesWidth)), scaleTexture);
    }

    private float XZoom => zoom * gridScale.x;
    private float YZoom => zoom * gridScale.y;
    
    float CalculateXProgressToNextDoubling(int doublingDepth)
    {
        doublingDepth += 1;
        float scaleRatio = XZoom / zoomRef;
        float tScaleRatio = XZoom / (zoomRef * doublingDepth);
        
        float a = (int)Mathf.Log(scaleRatio, cellDivides);
        float b = (int)Mathf.Log(tScaleRatio, cellDivides);
        float value = Mathf.Log(scaleRatio, cellDivides);

        return Mathf.InverseLerp(a, b, value);
    }
    
    float CalculateYProgressToNextDoubling(int doublingDepth)
    {
        doublingDepth += 1;
        
        float scaleRatio = YZoom / zoomRef;
        float tScaleRatio = YZoom / (zoomRef * doublingDepth);
        
        float a = (int)Mathf.Log(scaleRatio, cellDivides);
        float b = (int)Mathf.Log(tScaleRatio, cellDivides);
        float value = Mathf.Log(scaleRatio, cellDivides);

        return Mathf.InverseLerp(a, b, value);
    }

    private void DrawPoints()
    {
        foreach (var point in points)
        {
            Handles.DrawSolidDisc(point * gridScale, Vector3.forward, 6 / zoom);
        }
    }
}
