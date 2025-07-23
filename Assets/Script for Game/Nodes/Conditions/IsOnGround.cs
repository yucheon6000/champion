using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

[RequiresBTComponent(typeof(Movement))]

[NodeName(nameof(IsOnGround))]
[NodeDescription("Returns Success if the entity's feet are touching an object with the 'Ground' tag.")]
public class IsOnGround : ConditionNode
{
    private Movement movement;

    protected override void GetBTComponents()
    {
        base.GetBTComponents();
        movement = entity.GetComponent<Movement>();
    }

    public override NodeState Evaluate()
    {
        return movement.IsOnGround() ? ReturnSuccess() : ReturnFailure();
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
