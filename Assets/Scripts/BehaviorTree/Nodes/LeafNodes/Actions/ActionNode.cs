using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

[NodeType("action")]

public abstract class ActionNode : LeafNode
{
    public override JObject ToJson()
    {
        var json = new JObject
        {
            { "type", "action" }
        };
        return json;
    }
}
