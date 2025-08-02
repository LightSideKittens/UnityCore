using System.Collections.Generic;
using System.Diagnostics;
using Sirenix.OdinInspector;
using UnityEngine.SceneManagement;

namespace LSCore.Attributes
{
    [Conditional("UNITY_EDITOR")]
    public class SceneSelectorAttribute : ValueDropdownAttribute
    {
        public SceneSelectorAttribute() : base("") { }
        
        public static IEnumerable<string> SceneNames
        {
            get
            {
                int sceneCount = SceneManager.sceneCountInBuildSettings;

                for (int i = 0; i < sceneCount; i++)
                {
                    string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
                    string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
                    yield return sceneName;
                }
            }
        }
    }
}