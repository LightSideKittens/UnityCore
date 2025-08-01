using System;
using System.Collections.Generic;
using LSCore;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;

[Serializable]
public class LoadScene : DoIt
{
    [ValueDropdown("GetSceneNames")]
    public string sceneToLoad;
    public LoadSceneMode mode;
    public bool useAddressables;
    [SerializeReference] public DefaultLoader loader;

    private IEnumerable<string> GetSceneNames()
    {
        int sceneCount = SceneManager.sceneCountInBuildSettings;

        for (int i = 0; i < sceneCount; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
            yield return sceneName;
        }
    }

    public override void Do()
    {
        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            if (useAddressables)
            {
                if (loader != null)
                {
                    var handle = LSAddressables.LoadScene(sceneToLoad, mode);
                    loader.Show(handle, Do);
                }
                else
                {
                    LSAddressables.LoadScene(sceneToLoad, mode).WaitForCompletion();
                }
            }
            else
            {
                if (loader != null)
                {
                    var handle = SceneManager.LoadSceneAsync(sceneToLoad, mode);
                    loader.Show(handle);
                }
                else
                {
                    SceneManager.LoadScene(sceneToLoad, mode);
                }
            }
        }
        else
        {
            Debug.LogWarning("No scene selected to load!");
        }
    }
}