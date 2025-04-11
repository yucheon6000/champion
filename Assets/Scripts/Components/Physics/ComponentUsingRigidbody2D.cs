using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ComponentUsingRigidbody2D : IComponent
{
    protected new Rigidbody2D rigidbody2D;

    public override void Init(Entity entity)
    {
        base.Init(entity);
        CheckRigidbody2D();
    }

    protected void CheckRigidbody2D()
    {
        if (rigidbody2D != null) return;

        if (TryGetComponent(out Rigidbody2D component))
            rigidbody2D = component;
        else
            rigidbody2D = gameObject.AddComponent<Rigidbody2D>();
    }
}
