using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

[NodeType("condition")]
public abstract class ConditionNode : LeafNode
{
    public override JObject ToJson()
    {
        var json = new JObject
        {
            { "type", "condition" }
        };
        return json;
    }
}
