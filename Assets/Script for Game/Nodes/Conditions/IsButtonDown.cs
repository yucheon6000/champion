using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

[NodeName(nameof(IsButtonDown))]
[NodeDescription("Returns Success if the specified button was pressed down this frame.")]
public class IsButtonDown : ButtonCondition, IUsableNode
{
    public override NodeState Evaluate()
    {
        return controllerButton.IsButtonDown == true ? ReturnSuccess() : ReturnFailure();
    }
}
