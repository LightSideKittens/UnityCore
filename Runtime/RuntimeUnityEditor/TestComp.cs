using System;
using System.Collections.Generic;
using UnityEngine;

namespace LSCore
{
    [Serializable]
    public class TestData
    {
        public string testName;
        public int testInt;
        public float testFloat;
        public Vector2 vector2;
        public Vector3 vector3;
        public Color color;
    }

    [Serializable]
    public class TestData2 : TestData
    {
        public string testName2;
        public int testInt2;
        public float testFloat2;
        public Vector2 vector22;
        public Vector3 vector33;
        public Color color2;
        public SpriteRenderer renderer;
    }
    
    public class TestComp : MonoBehaviour
    {
        public int a;
        public float c;
        public Vector2 vector2;
        public Vector3 vector3;
        public Color color;
        public GameObject b;
        public TestData testData;
        [SerializeReference] private TestData testData2;
        [SerializeReference] private TestData[] testData3;
        [SerializeReference] private List<TestData> testData4;
    }
}