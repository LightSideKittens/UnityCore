using System;
using System.Collections.Generic;
using System.Diagnostics;
using LSCore.Extensions.Unity;
using Sirenix.Utilities;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace LSCore
{
    public class CustomCircleCollider : MonoBehaviour
    {
        [SerializeField] private float swTime;
        [SerializeField] private long total;
        [SerializeField] private int frames;
        [SerializeField] private LayerMask mask;
        [SerializeField] private float radius = 1;
        private HashSet<Collider2D> previousColliders = new HashSet<Collider2D>();

        private void FixedUpdate()
        {
            var sw = new Stopwatch();
            sw.Start();
            Collider2D[] currentColliders = Physics2DExt.FindAll(transform.position, radius, mask);
            var currentSet = new HashSet<Collider2D>(currentColliders);

            // Обработка событий входа
            foreach (var collider in currentSet)
            {
                if (!previousColliders.Contains(collider))
                {
                    //UnityEngine.Debug.Log($"Enter {collider.name}");
                }
            }

            // Обработка событий выхода
            foreach (var collider in previousColliders)
            {
                if (!currentSet.Contains(collider))
                {
                    //UnityEngine.Debug.Log($"Exit {collider.name}");
                }
            }

            // Обновление списка коллайдеров для следующей проверки
            previousColliders = currentSet;
            sw.Stop();
            total += sw.ElapsedTicks;
            swTime = total / (float)++frames;
            Debug.Log($"Ticks {sw.ElapsedTicks}");
        }
    }
}