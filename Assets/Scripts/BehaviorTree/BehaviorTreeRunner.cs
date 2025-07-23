using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BehaviorTreeRunner
{
    private Entity entity;

    [SerializeReference]
    private Node rootNode;

    // 노드별 실행 상태 저장용 딕셔너리
    private Dictionary<Node, NodeState> nodeStates = new Dictionary<Node, NodeState>();

    public BehaviorTreeRunner(Entity entity, Node rootNode)
    {
        this.entity = entity;
        SetRootNode(rootNode);
    }

    public void SetRootNode(Node rootNode)
    {
        this.rootNode = rootNode;
        rootNode.Init(entity);
    }

    public void Execute()
    {
        nodeStates.Clear();

        NodeState rootNodeState = rootNode.Evaluate();

        SaveNodeState(rootNode, rootNodeState);
    }

    public void SaveNodeState(Node node, NodeState nodeState)
    {
        if (nodeStates.ContainsKey(node))
            nodeStates[node] = nodeState;
        else
            nodeStates.Add(node, nodeState);
    }
}
