using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

[NodeType("composite")]
[NodeParam("children", NodeParamType.Nodes)]
public abstract class CompositeNode : BranchNode
{
    [SerializeField]
    protected List<Node> childrenNodes = new List<Node>();

    public override void Init(Entity entity)
    {
        base.Init(entity);
        foreach (Node child in childrenNodes)
            child.Init(entity);
    }

    public override void AddChildNode(Node childNode)
    {
        if (childrenNodes.Contains(childNode)) return;

        childrenNodes.Add(childNode);

        childNode.SetParent(this);
    }

    public void AddChildrenNodes(IEnumerable<Node> childrenNodes)
    {
        foreach (Node child in childrenNodes)
            AddChildNode(child);
    }

    public void RemoveChildNode(Node childNode)
    {
        childrenNodes.Remove(childNode);
    }

    public void RemoveAllChildrenNodes()
    {
        foreach (Node child in childrenNodes)
            child.SetParent(null);

        childrenNodes.Clear();
    }

    public override RequiresBTComponentAttribute[] GetRequiredBTComponents()
    {
        List<RequiresBTComponentAttribute> attrs = new List<RequiresBTComponentAttribute>(base.GetRequiredBTComponents());
        foreach (Node child in childrenNodes)
            attrs.AddRange(child.GetRequiredBTComponents());

        return attrs.ToArray();
    }

    public override JObject ToJson()
    {
        var json = new JObject
        {
            { "type", "composite" }
        };

        JArray childrenJArray = new JArray();
        foreach (Node childNode in childrenNodes)
            childrenJArray.Add(childNode.ToJson());

        json.Add("children", childrenJArray);

        return json;
    }

    public override void FromJson(JObject json)
    {
        RemoveAllChildrenNodes();

        JArray childrenJArray = (JArray)json["children"];
        foreach (JObject childJObject in childrenJArray)
        {
            Node childNode = BehaviorTreeFactory.CreateNodeFromJson(childJObject);
            childNode.FromJson(childJObject);

            childrenNodes.Add(childNode);
        }
    }
}
