/*using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

public class LottieAssetsHelper : MonoBehaviour
{
    public BaseLottieAsset[] assets;

    [Button]
    public void Get()
    {
        assets = FindObjectsByType<LottieImage>(FindObjectsSortMode.None)
            .Select(x => x.asset)
            .Concat(FindObjectsByType<LottieRenderer>(FindObjectsSortMode.None)
                .Select(x => x.asset)).ToArray();
    }

    [Button]
    public void Setup()
    {
        var renderers = FindObjectsByType<LottieImage>(FindObjectsSortMode.None)
            .Select(x =>  (Action<BaseLottieAsset>)((asset) => x.asset = asset))
            .Concat(FindObjectsByType<LottieRenderer>(FindObjectsSortMode.None)
                .Select(x => (Action<BaseLottieAsset>)((asset) => x.asset = asset))).ToArray();

        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i](assets[i]);
        }
    }
}*/

using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

public class LottieAssetsHelper : MonoBehaviour
{
    public BaseLottieAsset[] assets;

    [Button]
    public void Get()
    {
        assets = FindObjectsByType<LottieImage>(FindObjectsSortMode.None)
            .Select(x => x.manager.asset)
            .Concat(FindObjectsByType<LottieRenderer>(FindObjectsSortMode.None)
                .Select(x => x.manager.asset)).ToArray();
    }

    [Button]
    public void Setup()
    {
        var renderers = FindObjectsByType<LottieImage>(FindObjectsSortMode.None)
            .Select(x => (BaseLottieManager)x.manager)
            .Concat(FindObjectsByType<LottieRenderer>(FindObjectsSortMode.None)
                .Select(x => x.manager)).ToArray();

        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].asset = assets[i];
        }
    }
}