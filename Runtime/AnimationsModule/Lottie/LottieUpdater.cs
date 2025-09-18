using System;
using System.Linq;
using LSCore;
#if UNITY_EDITOR
using UnityEditor;
#endif

#if UNITY_EDITOR
[InitializeOnLoad]
#endif
internal static class LottieUpdater
{
    public static event Action Updated;
    
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

    private static void OnUpdate() => Updated?.Invoke();
}