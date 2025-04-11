using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ComponentUsingMovable : IComponent
{
    protected Movable movable;

    protected void CheckMovable()
    {
        if (movable != null) return;

        if (TryGetComponent(out Movable component))
            movable = component;
        else
            movable = gameObject.AddComponent<Movable>();
    }
}
