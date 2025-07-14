using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CompositeNode : BranchNode
{
    protected List<Node> childrenNodes = new List<Node>();

    public override void AddChildNode(Node childNode)
    {
        if (childrenNodes.Contains(childNode)) return;

        childrenNodes.Add(childNode);

        childNode.SetParent(this);
    }

    public void AddChildrenNodes(IEnumerable<Node> childrenNodes)
    {
        foreach (var child in childrenNodes)
            AddChildNode(child);
    }

    public void RemoveAllChildrenNodes()
    {
        foreach (var child in childrenNodes)
            child.SetParent(null);

        childrenNodes.Clear();
    }
}
