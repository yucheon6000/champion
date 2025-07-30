using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

[NodeParam("target", NodeParamType.EntityVariable)]
[NodeDescription("Destroys the target entity.")]
public class DestroyTarget : DestroySomething, IUsableNode
{
    private BTValue<Entity> target;

    protected override Entity GetEntity()
    {
        Debug.Log($"[DestroyTarget] GetEntity: {target.GetValue(entity.Blackboard)}");
        return target.GetValue(entity.Blackboard);
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
