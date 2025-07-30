using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

// [NodeParam("delay", NodeParamType.Float, isRequired: false)]
public abstract class DestroySomething : ActionNode
{
    protected BTValue<float> delay;

    public override NodeState Evaluate()
    {
        if (GetEntity() == null) return ReturnFailure();

        Debug.Log($"{entity.gameObject.name} destorys {GetEntity().gameObject.name}.");

        EntityManager.Instance.DestroyEntity(GetEntity());

        return ReturnSuccess();
    }

    protected virtual Entity GetEntity()
    {
        return null;
    }

    public override JObject ToJson()
    {
        JObject json = base.ToJson();
        // json.Add("delay", delay.GetValue(entity.Blackboard));
        return json;
    }

    public override void FromJson(JObject json)
    {
        // delay = BTValue<float>.FromJToken(json["delay"]);
    }
}
