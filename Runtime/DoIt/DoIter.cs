using UnityEngine;

[DefaultExecutionOrder(-7)]
public class DoIter : MonoBehaviour
{
    [SerializeReference] public DoIt[] doIts;
    private void Awake() => doIts.Do();
}