using UnityEngine;
using UnityEngine.EventSystems;
namespace LSCore
{
    public class SubmittableRect : MonoBehaviour, ISubmittableElement
    {
        [SerializeReference] public DefaultSubmittable submittable = new();
        object ISubmittableElement.Submittable => submittable;
        
        protected void Awake()
        {
            submittable.Init(transform);
        }
        
        protected void OnDisable()
        {
            submittable.OnDisable();
        }
    }
}