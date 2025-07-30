using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

[NodeParam("presetId", NodeParamType.String, isRequired: true)]
[NodeParam("spawnPoint", NodeParamType.EntityVariable, isRequired: false)]
[NodeParam("spawnedEntity", NodeParamType.EntityVariable, isRequired: false)]
[NodeDescription("Spawns an entity from a preset at the specified spawn point. If spawnPoint is not specified, the entity will be spawned at the current position of the entity with this node.")]
public class SpawnEntity : ActionNode, IUsableNode
{
    private BTValue<string> presetId;
    private BTValue<Entity> spawnPoint;
    private BTValue<Entity> spawnedEntity;

    public override NodeState Evaluate()
    {
        // Spawn
        Entity clonedEntity = EntityManager.Instance.CreateEntityFromPreset(presetId.GetValue(entity.Blackboard));

        // Set position
        if (spawnPoint == null || spawnPoint.GetValue(entity.Blackboard) == null)
            clonedEntity.transform.position = entity.transform.position;
        else
            clonedEntity.transform.position = spawnPoint.GetValue(entity.Blackboard).transform.position;

        // Output
        if (spawnedEntity != null)
            spawnedEntity.SetValue(entity.Blackboard, clonedEntity);

        return ReturnSuccess();
    }

    public override JObject ToJson()
    {
        JObject json = base.ToJson();
        json.Add("name", GetType().Name);
        json.Add("presetId", presetId.GetValue(entity.Blackboard));
        json.Add("spawnPoint", null);
        json.Add("spawnedEntity", null);
        return json;
    }

    public override void FromJson(JObject json)
    {
        presetId = BTValue<string>.FromJToken(json["presetId"]);

        if (json.TryGetValue("spawnPoint", out var spawnPointToken))
            spawnPoint = BTValue<Entity>.FromJToken(spawnPointToken);

        if (json.TryGetValue("spawnedEntity", out var spawnedEntityToken))
            spawnedEntity = BTValue<Entity>.FromJToken(spawnedEntityToken);
    }
}
