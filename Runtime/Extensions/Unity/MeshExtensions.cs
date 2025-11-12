using UnityEngine;

public static class MeshExtensions
{
    public static void Scale(this Mesh mesh, Vector3 scale, Vector3 pivot)
    {
        var verts = mesh.vertices;
        for (int i = 0; i < verts.Length; i++)
        {
            var v = verts[i] - pivot;
            v = Vector3.Scale(v, scale);
            verts[i] = v + pivot;
        }
        
        mesh.vertices = verts;
    }
}