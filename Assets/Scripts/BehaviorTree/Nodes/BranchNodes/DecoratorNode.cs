using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

public abstract class DecoratorNode : BranchNode
{
    [SerializeField]
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

    public override RequiresBTComponentAttribute[] GetRequiredBTComponents()
    {
        List<RequiresBTComponentAttribute> attrs = new List<RequiresBTComponentAttribute>(base.GetRequiredBTComponents());
        attrs.AddRange(childNode.GetRequiredBTComponents());

        return attrs.ToArray();
    }
}
