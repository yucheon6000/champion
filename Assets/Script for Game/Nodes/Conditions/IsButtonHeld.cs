using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

[NodeName(nameof(IsButtonHeld))]
[NodeDescription("Returns Success if the specified button is currently being held down.")]
public class IsButtonHeld : ButtonCondition, IUsableNode
{
    public override NodeState Evaluate()
    {
        return controllerButton.IsButtonHeld == true ? ReturnSuccess() : ReturnFailure();
    }

    public override JObject ToJson()
    {
        JObject json = base.ToJson();
        json.Add("name", GetType().Name);
        return json;
    }
}
