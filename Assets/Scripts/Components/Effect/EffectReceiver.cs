using UnityEngine;
using Newtonsoft.Json.Linq;

public abstract class EffectReceiver : IComponent
{
    protected abstract string EffectType { get; }

    public virtual void OnEffect(EffectEvent evt)
    {
        foreach (var effect in evt.effects)
        {
            if (effect.Type == EffectType)
            {
                ApplyEffect(evt.source, effect);

                SendMessage("OnEffectReceived", effect, SendMessageOptions.DontRequireReceiver);
            }
        }
    }

    protected abstract void ApplyEffect(Entity source, IEffect effect);
}