using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

public enum NodeState
{
    Running, Success, Failure
}

public interface INode
{
    public NodeState Evaluate(Entity entity);

    public JObject ToJson();
    public void FromJson(JObject json);
}
