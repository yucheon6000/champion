using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class SelectorNode : CompositeNode
{
    public override NodeState Evaluate(Entity entity)
    {
        if (childrenNodes.Count == 0)
            return NodeState.Failure;

        foreach (var child in childrenNodes)
        {
            switch (child.Evaluate(entity))
            {
                case NodeState.Running:
                    return NodeState.Running;
                case NodeState.Success:
                    return NodeState.Success;
            }
        }

        return NodeState.Failure;
    }

    public override void FromJson(JObject json)
    {
        throw new System.NotImplementedException();
    }

    public override JObject ToJson()
    {
        throw new System.NotImplementedException();
    }
}
