using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEngine;

[RequiresBTComponent(typeof(CollisionSensor))]

[NodeName(nameof(CheckCollision))]
[CustomNodeParam("direction", "up|down|right|left|any", isRequired: true)]
[CustomNodeParam("targetTags", "[string list]", isRequired: true)]
[CustomNodeParam("collisionType", "enter|exit|stay", isRequired: true)]
[NodeParam("outputTarget", NodeParamType.EntityVariable)]
[NodeDescription(
    "Returns Success if a recent collision (of the specified type) matches the given direction and target tags, saving the collided entity to the blackboard. Otherwise returns Failure. outputTargetKey is optional."
)]
public class CheckCollision : ConditionNode, IUsableNode
{
    private CollisionSensor sensor;

    private string direction;
    private string[] targetTags;
    private string collisionType;
    private BTValue<Entity> outputTarget;

    protected override void GetBTComponents()
    {
        base.GetBTComponents();
        sensor = entity.GetComponent<CollisionSensor>();
    }

    public override NodeState Evaluate()
    {
        if (sensor == null)
        {
            Debug.LogWarning($"[CheckCollision] {entity.name}에 CollisionSensor가 없습니다.");
            return ReturnFailure();
        }

        Entity collidedEntity;
        if (sensor.TryGetRecentCollision(direction, targetTags, collisionType, out collidedEntity) && collidedEntity != null)
        {
            if (outputTarget != null)
                outputTarget.SetValue(entity.Blackboard, collidedEntity);
            return ReturnSuccess();
        }
        return ReturnFailure();
    }

    public override JObject ToJson()
    {
        JObject json = base.ToJson();

        json.Add("name", nameof(CheckCollision));
        json.Add("direction", direction);
        json.Add("targetTags", "[" + string.Join(",", targetTags) + "]");
        json.Add("collisionType", collisionType);
        json.Add("outputTarget", null);

        return json;
    }

    public override void FromJson(JObject json)
    {
        if (json.TryGetValue("direction", out var dirToken))
            direction = dirToken.Value<string>();

        if (json.TryGetValue("targetTags", out var tagsToken))
            targetTags = tagsToken.ToObject<string[]>();

        if (json.TryGetValue("collisionType", out var typeToken))
            collisionType = typeToken.Value<string>();

        if (json.TryGetValue("outputTarget", out var keyToken))
            outputTarget = new BTValue<Entity>((string)keyToken, null);
    }
}
