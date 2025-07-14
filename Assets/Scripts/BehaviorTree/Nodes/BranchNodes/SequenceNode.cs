using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class SequenceNode : CompositeNode
{
    public override NodeState Evaluate(Entity entity)
    {
        if (childrenNodes.Count == 0)
            return NodeState.Failure;

        foreach (var child in childrenNodes)
        {
            switch (child.Evaluate( entity))
            {
                case NodeState.Running:
                    return NodeState.Running;
                case NodeState.Success:
                    continue;                   // Keep going
                case NodeState.Failure:
                    return NodeState.Failure;
            }
        }

        return NodeState.Success;
    }

    public override JObject ToJson()
    {
        throw new System.NotImplementedException();
    }

    public override void FromJson(JObject json)
    {
        throw new System.NotImplementedException();
    }
}
