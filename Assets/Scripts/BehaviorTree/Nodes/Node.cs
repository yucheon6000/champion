using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
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

    protected virtual string EditorTreeViewerParams()
    {
        JObject json = ToJson();
        json.Remove("type");
        json.Remove("name");

        if (json.Count == 0)
            return "";

        string jsonString = json.ToString();
        jsonString = jsonString.Replace("\"", "");
        jsonString = jsonString.Replace("\n", "");
        jsonString = jsonString.Replace("\r", "");
        jsonString = jsonString.Replace("\t", "");
        jsonString = jsonString.Replace(" ", "");
        jsonString = jsonString.Replace(":", ": ");
        jsonString = jsonString.Replace(",", ", ");
        return $"{jsonString}";
    }

    public string EditorTreeViewer(NodeState nodeState)
    {
        return $"{GetType().Name} : {EditorTreeViewerParams()} [{nodeState}]";
    }
}
