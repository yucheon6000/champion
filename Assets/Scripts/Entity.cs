using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class Entity : MonoBehaviour
{
    [Header("Read Only!")]
    [SerializeField]
    private List<IComponent> components;

    public JObject ToJson()
    {
        var json = new JObject();

        var componentsJArray = new JArray();
        foreach (IComponent component in components)
            componentsJArray.Add(component.ToJson());

        json.Add("components", componentsJArray);

        return json;
    }

    public void FromJson(JObject json)
    {
        JArray componentsJArray = (JArray)json["components"];
        AddComponentByJson(componentsJArray);
    }

    private void AddComponentByJson(JArray jsonArray)
    {
        foreach (JObject json in jsonArray)
        {
            string typeName = json["type"].Value<string>();
            print(typeName);
            Type type = Type.GetType(typeName);
            print(type);
            if (type == null) continue;

            IComponent component = (IComponent)gameObject.AddComponent(type);
            component.FromJson(json);

            components.Add(component);
        }
    }
}
