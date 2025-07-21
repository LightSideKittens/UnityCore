using LSCore.Extensions.Unity;
using UnityEngine;

namespace LSCore
{
    [RequireComponent(typeof(Camera))]
    [ExecuteAlways]
    public class BackgroundToCameraFitter : MonoBehaviour
    {
        public SpriteRenderer backgroundRenderer;
        private Camera cam;

        private void Awake()
        {
            cam = GetComponent<Camera>();
            if (!cam.orthographic)
            {
                Burger.Warning($"{nameof(BackgroundToCameraFitter)}: enable Orthographic on camera");
                cam.orthographic = true;
            }
        }

        private void Start() => Fit();
#if UNITY_EDITOR
        private void OnValidate()
        {
            if(cam) Fit();
        }
        
        private void Update()
        {
            if (World.IsEditMode)
            {
                Fit();
            }
        }
#else
        void OnEnable() => Fit();
#endif

        private void Fit()
        {
            if (!backgroundRenderer) return;
            
            Vector2 spriteSize = backgroundRenderer.sprite.bounds.size;
            var camSize = cam.GetSize();
            
            float k = Mathf.Max(camSize.x / spriteSize.x, camSize.y / spriteSize.y);
            var newScale = Vector3.one * k;
            
            backgroundRenderer.transform.localScale = newScale;
            
            Vector3 pos = cam.transform.position;
            var backPos = backgroundRenderer.transform.position; 
            pos.z = backPos.z;
            backgroundRenderer.transform.position = pos;
        }
    }
}