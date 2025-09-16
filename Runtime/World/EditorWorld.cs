#if UNITY_EDITOR
using System;
using LSCore;
using LSCore.Extensions;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
[ExecuteInEditMode]
public class EditorWorld : MonoBehaviour
{
    private static EditorWorld instance;
    public static event Action Updated;
    
    static EditorWorld()
    {
        CompilationPipeline.compilationFinished += x =>
        {
            Destroy();
        };

        World.Built += () =>
        {
            Destroy();
            Create();
        };
        
        EditorApplication.update += Update;

        void Update()
        {
            EditorApplication.update -= Update;
            Create();
            World.Creating += Destroy;
            World.Destroyed += Create;
        }
    }

    private void Update()
    {
        Updated.SafeInvoke();
    }

    private static void EditorUpdate()
    {
        if (Updated != null)
        {
            SceneView.RepaintAll();
            EditorApplication.QueuePlayerLoopUpdate();
        }
    }

    private static void Create()
    {
        var go = new GameObject("EditorWorld");
        go.hideFlags = HideFlags.HideAndDontSave;
        instance = go.AddComponent<EditorWorld>();
        EditorApplication.update += EditorUpdate;
    }

    private static void Destroy()
    {
        if (instance != null)
        { 
            DestroyImmediate(instance.gameObject);
        }
        instance = null;
        EditorApplication.update -= EditorUpdate;
    }
}
#endif