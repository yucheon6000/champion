using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

public abstract class Node : INode
{
    protected Node parentNode = null;

    public void SetParent(Node parentNode)
    {
        this.parentNode = parentNode;
    }

    public abstract NodeState Evaluate(Entity entity);

    public abstract JObject ToJson();
    public abstract void FromJson(JObject json);
}
