
using System;
using System.Collections.Generic;
using UnityEngine;

namespace LSCore.BattleModule
{
    public class TestCurve : MonoBehaviour
    {
        [Serializable]
        public class Test
        {
            public int a;
        }
        
        public List<Test> tests;
    }
}