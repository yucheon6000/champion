using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class BehaviorTreeFactory
{
    public static Node CreateNodeFromJson(JObject nodeJson)
    {
        Debug.Log(nodeJson);
        string nodeName = (string)nodeJson["name"];
        if (string.IsNullOrEmpty(nodeName))
        {
            Debug.LogWarning("JSON에 노드 'name'이 없습니다.");
            return null;
        }

        Type nodeType = Type.GetType(nodeName);

        Node node = (Node)Activator.CreateInstance(nodeType);

        return node;
    }

}
