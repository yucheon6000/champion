using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class Immovable : ComponentUsingRigidbody2D
{
    public override void Init(Entity entity)
    {
        base.Init(entity);

        CheckRigidbody2D();

        // Rigidbody2D의 이동과 회전을 모두 고정하여 외부 힘에 반응하지 않도록 설정
        if (rigidbody2D != null)
        {
            rigidbody2D.bodyType = RigidbodyType2D.Static;
        }
    }

    public override void FromJson(JObject json)
    {
        // Immovable은 별도 설정값이 없기 때문에 비워둠
    }

    protected override string GetComponentType() => "Immovable";
}
