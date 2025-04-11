using Newtonsoft.Json.Linq;
using UnityEngine;

public static class EffectFactory
{
    public static IEffect FromJson(JObject json)
    {
        string type = json["type"]?.ToString();

        switch (type)
        {
            case "stat":
                var stat = new StatEffect();
                stat.FromJson(json);
                return stat;

            case "knockback":
                var knock = new KnockbackEffect();
                knock.FromJson(json);
                return knock;

            // 향후 확장
            // case "spawn": ...
            // case "animation": ...

            default:
                Debug.LogWarning($"Unknown effect type: {type}");
                return null;
        }
    }
}