using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class EntityManager : MonoBehaviour
{
    [SerializeField]
    private GameObject entityPrefab;

    public static EntityManager Instance;

    [SerializeField]
    private List<Entity> entities = new();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    public Entity CreateEntityFromPreset(string presetId)
        => CreateEntityFromPreset(presetId, Vector2.zero);

    // Default
    public Entity CreateEntityFromPreset(string presetId, Vector3 position)
    {
        JObject preestJson = PresetManager.Instance.GetPresetJson(presetId);

        if (preestJson == null) return null;

        Entity clone = CreateEntityFromJson(preestJson, position);
        if (clone != null)
            entities.Add(clone);

        return clone;
    }

    public Entity CreateEntityFromJson(JObject json)
    {
        Vector2 pos = Vector2.zero;

        // 위치 처리
        if (json.TryGetValue("position", out var positionToken))
        {
            var posRaw = positionToken.Values<float>().ToArray();
            if (posRaw.Length >= 2)
                pos = new Vector2(posRaw[0], posRaw[1]);
            else
                Debug.LogWarning("Position data is incomplete. Using default position (0, 0).");
        }
        else
            Debug.LogWarning("Position key not found in JSON. Using default position (0, 0).");

        // ⭐ 프리셋 기반 처리
        if (json.TryGetValue("presetId", out var presetToken))
        {
            string presetId = presetToken.Value<string>();
            JObject presetJson = PresetManager.Instance.GetPresetJson(presetId);

            if (presetJson == null)
            {
                Debug.LogWarning($"Preset '{presetId}' not found.");
                return null;
            }

            // 프리셋과 json 병합 (preset 먼저, json이 우선 적용됨)
            JObject combinedJson = (JObject)presetJson.DeepClone();
            foreach (var prop in json)
                combinedJson[prop.Key] = prop.Value;

            return CreateEntityFromJson(combinedJson, pos);
        }

        return CreateEntityFromJson(json, pos);
    }

    // Default
    public Entity CreateEntityFromJson(JObject json, Vector2 position)
    {
        Entity entity = Instantiate(entityPrefab, position, Quaternion.identity).GetComponent<Entity>();
        entity.FromJson(json);
        entities.Add(entity);

        return entity;
    }

    public void DestroyEntity(Entity entity)
    {
        if (entities.Contains(entity))
        {
            entities.Remove(entity);
            Destroy(entity.gameObject);
        }
    }

    public void ClearAllEntities()
    {
        foreach (var e in entities)
        {
            if (e != null) Destroy(e);
        }
        entities.Clear();
    }

    public List<Entity> GetAllEntities()
    {
        return entities;
    }
}