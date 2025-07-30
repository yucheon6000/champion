using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

[NodeDescription("Destroys the current entity (the object with this node).")]
public class DestroySelf : DestroySomething, IUsableNode
{
    protected override Entity GetEntity()
    {
        return entity;
    }

    public override JObject ToJson()
    {
        JObject json = base.ToJson();
        json.Add("name", GetType().Name);
        return json;
    }
}
