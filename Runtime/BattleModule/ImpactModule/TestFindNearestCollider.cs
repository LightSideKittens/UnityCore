using System;
using LSCore.Extensions.Unity;
using UnityEngine;
using Random = UnityEngine.Random;

namespace LSCore
{
    public class TestFindNearestCollider : MonoBehaviour
    {
        [SerializeField] private Collider2D template;
        [SerializeField] private Collider2D template2;
        [SerializeField] private Collider2D template3;
        private Transform par;
        
        private void Start()
        {
            Physics2DExt.SetHitCollidersSize(10000);
            par = new GameObject("Parent").transform;

            for (int i = 0; i < 3_000; i++)
            {
                Instantiate(template, Random.insideUnitCircle * 1000, Quaternion.identity, par);
            }
            
            for (int i = 0; i < 3_000; i++)
            {
                Instantiate(template2, Random.insideUnitCircle * 1000, Quaternion.identity, par);
            }
            
            for (int i = 0; i < 3_000; i++)
            {
                Instantiate(template3, Random.insideUnitCircle * 1000, Quaternion.identity, par);
            }
        }
    }
}