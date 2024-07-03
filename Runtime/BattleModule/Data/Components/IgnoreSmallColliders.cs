using System;
using LSCore.BattleModule;
using LSCore.Extensions.Unity;
using UnityEngine;

[Serializable]
public class IgnoreSmallColliders : BaseComp
{
    private Collider2D collider;
    private float radius;
    public ContactFilter2D contactFilter;
    
    protected override void Init()
    {
        collider = transform.GetComponent<Collider2D>();
        radius = collider.bounds.size.magnitude;
        useFixedUpdate = true;
        IsRunning = true;
    }

    protected override void FixedUpdate()
    {
        var colliders = Physics2DExt.FindAll(transform.position, radius, contactFilter);

        for (int i = 0; i < colliders.Length; i++)
        {
            Collider2D otherCollider = colliders[i];
            if (ShouldIgnoreCollision(otherCollider))
            {
                Physics2D.IgnoreCollision(collider, otherCollider);
            }
        }
    }

    private bool ShouldIgnoreCollision(Collider2D otherCollider)
    {
        float otherColliderSize = otherCollider.bounds.size.magnitude;

        return radius > otherColliderSize * 4;
    }
}