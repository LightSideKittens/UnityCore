using Sirenix.OdinInspector;
using UnityEngine;

#if UNITY_EDITOR
[ExecuteInEditMode]
#endif
[DefaultExecutionOrder(-7)]
public class EditorDoIter : MonoBehaviour
{
    [SerializeReference] public DoIt[] doIts;
    private void Awake() => doIts.Do();
    
#if UNITY_EDITOR
    [Button]
    public void Do() => doIts.Do();
#endif
}