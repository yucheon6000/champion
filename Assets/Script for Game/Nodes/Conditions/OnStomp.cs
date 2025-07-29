using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

[RequiresBTComponent(typeof(Movement))]
[RequiresBTComponent(typeof(CollisionSensor))]

[NodeParam("tags", NodeParamType.StringList, isRequired: true)]
[NodeParam("outputTarget", NodeParamType.EntityVariable, isRequired: false)]
[NodeDescription("Returns Success if the entity's feet (the object with this node) hits an object with the specified tags, saving the collided entity to the blackboard. Otherwise returns Failure. outputTarget is optional.")]
public class OnStomp : ConditionNode, IUsableNode
{
    private Movement movement;
    private CollisionSensor collisionSensor;

    private string[] tags;
    private BTValue<Entity> outputTarget;

    protected override void GetBTComponents()
    {
        base.GetBTComponents();
        movement = entity.GetComponent<Movement>();
        collisionSensor = entity.GetComponent<CollisionSensor>();
    }

    public override NodeState Evaluate()
    {
        if (movement.Velocity.y < -0.01f)
        {
            if (collisionSensor.TryGetRecentCollision("down", tags, "enter", out Entity collidedEntity))
            {
                if (outputTarget != null)
                    outputTarget.SetValue(entity.Blackboard, collidedEntity);

                return ReturnSuccess();
            }
        }

        return ReturnFailure();
    }

    public override JObject ToJson()
    {
        JObject json = base.ToJson();
        json.Add("tags", "[" + string.Join(",", tags) + "]");
        json.Add("outputTarget", null);
        return json;
    }

    public override void FromJson(JObject json)
    {
        if (json.TryGetValue("tags", out var tagsToken))
            tags = tagsToken.ToObject<string[]>();

        if (json.TryGetValue("outputTarget", out var keyToken))
            outputTarget = BTValue<Entity>.FromJToken(keyToken);
    }
}
