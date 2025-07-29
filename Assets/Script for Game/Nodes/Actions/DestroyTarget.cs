using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

[NodeParam("target", NodeParamType.EntityVariable)]
[NodeDescription("Destroys the target entity.")]
public class DestroyTarget : ActionNode, IUsableNode
{
    private BTValue<Entity> target;

    public override NodeState Evaluate()
    {
        if (target.GetValue(entity.Blackboard) == null) return ReturnFailure();

        Object.Destroy(target.GetValue(entity.Blackboard).gameObject);

        return ReturnSuccess();
    }

    public override JObject ToJson()
    {
        JObject json = base.ToJson();
        json.Add("name", GetType().Name);
        json.Add("target", null);
        return json;
    }

    public override void FromJson(JObject json)
    {
        target = BTValue<Entity>.FromJToken(json["target"]);
    }
}
