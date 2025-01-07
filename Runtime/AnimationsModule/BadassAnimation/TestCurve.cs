using System;
using System.Collections.Generic;
using UnityEngine;

namespace LSCore.AnimationsModule
{
    [Serializable]
    public class Test
    {
        public List<BezierPoint> points;
    }
    
    public class TestCurve : MonoBehaviour
    {
        public BadassAnimationCurve[] curves;
        public Test[] test;
    }
}