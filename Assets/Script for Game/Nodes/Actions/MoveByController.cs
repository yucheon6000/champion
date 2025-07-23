using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

[RequiresBTComponent(typeof(Movement))]

[NodeName(nameof(MoveByController))]
[NodeParam("controllerId", NodeParamType.String, isRequired: true)]
[NodeParam("moveSpeed", NodeParamType.FloatOrVariable, isRequired: true)]
[NodeDescription("Moves the entity in the direction provided by the controller, using the specified move speed.")]
public class MoveByController : ActionNode, IUsableNode
{
    private Movement movement;

    private string controllerId;
    private Controller2D controller;

    private BTValue<float> moveSpeed;

    protected override void GetBTComponents()
    {
        base.GetBTComponents();
        movement = entity.GetComponent<Movement>();
    }

    public override NodeState Evaluate()
    {
        if (controller == null) return ReturnFailure();

        movement.Move(controller.Direction, moveSpeed.GetValue(entity.Blackboard));

        return ReturnSuccess();
    }

    public override JObject ToJson()
    {
        JObject json = base.ToJson();
        json.Add("name", GetType().Name);
        json.Add("controllerId", controllerId);
        json.Add("moveSpeed", moveSpeed.ToJToken());
        return json;
    }

    public override void FromJson(JObject json)
    {
        try
        {
            controllerId = (string)json.GetValue("controllerId");
            controller = ControllerManager.Instance.GetControllerById<Controller2D>(controllerId);
        }
        catch { }

        moveSpeed = BTValue<float>.FromJToken(json["moveSpeed"], 3);
    }
}
