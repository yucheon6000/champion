using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class EffectReceiverKnockback : EffectReceiver
{
    protected override string EffectType => "knockback";

    protected override void ApplyEffect(Entity source, IEffect effect)
    {
        KnockbackEffect e = (KnockbackEffect)effect;

        Vector2 direction = Vector2.zero;

        switch (e.Direction)
        {
            case "facing":
                direction = source.transform.right.normalized;
                break;

            case "fixed":
                direction = new Vector2(e.FixedDirection[0], e.FixedDirection[1]).normalized;
                break;

            case "fromSelf":
                direction = (transform.position - source.transform.position).normalized;
                break;
        }

        // space 변환 (source 기준 local)
        if (e.Space == "local")
        {
            direction = source.transform.TransformDirection(direction).normalized;
        }

        // Rigidbody2D가 있으면 물리 기반 넉백
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.AddForce(direction * e.Force, ForceMode2D.Impulse);
        }
        // else
        // {
        //     // 없을 경우 수동 이동 (짧은 넉백 연출용)
        //     transform.position += (Vector3)(direction * e.Force);
        // }
    }

    public override void FromJson(JObject json) { }

    protected override string GetComponentType() => "EffectReceiverKnockback";
}
