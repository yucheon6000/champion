using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

[NodeParam("seconds", NodeParamType.FloatOrVariable, isRequired: true)]
[NodeDescription("Waits for a specified number of seconds. Returns Success when the time has passed. Returns Running while waiting.")]
public class WaitSeconds : ActionNode, IUsableNode
{
    private BTValue<float> seconds;
    private float lastTime = 0f;

    public override NodeState Evaluate()
    {
        float secondsValue = seconds.GetValue(entity.Blackboard);
        float currentTime = Time.time;

        if (currentTime - lastTime >= secondsValue)
        {
            lastTime = currentTime;
            return ReturnSuccess();
        }

        return ReturnRunning();
    }

    public override JObject ToJson()
    {
        JObject json = base.ToJson();
        json.Add("name", GetType().Name);
        json.Add("seconds", seconds.GetValue(entity.Blackboard));
        return json;
    }

    public override void FromJson(JObject json)
    {
        seconds = BTValue<float>.FromJToken(json["seconds"]);
    }
}
