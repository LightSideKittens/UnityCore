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
        
        [Serializable]
        public struct TestStruct
        {
            [SerializeField] private TestClass testClass;
            
            [Serializable]
            private class TestClass
            {
                [SerializeReference] private TestStruct2.TestClass2[] testClasses;
            }
        }
        
        [Serializable]
        public struct TestStruct2
        {
            [Serializable]
            public abstract class TestClass2
            {
                public enum TestEnum
                {
                    None,
                    Done,
                    Fone,
                    Sone
                }
                
                [SerializeField] private Id[] id;
                [SerializeField] private Sprite[] sprites;
                public float speed;
                public int count;
                public TestEnum type;
                public bool testBool;
            }
            
            [Serializable]
            private class TestClass3 : TestClass2
            {
                
            }
        }
    }
    
    [Serializable]
    public class TestClass4 : TestCurveCreator.TestStruct2.TestClass2
    {
                
    }

    /*[CustomEditor(typeof(TestCurveCreator))]
    public class TestCurveCreatorEditor : Editor
    {
    }*/
}