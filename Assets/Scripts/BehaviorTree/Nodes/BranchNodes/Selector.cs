using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEngine;

[NodeName("Selector")]
[NodeDescription("Executes child nodes in order until one succeeds. Returns Success if any child succeeds, otherwise")]
public class Selector : CompositeNode, IUsableNode
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
                    return ReturnSuccess();
            }
        }

        return ReturnFailure();
    }
}
