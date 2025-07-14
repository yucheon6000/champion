using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BehaviorTreeRunner
{
    private Entity entity;
    private Node rootNode;

    public BehaviorTreeRunner(Entity entity, Node rootNode)
    {
        this.entity = entity;
        SetRootNode(rootNode);
    }

    public void SetRootNode(Node rootNode)
    {
        this.rootNode = rootNode;
    }

    public void Excute()
    {
        rootNode.Evaluate(entity);
    }
}
