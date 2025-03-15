using System;
using LSCore.Extensions.Unity;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.UI;

namespace LSCore
{
    public class LSRawImage : RawImage
    {
        public enum RotationMode
        {
            None = 0,
            D90 = 1,
            D180 = 2,
            D270 = 3,
        }
        
        private static readonly LSVertexHelper vertexHelper = new();
        
        static LSRawImage()
        {
            vertexHelper.Init();
        }
        
        public delegate void RotateAction(ref Vector3 value, in Vector2 center);
        private RotateAction rotateAction;
        
        [SerializeField] private bool preserveAspectRatio;
        [SerializeField] private int rotateId = 0;
        
        public RotationMode Rotation
        {
            get => (RotationMode)rotateId;
            set
            {
                rotateId = (int)value;
                SetVerticesDirty();
            }
        }
        
        public bool PreserveAspectRatio
        {
            get { return preserveAspectRatio; }
            set
            {
                preserveAspectRatio = value;
                SetVerticesDirty();
            }
        }
        
        protected override void UpdateGeometry()
        {
            DoMeshGeneration();
        }

        private void DoMeshGeneration()
        {
            Action<Mesh> fillMesh = vertexHelper.FillMeshUI;
            vertexHelper.Clear();

            if (rectTransform != null && rectTransform.rect is { width: > 0, height: > 0 })
            {
                OnPopulateMesh(vertexHelper);
            }

            var mesh = workerMesh;
            fillMesh(mesh);
            canvasRenderer.SetMesh(mesh);
        }
        
        protected void OnPopulateMesh(LSVertexHelper vh)
        {
            Texture tex = mainTexture;
            vh.Clear();
            if(tex == null) return;
            
            if (preserveAspectRatio)
            {
                float texAspect = tex.AspectRatio();
                Rect r = GetPixelAdjustedRect();
        
                Vector2 pivot = rectTransform.pivot;
        
                float newWidth, newHeight;
                float rAspect = r.width / r.height;
                if (rAspect > texAspect)
                {
                    newHeight = r.height;
                    newWidth = newHeight * texAspect;
                }
                else
                {
                    newWidth = r.width;
                    newHeight = newWidth / texAspect;
                }
        
                float offsetX = r.x + r.width * pivot.x - newWidth * pivot.x;
                float offsetY = r.y + r.height * pivot.y - newHeight * pivot.y;
        
                float xMin = offsetX;
                float yMin = offsetY;
                float xMax = offsetX + newWidth;
                float yMax = offsetY + newHeight;
        
                Color32 color32 = color;
                vh.AddVert(new Vector3(xMin, yMin), color32, new Vector2(0, 0));
                vh.AddVert(new Vector3(xMin, yMax), color32, new Vector2(0, 1));
                vh.AddVert(new Vector3(xMax, yMax), color32, new Vector2(1, 1));
                vh.AddVert(new Vector3(xMax, yMin), color32, new Vector2(1, 0));
        
                vh.AddTriangle(0, 1, 2);
                vh.AddTriangle(2, 3, 0);
            }
            else
            {
                var r = GetPixelAdjustedRect();
                var v = new Vector4(r.x, r.y, r.x + r.width, r.y + r.height);
                var scaleX = tex.width * tex.texelSize.x;
                var scaleY = tex.height * tex.texelSize.y;
                {
                    var color32 = color;
                    vh.AddVert(new Vector3(v.x, v.y), color32, new Vector2(0, 0));
                    vh.AddVert(new Vector3(v.x, v.w), color32, new Vector2(0, scaleY));
                    vh.AddVert(new Vector3(v.z, v.w), color32, new Vector2(scaleX, scaleY));
                    vh.AddVert(new Vector3(v.z, v.y), color32, new Vector2(scaleX, 0));

                    vh.AddTriangle(0, 1, 2);
                    vh.AddTriangle(2, 3, 0);
                }
            }
            
            PostProcessMesh(vh);
        }
        
        private void PostProcessMesh(LSVertexHelper vh)
        {
            RotateMesh(vh);
        }
        
        private void RotateMesh(LSVertexHelper vh)
        {
            if(rotateId == 0) return;
            
            UIVertex vert = new UIVertex();
            var count = vh.currentVertCount;
            var center = rectTransform.rect.center * 2;

            rotateAction = rotateId switch
            {
                1 => Rotate90,
                2 => Rotate180,
                3 => Rotate270,
                _ => null
            };
            
            for (int i = 0; i < count; i++)
            {
                vh.PopulateUIVertex(ref vert, i);
                var pos = vert.position;
                rotateAction(ref pos, center);
                vert.position = pos;
                vh.SetUIVertex(vert, i);
            }

            count = vh.currentIndexCount / 6 * 4;
            vh.ClearTriangles();
            
            if (rotateId % 2 == 1)
            {
                for (int i = 0; i < count; i += 4)
                {
                    vh.AddTriangle(i+1, i + 2, i+3);
                    vh.AddTriangle(i+3, i, i+1);
                }
            }
            else
            {
                for (int i = 0; i < count; i += 4)
                {
                    vh.AddTriangle(i, i + 1, i + 2);
                    vh.AddTriangle(i + 2, i + 3, i);
                }
            }

        }
        
        private void Rotate90(ref Vector3 pos, in Vector2 center)
        {
            var x = pos.x;
            pos.x = -pos.y + center.x;
            pos.y = x;
        }
        
        private void Rotate180(ref Vector3 pos, in Vector2 center)
        {
            pos.x = -pos.x + center.x;
            pos.y = -pos.y + center.y;
        }
        
        private void Rotate270(ref Vector3 pos, in Vector2 center)
        {
            var x = pos.x;
            pos.x = pos.y;
            pos.y = -x + center.y;
        }
    }
    
#if UNITY_EDITOR
    [CustomEditor(typeof(LSRawImage), true)]
    [CanEditMultipleObjects]
    public class LSRawImageEditor : RawImageEditor
    {
        LSRawImage image;
        SerializedProperty m_Texture;
        SerializedProperty preserveAspectRatio;
        SerializedProperty rotateId;

        protected override void OnEnable()
        {
            base.OnEnable();
            image = (LSRawImage)target;
            m_Texture = serializedObject.FindProperty("m_Texture");
            preserveAspectRatio = serializedObject.FindProperty("preserveAspectRatio");
            rotateId = serializedObject.FindProperty("rotateId");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(m_Texture);

            AppearanceControlsGUI();
            RaycastControlsGUI();
            MaskableControlsGUI();
            SetShowNativeSize(m_Texture.objectReferenceValue != null, false);
            NativeSizeButtonGUI();

            serializedObject.ApplyModifiedProperties();
            
            EditorGUILayout.PropertyField(preserveAspectRatio);
            DrawRotateButton();
            
            serializedObject.ApplyModifiedProperties();
        }
        
        protected virtual void DrawRotateButton()
        {
            GUILayout.Space(10);
            GUILayout.BeginHorizontal();

            for (int i = 0; i < 4; i++)
            {
                var targetAngle = i * 90;
                var text = rotateId.intValue == i ? $"{targetAngle}° ❤️" : $"{targetAngle}°";
                if (GUILayout.Button(text, GUILayout.Height(30)) && rotateId.intValue != i)
                {
                    rotateId.intValue = i;
                    image.SetVerticesDirty();
                }
            }

            GUILayout.EndHorizontal();
        }
    }
#endif
}