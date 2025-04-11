using UnityEngine;

public class EffectOnHit : EffectOnTrigger
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        Entity targetEntity = collision.gameObject.GetComponent<Entity>();
        if (targetEntity != null && MatchesTargetTag(targetEntity))
        {
            TriggerEffects(targetEntity);
        }
    }

    protected override string GetComponentType() => "EffectOnHit";
}