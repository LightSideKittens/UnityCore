using Sirenix.OdinInspector;
using UnityEngine;

#if UNITY_EDITOR
[ExecuteInEditMode]
#endif
[DefaultExecutionOrder(-7)]
public class EditorDoIter : MonoBehaviour
{
    [SerializeReference] public DoIt[] doIts;
    private void Awake() => InternalDo();
    
    
#if UNITY_EDITOR
    private bool did;

    private void OnValidate()
    {
        InternalDo();
    }

    [Button]
    public void Do() => InternalDo();
#endif
    
    private void InternalDo()
    {
#if UNITY_EDITOR
        if (did) return;
        did = true;
        UnityEditor.Compilation.CompilationPipeline.compilationFinished += x =>
        {
            did = false;
        };
#endif
        doIts.Do();
    }
}