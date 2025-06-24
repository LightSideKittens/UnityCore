using System;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public static class AddressablesWrapper
{
    public static AsyncOperationHandle ChainOperation => Addressables.Instance.ChainOperation;
    
#if UNITY_EDITOR
    public static HashSet<Type> GetTypesForAssetPath(string path)
    {
        return AssetPathToTypes.GetTypesForAssetPath(path);
    }
#endif
}