using UnityEngine;

namespace LSCore.AnimationsModule
{
    public class TestCurveCreator : MonoBehaviour
    {
        public int count = 100;
        public GameObject testCurve;
        
        public bool useParallel;
        public static bool UseParallel;
        
        private void Awake()
        {
            UseParallel = useParallel;
            
            for (int i = 0; i < count; i++)
            {
                Instantiate(testCurve);
            }
        }
    }
}