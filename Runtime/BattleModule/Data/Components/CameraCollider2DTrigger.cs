using System;
using System.Collections.Generic;
using System.Linq;
using LSCore.Extensions.Unity;
using Sirenix.Utilities;
using UnityEngine;

namespace LSCore.BattleModule
{
    public class CameraCollider2DTrigger : MonoBehaviour
    {
        [Serializable]
        public abstract class Handler : LSAction<Collider2D> { }
        
        public float cameraRectExpand = 5;
        public ContactFilter2D contactFilter;
        public Camera cam;
        private readonly HashSet<Collider2D> registeredColliders = new();
        private readonly HashSet<Collider2D> currentColliders = new();
        [SerializeReference] public List<Handler> onIn; 
        [SerializeReference] public List<Handler> onOut;
        private int frameCount;
        
        public bool Register(Collider2D col)
        {
            return registeredColliders.Add(col);
        }
        
        public bool Unregister(Collider2D col)
        {
            return registeredColliders.Remove(col);
        }
        
        private void FixedUpdate()
        {
            frameCount++;
            if (frameCount < 5)
            {
                return;
            }

            frameCount = 0;
            currentColliders.Clear();
            var results = Physics2DExt.FindAll(cam.GetRect().Expand(cameraRectExpand), cam.transform.eulerAngles.z, contactFilter);
            currentColliders.AddRange(results);

            for (int i = 0; i < results.Length; i++)
            {
                var result = results[i];
                if (registeredColliders.Contains(result))
                {
                    onIn.Invoke(result);
                }
            }
            
            foreach (var result in registeredColliders.Except(currentColliders))
            {
                onOut.Invoke(result);
            }
        }
    }
}