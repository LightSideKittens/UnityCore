using UnityEngine;

namespace LSCore.Editor
{
    public class LSCameraGizmos : MonoBehaviour
    {
        private void OnDrawGizmos()
        {
            Gizmos.DrawWireSphere(Vector2.one * -2, 3);
            Gizmos.DrawLine(Vector3.zero, Vector2.one);
        }
    }
}