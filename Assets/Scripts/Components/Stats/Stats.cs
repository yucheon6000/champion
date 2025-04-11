using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine.Events;

public class Stats : IComponent
{
    private Dictionary<string, float> stats = new Dictionary<string, float>();
    public Dictionary<string, float> AllStats => stats;

    // Key, OldStatValue, NewStatValue
    public UnityEvent<string, float, float> OnStatChanged { private set; get; } = new UnityEvent<string, float, float>();

    public override void FromJson(JObject json)
    {
        stats.Clear();

        if (json.TryGetValue("values", out var valuesToken) && valuesToken is JObject valuesObj)
            foreach (var pair in valuesObj)
                SetStat(pair.Key, pair.Value.Value<float>());
    }

    public override JObject ToJson()
    {
        var json = base.ToJson();
        var valuesObj = new JObject();

        foreach (var kv in stats)
        {
            valuesObj[kv.Key] = kv.Value;
        }

        json["values"] = valuesObj;
        return json;
    }

    protected override string GetComponentType() => "Stats";

    // 💡 유틸 함수들

    public float GetStat(string key, float defaultValue = 0f)
    {
        return stats.TryGetValue(key, out var value) ? value : defaultValue;
    }

    public void SetStat(string key, float value)
    {
        float oldValue = GetStat(key);
        stats[key] = value;

        OnStatChanged.Invoke(key, oldValue, value);
    }

    public void ModifyStat(string key, float delta)
    {
        if (delta == 0) return;

        SetStat(key, GetStat(key) + delta);
    }

    public bool HasStat(string key)
    {
        return stats.ContainsKey(key);
    }
}