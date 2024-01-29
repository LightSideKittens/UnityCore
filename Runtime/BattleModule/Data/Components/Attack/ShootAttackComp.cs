using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Scripting;
using static UnityEngine.Object;

namespace LSCore.BattleModule
{
    [Preserve, Serializable]
    internal class ShootAttackComp : AutoAttackComponent
    {
        [SerializeField] private GameObject bulletPrefab;
        [SerializeField] private float bulletFlyDuration;
        [SerializeField] private GameObject explosionPrefab;

        protected override void Attack(Transform target)
        {
            var bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
            bullet.transform.DOMove(target.position, bulletFlyDuration).OnComplete(() =>
            {
                TryApplyDamage(target);
                Instantiate(explosionPrefab, target.position, Quaternion.identity);
            });
        }
    }
}