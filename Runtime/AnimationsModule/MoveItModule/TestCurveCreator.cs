using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

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

            var sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < count; i++)
            {
                Instantiate(testCurve);
            }
            sw.Stop();
            Debug.Log(sw.ElapsedTicks);
        }
    }
}