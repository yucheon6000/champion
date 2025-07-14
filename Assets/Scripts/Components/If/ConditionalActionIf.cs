using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

public abstract class ConditionalActionIf : IComponent
{
    [SerializeField] protected string trigger = "";
    [SerializeField] protected string stat = "";
    [SerializeField] protected string operatorType = "lessThanOrEqual";
    [SerializeField] protected float value = 0f;
    [SerializeField] protected List<string> sourceFilters = new();
    protected const float EPSILON = 0.001f;

    public override void Init(Entity entity)
    {
        base.Init(entity);
        if (trigger == "onStatChanged")
        {
            GetIComponent<Stats>()?.OnStatChanged.AddListener(OnStatChanged);
        }
    }

    public override void FromJson(JObject json)
    {
        if (json.TryGetValue("trigger", out var trigToken))
            trigger = trigToken.Value<string>();
        if (json.TryGetValue("stat", out var statToken))
            stat = statToken.Value<string>();
        if (json.TryGetValue("operator", out var opToken))
            operatorType = opToken.Value<string>();
        if (json.TryGetValue("value", out var valToken))
            value = valToken.Value<float>();
        if (json.TryGetValue("sourceFilter", out var filterToken) && filterToken is JArray filterArray)
        {
            sourceFilters.Clear();
            foreach (var item in filterArray)
                sourceFilters.Add(item.ToString());
        }
    }

    public override JObject ToJson()
    {
        var json = base.ToJson();
        json["trigger"] = trigger;
        if (!string.IsNullOrEmpty(stat)) json["stat"] = stat;
        if (!string.IsNullOrEmpty(operatorType)) json["operator"] = operatorType;
        json["value"] = value;
        if (sourceFilters.Count > 0) json["sourceFilter"] = new JArray(sourceFilters);
        return json;
    }

    private void OnStatChanged(string changedStatKey, float oldValue, float newValue)
    {
        if (trigger != "onStatChanged" || changedStatKey != stat) return;
        if (EvaluateStatCondition(newValue)) ExecuteAction();
    }

    protected bool IsSourceMatch(string sourceId)
    {
        return sourceFilters.Count == 0 || sourceFilters.Contains(sourceId);
    }

    protected bool EvaluateStatConditionFromSelfOrIgnore()
    {
        if (string.IsNullOrEmpty(stat)) return true;
        var stats = GetIComponent<Stats>();
        if (stats == null) return false;
        return EvaluateStatCondition(stats.GetStat(stat));
    }

    protected bool EvaluateStatCondition(float currentValue)
    {
        return operatorType switch
        {
            "equal" => Mathf.Abs(currentValue - value) <= EPSILON,
            "greaterThan" => currentValue > value,
            "lessThan" => currentValue < value,
            "greaterThanOrEqual" => currentValue >= value - EPSILON,
            "lessThanOrEqual" => currentValue <= value + EPSILON,
            _ => false
        };
    }

    protected abstract void ExecuteAction();

    public void OnEffectGiven(EffectEvent evt)
    {
        if (trigger != "onEffectGiven") return;
        if (!IsSourceMatch(evt.sourceId)) return;
        // if (!EvaluateStatConditionFromSelfOrIgnore()) return;

        ExecuteAction();
    }

    public void OnEffectReceived(IEffect effect)
    {
        //??
    }
}