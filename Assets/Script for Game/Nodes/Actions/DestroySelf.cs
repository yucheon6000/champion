using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

[NodeDescription("Destroys the current entity.")]
public class DestroySelf : ActionNode, IUsableNode
{
    public override NodeState Evaluate()
    {
        Object.Destroy(entity.gameObject);
        return ReturnSuccess();
    }

    public override JObject ToJson()
    {
        JObject json = base.ToJson();
        json.Add("name", GetType().Name);
        return json;
    }

    public override void FromJson(JObject json)
    {

    }
}
