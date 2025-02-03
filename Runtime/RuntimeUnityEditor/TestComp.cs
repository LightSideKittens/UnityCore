using System;
using UnityEngine;

namespace LSCore
{
    [Serializable]
    public class TestData
    {
        public string testName;
        public int testInt;
        public float testFloat;
    }

    [Serializable]
    public class TestData2 : TestData
    {
        public string testName2;
        public int testInt2;
        public float testFloat2;
        public SpriteRenderer renderer;
    }
    
    public class TestComp : MonoBehaviour
    {
        public int a;
        public float c;
        public GameObject b;
        public TestData testData;
        [SerializeReference] private TestData testData2;
    }
}