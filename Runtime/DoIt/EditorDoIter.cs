using Sirenix.OdinInspector;
using UnityEngine;

[ExecuteInEditMode]
public class EditorDoIter : MonoBehaviour
{
#if UNITY_EDITOR
    [SerializeReference] public DoIt[] doIts;
    private void Awake() => Do();
    
    [Button]
    public void Do() => doIts.Do();
#endif
}