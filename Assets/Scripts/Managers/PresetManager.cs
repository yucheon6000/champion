using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class PresetManager : MonoBehaviour
{
    public static PresetManager Instance;

    private Dictionary<string, JObject> presetMap = new();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    public void Reset()
    {
        presetMap.Clear();
    }

    public void Init(JObject presetsJObject)
    {
        Reset();

        foreach (var kv in presetsJObject)
        {
            string id = kv.Key;
            if (kv.Value is JObject json)
            {
                // Set preset
                presetMap[id] = json;

                // Generate sprite if not exists
                if (!PromptManager.Instance.HasSprite(id))
                    PromptManager.Instance.GenerateSprite(id, json["imagePrompt"].ToString());  // hasTransparent is true by default
            }
        }
    }

    public JObject GetPresetJson(string id)
    {
        if (presetMap.TryGetValue(id, out var preset))
            return preset.DeepClone() as JObject; // 안전하게 복제해서 반환
        return null;
    }
}