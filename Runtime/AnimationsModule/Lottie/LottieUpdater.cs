using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using LSCore;
using Debug = UnityEngine.Debug;
#if UNITY_EDITOR
using UnityEditor;
#endif

#if UNITY_EDITOR
[InitializeOnLoad]
#endif
internal static class LottieUpdater
{
    public static List<BaseLottieManager> managers = new(64);
    
    static LottieUpdater()
    {
#if UNITY_EDITOR
        Selection.selectionChanged += OnSelectionChanged;
        EditorApplication.update += OnEditorUpdate;
        
        void OnSelectionChanged()
        {
            EditorWorld.Updated -= OnUpdate;
            if (Selection.gameObjects.Any(go => go && (
                    go.GetComponent<LottieRenderer>() || go.GetComponent<LottieImage>())))
            {
                EditorWorld.Updated += OnUpdate;
            }
        }

        void OnEditorUpdate()
        {
            EditorApplication.update -= OnEditorUpdate;
            OnSelectionChanged();
        }
#endif
        World.Updated += OnUpdate;
    }
    
    private static void OnUpdate()
    {
        Update();
    }
    
    public static void Update()
    {
        for (int i = 0; i < managers.Count; i++)
        {
            managers[i].Update();
        }
    }
}