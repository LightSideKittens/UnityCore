using System;
using System.Linq;
using LSCore;
#if UNITY_EDITOR
using UnityEditor;
#endif

[InitializeOnLoad]
internal static class LottieUpdater
{
    public static event Action Updated;

#if UNITY_EDITOR
    static LottieUpdater()
    {
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