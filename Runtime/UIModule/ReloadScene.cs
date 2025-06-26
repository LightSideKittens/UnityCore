using System;
using LSCore;
using UnityEngine;
using UnityEngine.SceneManagement;

[Serializable]
public class ReloadScene : DoIt
{
    public bool useAddressables;
    [SerializeReference] public DefaultLoader loader;
    
    public override void Do()
    {
        var loadScene = new LoadScene
        {
            useAddressables = useAddressables,
            loader = loader,
            sceneToLoad = SceneManager.GetActiveScene().name
        };
        
        loadScene.Do();
    }
}