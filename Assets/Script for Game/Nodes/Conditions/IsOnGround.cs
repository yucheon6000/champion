using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

[RequiresBTComponent(typeof(CollisionSensor))]

[NodeName(nameof(IsOnGround))]
[NodeDescription("Returns Success if the entity's feet are touching an object with the 'Ground' tag.")]
public class IsOnGround : ConditionNode, IUsableNode
{
    private CollisionSensor collisionSensor;

    protected override void GetBTComponents()
    {
        base.GetBTComponents();
        collisionSensor = entity.GetComponent<CollisionSensor>();
    }

    public override NodeState Evaluate()
    {
        return collisionSensor.TryGetRecentCollision("down", new string[] { "Ground" }, "stay", out Entity collidedEntity)
                ? ReturnSuccess() : ReturnFailure();
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
