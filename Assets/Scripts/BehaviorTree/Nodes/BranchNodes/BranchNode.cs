using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BranchNode : Node
{
    public abstract void AddChildNode(Node childNode);

    protected override string EditorTreeViewerParams()
    {
        return "";
    }
}
