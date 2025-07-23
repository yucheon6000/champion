using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

public enum NodeState
{
    Running, Success, Failure,
    None
}

public interface INode
{
    public NodeState Evaluate();

    public JObject ToJson();
    public void FromJson(JObject json);
}

public interface IUsableNode { }