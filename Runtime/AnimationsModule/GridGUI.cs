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
    private Vector2 gridRange = new(1, 10_000);
    private Vector2 gridScale = new (100, 100);
    private Vector2 dragCurrent;
    private Vector2 scaledOffset;
    private Vector2 pan;
    private Vector2 mp;
    private Vector2 center;
    private Rect area;
    private Vector2 gridSpacingRef;
    private float zoom;
    private int cellDivides = 10;
    private bool isDragging;
    private bool isInited;
    public bool NeedRepaint { get; private set; }
    private Texture2D scaleTexture;

    public GridGUI()
    {
        zoom = zoomRange.y * 0.1f;
        gridSpacingRef = new Vector2(4, 4) / gridScale / zoomRange.y;
    }

    private void Init()
    {
        if(isInited) return;
        isInited = true;
        
        if (scaleTexture == null)
        {
            scaleTexture = EditorUtils.GetTextureByColor(Color.black);
        }
        
        GoToWorldPoint(center / gridScale);
    }
    
    public void OnGUI(Rect rectArea)
    {
        mp = Event.current.mousePosition;
        NeedRepaint = false;
        area = rectArea;
        center = area.size / 2;
        Init();
        HandleInput();
        Vector2 pivotPoint = new Vector2(0, 21) + rectArea.position;
        scaledOffset = new Vector2(-pivotPoint.x * (zoom - 1), -pivotPoint.y * (zoom - 1));
        var clip = rectArea;
        clip.size /= zoom;
        var offset = pan / zoom;
        clip.position -= pan / zoom;
        
        GUI.matrix = TransformMatrix();
        GUI.BeginGroup(clip);
        {
            rectArea.position = offset;
            GUI.BeginGroup(rectArea);
            {
                var box = rectArea;
                box.position = Vector2.zero;
                GUI.Box(box, "");
                DrawGrid(gridSpacingRef, 1, Color.white);
                DrawPoints();
            }
            GUI.EndGroup();
        }
        GUI.EndGroup();
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
            float delta = e.delta.y;
            float gridDelta = delta * -Mathf.InverseLerp(0, gridRange.y * gridRange.y, gridScale.sqrMagnitude) * 4000f;
            Vector2 mousePos =  WorldMousePosition;
            
            if (e.control)
            {
                gridScale.x -= gridDelta;
                ClampGridScale();
            }
            else if(e.alt)
            {
                gridScale.y -= gridDelta;
                ClampGridScale();
            }
            else
            { 
                float zoomDelta = -delta * Mathf.InverseLerp(0, zoomRange.y, zoom) * 400;
                zoom += zoomDelta;
                zoom = Mathf.Clamp(zoom, zoomRange.x, zoomRange.y);
            }
            
            GoToWorldPoint(mousePos);
            Vector2 newMousePos = WorldMousePosition;
            GoToWorldPoint(mousePos - (newMousePos - mousePos));
            
            ClampPan();
            e.Use();
            NeedRepaint = true;
        }

        if (e.type == EventType.MouseDown && e.button == 0)
        {
            Vector2 mousePos = WorldMousePosition;
            if (!isDragging)
            {
                isDragging = true;
            }

            points.Add(mousePos);
            NeedRepaint = true;
        }

        if (e.type == EventType.MouseDrag && e.button == 0 && isDragging)
        {
            dragCurrent = WorldMousePosition;
            points[^1] = dragCurrent;
            NeedRepaint = true;
        }

        if (e.type == EventType.MouseUp && e.button == 0)
        {
            isDragging = false;
        }
    }

    public void GoToWorldPoint(Vector2 worldPoint)
    {
        pan = -(worldPoint * zoom * gridScale);
        pan -= area.size / zoom;
    }

    private void ClampPan()
    {
        pan = pan.Clamp(-(area.size * zoom - area.size), Vector2.zero);
    }
    
    private void ClampGridScale()
    {
        gridScale = gridScale.Clamp(Vector2.one * gridRange.x, Vector2.one * gridRange.y);
    }
    
    private Vector2 WorldMousePosition => (MouseOffseted - pan) / zoom / gridScale;
    private Vector2 MouseOffseted => mp - area.position;

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
            var gridSpacingMultiply = GetGridMultiplyByZoom();
            var gridSpacingMultiplyByScale = GetGridMultiplyByGridScale();
            gridSpacingMultiplyByScale.x *= gridSpacingMultiply;
            gridSpacingMultiplyByScale.y *= gridSpacingMultiply;
            
            gridSpacing.x = gridSpacingRef.x * gridSpacingMultiplyByScale.x * gridScale.x;
            gridSpacing.y = gridSpacingRef.y * gridSpacingMultiplyByScale.y * gridScale.y;

            var alphaByZoom = GetProgressToNextDoublingByZoom(depth);
            var alphaByScale = GetProgressToNextDoublingByScale(depth);
            alphaByScale *= alphaByZoom;

            var panXZoom = -pan.x / zoom;
            var panYZoom = -pan.y / zoom;
            var diffX = (panXZoom) % gridSpacing.x;
            var diffY = (panYZoom) % gridSpacing.y;
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
            index = Mathf.RoundToInt(y  / gridSpacing.y);
            for (;y < h; y += gridSpacing.y)
            {
                HandleOpacityY();
                Handles.DrawAAPolyLine(lw / zoom, new Vector3(panXZoom, y, 0), new Vector3(w, y, 0f));
            }

            Handles.color = Color.white;
            
            void HandleOpacityX()
            {
                if (index % cellDivides != 0)
                {
                    c.a = alphaByScale.x;
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
                if (index % cellDivides != 0)
                {
                    c.a = alphaByScale.y;
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

    private float GetGridMultiplyByZoom()
    {
        float scaleRatio = zoomRange.y / zoom;
        int doublingCount = (int)Mathf.Log(scaleRatio, cellDivides);
        float gridSpacingMultiply = Mathf.Pow(cellDivides, doublingCount);
        return gridSpacingMultiply;
    }
    
    private Vector2 GetGridMultiplyByGridScale()
    {
        float scaleXRatio = gridRange.y / gridScale.x;
        float scaleYRatio = gridRange.y / gridScale.y;
        int doublingXCount = (int)Mathf.Log(scaleXRatio, cellDivides);
        int doublingYCount = (int)Mathf.Log(scaleYRatio, cellDivides);
        float gridSpacingMultiplyX = Mathf.Pow(cellDivides, doublingXCount);
        float gridSpacingMultiplyY = Mathf.Pow(cellDivides, doublingYCount);

        return new Vector2(gridSpacingMultiplyX, gridSpacingMultiplyY);
    }
    
    float GetProgressToNextDoublingByZoom(int doublingDepth)
    {
        return GetLerped(zoomRange.y / zoom, doublingDepth);
    }
    
    Vector2 GetProgressToNextDoublingByScale(int doublingDepth)
    {
        return new Vector2(
            GetLerped(gridRange.y / gridScale.x, doublingDepth),
            GetLerped(gridRange.y / gridScale.y, doublingDepth));
    }

    float GetLerped(float scaleRatio, int depth)
    {
        float value = Mathf.Log(scaleRatio, cellDivides);
        float a = (int)value;
        float b = a + depth;

        return Mathf.InverseLerp(b, a, value);
    }

    private void DrawPoints()
    {
        foreach (var point in points)
        {
            Handles.DrawSolidDisc(point * gridScale, Vector3.forward, 6 / zoom);
        }
    }
}
