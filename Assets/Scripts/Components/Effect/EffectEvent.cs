using System.Collections.Generic;
using UnityEngine;

public class EffectEvent
{
    public Entity source;
    public Entity target;
    public List<IEffect> effects;

    public string sourceId;
}