using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

[Serializable]
public class LoadScene : LSAction
{
    [ValueDropdown("GetSceneNames")]
    public string sceneToLoad;

    public bool useAddressables;
    public bool async;

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

    public override void Invoke()
    {
        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            if (useAddressables)
            {
                if (async)
                {
                    Addressables.LoadSceneAsync(sceneToLoad);
                }
                else
                {
                    Addressables.LoadSceneAsync(sceneToLoad).WaitForCompletion();
                }
            }
            else
            {
                if (async)
                {
                    SceneManager.LoadSceneAsync(sceneToLoad);
                }
                else
                {
                    SceneManager.LoadScene(sceneToLoad);
                }
            }
        }
        else
        {
            Debug.LogWarning("No scene selected to load!");
        }
    }
}