using System;
using System.Collections.Generic;
using UnityEngine.Pool;

namespace UnityEngine.UI
{
    public class LSVertexHelper : IDisposable
    {
        private List<Vector3> positions;
        private List<Color32> colors;
        private List<Vector4> uvs0;
        private List<Vector4> uvs1;
        private List<Vector4> uvs2;
        private List<Vector4> uvs3;
        private List<Vector3> normals;
        private List<Vector4> tangents;
        private List<int> indices;

        private static readonly Vector4 defaultTangent = new Vector4(1.0f, 0.0f, 0.0f, -1.0f);
        private static readonly Vector3 defaultNormal = Vector3.back;

        private bool listsInitalized = false;

        public LSVertexHelper()
        {}

        public LSVertexHelper(Mesh m)
        {
            InitializeListIfRequired();

            positions.AddRange(m.vertices);
            colors.AddRange(m.colors32);
            List<Vector4> tempUVList = new List<Vector4>();
            m.GetUVs(0, tempUVList);
            uvs0.AddRange(tempUVList);
            m.GetUVs(1, tempUVList);
            uvs1.AddRange(tempUVList);
            m.GetUVs(2, tempUVList);
            uvs2.AddRange(tempUVList);
            m.GetUVs(3, tempUVList);
            uvs3.AddRange(tempUVList);
            normals.AddRange(m.normals);
            tangents.AddRange(m.tangents);
            indices.AddRange(m.GetIndices(0));
        }

        public VertexHelper ToLegacy()
        {
            var vh = new VertexHelper();
            var zero = Vector4.zero;
            for (int i = 0; i < currentVertCount; i++)
            {
                vh.AddVert(positions[i], colors[i], uvs0[i], i < uvs1.Count ? uvs1[i] : zero, i < uvs2.Count ? uvs2[i] : zero, i < uvs3.Count ? uvs3[i] : zero, defaultNormal, defaultTangent );
            }

            for (int i = 0; i < currentIndexCount / 3; i++)
            {
                vh.AddTriangle(indices[i], indices[i+1], indices[i+2]);
            }

            return vh;
        }
        
        private void InitializeListIfRequired()
        {
            if (!listsInitalized)
            {
                positions = ListPool<Vector3>.Get();
                colors = ListPool<Color32>.Get();
                uvs0 = ListPool<Vector4>.Get();
                uvs1 = ListPool<Vector4>.Get();
                uvs2 = ListPool<Vector4>.Get();
                uvs3 = ListPool<Vector4>.Get();
                normals = ListPool<Vector3>.Get();
                tangents = ListPool<Vector4>.Get();
                indices = ListPool<int>.Get();
                listsInitalized = true;
            }
        }

        /// <summary>
        /// Cleanup allocated memory.
        /// </summary>
        public void Dispose()
        {
            if (listsInitalized)
            {
                ListPool<Vector3>.Release(positions);
                ListPool<Color32>.Release(colors);
                ListPool<Vector4>.Release(uvs0);
                ListPool<Vector4>.Release(uvs1);
                ListPool<Vector4>.Release(uvs2);
                ListPool<Vector4>.Release(uvs3);
                ListPool<Vector3>.Release(normals);
                ListPool<Vector4>.Release(tangents);
                ListPool<int>.Release(indices);

                positions = null;
                colors = null;
                uvs0 = null;
                uvs1 = null;
                uvs2 = null;
                uvs3 = null;
                normals = null;
                tangents = null;
                indices = null;

                listsInitalized = false;
            }
        }

        /// <summary>
        /// Clear all vertices from the stream.
        /// </summary>
        public void Clear()
        {
            // Only clear if we have our lists created.
            if (listsInitalized)
            {
                positions.Clear();
                colors.Clear();
                uvs0.Clear();
                uvs1.Clear();
                uvs2.Clear();
                uvs3.Clear();
                normals.Clear();
                tangents.Clear();
                indices.Clear();
            }
        }

        /// <summary>
        /// Current number of vertices in the buffer.
        /// </summary>
        public int currentVertCount
        {
            get { return positions != null ? positions.Count : 0; }
        }

        /// <summary>
        /// Get the number of indices set on the VertexHelper.
        /// </summary>
        public int currentIndexCount
        {
            get { return indices != null ? indices.Count : 0; }
        }

        /// <summary>
        /// Fill a UIVertex with data from index i of the stream.
        /// </summary>
        /// <param name="vertex">Vertex to populate</param>
        /// <param name="i">Index to populate.</param>
        public void PopulateUIVertex(ref UIVertex vertex, in int i)
        {
            InitializeListIfRequired();

            vertex.position = positions[i];
            vertex.color = colors[i];
            vertex.uv0 = uvs0[i];
            vertex.uv1 = uvs1[i];
            vertex.uv2 = uvs2[i];
            vertex.uv3 = uvs3[i];
            vertex.normal = normals[i];
            vertex.tangent = tangents[i];
        }

        /// <summary>
        /// Set a UIVertex at the given index.
        /// </summary>
        /// <param name="vertex">The vertex to fill</param>
        /// <param name="i">the position in the current list to fill.</param>
        public void SetUIVertex(in UIVertex vertex, in int i)
        {
            InitializeListIfRequired();

            positions[i] = vertex.position;
            colors[i] = vertex.color;
            uvs0[i] = vertex.uv0;
            uvs1[i] = vertex.uv1;
            uvs2[i] = vertex.uv2;
            uvs3[i] = vertex.uv3;
            normals[i] = vertex.normal;
            tangents[i] = vertex.tangent;
        }

        /// <summary>
        /// Fill the given mesh with the stream data.
        /// </summary>
        public void FillMesh(Mesh mesh)
        {
            InitializeListIfRequired();

            mesh.Clear();

            if (positions.Count >= 65000)
                throw new ArgumentException("Mesh can not have more than 65000 vertices");

            mesh.SetVertices(positions);
            mesh.SetColors(colors);
            mesh.SetUVs(0, uvs0);
            mesh.SetUVs(1, uvs1);
            mesh.SetUVs(2, uvs2);
            mesh.SetUVs(3, uvs3);
            mesh.SetNormals(normals);
            mesh.SetTangents(tangents);
            mesh.SetTriangles(indices, 0);
            mesh.RecalculateBounds();
        }

        /// <summary>
        /// Add a single vertex to the stream.
        /// </summary>
        /// <param name="position">Position of the vert</param>
        /// <param name="color">Color of the vert</param>
        /// <param name="uv0">UV of the vert</param>
        /// <param name="uv1">UV1 of the vert</param>
        /// <param name="uv2">UV2 of the vert</param>
        /// <param name="uv3">UV3 of the vert</param>
        /// <param name="normal">Normal of the vert.</param>
        /// <param name="tangent">Tangent of the vert</param>
        public void AddVert(in Vector3 position, in Color32 color, in Vector4 uv0, in Vector4 uv1, in Vector4 uv2, in Vector4 uv3, in Vector3 normal, in Vector4 tangent)
        {
            InitializeListIfRequired();

            positions.Add(position);
            colors.Add(color);
            uvs0.Add(uv0);
            uvs1.Add(uv1);
            uvs2.Add(uv2);
            uvs3.Add(uv3);
            normals.Add(normal);
            tangents.Add(tangent);
        }
        
        public void AddVert(in Vector3 position, in Color32 color, in Vector4 uv0, in Vector4 uv1, in Vector4 uv2, in Vector4 uv3)
        {
            InitializeListIfRequired();

            positions.Add(position);
            colors.Add(color);
            uvs0.Add(uv0);
            uvs1.Add(uv1);
            uvs2.Add(uv2);
            uvs3.Add(uv3);
            normals.Add(defaultNormal);
            tangents.Add(defaultTangent);
        }

        /// <summary>
        /// Add a single vertex to the stream.
        /// </summary>
        /// <param name="position">Position of the vert</param>
        /// <param name="color">Color of the vert</param>
        /// <param name="uv0">UV of the vert</param>
        /// <param name="uv1">UV1 of the vert</param>
        /// <param name="normal">Normal of the vert.</param>
        /// <param name="tangent">Tangent of the vert</param>
        public void AddVert(in Vector3 position, in Color32 color, in Vector4 uv0, in Vector4 uv1, in Vector3 normal, in Vector4 tangent)
        {
            AddVert(position, color, uv0, uv1, Vector4.zero, Vector4.zero, normal, tangent);
        }

        /// <summary>
        /// Add a single vertex to the stream.
        /// </summary>
        /// <param name="position">Position of the vert</param>
        /// <param name="color">Color of the vert</param>
        /// <param name="uv0">UV of the vert</param>
        public void AddVert(in Vector3 position, in Color32 color, in Vector4 uv0)
        {
            AddVert(position, color, uv0, Vector4.zero, defaultNormal, defaultTangent);
        }

        /// <summary>
        /// Add a single vertex to the stream.
        /// </summary>
        /// <param name="v">The vertex to add</param>
        public void AddVert(in UIVertex v)
        {
            AddVert(v.position, v.color, v.uv0, v.uv1, v.uv2, v.uv3, v.normal, v.tangent);
        }

        /// <summary>
        /// Add a triangle to the buffer.
        /// </summary>
        /// <param name="idx0">index 0</param>
        /// <param name="idx1">index 1</param>
        /// <param name="idx2">index 2</param>
        public void AddTriangle(in int idx0, in int idx1, in int idx2)
        {
            InitializeListIfRequired();

            indices.Add(idx0);
            indices.Add(idx1);
            indices.Add(idx2);
        }

        /// <summary>
        /// Add a quad to the stream.
        /// </summary>
        /// <param name="verts">4 Vertices representing the quad.</param>
        public void AddUIVertexQuad(UIVertex[] verts)
        {
            int startIndex = currentVertCount;

            for (int i = 0; i < 4; i++)
                AddVert(verts[i].position, verts[i].color, verts[i].uv0, verts[i].uv1, verts[i].normal, verts[i].tangent);

            AddTriangle(startIndex, startIndex + 1, startIndex + 2);
            AddTriangle(startIndex + 2, startIndex + 3, startIndex);
        }

        /// <summary>
        /// Add a stream of custom UIVertex and corresponding indices.
        /// </summary>
        /// <param name="verts">The custom stream of verts to add to the helpers internal data.</param>
        /// <param name="indices">The custom stream of indices to add to the helpers internal data.</param>
        public void AddUIVertexStream(List<UIVertex> verts, List<int> indices)
        {
            InitializeListIfRequired();

            if (verts != null)
            {
                CanvasRenderer.AddUIVertexStream(verts, positions, colors, uvs0, uvs1, uvs2, uvs3, normals, tangents);
            }

            if (indices != null)
            {
                indices.AddRange(indices);
            }
        }

        /// <summary>
        /// Add a list of triangles to the stream.
        /// </summary>
        /// <param name="verts">Vertices to add. Length should be divisible by 3.</param>
        public void AddUIVertexTriangleStream(List<UIVertex> verts)
        {
            if (verts == null)
                return;

            InitializeListIfRequired();

            CanvasRenderer.SplitUIVertexStreams(verts, positions, colors, uvs0, uvs1, uvs2, uvs3, normals, tangents, indices);
        }

        /// <summary>
        /// Create a stream of UI vertex (in triangles) from the stream.
        /// </summary>
        public void GetUIVertexStream(List<UIVertex> stream)
        {
            if (stream == null)
                return;

            InitializeListIfRequired();

            CanvasRenderer.CreateUIVertexStream(stream, positions, colors, uvs0, uvs1, uvs2, uvs3, normals, tangents, indices);
        }
    }
}
