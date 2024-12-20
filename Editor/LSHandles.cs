﻿using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace LSCore.Editor
{
    public static partial class LSHandles
    {
        public static int bezieQuality = 50;
        private static readonly EditorHiddenObjectPool<LineRenderer> lines = new(shouldStoreActive: true);
        private static readonly EditorHiddenObjectPool<Transform> points = new(shouldStoreActive: true);
        private static readonly EditorHiddenObjectPool<SpriteRenderer> sprites = new(shouldStoreActive: true);
        private static Rect rect;
        private static Camera cam;
        private static Material lineMaterial;
        private static Material spriteMaterial;
        private static CameraData camData;
        private static RenderTexture targetTexture;
        private static readonly MethodInfo blitMaterialInfo;
        private static readonly HashSet<GameObject> allObjects = new();
        private static EventType eventType;
        private static Vector3 lastMp;
        private static Scene scene;
        private static int currentDrawLayer;

        public static float CamSize => cam.orthographicSize;
        
        static LSHandles()
        {
            var guiType = Type.GetType("UnityEngine.GUI,UnityEngine");
            var propInfo = guiType.GetProperty("blitMaterial", BindingFlags.Static | BindingFlags.NonPublic);
            blitMaterialInfo = propInfo.GetGetMethod(true);
            lineMaterial = new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply"));
            spriteMaterial = new Material(Shader.Find("Sprites/Default"));
            lines.Created += AddGameObject;
            lines.Got += line => line.enabled = true;
            lines.Released += line => line.enabled = false;
            releasePools += lines.ReleaseAll;

            points.Created += AddGameObject;
            releasePools += points.ReleaseAll;

            sprites.Created += AddGameObject;
            sprites.Got += sprite => sprite.enabled = true;
            sprites.Released += sprite => sprite.enabled = false;
            releasePools += sprites.ReleaseAll;
        }
        
        public static void Begin(Rect rect, CameraData camData)
        {
            HandleInput();
            eventType = Event.current.type;
            LSHandles.rect = rect;
            LSHandles.camData = camData;
            
            if (cam == null)
            {
                scene = EditorSceneManager.NewPreviewScene();
                if (!scene.IsValid())
                    throw new InvalidOperationException("Preview scene could not be created");

                scene.name = "PreviewScene";
                GameObject objectWithHideFlags1 = EditorUtility.CreateGameObjectWithHideFlags("GridCam", HideFlags.None, typeof (Camera));
                cam = objectWithHideFlags1.GetComponent<Camera>();
                AddGameObject(cam);
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.enabled = false;
                cam.cameraType = CameraType.Preview;
                cam.scene = scene;
                cam.orthographic = true;
                cam.orthographicSize = camData.Size;
                cam.renderingPath = RenderingPath.Forward;
            }
            
            cam.backgroundColor = camData.backColor;
            cam.transform.position = camData.position;
            
            if (targetTexture == null || eventType == EventType.Repaint)
            {
                CreateCameraTargetTexture(rect, true);
            }
            
            cam.targetTexture = targetTexture;
        }

        private static Action releasePools;
        
        public static void End()
        {
            if(eventType != EventType.Repaint) return; 
            RenderCamera();
            Graphics.DrawTexture(
                rect,
                targetTexture,
                new Rect(0.0f, 0.0f, 1f, 1f),
                0, 0,
                0, 0,
                GUI.color, (Material)blitMaterialInfo.Invoke(null, null));
            
            releasePools();
            currentDrawLayer = 0;
        }
        
        private static void AddGameObject<T>(T comp) where T : Component
        {
            var go = comp.gameObject;
            if (allObjects.Contains(go))
                return;

            SceneManager.MoveGameObjectToScene(go, scene);
            allObjects.Add(go);
        }

        public static Vector2 MouseInWorldPoint
        {
            get
            {
                Event e = Event.current;
                Vector2 mp = e.mousePosition;
                mp.y *= -1;
                mp.y += rect.height;

                return cam.ScreenToWorldPoint(mp);
            }
        }

        private static Vector3 lastMpForDelta;

        public static Vector2 MouseDeltaInWorldPoint
        {
            get
            {
                Event e = Event.current;
                Vector3 mp = e.mousePosition;
                mp.y *= -1;
                mp.y += rect.height;
                
                var wpForDelta = cam.ScreenToWorldPoint(mp);
                var value = wpForDelta - lastMpForDelta;
                lastMpForDelta = wpForDelta;
                return value;
            }
        }

        private static void HandleInput()
        {
            if(cam == null) return;
            
            Event e = Event.current;
            Vector3 mp = e.mousePosition;
            mp.y *= -1;
            mp.y += rect.height;
            
            if (e.type == EventType.MouseDown && e.button == 2)
            {
                lastMp = cam.ScreenToWorldPoint(mp);
            }
            else if (e.type == EventType.MouseDrag && e.button == 2)
            {
                camData.position -= cam.ScreenToWorldPoint(mp) - lastMp;
                GUI.changed = true;
            }
            else if (e.type == EventType.ScrollWheel)
            {
                var point = cam.ScreenToWorldPoint(mp);
                camData.Size += e.delta.y * camData.Size / 30;
                cam.orthographicSize = camData.Size;
                var newPoint = cam.ScreenToWorldPoint(mp);
                camData.position -= newPoint - point;
                GUI.changed = true;
            }
        }
        
        
        private static bool inProgress;
        private static int lastWidth;
        private static int lastHeight;

        private static void CreateCameraTargetTexture(Rect cameraRect, bool hdr)
        {
            GraphicsFormat colorFormat = !hdr || !SystemInfo.IsFormatSupported(GraphicsFormat.R16G16B16A16_SFloat, FormatUsage.Render) ? SystemInfo.GetGraphicsFormat(DefaultFormat.LDR) : GraphicsFormat.R16G16B16A16_SFloat;
            if (targetTexture != null && targetTexture.graphicsFormat != colorFormat)
            {
                Object.DestroyImmediate(targetTexture);
                targetTexture = null;
            }
            
            Rect cameraRect1 = cameraRect;
            int width = (int) cameraRect1.width;
            int height = (int) cameraRect1.height;
            
            if (targetTexture == null || lastWidth != width || lastHeight != height)
            {
                lastWidth = width;
                lastHeight = height;
                if (targetTexture != null)
                {
                    UnityEngine.Object.DestroyImmediate(targetTexture);
                }
                RenderTexture renderTexture = new RenderTexture(0, 0, colorFormat, SystemInfo.GetGraphicsFormat(DefaultFormat.DepthStencil));
                renderTexture.name = "GridCam RT";
                renderTexture.antiAliasing = 1;
                renderTexture.hideFlags = HideFlags.HideAndDontSave;
                targetTexture = renderTexture;
                targetTexture.width = width;
                targetTexture.height = height;
                inProgress = false;
            }
            else if(!inProgress)
            {
                inProgress = true;
                if (targetTexture != null)
                {
                    UnityEngine.Object.DestroyImmediate(targetTexture);
                }
                var renderTexture = new RenderTexture(0, 0, colorFormat, SystemInfo.GetGraphicsFormat(DefaultFormat.DepthStencil));
                renderTexture.name = "GridCam RT";
                renderTexture.antiAliasing = 8;
                renderTexture.hideFlags = HideFlags.HideAndDontSave;
                targetTexture = renderTexture;
                targetTexture.width = lastWidth;
                targetTexture.height = lastHeight;
            }
        }

        private static void RenderCamera()
        {
            cam.Render();
        }
        
        public static void DrawGrid(GridData data)
        { 
            data.Draw();
        }

        private static bool IsInCamera(Vector2 point)
        {
            Vector3 pointInView = cam.WorldToViewportPoint(point);
            return pointInView.x is >= 0 and <= 1 
                   && pointInView.y is >= 0 and <= 1 
                   && pointInView.z > 0;
        }
        
        public static void DrawBezier(
            Vector3 startPoint,
            Vector3 startTangent,
            Vector3 endTangent,
            Vector3 endPoint,
            Color color,
            float width,
            bool dependsOnCam = true)
        {
            if(eventType != EventType.Repaint) return; 
            var bezie = GetLine(color, width, dependsOnCam);
            DrawBezierCurve(bezie, startPoint, startTangent, endTangent, endPoint);
        }

        private static LineRenderer GetLine(Color color, float width, bool dependsOnCam = true)
        {
            var line = lines.Get();
            line.material = lineMaterial;
            line.startColor = color;
            line.endColor = color;
            if (dependsOnCam)
            {
                width *= cam.orthographicSize / 3;
            }
            line.startWidth = width;
            line.endWidth = width;
            return line;
        }
        
        private static SpriteRenderer GetCircle()
        {
            var sprite = sprites.Get();
            sprite.material = spriteMaterial;
            sprite.sprite = LSIcons.GetSprite("circle");
            return sprite;
        }

        private static void DrawBezierCurve(LineRenderer bezie,
            Vector3 startPoint,
            Vector3 startTangent,
            Vector3 endTangent,
            Vector3 endPoint)
        {
            Vector3[] positions = new Vector3[bezieQuality];
            for (int i = 0; i < bezieQuality; i++)
            {
                float t = i / (float)(bezieQuality - 1);
                positions[i] = CalculateBezierPoint(t, startPoint, startTangent, endTangent, endPoint);
            }
            
            bezie.positionCount = bezieQuality;
            bezie.SetPositions(positions);
        }

        private static Vector3 CalculateBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
        {
            float u = 1 - t;
            float tt = t * t;
            float uu = u * u;
            float uuu = uu * u;
            float ttt = tt * t;

            Vector3 p = uuu * p0;
            p += 3 * uu * t * p1;
            p += 3 * u * tt * p2;
            p += ttt * p3;

            return p;
        }
        
        public static void DrawLine(float width, Color color, params Vector3[] points)
        {
            DrawLine(width, color, true, points);
        }
        
        public static void DrawLine(float width, Color color, bool dependsOnCam, params Vector3[] points)
        {
            if(eventType != EventType.Repaint) return; 
            var line = GetLine(color, width, dependsOnCam);
            
            line.positionCount = 2;
            line.SetPositions(points);
            line.sortingOrder = currentDrawLayer++;
        }
        
        public static void DrawSolidCircle(Vector2 pos, float size, Color color)
        {
            DrawSolidCircle(pos, new Vector2(size, size), color);
        }
        
        public static void DrawSolidCircle(Vector2 pos, Vector2 size, Color color, bool dependsOnCam = true)
        {
            if(eventType != EventType.Repaint) return; 
            var sprite = GetCircle();
            var tr = sprite.transform;
            tr.position = pos;
            Vector3 scale = size;
            scale.z = 1;
            tr.localScale = scale;
            sprite.color = color;
            sprite.sortingOrder = currentDrawLayer++;
            
            if (dependsOnCam)
            {
                tr.localScale = scale * (cam.orthographicSize / 3);
            }
            else
            {
                tr.localScale = scale;
            }
        }
    }
}