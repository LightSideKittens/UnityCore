using LSCore;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Animatable
{
    public class AnimatableCanvas : SingleService<AnimatableCanvas>
    {
        private Canvas canvas;
        private static Camera Cam => Instance.canvas.worldCamera;

        [SerializeField] private AnimText animText;
        [SerializeField] private HealthBar healthBar;
        [SerializeField] private HealthBar opponentHealthBar;
        [SerializeField] private Loader loader;
        [SerializeField] private PopupText popupText;
        [SerializeField] private ParticlesAttractor particlesAttractor;

        public static int SortingOrder
        {
            get => Instance.canvas.sortingOrder;
            set => Instance.canvas.sortingOrder = value;
        }
        
        public static Transform SpawnPoint => Instance.transform;
        internal static AnimText AnimText => Instance.animText;
        internal static PopupText PopupText => Instance.popupText;
        internal static HealthBar HealthBar => Instance.healthBar;
        internal static HealthBar OpponentHealthBar => Instance.opponentHealthBar;
        internal static ParticlesAttractor ParticlesAttractor => Instance.particlesAttractor;
        internal static Loader Loader => Instance.loader;

        private void Awake()
        {
            DontDestroyOnLoad(this);
        }
        
        protected override void Init()
        {
            base.Init();
            animText.Init();
            popupText.Init();
            healthBar.Init();
            opponentHealthBar.Init();
            particlesAttractor.Init();
            canvas = GetComponent<Canvas>();
            canvas.worldCamera = Camera.main;
            
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        protected override void DeInit()
        {
            base.DeInit();
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
            canvas.worldCamera = Camera.main;
            Clean();
        }

        internal static Vector3 GetLocalPosition(Vector3 worldPos)
        {
            var targetLocalPosByCam = Cam.transform.InverseTransformPoint(worldPos);
            targetLocalPosByCam /= Instance.canvas.transform.lossyScale.x;
            targetLocalPosByCam.z = 0;
            return targetLocalPosByCam;
        }
        
        public static void Clean()
        {
            var instance = Instance;
            instance.animText.ReleaseAll();
            instance.healthBar.ReleaseAll();
            instance.popupText.ReleaseAll();
            instance.opponentHealthBar.ReleaseAll();
            instance.particlesAttractor.ReleaseAll();
        }
    }
}