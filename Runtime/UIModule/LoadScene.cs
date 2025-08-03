using System;
using System.Collections.Generic;
using LSCore;
using LSCore.Attributes;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;

[Serializable]
[Unwrap]
public class LoadScene : DoIt
{
    [SceneSelector] public string sceneToLoad;
    public LoadSceneMode mode;
    public bool useAddressables;
    [SerializeReference] public DefaultLoader loader;

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