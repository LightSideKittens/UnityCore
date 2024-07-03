using System.Collections.Generic;
using System.Linq;
using LSCore.Extensions.Unity;
using Sirenix.Utilities;
using UnityEngine;

namespace LSCore.BattleModule
{
    public class DisableCollidersOutCamera : MonoBehaviour
    {
        public ContactFilter2D contactFilter;
        public Camera cam;
        private readonly HashSet<Collider2D> previousColliders = new();
        private readonly HashSet<Collider2D> currentColliders = new();
        
        private void FixedUpdate()
        {
            currentColliders.Clear();
            var results = Physics2DExt.FindAll(cam.GetRect().Expand(10), cam.transform.eulerAngles.z, contactFilter);
            currentColliders.AddRange(results);

            foreach (var result in currentColliders.Except(previousColliders))
            {
                result.excludeLayers = 0;
            }
            
            foreach (var result in previousColliders.Except(currentColliders))
            {
                result.excludeLayers = -1;
            }
            
            previousColliders.Clear();
            previousColliders.AddRange(currentColliders);
        }
    }
}