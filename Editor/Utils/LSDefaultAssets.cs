using UnityEditor;
using UnityEngine;

public static class LSDefaultAssets
{
    private static Material lineMaterial;
    public static Material LineMaterial
    {
        get
        {
            lineMaterial ??= AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/Materials/Default-Line.mat");
        
            if (lineMaterial == null)
            {
                lineMaterial = new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply"));
                AssetDatabase.CreateAsset(lineMaterial, "Assets/Art/Materials/Default-Line.mat");
            }

            return lineMaterial;
        }
    }
}
