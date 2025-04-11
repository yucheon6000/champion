using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class EffectReceiverStat : EffectReceiver
{
    protected override string EffectType => "stat";

    public override void FromJson(JObject json) { }

    protected override void ApplyEffect(Entity source, IEffect effect)
    {
        StatEffect e = (StatEffect)effect;
        var stats = GetIComponent<Stats>();
        stats.ModifyStat(e.Key, e.Value);
    }

    protected override string GetComponentType() => "EffectReceiverStat";
}