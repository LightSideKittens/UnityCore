
using System;
using System.Collections.Generic;
using UnityEngine;

namespace LSCore.BattleModule
{
    public class TestCurve2 : MonoBehaviour
    {
        [Serializable]
        public abstract class BaseTest
        {
            
        }
        
        public class Test : BaseTest
        {
            public int a;
        }

        [SerializeReference] public List<BaseTest> tests;
    }
}