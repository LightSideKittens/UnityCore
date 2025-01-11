using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace LSCore.AnimationsModule
{
    public class AnimationContollerSlow : MonoBehaviour
    {
        public int count;
        public AnimationCurve curve;
        public BadassCurve curve1;
        public bool use;
        private float xValue;

        public void Update()
        {
            var sw = new Stopwatch();
            sw.Start();
            
            xValue += Time.deltaTime;

            if (use)
            {
                for (int i = 0; i < count; i++)
                {
                    Evaluate();
                }
            }
            else
            {
                for (int i = 0; i < count; i++)
                {
                    var y = curve1.Evaluate(xValue);
                }
                //Parallel.For(0, count, i => curve1.Evaluate(xValue));
            }


            sw.Stop();
            Debug.Log(sw.ElapsedTicks);
        }

        void Evaluate()
        {
            Evaluate1();
        }

        void Evaluate1()
        {
            var y = curve.Evaluate(xValue);
        }
    }
}