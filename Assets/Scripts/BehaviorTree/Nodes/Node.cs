using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

[Serializable]
public abstract class Node : INode
{
    protected Node parentNode = null;
    protected Entity entity = null;

    public void SetParent(Node parentNode)
    {
        this.parentNode = parentNode;
    }

    public virtual void Init(Entity entity)
    {
        this.entity = entity;
        GetBTComponents();
    }

    public virtual RequiresBTComponentAttribute[] GetRequiredBTComponents()
    {
        var nodeType = GetType();
        var requiredAttrs = (RequiresBTComponentAttribute[])nodeType.GetCustomAttributes(typeof(RequiresBTComponentAttribute), true);
        return requiredAttrs;
    }

    protected virtual void GetBTComponents() { }

    public abstract NodeState Evaluate();

    protected NodeState ReturnSuccess()
    {
        entity.BehaviorTreeRunner.SaveNodeState(this, NodeState.Success);
        return NodeState.Success;
    }

    protected NodeState ReturnRunning()
    {
        entity.BehaviorTreeRunner.SaveNodeState(this, NodeState.Running);
        return NodeState.Running;
    }

    protected NodeState ReturnFailure()
    {
        entity.BehaviorTreeRunner.SaveNodeState(this, NodeState.Failure);
        return NodeState.Failure;
    }

    public abstract JObject ToJson();
    public abstract void FromJson(JObject json);
}
