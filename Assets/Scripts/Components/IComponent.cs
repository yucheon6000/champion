using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

public abstract class IComponent : MonoBehaviour
{
    public virtual JObject ToJson()
    {
        var json = new JObject();
        json.Add("type", GetComponentType());
        return json;
    }

    public abstract void FromJson(JObject json);

    protected abstract string GetComponentType();

    protected T FindControllerByIndex<T>(int index) where T : Controller
    {
        return ControllerManager.Instance.GetControllerByIndex<T>(index);
    }

    protected bool IsCorrectJson(JObject json)
    {
        if (json.ContainsKey("type") == false) return false;

        return json["type"].Value<string>() == GetComponentType();
    }
}
