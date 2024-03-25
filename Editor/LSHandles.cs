using System;
using System.Reflection;
using LSCore.Extensions.Unity;
using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace LSCore.Editor
{
    public static class LSHandles
    {
        [Serializable]
        public class CameraData
        {
            public Color backColor = new(0.2f, 0.2f, 0.2f);
            public Vector3 position = Vector3.forward * -10;
            public float size = 10;
        }
        
        [Serializable]
        public class GridData
        {
            public int cellDivides = 10;
            public Vector2 scale = Vector2.one;
            public Color color = Color.white;
        }
        
        public static int bezieQuality = 50;
        private static readonly EditorHiddenObjectPool<LineRenderer> lines = new(shouldStoreActive: true);
        private static readonly EditorHiddenObjectPool<Transform> points = new(shouldStoreActive: true);
        private static Rect rect;
        private static Camera cam;
        private static Material lineMaterial;
        private static CameraData camData;
        private static RenderTexture targetTexture;
        private static readonly MethodInfo blitMaterialInfo;
        private static Vector3 lastMp;
        private static CommandBuffer commandBuffer;

        static LSHandles()
        {
            var guiType = Type.GetType("UnityEngine.GUI,UnityEngine");
            var propInfo = guiType.GetProperty("blitMaterial", BindingFlags.Static | BindingFlags.NonPublic);
            blitMaterialInfo = propInfo.GetGetMethod(true);
        }
        
        public static void Begin(Rect rect, CameraData camData)
        {
            LSHandles.rect = rect;
            LSHandles.camData = camData;
            lineMaterial ??= new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply"));
            if (cam == null)
            {
                GameObject objectWithHideFlags1 = EditorUtility.CreateGameObjectWithHideFlags("GridCam", HideFlags.HideAndDontSave, typeof (Camera));
                cam = objectWithHideFlags1.GetComponent<Camera>();
                cam.clearFlags = CameraClearFlags.Color;
                cam.enabled = false;
                cam.cameraType = CameraType.SceneView;
                cam.scene = default;
                cam.orthographic = true;
                cam.orthographicSize = camData.size;
                cam.cullingMask = 0;
                commandBuffer = new CommandBuffer();
                commandBuffer.name = "Render Selected Objects";
                cam.RemoveAllCommandBuffers();
                cam.AddCommandBuffer(CameraEvent.AfterEverything, commandBuffer);
            }
            
            cam.backgroundColor = camData.backColor;
            cam.transform.position = camData.position;
            
            if (targetTexture == null || Event.current.type != EventType.Repaint)
            {
                CreateCameraTargetTexture(rect, true);
            }
            
            cam.targetTexture = targetTexture;
            ClearRenderCommand();
            HandleInput();
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

        public static void End()
        {
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

        private static void CreateCameraTargetTexture(Rect cameraRect, bool hdr)
        {
            GraphicsFormat colorFormat = !hdr || !SystemInfo.IsFormatSupported(GraphicsFormat.R16G16B16A16_SFloat, FormatUsage.Render) ? SystemInfo.GetGraphicsFormat(DefaultFormat.LDR) : GraphicsFormat.R16G16B16A16_SFloat;
            if (targetTexture != null && targetTexture.graphicsFormat != colorFormat)
            {
                UnityEngine.Object.DestroyImmediate(targetTexture);
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

        private static bool inProgress;
        private static int lastWidth;
        private static int lastHeight;

        private static void RenderCamera()
        {
            cam.Render();
        }

        public static void DrawGrid(GridData data)
        {
            Vector2 startPoint =  cam.ViewportToWorldPoint(new Vector3(0, 0, cam.nearClipPlane));
            Vector2 endPoint = cam.ViewportToWorldPoint(new Vector3(1, 1, cam.nearClipPlane));
            var zoomStepMultiplies = GetGridMultiplyByZoom(data);
            var scaleStepMultiplies = GetGridMultiplyByGridScale(data);
            scaleStepMultiplies *= zoomStepMultiplies;
            
            var width = 0.001f;
            var step = Vector2.one / data.cellDivides * data.scale;
            step *= scaleStepMultiplies;
            
            startPoint.x -= startPoint.x % step.x;
            startPoint.y -= startPoint.y % step.y;
            var a = new Vector2(0, 0);
            var b = new Vector2(2f, 2f);
            var v1 = step / cam.orthographicSize;
            var v2 = v1 * data.cellDivides;
            
            var opacity1 = (v1).InverseLerp(a, b);
            var opacity2 = (v2).InverseLerp(a, b);

            Debug.Log((opacity1, opacity2));
            var positions = new Vector3[2];
            var c = data.color;
            
            float minValue = -100_000_000_000_000f;
            float maxValue = 100_000_000_000_000f;

            var index = Mathf.RoundToInt(startPoint.x / step.x);
            
            while (startPoint.x < endPoint.x)
            {
                if (index % data.cellDivides == 0)
                {
                    width = 0.004f;
                    c.a = opacity2;
                }
                else
                {
                    width = 0.001f;
                    c.a = opacity1;
                }
                
                index++;;
                
                var line = GetLine(c, width);
                positions[0] = new Vector3(startPoint.x, minValue);
                positions[1] = new Vector3(startPoint.x, maxValue);
                line.positionCount = 2;
                line.SetPositions(positions);
                AddDrawRenderer(line);
                startPoint.x += step.x;
            }
            
            index = Mathf.RoundToInt(startPoint.y / step.y);
            
            while (startPoint.y < endPoint.y)
            {
                if (index % data.cellDivides == 0)
                {
                    width = 0.004f;
                    c.a = opacity2;
                }
                else
                {
                    width = 0.001f;
                    c.a = opacity1;
                }
                
                index++;
                
                var line = GetLine(c, width);
                positions[0] = new Vector3(minValue, startPoint.y);
                positions[1] = new Vector3(maxValue, startPoint.y);
                line.positionCount = 2;
                line.SetPositions(positions);
                AddDrawRenderer(line);
                startPoint.y += step.y;
            }
        }
        
        private static float GetGridMultiplyByZoom(GridData data)
        {
            float scaleRatio = cam.orthographicSize * 2;
            int doublingCount = (int)Mathf.Log(scaleRatio, data.cellDivides);
            float gridSpacingMultiply = Mathf.Pow(data.cellDivides, doublingCount);
            return gridSpacingMultiply;
        }
    
        private static Vector2 GetGridMultiplyByGridScale(GridData data)
        {
            float scaleXRatio = 4 / data.scale.x;
            float scaleYRatio = 4 / data.scale.y;
            int doublingXCount = (int)Mathf.Log(scaleXRatio, data.cellDivides);
            int doublingYCount = (int)Mathf.Log(scaleYRatio, data.cellDivides);
            float gridSpacingMultiplyX = Mathf.Pow(data.cellDivides, doublingXCount);
            float gridSpacingMultiplyY = Mathf.Pow(data.cellDivides, doublingYCount);

            return new Vector2(gridSpacingMultiplyX, gridSpacingMultiplyY);
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
            var bezie = GetLine(color, width);
            DrawBezierCurve(bezie, startPoint, startTangent, endTangent, endPoint);
            
            AddDrawRenderer(bezie);
        }

        private static LineRenderer GetLine(Color color, float width)
        {
            var line = lines.Get();
            line.material = lineMaterial;
            line.enabled = false;
            line.startColor = color;
            line.endColor = color;
            line.startWidth = width;
            line.endWidth = width;
            return line;
        }

        private static void ClearRenderCommand()
        {
            commandBuffer.Clear();
        }
        
        private static void AddDrawRenderer(Renderer renderer)
        {
            commandBuffer.DrawRenderer(renderer, renderer.sharedMaterial);
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