using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

[NodeDescription("Makes the entity win the game.")]
public class Win : ActionNode, IUsableNode
{
    public override NodeState Evaluate()
    {
        return WinLoseManager.Instance.Win() ? ReturnSuccess() : ReturnFailure();
    }

    public override JObject ToJson()
    {
        JObject json = base.ToJson();
        json.Add("name", GetType().Name);
        return json;
    }

    public override void FromJson(JObject json) { }
}
