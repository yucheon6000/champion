using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

public abstract class EffectOnTrigger : IComponent
{
    [SerializeField]
    protected string id = "";
    [SerializeField]
    protected List<string> targetTags = new();
    [SerializeField]
    protected List<IEffect> effects = new();

    protected void TriggerEffects(Entity target)
    {
        var effectEvent = new EffectEvent
        {
            source = entity,
            target = target,
            effects = new List<IEffect>(effects),
            sourceId = id
        };

        target.BroadcastMessage("OnEffect", effectEvent, SendMessageOptions.DontRequireReceiver);
        SendMessage("OnEffectGiven", effectEvent, SendMessageOptions.DontRequireReceiver);
    }

    protected bool MatchesTargetTag(Entity entity)
    {
        foreach (var tag in targetTags)
        {
            if (entity.HasTag(tag)) return true;
        }
        return false;
    }

    public override void FromJson(JObject json)
    {
        if (json.TryGetValue("id", out var idToken))
            id = idToken.Value<string>();

        targetTags.Clear();
        if (json.TryGetValue("targetTags", out var tagArray) && tagArray is JArray tags)
        {
            foreach (var tag in tags)
                targetTags.Add(tag.ToString());
        }

        effects.Clear();
        if (json.TryGetValue("effects", out var effectsArray) && effectsArray is JArray array)
        {
            foreach (var item in array)
            {
                if (item is JObject effectObj)
                {
                    var effect = EffectFactory.FromJson(effectObj);
                    if (effect != null) effects.Add(effect);
                }
            }
        }
    }

    public override JObject ToJson()
    {
        var json = base.ToJson();
        if (!string.IsNullOrEmpty(id))
            json["id"] = id;

        json["targetTags"] = new JArray(targetTags);

        var effectsArray = new JArray();
        foreach (var e in effects)
        {
            effectsArray.Add(e.ToJson());
        }

        json["effects"] = effectsArray;
        return json;
    }
}