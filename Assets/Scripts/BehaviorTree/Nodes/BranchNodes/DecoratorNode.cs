using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class DecoratorNode : BranchNode
{
    protected Node childNode = null;

    public override void AddChildNode(Node childNode)
    {
        if (this.childNode == childNode) return;

        this.childNode = childNode;

        childNode.SetParent(this);
    }

    public void RemoveChildNode()
    {
        childNode.SetParent(this);

        childNode = null;
    }
}
