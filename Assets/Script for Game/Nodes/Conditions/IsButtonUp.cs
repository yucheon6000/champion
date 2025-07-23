using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

[NodeName(nameof(IsButtonUp))]
[NodeDescription("Returns Success if the specified button was released this frame.")]
public class IsButtonUp : ButtonCondition, IUsableNode
{
    public override NodeState Evaluate()
    {
        return controllerButton.IsButtonUp == true ? ReturnSuccess() : ReturnFailure();
    }
}
