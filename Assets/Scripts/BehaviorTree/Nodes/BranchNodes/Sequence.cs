using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

[NodeName("Sequence")]
[NodeDescription("Executes child nodes in order until one fails or all succeed. Returns Success if all children succeed.")]
public class Sequence : CompositeNode, IUsableNode
{
    public override NodeState Evaluate()
    {
        if (childrenNodes.Count == 0)
            return ReturnFailure();

        foreach (var child in childrenNodes)
        {
            switch (child.Evaluate())
            {
                case NodeState.Running:
                    return ReturnRunning();
                case NodeState.Success:
                    continue;                   // Keep going
                case NodeState.Failure:
                    return ReturnFailure();
            }
        }

        return ReturnSuccess();
    }
}
