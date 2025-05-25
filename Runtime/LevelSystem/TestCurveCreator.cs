using System;
using LSCore.LevelSystem;
using UnityEngine;

namespace LSCore.BattleModule
{
    public class TestCurveCreator : MonoBehaviour
    {
        public Sprite sprite;
        public Sprite[] sprites;
        public SpriteRenderer spriteRenderer;
        public TestCurveCreator curveCreator;
        public GameObject curvePrefab;
        
        public LevelsManager manager;
        public bool useParallel; 
        public TestStruct testStruct;

        [SerializeReference] private TestStruct2.TestClass2 testClass;
        public TestClass4 testClass4;
        
        [Serializable]
        public class TestStruct
        {
            public float a;
            public float[] ass;
            public TestClass testClass;
            
            [Serializable]
            public struct TestClass
            {
                public float a;
                [SerializeReference] private TestStruct2.TestClass2[] testClasses;
            }
        }
        
        [Serializable]
        public struct TestStruct2
        {
            [Serializable]
            public abstract class TestClass2
            {
            }
            
            [Serializable]
            private class TestClass3 : TestClass2
            {
                public int count;
            }
        }
    }
    
    [Serializable]
    public class TestClass4 : TestCurveCreator.TestStruct2.TestClass2
    {
        public int count;
    }

    /*[CustomEditor(typeof(TestCurveCreator))]
    public class TestCurveCreatorEditor : Editor
    {
    }*/
}