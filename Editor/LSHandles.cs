using System;
using System.Collections.Generic;
using System.Linq;
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
        private static Rect rect;
        private static Camera cam;
        private static Material lineMaterial;
        private static CameraData camData;
        private static RenderTexture targetTexture;
        private static readonly MethodInfo blitMaterialInfo;
        private static readonly HashSet<GameObject> allObjects = new();
        private static EventType eventType;
        private static Vector3 lastMp;
        private static Scene scene;
        
        static LSHandles()
        {
            var guiType = Type.GetType("UnityEngine.GUI,UnityEngine");
            var propInfo = guiType.GetProperty("blitMaterial", BindingFlags.Static | BindingFlags.NonPublic);
            blitMaterialInfo = propInfo.GetGetMethod(true);
            lineMaterial = new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply"));
            lines.Created += AddGameObject;
            lines.Got += line => line.enabled = true;
            lines.Released += line => line.enabled = false;
            points.Created += AddGameObject;
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
                cam.orthographicSize = camData.size;
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

            lines.ReleaseAll();
            points.ReleaseAll();
        }
        
        private static void AddGameObject<T>(T comp) where T : Component
        {
            var go = comp.gameObject;
            if (allObjects.Contains(go))
                return;

            SceneManager.MoveGameObjectToScene(go, scene);
            allObjects.Add(go);
        }
        
        private static void HandleInput()
        {
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
                camData.size += e.delta.y * camData.size / 30;
                cam.orthographicSize = camData.size;
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
            Texture2D texture,
            float width)
        {
            if(eventType != EventType.Repaint) return; 
            var bezie = GetLine(color, width);
            DrawBezierCurve(bezie, startPoint, startTangent, endTangent, endPoint);
        }

        private static LineRenderer GetLine(Color color, float width)
        {
            var line = lines.Get();
            line.material = lineMaterial;
            line.startColor = color;
            line.endColor = color;
            line.startWidth = width;
            line.endWidth = width;
            return line;
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

    }
}