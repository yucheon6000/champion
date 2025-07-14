using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class MoveByController : ActionNode
{
    private string controllerId;
    private Controller2D controller;

    public override NodeState Evaluate(Entity entity)
    {
        if (controller == null) return NodeState.Failure;

        // Movement != null
        // Move(controller.Direction)
        // Movement == null
        // 있
        // 없으면 추가
        return NodeState.Success;
    }

    public override void FromJson(JObject json)
    {
        controllerId = json.GetValue("controllerId").ToString();
        controller = ControllerManager.Instance.GetControllerById<Controller2D>(controllerId);
    }

    public override JObject ToJson()
    {
        JObject json = base.ToJson();

        json.Add("task", "MoveByController");
        json.Add("controllerId", controllerId);

        return json;
    }
}
