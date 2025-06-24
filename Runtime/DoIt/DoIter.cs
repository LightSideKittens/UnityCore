using UnityEngine;

public class DoIter : MonoBehaviour
{
    [SerializeReference] public DoIt[] doIts;
    private void Awake() => doIts.Do();
}