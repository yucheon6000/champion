using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class Spawner : IComponent
{
    public string presetId = "Enemy";
    public float spawnInterval = 1f;
    public int maxSpawnCount = -1; // -1이면 무제한 소환

    private float timer = 0f;
    private int spawnedCount = 0;

    public override void Init(Entity entity)
    {
        base.Init(entity);
        timer = 0f;
        spawnedCount = 0;
    }

    private void Update()
    {
        if (!init) return;

        timer += Time.deltaTime;

        bool canSpawnMore = (maxSpawnCount <= 0) || (spawnedCount < maxSpawnCount);

        if (timer >= spawnInterval && canSpawnMore)
        {
            timer = 0f;
            Spawn();
        }
    }

    private void Spawn()
    {
        // Debug.Log($"[Spawner] Spawning prefab: {presetId} from entity: {entity.name}");
        EntityManager.Instance.CreateEntityFromPreset(presetId, entity.transform.position);
        spawnedCount++;
    }

    public override JObject ToJson()
    {
        var json = base.ToJson();
        json["presetId"] = presetId;
        json["spawnInterval"] = spawnInterval;
        if (maxSpawnCount > 0)
            json["maxSpawnCount"] = maxSpawnCount;  // 무제한이면 저장 생략
        return json;
    }

    public override void FromJson(JObject json)
    {
        presetId = json.Value<string>("presetId") ?? "Enemy";

        float? interval = json.Value<float?>("spawnInterval");
        spawnInterval = (interval.HasValue && interval.Value > 0f) ? interval.Value : 1f;

        int? count = json.Value<int?>("maxSpawnCount");
        maxSpawnCount = (count.HasValue && count.Value > 0) ? count.Value : -1; // 무제한
    }

    protected override string GetComponentType() => "Spawner";
}