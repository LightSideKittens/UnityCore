using System;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Collections.Generic;
using LSCore;
using UnityEngine;

public static class AddressablesCatalogUpdater
{
    public static event Action UpdateCompleted;
    private static Action updateCompleted;
    private static bool isProcessing;

    static AddressablesCatalogUpdater()
    {
        World.Destroyed += Reset;
    }
    
    public static void UpdateCatalog()
    {
        if (isProcessing)
        {
            return;
        }

        isProcessing = true;
        var checkForUpdates = Addressables.CheckForCatalogUpdates();
        checkForUpdates.Completed += OnCatalogCheckComplete;
    }
    
    public static void UpdateCatalog(Action onComplete)
    {
        updateCompleted += onComplete;
        UpdateCatalog();
    }

    private static void OnCatalogCheckComplete(AsyncOperationHandle<List<string>> checkForUpdates)
    {
        if (checkForUpdates.Status == AsyncOperationStatus.Succeeded && checkForUpdates.Result.Count > 0)
        {
            Debug.Log("Catalog Updated");
            Addressables.UpdateCatalogs(checkForUpdates.Result).OnComplete(OnUpdate);
        }
        else
        {
            OnUpdate();
        }
    }

    private static void OnUpdate()
    {
        UpdateCompleted?.Invoke();
        updateCompleted?.Invoke();
        Reset();
    }

    private static void Reset()
    {
        updateCompleted = null;
        isProcessing = false;
    }
}