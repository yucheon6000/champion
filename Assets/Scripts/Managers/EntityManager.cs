using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GenerativeAI;
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

    private string ToPascalCase(string str)
    {
        string result = str.Replace("_", " ").ToCamelCase();
        result = result[0].ToString().ToUpper() + result.Substring(1);
        return result;
    }

    public Entity CreateEntityFromPreset(string presetId)
        => CreateEntityFromPreset(presetId, Vector2.zero);

    // Default
    public Entity CreateEntityFromPreset(string presetId, Vector2 position)
    {
        JObject preestJson = PresetManager.Instance.GetPresetJson(presetId);

        if (preestJson == null) return null;

        Debug.Log($"[EntityManager] CreateEntityFromPreset: {presetId}");
        Entity clone = CreateEntityFromJson(preestJson, position);
        if (clone != null)
        {
            clone.gameObject.name = $"{ToPascalCase(presetId)} (Entity)";
            clone.SetPresetId(presetId);
            entities.Add(clone);
        }

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

        Entity entity = CreateEntityFromJson(json, pos);

        return entity;
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
            entity.Destroy();
            // Destroy(entity.gameObject);
        }
    }

    public void ClearAllEntities()
    {
        foreach (var e in entities)
        {
            try
            {
                Destroy(e.gameObject);
            }
            catch { }
        }

        entities.Clear();
    }

    public List<Entity> GetAllEntities()
    {
        return entities;
    }
}