using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

public abstract class InputCondition : ConditionNode
{ }

[NodeParam("buttonId", NodeParamType.String, isRequired: true)]
public abstract class ButtonCondition : InputCondition
{
    protected string buttonId;
    protected ControllerButton controllerButton;

    public override JObject ToJson()
    {
        JObject json = base.ToJson();
        json.Add("buttonId", buttonId);
        return json;
    }

    public override void FromJson(JObject json)
    {
        buttonId = (string)json["buttonId"];
        controllerButton = ControllerManager.Instance.GetControllerById<ControllerButton>(buttonId);
    }
}