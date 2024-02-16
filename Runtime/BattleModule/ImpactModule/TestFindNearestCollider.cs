using System;
using LSCore.Extensions.Unity;
using UnityEngine;
using Random = UnityEngine.Random;

namespace LSCore
{
    public class TestFindNearestCollider : MonoBehaviour
    {
        [SerializeField] private float fps;
        [SerializeField] private bool useColliders;
        [SerializeField] private Collider2D template;
        [SerializeField] private Collider2D template2;
        [SerializeField] private Collider2D template3;
        private Collider2D self;
        private Transform par;
        
        private void Start()
        {
            Physics2DExt.SetHitCollidersSize(10000);
            self = GetComponent<Collider2D>();
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

        private void Update()
        {
            fps = Time.frameCount / Time.time;

            if (useColliders)
            {
                //self.enabled = false;
                Physics2DExt.TryFindNearestCollider(transform.position, LayerMask.GetMask("Default"), out _);
                //self.enabled = true;
            }
            else
            {
                Transform closest = null;
                float closestDistanceSqr = Mathf.Infinity;
        
                for (int i = 0; i < par.childCount; i++)
                {
                    var tr = par.GetChild(i);
                    Vector3 directionToTarget = tr.position - transform.position;
                    float dSqrToTarget = directionToTarget.sqrMagnitude;
                    if (dSqrToTarget < closestDistanceSqr)
                    {
                        closestDistanceSqr = dSqrToTarget;
                        closest = tr;
                    }
                }
            }
        }
    }
}