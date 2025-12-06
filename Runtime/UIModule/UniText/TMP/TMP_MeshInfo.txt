using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Rendering;


namespace TMPro
{
    public enum VertexSortingOrder
    {
        Normal,
        Reverse
    };

    public struct TMP_MeshInfo
    {
        private static readonly Color32
            s_DefaultColor = new(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);

        private static readonly Vector3 s_DefaultNormal = new(0.0f, 0.0f, -1f);
        private static readonly Vector4 s_DefaultTangent = new(-1f, 0.0f, 0.0f, 1f);
        private static readonly Bounds s_DefaultBounds = new();

        public Mesh mesh;
        public int vertexCount;

        public Vector3[] vertices;
        public Vector3[] normals;
        public Vector4[] tangents;

        public Vector4[] uvs0;

        public Vector2[] uvs2;

        public Color32[] colors32;
        public int[] triangles;

        public Material material;


        public TMP_MeshInfo(Mesh mesh, int size)
        {
            if (mesh == null)
                mesh = new();
            else
                mesh.Clear();

            this.mesh = mesh;

            size = Mathf.Min(size, 16383);

            int sizeX4 = size * 4;
            int sizeX6 = size * 6;

            vertexCount = 0;

            vertices = new Vector3[sizeX4];
            uvs0 = new Vector4[sizeX4];
            uvs2 = new Vector2[sizeX4];
            colors32 = new Color32[sizeX4];

            normals = new Vector3[sizeX4];
            tangents = new Vector4[sizeX4];

            triangles = new int[sizeX6];

            int index_X6 = 0;
            int index_X4 = 0;
            while (index_X4 / 4 < size)
            {
                for (int i = 0; i < 4; i++)
                {
                    vertices[index_X4 + i] = Vector3.zero;
                    uvs0[index_X4 + i] = Vector2.zero;
                    uvs2[index_X4 + i] = Vector2.zero;
                    colors32[index_X4 + i] = s_DefaultColor;
                    normals[index_X4 + i] = s_DefaultNormal;
                    tangents[index_X4 + i] = s_DefaultTangent;
                }

                triangles[index_X6 + 0] = index_X4 + 0;
                triangles[index_X6 + 1] = index_X4 + 1;
                triangles[index_X6 + 2] = index_X4 + 2;
                triangles[index_X6 + 3] = index_X4 + 2;
                triangles[index_X6 + 4] = index_X4 + 3;
                triangles[index_X6 + 5] = index_X4 + 0;

                index_X4 += 4;
                index_X6 += 6;
            }

            this.mesh.vertices = vertices;
            this.mesh.normals = normals;
            this.mesh.tangents = tangents;
            this.mesh.triangles = triangles;
            this.mesh.bounds = s_DefaultBounds;
            material = null;
        }


        public TMP_MeshInfo(Mesh mesh, int size, bool isVolumetric)
        {
            if (mesh == null)
                mesh = new();
            else
                mesh.Clear();

            this.mesh = mesh;

            int s0 = !isVolumetric ? 4 : 8;
            int s1 = !isVolumetric ? 6 : 36;

            size = Mathf.Min(size, 65532 / s0);

            int size_x_s0 = size * s0;
            int size_x_s1 = size * s1;

            vertexCount = 0;

            vertices = new Vector3[size_x_s0];
            uvs0 = new Vector4[size_x_s0];
            uvs2 = new Vector2[size_x_s0];
            colors32 = new Color32[size_x_s0];

            normals = new Vector3[size_x_s0];
            tangents = new Vector4[size_x_s0];

            triangles = new int[size_x_s1];

            int index_x_s0 = 0;
            int index_x_s1 = 0;
            while (index_x_s0 / s0 < size)
            {
                for (int i = 0; i < s0; i++)
                {
                    vertices[index_x_s0 + i] = Vector3.zero;
                    uvs0[index_x_s0 + i] = Vector2.zero;
                    uvs2[index_x_s0 + i] = Vector2.zero;
                    colors32[index_x_s0 + i] = s_DefaultColor;
                    normals[index_x_s0 + i] = s_DefaultNormal;
                    tangents[index_x_s0 + i] = s_DefaultTangent;
                }

                triangles[index_x_s1 + 0] = index_x_s0 + 0;
                triangles[index_x_s1 + 1] = index_x_s0 + 1;
                triangles[index_x_s1 + 2] = index_x_s0 + 2;
                triangles[index_x_s1 + 3] = index_x_s0 + 2;
                triangles[index_x_s1 + 4] = index_x_s0 + 3;
                triangles[index_x_s1 + 5] = index_x_s0 + 0;

                if (isVolumetric)
                {
                    triangles[index_x_s1 + 6] = index_x_s0 + 4;
                    triangles[index_x_s1 + 7] = index_x_s0 + 5;
                    triangles[index_x_s1 + 8] = index_x_s0 + 1;
                    triangles[index_x_s1 + 9] = index_x_s0 + 1;
                    triangles[index_x_s1 + 10] = index_x_s0 + 0;
                    triangles[index_x_s1 + 11] = index_x_s0 + 4;

                    triangles[index_x_s1 + 12] = index_x_s0 + 3;
                    triangles[index_x_s1 + 13] = index_x_s0 + 2;
                    triangles[index_x_s1 + 14] = index_x_s0 + 6;
                    triangles[index_x_s1 + 15] = index_x_s0 + 6;
                    triangles[index_x_s1 + 16] = index_x_s0 + 7;
                    triangles[index_x_s1 + 17] = index_x_s0 + 3;

                    triangles[index_x_s1 + 18] = index_x_s0 + 1;
                    triangles[index_x_s1 + 19] = index_x_s0 + 5;
                    triangles[index_x_s1 + 20] = index_x_s0 + 6;
                    triangles[index_x_s1 + 21] = index_x_s0 + 6;
                    triangles[index_x_s1 + 22] = index_x_s0 + 2;
                    triangles[index_x_s1 + 23] = index_x_s0 + 1;

                    triangles[index_x_s1 + 24] = index_x_s0 + 4;
                    triangles[index_x_s1 + 25] = index_x_s0 + 0;
                    triangles[index_x_s1 + 26] = index_x_s0 + 3;
                    triangles[index_x_s1 + 27] = index_x_s0 + 3;
                    triangles[index_x_s1 + 28] = index_x_s0 + 7;
                    triangles[index_x_s1 + 29] = index_x_s0 + 4;

                    triangles[index_x_s1 + 30] = index_x_s0 + 7;
                    triangles[index_x_s1 + 31] = index_x_s0 + 6;
                    triangles[index_x_s1 + 32] = index_x_s0 + 5;
                    triangles[index_x_s1 + 33] = index_x_s0 + 5;
                    triangles[index_x_s1 + 34] = index_x_s0 + 4;
                    triangles[index_x_s1 + 35] = index_x_s0 + 7;
                }

                index_x_s0 += s0;
                index_x_s1 += s1;
            }

            this.mesh.vertices = vertices;
            this.mesh.normals = normals;
            this.mesh.tangents = tangents;
            this.mesh.triangles = triangles;
            this.mesh.bounds = s_DefaultBounds;
            material = null;
        }


        public void ResizeMeshInfo(int size)
        {
            size = Mathf.Min(size, 16383);

            int size_X4 = size * 4;
            int size_X6 = size * 6;

            int previousSize = vertices.Length / 4;

            Array.Resize(ref vertices, size_X4);
            Array.Resize(ref normals, size_X4);
            Array.Resize(ref tangents, size_X4);

            Array.Resize(ref uvs0, size_X4);
            Array.Resize(ref uvs2, size_X4);

            Array.Resize(ref colors32, size_X4);

            Array.Resize(ref triangles, size_X6);


            if (size <= previousSize)
            {
                mesh.triangles = triangles;
                mesh.vertices = vertices;
                mesh.normals = normals;
                mesh.tangents = tangents;

                return;
            }

            for (int i = previousSize; i < size; i++)
            {
                int index_X4 = i * 4;
                int index_X6 = i * 6;

                normals[0 + index_X4] = s_DefaultNormal;
                normals[1 + index_X4] = s_DefaultNormal;
                normals[2 + index_X4] = s_DefaultNormal;
                normals[3 + index_X4] = s_DefaultNormal;

                tangents[0 + index_X4] = s_DefaultTangent;
                tangents[1 + index_X4] = s_DefaultTangent;
                tangents[2 + index_X4] = s_DefaultTangent;
                tangents[3 + index_X4] = s_DefaultTangent;

                triangles[0 + index_X6] = 0 + index_X4;
                triangles[1 + index_X6] = 1 + index_X4;
                triangles[2 + index_X6] = 2 + index_X4;
                triangles[3 + index_X6] = 2 + index_X4;
                triangles[4 + index_X6] = 3 + index_X4;
                triangles[5 + index_X6] = 0 + index_X4;
            }

            mesh.vertices = vertices;
            mesh.normals = normals;
            mesh.tangents = tangents;
            mesh.triangles = triangles;
        }


        public void ResizeMeshInfo(int size, bool isVolumetric)
        {
            int s0 = !isVolumetric ? 4 : 8;
            int s1 = !isVolumetric ? 6 : 36;

            size = Mathf.Min(size, 65532 / s0);

            int size_X4 = size * s0;
            int size_X6 = size * s1;

            int previousSize = vertices.Length / s0;

            Array.Resize(ref vertices, size_X4);
            Array.Resize(ref normals, size_X4);
            Array.Resize(ref tangents, size_X4);

            Array.Resize(ref uvs0, size_X4);
            Array.Resize(ref uvs2, size_X4);

            Array.Resize(ref colors32, size_X4);

            Array.Resize(ref triangles, size_X6);


            if (size <= previousSize)
            {
                mesh.triangles = triangles;
                mesh.vertices = vertices;
                mesh.normals = normals;
                mesh.tangents = tangents;

                return;
            }

            for (int i = previousSize; i < size; i++)
            {
                int index_X4 = i * s0;
                int index_X6 = i * s1;

                normals[0 + index_X4] = s_DefaultNormal;
                normals[1 + index_X4] = s_DefaultNormal;
                normals[2 + index_X4] = s_DefaultNormal;
                normals[3 + index_X4] = s_DefaultNormal;

                tangents[0 + index_X4] = s_DefaultTangent;
                tangents[1 + index_X4] = s_DefaultTangent;
                tangents[2 + index_X4] = s_DefaultTangent;
                tangents[3 + index_X4] = s_DefaultTangent;

                if (isVolumetric)
                {
                    normals[4 + index_X4] = s_DefaultNormal;
                    normals[5 + index_X4] = s_DefaultNormal;
                    normals[6 + index_X4] = s_DefaultNormal;
                    normals[7 + index_X4] = s_DefaultNormal;

                    tangents[4 + index_X4] = s_DefaultTangent;
                    tangents[5 + index_X4] = s_DefaultTangent;
                    tangents[6 + index_X4] = s_DefaultTangent;
                    tangents[7 + index_X4] = s_DefaultTangent;
                }

                triangles[0 + index_X6] = 0 + index_X4;
                triangles[1 + index_X6] = 1 + index_X4;
                triangles[2 + index_X6] = 2 + index_X4;
                triangles[3 + index_X6] = 2 + index_X4;
                triangles[4 + index_X6] = 3 + index_X4;
                triangles[5 + index_X6] = 0 + index_X4;

                if (isVolumetric)
                {
                    triangles[index_X6 + 6] = index_X4 + 4;
                    triangles[index_X6 + 7] = index_X4 + 5;
                    triangles[index_X6 + 8] = index_X4 + 1;
                    triangles[index_X6 + 9] = index_X4 + 1;
                    triangles[index_X6 + 10] = index_X4 + 0;
                    triangles[index_X6 + 11] = index_X4 + 4;

                    triangles[index_X6 + 12] = index_X4 + 3;
                    triangles[index_X6 + 13] = index_X4 + 2;
                    triangles[index_X6 + 14] = index_X4 + 6;
                    triangles[index_X6 + 15] = index_X4 + 6;
                    triangles[index_X6 + 16] = index_X4 + 7;
                    triangles[index_X6 + 17] = index_X4 + 3;

                    triangles[index_X6 + 18] = index_X4 + 1;
                    triangles[index_X6 + 19] = index_X4 + 5;
                    triangles[index_X6 + 20] = index_X4 + 6;
                    triangles[index_X6 + 21] = index_X4 + 6;
                    triangles[index_X6 + 22] = index_X4 + 2;
                    triangles[index_X6 + 23] = index_X4 + 1;

                    triangles[index_X6 + 24] = index_X4 + 4;
                    triangles[index_X6 + 25] = index_X4 + 0;
                    triangles[index_X6 + 26] = index_X4 + 3;
                    triangles[index_X6 + 27] = index_X4 + 3;
                    triangles[index_X6 + 28] = index_X4 + 7;
                    triangles[index_X6 + 29] = index_X4 + 4;

                    triangles[index_X6 + 30] = index_X4 + 7;
                    triangles[index_X6 + 31] = index_X4 + 6;
                    triangles[index_X6 + 32] = index_X4 + 5;
                    triangles[index_X6 + 33] = index_X4 + 5;
                    triangles[index_X6 + 34] = index_X4 + 4;
                    triangles[index_X6 + 35] = index_X4 + 7;
                }
            }

            mesh.vertices = vertices;
            mesh.normals = normals;
            mesh.tangents = tangents;
            mesh.triangles = triangles;
        }


        public void Clear()
        {
            if (vertices == null) return;

            Array.Clear(vertices, 0, vertices.Length);
            vertexCount = 0;

            if (mesh != null)
                mesh.vertices = vertices;
        }


        public void Clear(bool uploadChanges)
        {
            if (vertices == null) return;

            Array.Clear(vertices, 0, vertices.Length);
            vertexCount = 0;

            if (uploadChanges && mesh != null)
                mesh.vertices = vertices;

            if (mesh != null)
                mesh.bounds = s_DefaultBounds;
        }


        public void ClearUnusedVertices()
        {
            int length = vertices.Length - vertexCount;

            if (length > 0)
                Array.Clear(vertices, vertexCount, length);
        }


        public void ClearUnusedVertices(int startIndex)
        {
            int length = vertices.Length - startIndex;

            if (length > 0)
                Array.Clear(vertices, startIndex, length);
        }


        public void ClearUnusedVertices(int startIndex, bool updateMesh)
        {
            int length = vertices.Length - startIndex;

            if (length > 0)
                Array.Clear(vertices, startIndex, length);

            if (updateMesh && mesh != null)
                mesh.vertices = vertices;
        }


        public void SortGeometry(VertexSortingOrder order)
        {
            switch (order)
            {
                case VertexSortingOrder.Normal:
                    break;
                case VertexSortingOrder.Reverse:
                    int size = vertexCount / 4;
                    for (int i = 0; i < size; i++)
                    {
                        int src = i * 4;
                        int dst = (size - i - 1) * 4;

                        if (src < dst)
                            SwapVertexData(src, dst);
                    }

                    break;
            }
        }


        public void SortGeometry(IList<int> sortingOrder)
        {
            int indexCount = sortingOrder.Count;

            if (indexCount * 4 > vertices.Length) return;

            int src_index;

            for (int dst_index = 0; dst_index < indexCount; dst_index++)
            {
                src_index = sortingOrder[dst_index];

                while (src_index < dst_index)
                {
                    src_index = sortingOrder[src_index];
                }

                if (src_index != dst_index)
                    SwapVertexData(src_index * 4, dst_index * 4);
            }
        }


        public void SwapVertexData(int src, int dst)
        {
            int src_Index = src;
            int dst_Index = dst;

            Vector3 vertex;
            vertex = vertices[dst_Index + 0];
            vertices[dst_Index + 0] = vertices[src_Index + 0];
            vertices[src_Index + 0] = vertex;

            vertex = vertices[dst_Index + 1];
            vertices[dst_Index + 1] = vertices[src_Index + 1];
            vertices[src_Index + 1] = vertex;

            vertex = vertices[dst_Index + 2];
            vertices[dst_Index + 2] = vertices[src_Index + 2];
            vertices[src_Index + 2] = vertex;

            vertex = vertices[dst_Index + 3];
            vertices[dst_Index + 3] = vertices[src_Index + 3];
            vertices[src_Index + 3] = vertex;


            Vector4 uvs;
            uvs = uvs0[dst_Index + 0];
            uvs0[dst_Index + 0] = uvs0[src_Index + 0];
            uvs0[src_Index + 0] = uvs;

            uvs = uvs0[dst_Index + 1];
            uvs0[dst_Index + 1] = uvs0[src_Index + 1];
            uvs0[src_Index + 1] = uvs;

            uvs = uvs0[dst_Index + 2];
            uvs0[dst_Index + 2] = uvs0[src_Index + 2];
            uvs0[src_Index + 2] = uvs;

            uvs = uvs0[dst_Index + 3];
            uvs0[dst_Index + 3] = uvs0[src_Index + 3];
            uvs0[src_Index + 3] = uvs;

            uvs = uvs2[dst_Index + 0];
            uvs2[dst_Index + 0] = uvs2[src_Index + 0];
            uvs2[src_Index + 0] = uvs;

            uvs = uvs2[dst_Index + 1];
            uvs2[dst_Index + 1] = uvs2[src_Index + 1];
            uvs2[src_Index + 1] = uvs;

            uvs = uvs2[dst_Index + 2];
            uvs2[dst_Index + 2] = uvs2[src_Index + 2];
            uvs2[src_Index + 2] = uvs;

            uvs = uvs2[dst_Index + 3];
            uvs2[dst_Index + 3] = uvs2[src_Index + 3];
            uvs2[src_Index + 3] = uvs;

            Color32 color;
            color = colors32[dst_Index + 0];
            colors32[dst_Index + 0] = colors32[src_Index + 0];
            colors32[src_Index + 0] = color;

            color = colors32[dst_Index + 1];
            colors32[dst_Index + 1] = colors32[src_Index + 1];
            colors32[src_Index + 1] = color;

            color = colors32[dst_Index + 2];
            colors32[dst_Index + 2] = colors32[src_Index + 2];
            colors32[src_Index + 2] = color;

            color = colors32[dst_Index + 3];
            colors32[dst_Index + 3] = colors32[src_Index + 3];
            colors32[src_Index + 3] = color;
        }
    }
}