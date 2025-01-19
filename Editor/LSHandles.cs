using System;
using System.Collections.Generic;
using System.Reflection;
using LSCore.Extensions.Unity;
using UnityEditor;
using UnityEditor.Compilation;
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

        public static Matrix4x4 Matrix => currentMatrix;
        private static Matrix4x4 currentMatrix = Matrix4x4.identity;
        public static float CamSize => cam.orthographicSize;
        public static float ScaleMultiplier => 1000 / rect.height;
        public static float TotalScaleMultiplier => ScaleMultiplier * cam.orthographicSize;
        
        static LSHandles()
        {
            CompilationPipeline.compilationStarted += x =>
            {
                EditorSceneManager.ClosePreviewScene(scene);
            };
            
            var guiType = Type.GetType("UnityEngine.GUI,UnityEngine");
            var propInfo = guiType.GetProperty("blitMaterial", BindingFlags.Static | BindingFlags.NonPublic);
            blitMaterialInfo = propInfo.GetGetMethod(true);
            lineMaterial = new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply"));
            spriteMaterial = new Material(Shader.Find("Sprites/Default"));
            lineMaterial.hideFlags = HideFlags.HideAndDontSave;
            spriteMaterial.hideFlags = HideFlags.HideAndDontSave;
            lines.Created += AddGameObject;
            lines.Got += line => line.enabled = true;
            lines.Released += line => line.enabled = false;
            releasePools += lines.ReleaseAll;

            sprites.Created += AddGameObject;
            sprites.Got += sprite => sprite.enabled = true;
            sprites.Released += sprite => sprite.enabled = false;
            releasePools += sprites.ReleaseAll;
        }

        
        #region Matrix stack
        
        public static void StartMatrix(Matrix4x4 matrix)
        {
            currentMatrix = matrix;
        }
        
        public static Matrix4x4 EndMatrix()
        {
            var m = currentMatrix;
            currentMatrix = Matrix4x4.identity;
            return m;
        }

        #endregion
        
        public static void Begin(Rect rect, CameraData camData)
        {
            HandleInput();
            eventType = Event.current.type;
            LSHandles.rect = rect;
            LSHandles.camData = camData;
            
            if (cam is null)
            {
                scene = EditorSceneManager.NewPreviewScene();
                
                if (!scene.IsValid())
                {
                    throw new InvalidOperationException("Preview scene could not be created");
                }

                scene.name = "PreviewScene";
                var objectWithHideFlags1 = EditorUtility.CreateGameObjectWithHideFlags("GridCam", HideFlags.None, typeof (Camera));
                cam = objectWithHideFlags1.GetComponent<Camera>();
                AddGameObject(cam);
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.enabled = false;
                cam.cameraType = CameraType.Game;
                cam.scene = scene;
                cam.orthographic = true;
                cam.orthographicSize = camData.Size;
                cam.renderingPath = RenderingPath.UsePlayerSettings;
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
            cam.Render();
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

        public static bool IsInDistance(Vector2 point, Vector2 worldMousePos, float distance)
        {
            distance *= ScaleMultiplier;
            Vector2 dis = Vector2.Scale(LSVector2.oneDir * distance, Scale);
            return Vector2.Distance(point, worldMousePos) <= dis.magnitude;
        }

        public static Vector2 TransformScreenPosition(Vector2 screenPosition)
        {
            screenPosition -= rect.position;
            screenPosition.y *= -1;
            screenPosition.y += rect.height;
            return screenPosition;
        }


        public static Vector3 ScreenToWorld(Vector2 screenPosition)
        {
            return currentMatrix.inverse.MultiplyPoint3x4(cam.ScreenToWorldPoint(TransformScreenPosition(screenPosition)));
        }

        public static Vector2 WorldToScreen(Vector3 worldPosition) 
        {
            Vector3 transformedPosition = currentMatrix.MultiplyPoint3x4(worldPosition);
            Vector3 screenPoint = cam.WorldToScreenPoint(transformedPosition);
            Vector2 screenPosition = InverseTransformScreenPosition(screenPoint);

            return screenPosition;
        }

        private static Vector2 InverseTransformScreenPosition(Vector2 transformedScreenPosition)
        {
            Vector2 screenPosition = transformedScreenPosition;

            screenPosition.y -= rect.height;
            screenPosition.y *= -1;
            screenPosition += rect.position;

            return screenPosition;
        }

        public static Vector2 MousePos
        {
            get
            {
                var e = Event.current;
                var mp = e.mousePosition;
                return TransformScreenPosition(mp);
            }
        }
        
        public static Vector2 MouseInWorldPoint => currentMatrix.inverse.MultiplyPoint3x4(cam.ScreenToWorldPoint(MousePos));

        private static Vector3 lastMpForDelta;

        public static Vector2 MouseDeltaInWorldPoint
        {
            get
            {
                var wpForDelta = currentMatrix.inverse.MultiplyPoint3x4(cam.ScreenToWorldPoint(MousePos));
                var value = wpForDelta - lastMpForDelta;
                lastMpForDelta = wpForDelta;
                return value;
            }
        }

        private static Vector3 mpForMatrix;
        private const float minScale = 0.0001f;
        private const float maxScale = 10000f;

        private static void HandleInput()
        {
            if (cam == null) return;

            Event e = Event.current;
            if (e.type is EventType.Layout or EventType.Repaint) return;
            Vector3 mp = MousePos;

            if (e.type == EventType.MouseDown && e.button == 2)
            {
                lastMp = mp;
                mpForMatrix = mp;
            }
            else if (e.type == EventType.MouseDrag && e.button == 2)
            {
                if (e.control)
                {
                    float sensitivity = 0.01f;
                    float scaleX = 1 + e.delta.x * sensitivity;
                    float scaleY = 1 + -e.delta.y * sensitivity;
                    ScaleMatrix(new Vector3(scaleX, scaleY, 1), cam.ScreenToWorldPoint(mpForMatrix));
                }
                else
                {
                    var delta = cam.ScreenToWorldPoint(mp) - cam.ScreenToWorldPoint(lastMp);
                    camData.position -= delta;
                }

                lastMp = mp;
                GUI.changed = true;
            }
            else if (e.type == EventType.ScrollWheel)
            {
                float sensitivity = 0.04f;
                float scalex = 1 + -e.delta.y * sensitivity;
                var scale = new Vector3(scalex, scalex, 1);
                ScaleMatrix(scale, cam.ScreenToWorldPoint(mp));
                
                /*var point = cam.ScreenToWorldPoint(mp);
                var camSize = camData.Size;
                camSize += e.delta.y * camData.Size / 30;
                var matrixScale = GetScaleByCameraSize(camSize);
                    
                if((matrixScale.x is < minScale or > maxScale)
                   || (matrixScale.y is < minScale or > maxScale))
                {
                    return;
                }

                camData.Size = camSize;
                cam.orthographicSize = camSize;
                var newPoint = cam.ScreenToWorldPoint(mp);
                camData.position -= newPoint - point;*/
                
                GUI.changed = true;
            }

            void ScaleMatrix(Vector3 scale, Vector3 lmp)
            {
                var matrixScale = Vector3.one.Divide(Vector3.Scale(currentMatrix.lossyScale, scale)) * camData.Size;
                    
                if(matrixScale.x is < minScale or > maxScale)
                {
                    scale.x = 1;
                }
                    
                if(matrixScale.y is < minScale or > maxScale)
                {
                    scale.y = 1;
                }
                    
                Matrix4x4 translateToPoint = Matrix4x4.Translate(lmp);
                Matrix4x4 scaleMatrix = Matrix4x4.Scale(scale);
                Matrix4x4 translateBack = Matrix4x4.Translate(-lmp);

                Matrix4x4 transformMatrix = translateToPoint * scaleMatrix * translateBack;

                currentMatrix = transformMatrix * currentMatrix;
            }
        }

        private static bool inProgress;
        private static int lastWidth;
        private static int lastHeight;

        private static void CreateCameraTargetTexture(Rect cameraRect, bool hdr)
        {
            var colorFormat = !hdr || !SystemInfo.IsFormatSupported(GraphicsFormat.R16G16B16A16_SFloat, FormatUsage.Render) ? SystemInfo.GetGraphicsFormat(DefaultFormat.LDR) : GraphicsFormat.R16G16B16A16_SFloat;
            if (targetTexture != null && targetTexture.graphicsFormat != colorFormat)
            {
                Object.DestroyImmediate(targetTexture);
                targetTexture = null;
            }
            
            var cameraRect1 = cameraRect;
            var width = (int) cameraRect1.width;
            var height = (int) cameraRect1.height;
            
            if (targetTexture == null || lastWidth != width || lastHeight != height)
            {
                lastWidth = width;
                lastHeight = height;
                if (targetTexture != null)
                {
                    Object.DestroyImmediate(targetTexture);
                }
                var renderTexture = new RenderTexture(0, 0, colorFormat, SystemInfo.GetGraphicsFormat(DefaultFormat.DepthStencil));
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
                    Object.DestroyImmediate(targetTexture);
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
        
        public static void DrawGrid(GridData data)
        {
            var layer = currentDrawLayer;
            currentDrawLayer = -32000;
            data.Draw();
            currentDrawLayer = layer;
        }

        private static bool IsInCamera(Vector2 point)
        {
            var pointInView = cam.WorldToViewportPoint(point);
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
            
            startPoint   = currentMatrix.MultiplyPoint3x4(startPoint);
            startTangent = currentMatrix.MultiplyPoint3x4(startTangent);
            endTangent   = currentMatrix.MultiplyPoint3x4(endTangent);
            endPoint     = currentMatrix.MultiplyPoint3x4(endPoint);
            
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
                width *= cam.orthographicSize;
            }

            width *= ScaleMultiplier;
            line.startWidth = width;
            line.endWidth = width;
            currentDrawLayer += 10;
            line.sortingOrder = currentDrawLayer;
            return line;
        }
        
        private static SpriteRenderer GetCircle() => GetSprite("circle");
        private static SpriteRenderer GetRing() => GetSprite("ring");
        private static SpriteRenderer GetSquare() => GetSprite("square");
        private static SpriteRenderer GetTriangle() => GetSprite("triangle");

        private static SpriteRenderer GetSprite(string iconName)
        {
            var sprite = sprites.Get();
            sprite.material = spriteMaterial;
            sprite.sprite = LSIcons.GetSprite(iconName);
            currentDrawLayer += 10;
            sprite.sortingOrder = currentDrawLayer;
            return sprite;
        }

        private static void DrawBezierCurve(LineRenderer bezie,
            Vector3 startPoint,
            Vector3 startTangent,
            Vector3 endTangent,
            Vector3 endPoint)
        {
            var positions = new Vector3[bezieQuality];
            for (var i = 0; i < bezieQuality; i++)
            {
                var t = i / (float)(bezieQuality - 1);
                positions[i] = CalculateBezierPoint(t, startPoint, startTangent, endTangent, endPoint);
            }
            
            bezie.positionCount = bezieQuality;
            bezie.SetPositions(positions);
        }

        private static Vector3 CalculateBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
        {
            var u = 1 - t;
            var tt = t * t;
            var uu = u * u;
            var uuu = uu * u;
            var ttt = tt * t;

            var p = uuu * p0;
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

            for (var i = 0; i < points.Length; i++)
            {
                points[i] = currentMatrix.MultiplyPoint3x4(points[i]);
            }
            
            line.positionCount = points.Length;
            line.SetPositions(points);
        }
        
        public static void DrawCircle(Vector2 pos, float size, Color color)
        {
            DrawCircle(pos, new Vector2(size, size), color);
        }
        
        public static void DrawCircle(Vector2 pos, Vector2 size, Color color, bool dependsOnCam = true)
        {
            if(eventType != EventType.Repaint) return; 
            var sprite = GetCircle();
            SetupSpriteRenderer(sprite, pos, size, color, dependsOnCam);
        }
        
        public static void DrawRing(Vector2 pos, float size, Color color)
        {
            DrawRing(pos, new Vector2(size, size), color);
        }
        
        public static void DrawRing(Vector2 pos, Vector2 size, Color color, bool dependsOnCam = true)
        {
            if(eventType != EventType.Repaint) return; 
            var sprite = GetRing();
            SetupSpriteRenderer(sprite, pos, size, color, dependsOnCam);
        }
        
        public static void DrawSquare(Rect r, Color color, bool dependsOnCam = true)
        {
            if(eventType != EventType.Repaint) return; 
            var sprite = GetSquare();
            var c = r.center;
            var size = r.size / ScaleMultiplier;
            SetupSpriteRenderer(sprite, c, size, color, dependsOnCam);
        }
        
        public static void DrawTriangle(Vector2 pos, float size, Color color, bool dependsOnCam = true)
        {
            if(eventType != EventType.Repaint) return; 
            var sprite = GetTriangle();
            SetupSpriteRenderer(sprite, pos, Vector2.one * size, color, dependsOnCam);
        }
        
        private static void SetupSpriteRenderer(SpriteRenderer sprite, Vector2 pos, Vector2 size, Color color,
            bool dependsOnCam = true)
        {
            var transformedPos = currentMatrix.MultiplyPoint3x4(pos);
            
            var tr = sprite.transform;
            tr.position = transformedPos;

            Vector3 scale = size;
            scale.z = 1;

            if (dependsOnCam)
            {
                scale *= cam.orthographicSize;
            }

            scale *= ScaleMultiplier;
            tr.localScale = scale;
            sprite.color = color;
        }
        
        public static RestoreMatrix SetIdentityMatrix() => SetMatrix(Matrix4x4.identity);

        public static RestoreMatrix SetMatrix(Matrix4x4 target)
        {
            var last = currentMatrix;
            currentMatrix = target;
            return new RestoreMatrix{ matrix = last };
        }
        
        public struct RestoreMatrix : IDisposable
        {
            public Matrix4x4 matrix;
            
            public void Dispose()
            {
                currentMatrix = matrix;
            }
        }
    }
}
