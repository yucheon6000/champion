using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

[NodeDescription("Returns Success if the entity (this node is attached to) is destroyed. Returns Failure if the entity is not destroyed.")]
public class OnDestroyed : ConditionNode, IUsableNode
{
    public override NodeState Evaluate()
    {
        if (entity.WillBeDestroyed)
            return ReturnSuccess();

        return ReturnFailure();
    }

    public override JObject ToJson()
    {
        JObject json = base.ToJson();
        json.Add("name", GetType().Name);
        return json;
    }

    public override void FromJson(JObject json) { }
}
