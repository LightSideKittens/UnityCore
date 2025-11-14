using LSCore;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Animatable
{
    public abstract class BaseAnimatableCanvas<T> : SingleService<T> where T : BaseAnimatableCanvas<T>
    {
         private Canvas canvas;
        private static Camera Cam => Instance.canvas.worldCamera;
        public static int SortingOrder
        {
            get => Instance.canvas.sortingOrder;
            set => Instance.canvas.sortingOrder = value;
        }
        
        public static Transform SpawnPoint => Instance.transform;


        private void Awake()
        {
            DontDestroyOnLoad(this);
        }
        
        protected override void Init()
        {
            base.Init();
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

        public static Vector3 GetLocalPosition(Vector3 worldPos)
        {
            var targetLocalPosByCam = Cam.transform.InverseTransformPoint(worldPos);
            targetLocalPosByCam /= Instance.canvas.transform.lossyScale.x;
            targetLocalPosByCam.z = 0;
            return targetLocalPosByCam;
        }

        protected virtual void OnClean() { }
        
        public static void Clean() => Instance.OnClean();
    }

    public class AnimatableCanvas : BaseAnimatableCanvas<AnimatableCanvas>
    {
        [SerializeField] private AnimText animText;
        [SerializeField] private HealthBar healthBar;
        [SerializeField] private HealthBar opponentHealthBar;
        [SerializeField] private Loader loader;
        [SerializeField] private PopupText popupText;
        [SerializeField] private ParticlesAttractor particlesAttractor;

        
        internal static AnimText AnimText => Instance.animText;
        internal static PopupText PopupText => Instance.popupText;
        internal static HealthBar HealthBar => Instance.healthBar;
        internal static HealthBar OpponentHealthBar => Instance.opponentHealthBar;
        internal static ParticlesAttractor ParticlesAttractor => Instance.particlesAttractor;
        internal static Loader Loader => Instance.loader;

        protected override void Init()
        {
            base.Init();
            animText.Init();
            popupText.Init();
            healthBar.Init();
            opponentHealthBar.Init();
            particlesAttractor.Init();
        }

        protected override void OnClean()
        {
            base.OnClean();
            animText.ReleaseAll();
            healthBar.ReleaseAll();
            popupText.ReleaseAll();
            opponentHealthBar.ReleaseAll();
            particlesAttractor.ReleaseAll();
        }
    }
}