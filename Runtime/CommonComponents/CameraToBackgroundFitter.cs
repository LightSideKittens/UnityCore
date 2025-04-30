using LSCore.Extensions.Unity;
using UnityEngine;

namespace LSCore
{
    [RequireComponent(typeof(Camera))]
    [ExecuteAlways]
    public class CameraToBackgroundFitter : MonoBehaviour
    {
        public SpriteRenderer backgroundRenderer;
        public float cameraSizeRef = 10;
        private Camera cam;

        private void Awake()
        {
            cam = GetComponent<Camera>();
            if (!cam.orthographic)
            {
                Burger.Warning($"{nameof(CameraToBackgroundFitter)}: enable Orthographic on camera");
                cam.orthographic = true;
            }
        }

        private void Start() => Fit();
#if UNITY_EDITOR
        private void OnValidate()
        {
            if(cam) Fit();
        }

        private void Update() => Fit();
#else
        void OnEnable() => Fit();
#endif

        private void Fit()
        {
            if (!backgroundRenderer) return;

            Bounds b = backgroundRenderer.bounds;
            float w = b.size.x;
            float h = b.size.y;
            
            var camSize = CameraExtensions.GetSize(cameraSizeRef, cam.aspect);

            if (camSize.x < w)
            {
                cam.SetSizeByWidth(w);
            }
            else if(camSize.y < h)
            {
                cam.SetSizeByHeight(h);
            }
            else
            {
                cam.orthographicSize = cameraSizeRef;
            }
            
            Vector3 pos = cam.transform.position;
            pos.x = b.center.x;
            pos.y = b.center.y;
            cam.transform.position = pos;
        }
    }
}