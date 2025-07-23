using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

[RequiresBTComponent(typeof(Movement))]
[RequiresBTComponent(typeof(Gravity))]

[NodeName(nameof(Jump))]
[NodeParam("jumpForce", NodeParamType.FloatOrVariable, isRequired: true)]
[NodeDescription("Makes the entity perform a jump action using the specified jump force.")]
public class Jump : ActionNode, IUsableNode
{
    private Movement movement;

    private BTValue<float> jumpForce;

    protected override void GetBTComponents()
    {
        base.GetBTComponents();
        movement = entity.GetComponent<Movement>();
    }

    public override NodeState Evaluate()
    {
        movement.Jump(jumpForce.GetValue(entity.Blackboard));

        return ReturnSuccess();
    }

    public override JObject ToJson()
    {
        JObject json = base.ToJson();
        json.Add("jumpForce", jumpForce.ToJToken());
        return json;
    }

    public override void FromJson(JObject json)
    {
        jumpForce = BTValue<float>.FromJToken(json["jumpForce"], 1);
    }
}
