#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

namespace LSCore
{
    public class PostProcessUIGradientAssets : AssetModificationProcessor
    {
        static string[] OnWillSaveAssets(string[] paths)
        {
            UIGradientutility.UpdateAllGradients();
            List<string> newpaths = new List<string>();
            //if material is gradient material skip it.
            foreach (string path in paths)
            {
                if (!(string.Equals(Path.GetExtension(path), ".mat") && IsGradientMat(path)))
                {
                    newpaths.Add(path);
                }
            }
            return newpaths.ToArray();
        }

        static bool IsGradientMat(string path)
        {
            Material mat = (Material)AssetDatabase.LoadAssetAtPath(path, typeof(Material));
            Shader gradientshader = Shader.Find(UIGradient.GradientShaderPath);
            if (mat.shader == gradientshader)
            {
                return true;
            }
            return false;
        }
    }

    /// <summary>
    /// Menu utiity to refresh all gradients.
    /// Not required in any cases so far.
    /// </summary>
    public class UIGradientutility : MonoBehaviour
    {
        [MenuItem("Window/UI Gradient/Refresh Gradients")]
        public static void UpdateAllGradients()
        {
            UIGradient[] allGradients = FindObjectsByType<UIGradient>(FindObjectsSortMode.None);
            foreach (var item in allGradients)
            {
                if (item.gameObject.activeInHierarchy)
                {
                    item.UpdateMaterial();
                }
            }
        }
    }
}

#endif