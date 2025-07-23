using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

[NodeName(nameof(Lose))]
[NodeDescription("Makes the entity lose the game.")]
public class Lose : ActionNode, IUsableNode
{
    public override NodeState Evaluate()
    {
        return WinLoseManager.Instance.Lose() ? ReturnSuccess() : ReturnFailure();
    }

    public override JObject ToJson()
    {
        JObject json = base.ToJson();
        json.Add("name", GetType().Name);
        return json;
    }

    public override void FromJson(JObject json) { }
}
