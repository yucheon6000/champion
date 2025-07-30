using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BTComponent : MonoBehaviour
{
    protected Entity entity;

    protected virtual void Start()
    {
        FindEntity();
        FindComponents();
        InitByTags(entity.Tags);
    }


    protected virtual void FindEntity()
    {
        entity = GetComponent<Entity>();
    }

    protected virtual void FindComponents() { }

    protected virtual void InitByTags(List<string> tags) { }
}
