using Attributes;
using LSCore;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Animatable
{
    public class AnimatableCanvas : SingleService<AnimatableCanvas>
    {
        private Canvas canvas;
        private static Camera Cam => Instance.canvas.worldCamera;
        
        [ColoredField, SerializeField] private AnimText animText;
        [ColoredField, SerializeField] private HealthBar healthBar;
        [ColoredField, SerializeField] private HealthBar opponentHealthBar;
        [ColoredField, SerializeField] private Loader loader;

        public static int SortingOrder
        {
            get => Instance.canvas.sortingOrder;
            set => Instance.canvas.sortingOrder = value;
        }
        
        public static Transform SpawnPoint => Instance.transform;
        internal static AnimText AnimText => Instance.animText;
        internal static HealthBar HealthBar => Instance.healthBar;
        internal static HealthBar OpponentHealthBar => Instance.opponentHealthBar;
        internal static Loader Loader => Instance.loader;

        private void Awake()
        {
            DontDestroyOnLoad(this);
        }
        
        protected override void Init()
        {
            base.Init();
            animText.Init();
            healthBar.Init();
            opponentHealthBar.Init();
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
            instance.opponentHealthBar.ReleaseAll();
        }
    }
}